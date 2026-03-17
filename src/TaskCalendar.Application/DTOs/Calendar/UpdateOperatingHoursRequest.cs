namespace TaskCalendar.Application.DTOs.Calendar;

public sealed class UpdateOperatingHoursRequest
{
    public List<UserOperatingHourDto> Days { get; set; } = [];
}
