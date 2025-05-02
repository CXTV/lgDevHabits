using lgDevHabit.Api.Database;
using lgDevHabit.Api.DTOs.GitHub;
using lgDevHabit.Api.Entities;
using lgDevHabit.Api.Services.GitHub;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace lgDevHabit.Api.Jobs;

[DisallowConcurrentExecution]
public sealed class GitHubHabitProcessorJob(
    ApplicationDbContext dbContext,
    GitHubAccessTokenService gitHubAccessTokenService,
    GitHubService gitHubService,
    ILogger<GitHubHabitProcessorJob> logger) : IJob
{
    //GitHub 事件的常量，只处理 PushEvent 类型
    private const string PushEventType = "PushEvent";

    //Quartz.NET 调度器
    public async Task Execute(IJobExecutionContext context)
    {
        //从调度器中取出 habitId，这个 ID 是上一个调度 Job（scheduler）传入的
        string habitId = context.JobDetail.JobDataMap.GetString("habitId")
            ?? throw new InvalidOperationException("HabitId not found in job data");

        try
        {
            logger.LogInformation("Processing GitHub events for habit {HabitId}", habitId);

            // 去数据库里找 habitId 对应的 habit，确保：是 GitHub 自动化类型的没有被归档
            Habit? habit = await dbContext.Habits
                .FirstOrDefaultAsync(h => h.Id == habitId &&
                    h.AutomationSource == AutomationSource.GitHub &&
                    !h.IsArchived,
                    context.CancellationToken);

            if (habit is null)
            {
                logger.LogWarning("Habit {HabitId} not found or no longer configured for GitHub automation", habitId);
                return;
            }

            // Get the user's GitHub access token
            string? accessToken = await gitHubAccessTokenService.GetAsync(habit.UserId, context.CancellationToken);

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                logger.LogWarning("No GitHub access token found for user {UserId}", habit.UserId);
                return;
            }

            // Get GitHub profile
            GitHubUserProfileDto? profile = await gitHubService.GetUserProfileAsync(
                accessToken,
                context.CancellationToken);

            if (profile is null)
            {
                logger.LogWarning("Couldn't retrieve GitHub profile for user {UserId}", habit.UserId);
                return;
            }

            // Get GitHub events
            List<GitHubEventDto> gitHubEvents = [];
            const int perPage = 100;
            const int pagesToFetch = 10;

            for (int page = 1; page <= pagesToFetch; page++)
            {
                IReadOnlyList<GitHubEventDto>? pageEvents = await gitHubService.GetUserEventsAsync(
                    profile.Login,
                    accessToken,
                    page,
                    perPage,
                    context.CancellationToken);

                if (pageEvents is null || !pageEvents.Any())
                {
                    break;
                }

                gitHubEvents.AddRange(pageEvents);
            }

            if (!gitHubEvents.Any())
            {
                logger.LogWarning("Couldn't retrieve GitHub events for user {UserId}", habit.UserId);
                return;
            }

            // 只留下 PushEvent 类型的事件（即用户 push 的提交）
            var pushEvents = gitHubEvents
                .Where(a => a.Type == PushEventType)
                .ToList();

            logger.LogInformation("Found {Count} push events for habit {HabitId}", pushEvents.Count, habitId);

            foreach (GitHubEventDto gitHubEventDto in pushEvents)
            {
                // 检查该事件是否已存在（防止重复录入）
                bool exists = await dbContext.Entries.AnyAsync(
                    e => e.HabitId == habitId &&
                         e.ExternalId == gitHubEventDto.Id,
                    context.CancellationToken);

                if (exists)
                {
                    logger.LogDebug("Entry already exists for event {EventId}", gitHubEventDto.Id);
                    continue;
                }

                // Create a new entry
                var entry = new Entry
                {
                    Id = $"e_{Guid.CreateVersion7()}",
                    HabitId = habit.Id,
                    UserId = habit.UserId,
                    Value = 1, // Each push counts as 1
                    Notes =
                        $"""
                         {gitHubEventDto.Actor.Login} pushed:
                         
                         {string.Join(
                             Environment.NewLine,
                             gitHubEventDto.Payload.Commits?.Select(c => $"- {c.Message}") ?? [])}
                         """,
                    Date = DateOnly.FromDateTime(gitHubEventDto.CreatedAt),
                    Source = EntrySource.Automation,
                    ExternalId = gitHubEventDto.Id,
                    CreatedAtUtc = DateTime.UtcNow
                };

                dbContext.Entries.Add(entry);
                logger.LogInformation(
                    "Created entry for event {EventId} on habit {HabitId}",
                    gitHubEventDto.Id,
                    habitId);
            }

            await dbContext.SaveChangesAsync(context.CancellationToken);
            logger.LogInformation("Completed processing GitHub events for habit {HabitId}", habitId);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error processing GitHub events for habit {HabitId}",
                habitId);
            throw;
        }
    }
}
