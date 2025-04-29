using lgDevHabit.Api.DTOs.GitHub;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace lgDevHabit.Api.Services.GitHub;


public sealed class GitHubService(IHttpClientFactory httpClientFactory, ILogger<GitHubService> logger)
{
    //调用 GitHub API 的 /user 接口，获取当前登录用户的基本信息
    public async Task<GitHubUserProfileDto?> GetUserProfileAsync(
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        using HttpClient client = CreateGitHubClient(accessToken);
        //时间请求的是github/user的接口
        HttpResponseMessage response = await client.GetAsync("user", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("Failed to get user profile from GitHub. Status code: {StatusCode}", response.StatusCode);
            return null;
        }

        string content = await response.Content.ReadAsStringAsync(cancellationToken);

        return JsonConvert.DeserializeObject<GitHubUserProfileDto>(content);
    }

    //获取指定用户最近的 GitHub 活动
    public async Task<IReadOnlyList<GitHubEventDto>?> GetUserEventsAsync(
        string username,
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        //检查用户名是否为空
        ArgumentException.ThrowIfNullOrEmpty(username);

        using HttpClient client = CreateGitHubClient(accessToken);
        //请求的是github/users/{username}/events的接口
        HttpResponseMessage response = await client.GetAsync(
            $"users/{username}/events?per_page=100",
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("Failed to get user events from GitHub. Status code: {StatusCode}", response.StatusCode);
            return null;
        }

        //读取响应体（内容），并将其转换为字符串。
        string content = await response.Content.ReadAsStringAsync(cancellationToken);
        //使用 Newtonsoft.Json 将 JSON 字符串反序列化为 GitHubEventDto 的只读列表
        return JsonConvert.DeserializeObject<IReadOnlyList<GitHubEventDto>>(content);
    }

    //创建为 GitHub API 配置好的 HttpClient并附带了访问令牌用于认证，供后续请求使用。
    private HttpClient CreateGitHubClient(string accessToken)
    {
        HttpClient client = httpClientFactory.CreateClient("github");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        return client;
    }
}
