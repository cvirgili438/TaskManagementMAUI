namespace TaskCalendar.Domain.Entities;

public sealed class UserOperatingHour
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public bool IsEnabled { get; set; } = true;
    public TimeOnly StartTime { get; set; } = new(9, 0);
    public TimeOnly EndTime { get; set; } = new(18, 0);
}
