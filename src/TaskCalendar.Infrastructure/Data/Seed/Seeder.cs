using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskCalendar.Domain.Entities;
using TaskCalendar.Domain.Enums;
using TaskCalendar.Infrastructure.Identity;
using TaskItemStatus = TaskCalendar.Domain.Enums.TaskStatus;

namespace TaskCalendar.Infrastructure.Data.Seed;

public static class Seeder
{
    public static async Task SeedAsync(TaskCalendarDbContext dbContext, UserManager<AppUser> userManager)
    {
        await dbContext.Database.EnsureCreatedAsync();

        var demoEmail = "demo@taskcalendar.local";
        var user = await userManager.Users.FirstOrDefaultAsync(x => x.Email == demoEmail);
        if (user is null)
        {
            user = new AppUser
            {
                UserName = demoEmail,
                Email = demoEmail,
                DisplayName = "Demo User",
                PreferredCulture = "es-AR",
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(user, "Demo123!");
            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(string.Join(", ", createResult.Errors.Select(x => x.Description)));
            }
        }

        var hasHours = await dbContext.UserOperatingHours.AnyAsync(x => x.UserId == user.Id);
        if (!hasHours)
        {
            var defaults = Enum.GetValues<DayOfWeek>()
                .Select(day => new UserOperatingHour
                {
                    UserId = user.Id,
                    DayOfWeek = day,
                    IsEnabled = day is not DayOfWeek.Saturday and not DayOfWeek.Sunday,
                    StartTime = new TimeOnly(8, 0),
                    EndTime = new TimeOnly(18, 0)
                });

            await dbContext.UserOperatingHours.AddRangeAsync(defaults);
        }

        var hasTasks = await dbContext.ScheduledTasks.AnyAsync(x => x.UserId == user.Id);
        if (!hasTasks)
        {
            var offset = DateTimeOffset.Now.Offset;
            var today = new DateTimeOffset(DateTime.Today, offset);
            await dbContext.ScheduledTasks.AddRangeAsync(
            [
                new ScheduledTask
                {
                    UserId = user.Id,
                    Title = "Planificacion semanal",
                    Description = "Revisar objetivos y capacidad del equipo.",
                    Priority = TaskPriority.High,
                    Status = TaskItemStatus.InProgress,
                    StartAt = today.AddHours(9),
                    EndAt = today.AddHours(10),
                    RecurrenceType = RecurrenceType.Weekly,
                    RecurrenceInterval = 1,
                    RecurrenceEndAt = today.AddMonths(2)
                },
                new ScheduledTask
                {
                    UserId = user.Id,
                    Title = "Espacio de foco",
                    Description = "Bloque sin interrupciones para tareas de alta prioridad.",
                    Priority = TaskPriority.Critical,
                    Status = TaskItemStatus.Pending,
                    StartAt = today.AddHours(11),
                    EndAt = today.AddHours(12),
                    RecurrenceType = RecurrenceType.Daily,
                    RecurrenceInterval = 1,
                    RecurrenceEndAt = today.AddDays(21)
                },
                new ScheduledTask
                {
                    UserId = user.Id,
                    Title = "Cierre mensual",
                    Description = "Revision de avances del mes.",
                    Priority = TaskPriority.Medium,
                    Status = TaskItemStatus.Pending,
                    StartAt = new DateTimeOffset(today.Year, today.Month, Math.Min(28, today.Day), 16, 0, 0, DateTimeOffset.Now.Offset),
                    EndAt = new DateTimeOffset(today.Year, today.Month, Math.Min(28, today.Day), 17, 0, 0, DateTimeOffset.Now.Offset),
                    RecurrenceType = RecurrenceType.Monthly,
                    RecurrenceInterval = 1,
                    RecurrenceEndAt = today.AddMonths(6)
                }
            ]);
        }

        await dbContext.SaveChangesAsync();
    }
}
