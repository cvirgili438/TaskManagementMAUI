using TaskCalendar.Application.Services;
using TaskCalendar.Domain.Entities;
using TaskCalendar.Domain.Enums;

namespace TaskCalendar.Tests;

public sealed class RecurrenceCalculatorTests
{
    [Fact]
    public void ExpandOccurrences_ShouldReturnOccurrencesInsideRange()
    {
        var start = new DateTimeOffset(2026, 3, 17, 9, 0, 0, TimeSpan.Zero);
        var task = new ScheduledTask
        {
            Title = "Daily focus",
            StartAt = start,
            EndAt = start.AddHours(1),
            RecurrenceType = RecurrenceType.Daily,
            RecurrenceInterval = 1,
            RecurrenceEndAt = start.AddDays(2)
        };

        var occurrences = RecurrenceCalculator.ExpandOccurrences(task, start, start.AddDays(3));

        Assert.Equal(3, occurrences.Count);
        Assert.All(occurrences, occurrence => Assert.Equal(TimeSpan.FromHours(1), occurrence.EndAt - occurrence.StartAt));
    }
}
