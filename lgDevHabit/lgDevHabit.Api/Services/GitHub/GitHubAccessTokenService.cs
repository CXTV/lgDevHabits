using lgDevHabit.Api.Database;
using lgDevHabit.Api.DTOs.GitHub;
using lgDevHabit.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace lgDevHabit.Api.Services.GitHub;

public sealed class GitHubAccessTokenService(
    ApplicationDbContext dbContext,
    EncryptionService encryptionService
    )
{
    //存储GitHub Access Token
    public async Task StoreAsync(
        string userId,
        StoreGitHubAccessTokenDto accessTokenDto,
        CancellationToken cancellationToken = default)
    {
        //查询数据库里的token
        GitHubAccessToken? existingAccessToken = await GetAccessTokenAsync(userId, cancellationToken);
        //加密token
        string accessToken = encryptionService.Encrypt(accessTokenDto.AccessToken);

        if (existingAccessToken is not null)
        {
            existingAccessToken.Token = accessToken;
            existingAccessToken.ExpiresAtUtc = DateTime.UtcNow.AddDays(accessTokenDto.ExpiresInDays);
        }
        else
        {
            dbContext.GitHubAccessTokens.Add(new GitHubAccessToken
            {
                Id = $"gh_{Guid.CreateVersion7()}",
                UserId = userId,
                Token = accessToken,
                CreatedAtUtc = DateTime.UtcNow,
                ExpiresAtUtc = DateTime.UtcNow.AddDays(accessTokenDto.ExpiresInDays)
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    //获取GitHub Access Token,并解密
    public async Task<string?> GetAsync(string userId, CancellationToken cancellationToken = default)
    {
        GitHubAccessToken? accessToken = await GetAccessTokenAsync(userId, cancellationToken);

        if (accessToken is null)
        {
            return null;
        }
        string decryptedToken = encryptionService.Decrypt(accessToken.Token);

        return decryptedToken;
    }

    //撤销GitHub Access Token
    public async Task RevokeAsync(string userId, CancellationToken cancellationToken = default)
    {
        GitHubAccessToken? accessToken = await GetAccessTokenAsync(userId, cancellationToken);

        if (accessToken is null)
        {
            return;
        }

        dbContext.GitHubAccessTokens.Remove(accessToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
    //读取数据库中的token
    private async Task<GitHubAccessToken?> GetAccessTokenAsync(string userId, CancellationToken cancellationToken)
    {
        return await dbContext.GitHubAccessTokens
            .SingleOrDefaultAsync(at => at.UserId == userId, cancellationToken);
    }
}
