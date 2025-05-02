namespace lgDevHabit.Api.Entities;

public sealed class Entry
{
    public string Id { get; set; } // 条目的唯一标识符，例如 "e_xxxxx"
    public string HabitId { get; set; } // 所属的习惯 ID（外键，关联 Habit 表）
    public string UserId { get; set; } // 该条目所属用户的 ID
    public int Value { get; set; } // 数值，比如某习惯的完成次数、时长等
    public string? Notes { get; set; } // 备注信息，可选填写
    public EntrySource Source { get; init; } // 条目的来源（手动、自动、文件导入等）
    public string? ExternalId { get; init; } // 外部系统的 ID（用于去重或跟踪导入来源）
    public bool IsArchived { get; set; } // 是否被归档，不再显示在活跃列表中
    public DateOnly Date { get; set; } // 条目的日期（例如 2025-04-21）
    public DateTime CreatedAtUtc { get; set; } // 创建时间（UTC 时间）
    public DateTime? UpdatedAtUtc { get; set; } // 最后更新时间（可为空）

    public Habit Habit { get; set; } // 导航属性，用于 Entity Framework 中的关联查询


}

public enum EntrySource
{
    Manual = 0,      // 手动添加
    Automation = 1,  // 通过系统自动生成（例如 GitHub 数据等）
}
