using TaskCalendar.Domain.Enums;
using TaskItemStatus = TaskCalendar.Domain.Enums.TaskStatus;

namespace TaskCalendar.Application.Models;

public sealed class TaskOccurrence
{
    public Guid TaskId { get; init; }
    public Guid UserId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public TaskPriority Priority { get; init; }
    public TaskItemStatus Status { get; init; }
    public DateTimeOffset StartAt { get; init; }
    public DateTimeOffset EndAt { get; init; }
    public bool IsRecurring { get; init; }
}
