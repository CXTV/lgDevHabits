namespace lgDevHabit.Api.Entities;

public sealed class Tag
{
    public string Id { get; set; }
    public string UserId { get; set; }  //用户ID
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}
