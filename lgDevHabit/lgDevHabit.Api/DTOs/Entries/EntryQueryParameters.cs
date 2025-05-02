using lgDevHabit.Api.DTOs.Common;

namespace lgDevHabit.Api.DTOs.Entries;


public sealed record EntryQueryParameters : AcceptHeaderDto
{
    public string? Fields { get; init; }
}
