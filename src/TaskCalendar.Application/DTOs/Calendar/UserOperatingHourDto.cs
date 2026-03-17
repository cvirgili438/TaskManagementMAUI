namespace TaskCalendar.Application.DTOs.Calendar;

public sealed class UserOperatingHourDto
{
    public DayOfWeek DayOfWeek { get; set; }
    public bool IsEnabled { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
}
