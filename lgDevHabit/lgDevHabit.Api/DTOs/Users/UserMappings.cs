using lgDevHabit.Api.DTOs.Auth;
using lgDevHabit.Api.Entities;

namespace lgDevHabit.Api.DTOs.Users;

public static class UserMappings
{
    public static User ToEntity(this RegisterUserDto dto)
    {
        return new User
        {
            Id = $"u_{Guid.CreateVersion7()}",
            Email = dto.Email,
            Name = dto.Name,
            CreatedAtUtc = DateTime.UtcNow
        };
    }
}
