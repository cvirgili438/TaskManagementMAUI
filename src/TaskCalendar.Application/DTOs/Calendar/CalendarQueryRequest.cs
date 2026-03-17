namespace TaskCalendar.Application.DTOs.Calendar;

public sealed class CalendarQueryRequest
{
    public DateTimeOffset From { get; set; }
    public DateTimeOffset To { get; set; }
}
