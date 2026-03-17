using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskCalendar.Api.Extensions;
using TaskCalendar.Application.DTOs.Calendar;
using TaskCalendar.Domain.Entities;
using TaskCalendar.Infrastructure.Data;

namespace TaskCalendar.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class OperatingHoursController(TaskCalendarDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserOperatingHourDto>>> Get()
    {
        var userId = User.GetUserId();
        var days = await dbContext.UserOperatingHours
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.DayOfWeek)
            .Select(x => new UserOperatingHourDto
            {
                DayOfWeek = x.DayOfWeek,
                IsEnabled = x.IsEnabled,
                StartTime = x.StartTime,
                EndTime = x.EndTime
            })
            .ToListAsync();

        return Ok(days);
    }

    [HttpPut]
    public async Task<IActionResult> Update(UpdateOperatingHoursRequest request)
    {
        var userId = User.GetUserId();
        var existing = await dbContext.UserOperatingHours
            .Where(x => x.UserId == userId)
            .ToListAsync();

        dbContext.UserOperatingHours.RemoveRange(existing);
        await dbContext.UserOperatingHours.AddRangeAsync(request.Days.Select(day => new UserOperatingHour
        {
            UserId = userId,
            DayOfWeek = day.DayOfWeek,
            IsEnabled = day.IsEnabled,
            StartTime = day.StartTime,
            EndTime = day.EndTime
        }));

        await dbContext.SaveChangesAsync();
        return NoContent();
    }
}
