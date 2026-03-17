using TaskCalendar.Domain.Enums;
using TaskItemStatus = TaskCalendar.Domain.Enums.TaskStatus;

namespace TaskCalendar.Application.DTOs.Calendar;

public sealed class TaskOccurrenceResponse
{
    public Guid TaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TaskPriority Priority { get; set; }
    public TaskItemStatus Status { get; set; }
    public DateTimeOffset StartAt { get; set; }
    public DateTimeOffset EndAt { get; set; }
    public bool IsRecurring { get; set; }
}
