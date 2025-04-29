using lgDevHabit.Api.Entities;
using Microsoft.AspNetCore.Mvc;

namespace lgDevHabit.Api.DTOs.Habits;

public sealed record HabitsQueryParameters
{
    [FromQuery(Name = "q")]
    public string? Search { get; set; }
    public HabitType? Type { get; init; }
    public HabitStatus? Status { get; init; }
    //排序
    public string? Sort { get; init; }
    //分页
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    //字段
    public string? Fields { get; init; }
    //Content Negotiation
    [FromHeader(Name = "Accept")]
    public string? Accept { get; init; }
}
