using TaskCalendar.Application.DTOs.Calendar;
using TaskCalendar.Application.Services;
using TaskCalendar.Domain.Entities;
using TaskCalendar.Domain.Enums;
using TaskItemStatus = TaskCalendar.Domain.Enums.TaskStatus;

namespace TaskCalendar.Tests;

public sealed class TaskValidationServiceTests
{
    private readonly TaskValidationService _service = new();
    private readonly Guid _userId = Guid.NewGuid();

    [Fact]
    public void Validate_ShouldRejectOverlapGreaterThanFiveMinutes()
    {
        var request = new TaskItemRequest
        {
            Title = "Second task",
            Description = "Overlap test",
            Priority = TaskPriority.Medium,
            Status = TaskItemStatus.Pending,
            StartAt = new DateTimeOffset(2026, 3, 17, 10, 4, 0, TimeSpan.Zero),
            EndAt = new DateTimeOffset(2026, 3, 17, 11, 0, 0, TimeSpan.Zero)
        };

        var existing = new[]
        {
            new ScheduledTask
            {
                UserId = _userId,
                Title = "Existing task",
                StartAt = new DateTimeOffset(2026, 3, 17, 9, 0, 0, TimeSpan.Zero),
                EndAt = new DateTimeOffset(2026, 3, 17, 10, 10, 0, TimeSpan.Zero)
            }
        };

        var operatingHours = BuildOperatingHours();

        var result = _service.Validate(request, existing, operatingHours, _userId);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Contains("overlaps", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_ShouldAllowTaskInsideOperatingHoursWithoutMeaningfulOverlap()
    {
        var request = new TaskItemRequest
        {
            Title = "Planning block",
            Description = "Valid test",
            Priority = TaskPriority.High,
            Status = TaskItemStatus.Pending,
            StartAt = new DateTimeOffset(2026, 3, 17, 10, 5, 0, TimeSpan.Zero),
            EndAt = new DateTimeOffset(2026, 3, 17, 11, 0, 0, TimeSpan.Zero)
        };

        var existing = new[]
        {
            new ScheduledTask
            {
                UserId = _userId,
                Title = "Existing task",
                StartAt = new DateTimeOffset(2026, 3, 17, 9, 0, 0, TimeSpan.Zero),
                EndAt = new DateTimeOffset(2026, 3, 17, 10, 10, 0, TimeSpan.Zero)
            }
        };

        var operatingHours = BuildOperatingHours();

        var result = _service.Validate(request, existing, operatingHours, _userId);

        Assert.True(result.IsValid);
    }

    private IEnumerable<UserOperatingHour> BuildOperatingHours()
    {
        return Enum.GetValues<DayOfWeek>().Select(day => new UserOperatingHour
        {
            UserId = _userId,
            DayOfWeek = day,
            IsEnabled = true,
            StartTime = new TimeOnly(8, 0),
            EndTime = new TimeOnly(18, 0)
        });
    }
}
