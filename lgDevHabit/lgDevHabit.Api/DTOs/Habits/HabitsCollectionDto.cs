using lgDevHabit.Api.DTOs.Common;

namespace lgDevHabit.Api.DTOs.Habits;

public sealed record HabitsCollectionDto: ICollectionResponse<HabitDto>
{
    public List<HabitDto> Items { get; init; }
}
