using lgDevHabit.Api.Database;
using lgDevHabit.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace lgDevHabit.Api.Jobs;


[DisallowConcurrentExecution] //不能并发
public sealed class GitHubAutomationSchedulerJob(
    ApplicationDbContext dbContext,
    ILogger<GitHubAutomationSchedulerJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            logger.LogInformation("Starting GitHub automation scheduler job");

            //获取数据库中标志是github自动获取的习惯
            List<Habit> habitsToProcess = await dbContext.Habits
                .Where(h => h.AutomationSource == AutomationSource.GitHub && !h.IsArchived)
                .ToListAsync(context.CancellationToken);

            logger.LogInformation("Found {Count} habits with GitHub automation", habitsToProcess.Count);

            //创建一个触发器给每个habit，立即执行
            foreach (Habit habit in habitsToProcess)
            {
                // Create a trigger for immediate execution
                ITrigger trigger = TriggerBuilder.Create()
                    .WithIdentity($"github-habit-{habit.Id}", "github-habits")
                    .StartNow()
                    .Build();

                // Create the job with habit data
                IJobDetail jobDetail = JobBuilder.Create<GitHubHabitProcessorJob>()
                    .WithIdentity($"github-habit-{habit.Id}", "github-habits")
                    .UsingJobData("habitId", habit.Id)
                    .Build();

                // Schedule the job
                await context.Scheduler.ScheduleJob(jobDetail, trigger);
                logger.LogInformation("Scheduled processor job for habit {HabitId}", habit.Id);
            }

            logger.LogInformation("Completed GitHub automation scheduler job");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error executing GitHub automation scheduler job");
            throw;
        }
    }
}
