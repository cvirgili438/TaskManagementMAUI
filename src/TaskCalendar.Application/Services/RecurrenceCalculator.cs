using TaskCalendar.Application.Models;
using TaskCalendar.Domain.Entities;
using TaskCalendar.Domain.Enums;

namespace TaskCalendar.Application.Services;

public static class RecurrenceCalculator
{
    public static IReadOnlyList<TaskOccurrence> ExpandOccurrences(
        ScheduledTask task,
        DateTimeOffset rangeStart,
        DateTimeOffset rangeEnd)
    {
        var occurrences = new List<TaskOccurrence>();
        if (rangeEnd <= rangeStart)
        {
            return occurrences;
        }

        var duration = task.EndAt - task.StartAt;
        if (duration <= TimeSpan.Zero)
        {
            return occurrences;
        }

        var cursor = task.StartAt;
        var effectiveEnd = task.RecurrenceType == RecurrenceType.None
            ? task.EndAt
            : task.RecurrenceEndAt ?? rangeEnd;

        while (cursor <= effectiveEnd)
        {
            var occurrenceEnd = cursor + duration;
            if (occurrenceEnd >= rangeStart && cursor <= rangeEnd)
            {
                occurrences.Add(new TaskOccurrence
                {
                    TaskId = task.Id,
                    UserId = task.UserId,
                    Title = task.Title,
                    Description = task.Description,
                    Priority = task.Priority,
                    Status = task.Status,
                    StartAt = cursor,
                    EndAt = occurrenceEnd,
                    IsRecurring = task.RecurrenceType != RecurrenceType.None
                });
            }

            if (task.RecurrenceType == RecurrenceType.None)
            {
                break;
            }

            cursor = task.RecurrenceType switch
            {
                RecurrenceType.Daily => cursor.AddDays(task.RecurrenceInterval),
                RecurrenceType.Weekly => cursor.AddDays(task.RecurrenceInterval * 7),
                RecurrenceType.Monthly => cursor.AddMonths(task.RecurrenceInterval),
                _ => cursor
            };

            if (cursor == task.StartAt)
            {
                break;
            }
        }

        return occurrences;
    }
}
