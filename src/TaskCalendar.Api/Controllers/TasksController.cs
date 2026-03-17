using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskCalendar.Api.Extensions;
using TaskCalendar.Application.DTOs.Calendar;
using TaskCalendar.Application.Services;
using TaskCalendar.Domain.Entities;
using TaskCalendar.Infrastructure.Data;

namespace TaskCalendar.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class TasksController(
    TaskCalendarDbContext dbContext,
    TaskValidationService validationService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TaskOccurrenceResponse>>> GetOccurrences([FromQuery] DateTimeOffset from, [FromQuery] DateTimeOffset to)
    {
        var userId = User.GetUserId();
        var tasks = await dbContext.ScheduledTasks
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.StartAt)
            .ToListAsync();

        var occurrences = tasks
            .SelectMany(task => RecurrenceCalculator.ExpandOccurrences(task, from, to))
            .OrderBy(x => x.StartAt)
            .Select(x => new TaskOccurrenceResponse
            {
                TaskId = x.TaskId,
                Title = x.Title,
                Description = x.Description,
                Priority = x.Priority,
                Status = x.Status,
                StartAt = x.StartAt,
                EndAt = x.EndAt,
                IsRecurring = x.IsRecurring
            })
            .ToList();

        return Ok(occurrences);
    }

    [HttpGet("definitions")]
    public async Task<ActionResult<IReadOnlyList<TaskItemResponse>>> GetDefinitions()
    {
        var userId = User.GetUserId();
        var items = await dbContext.ScheduledTasks
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.StartAt)
            .Select(x => new TaskItemResponse
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                Priority = x.Priority,
                Status = x.Status,
                StartAt = x.StartAt,
                EndAt = x.EndAt,
                RecurrenceType = x.RecurrenceType,
                RecurrenceInterval = x.RecurrenceInterval,
                RecurrenceEndAt = x.RecurrenceEndAt
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpPost]
    public async Task<ActionResult<TaskItemResponse>> Create(TaskItemRequest request)
    {
        var userId = User.GetUserId();
        var existingTasks = await dbContext.ScheduledTasks.Where(x => x.UserId == userId).ToListAsync();
        var operatingHours = await dbContext.UserOperatingHours.Where(x => x.UserId == userId).ToListAsync();

        var validation = validationService.Validate(request, existingTasks, operatingHours, userId);
        if (!validation.IsValid)
        {
            return BadRequest(validation.Errors);
        }

        var entity = new ScheduledTask
        {
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

        dbContext.ScheduledTasks.Add(entity);
        await dbContext.SaveChangesAsync();

        return Ok(ToResponse(entity));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TaskItemResponse>> Update(Guid id, TaskItemRequest request)
    {
        var userId = User.GetUserId();
        var entity = await dbContext.ScheduledTasks.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
        if (entity is null)
        {
            return NotFound();
        }

        var existingTasks = await dbContext.ScheduledTasks.Where(x => x.UserId == userId).ToListAsync();
        var operatingHours = await dbContext.UserOperatingHours.Where(x => x.UserId == userId).ToListAsync();
        var validation = validationService.Validate(request, existingTasks, operatingHours, userId, id);
        if (!validation.IsValid)
        {
            return BadRequest(validation.Errors);
        }

        entity.Title = request.Title.Trim();
        entity.Description = request.Description.Trim();
        entity.Priority = request.Priority;
        entity.Status = request.Status;
        entity.StartAt = request.StartAt;
        entity.EndAt = request.EndAt;
        entity.RecurrenceType = request.RecurrenceType;
        entity.RecurrenceInterval = request.RecurrenceInterval;
        entity.RecurrenceEndAt = request.RecurrenceEndAt;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync();
        return Ok(ToResponse(entity));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = User.GetUserId();
        var entity = await dbContext.ScheduledTasks.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
        if (entity is null)
        {
            return NotFound();
        }

        dbContext.ScheduledTasks.Remove(entity);
        await dbContext.SaveChangesAsync();
        return NoContent();
    }

    private static TaskItemResponse ToResponse(ScheduledTask entity)
    {
        return new TaskItemResponse
        {
            Id = entity.Id,
            Title = entity.Title,
            Description = entity.Description,
            Priority = entity.Priority,
            Status = entity.Status,
            StartAt = entity.StartAt,
            EndAt = entity.EndAt,
            RecurrenceType = entity.RecurrenceType,
            RecurrenceInterval = entity.RecurrenceInterval,
            RecurrenceEndAt = entity.RecurrenceEndAt
        };
    }
}
