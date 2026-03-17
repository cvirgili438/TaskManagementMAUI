using TaskCalendar.Application.DTOs.Calendar;
using TaskCalendar.Application.Models;
using TaskCalendar.Domain.Entities;
using TaskCalendar.Domain.Enums;

namespace TaskCalendar.Application.Services;

public sealed class TaskValidationService
{
    private static readonly TimeSpan MaxAllowedOverlap = TimeSpan.FromMinutes(5);

    public ValidationResult Validate(
        TaskItemRequest request,
        IEnumerable<ScheduledTask> existingTasks,
        IEnumerable<UserOperatingHour> operatingHours,
        Guid userId,
        Guid? currentTaskId = null)
    {
        var result = new ValidationResult();

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            result.Errors.Add("The title is required.");
        }

        if (request.EndAt <= request.StartAt)
        {
            result.Errors.Add("The end date must be greater than the start date.");
        }

        if (request.RecurrenceType != RecurrenceType.None && request.RecurrenceEndAt is null)
        {
            result.Errors.Add("Recurring tasks require an end date.");
        }

        if (request.RecurrenceInterval <= 0)
        {
            result.Errors.Add("Recurrence interval must be greater than zero.");
        }

        if (!result.IsValid)
        {
            return result;
        }

        var candidate = new ScheduledTask
        {
            Id = currentTaskId ?? Guid.NewGuid(),
            UserId = userId,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Priority = request.Priority,
            Status = request.Status,
            StartAt = request.StartAt,
            EndAt = request.EndAt,
            RecurrenceType = request.RecurrenceType,
            RecurrenceInterval = request.RecurrenceInterval,
            RecurrenceEndAt = request.RecurrenceEndAt
        };

        var validationEnd = candidate.RecurrenceEndAt ?? candidate.EndAt;
        var candidateOccurrences = RecurrenceCalculator.ExpandOccurrences(candidate, candidate.StartAt, validationEnd);
        if (candidateOccurrences.Count == 0)
        {
            result.Errors.Add("Unable to generate valid task occurrences.");
            return result;
        }

        var hourMap = operatingHours.ToDictionary(x => x.DayOfWeek);
        foreach (var occurrence in candidateOccurrences)
        {
            if (!hourMap.TryGetValue(occurrence.StartAt.DayOfWeek, out var dayHour) || !dayHour.IsEnabled)
            {
                result.Errors.Add($"No operating hours configured for {occurrence.StartAt.DayOfWeek}.");
                continue;
            }

            var startTime = TimeOnly.FromDateTime(occurrence.StartAt.LocalDateTime);
            var endTime = TimeOnly.FromDateTime(occurrence.EndAt.LocalDateTime);
            if (startTime < dayHour.StartTime || endTime > dayHour.EndTime)
            {
                result.Errors.Add($"The task must stay inside the operating hours for {occurrence.StartAt.DayOfWeek}.");
            }
        }

        if (!result.IsValid)
        {
            return result;
        }

        var peers = existingTasks
            .Where(x => x.UserId == userId && x.Id != currentTaskId)
            .ToList();

        foreach (var existingTask in peers)
        {
            var existingWindowStart = candidate.StartAt < existingTask.StartAt ? candidate.StartAt : existingTask.StartAt;
            var existingWindowEnd = validationEnd > (existingTask.RecurrenceEndAt ?? existingTask.EndAt)
                ? validationEnd
                : existingTask.RecurrenceEndAt ?? existingTask.EndAt;

            var existingOccurrences = RecurrenceCalculator.ExpandOccurrences(existingTask, existingWindowStart, existingWindowEnd);
            foreach (var candidateOccurrence in candidateOccurrences)
            {
                foreach (var existingOccurrence in existingOccurrences)
                {
                    var overlapStart = candidateOccurrence.StartAt > existingOccurrence.StartAt
                        ? candidateOccurrence.StartAt
                        : existingOccurrence.StartAt;
                    var overlapEnd = candidateOccurrence.EndAt < existingOccurrence.EndAt
                        ? candidateOccurrence.EndAt
                        : existingOccurrence.EndAt;

                    var overlap = overlapEnd - overlapStart;
                    if (overlap > MaxAllowedOverlap)
                    {
                        result.Errors.Add(
                            $"The task overlaps with '{existingOccurrence.Title}' on {candidateOccurrence.StartAt:yyyy-MM-dd HH:mm}.");
                        return result;
                    }
                }
            }
        }

        return result;
    }
}
