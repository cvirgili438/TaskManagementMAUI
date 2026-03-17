using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskCalendar.Domain.Entities;
using TaskCalendar.Infrastructure.Identity;

namespace TaskCalendar.Infrastructure.Data;

public sealed class TaskCalendarDbContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
{
    public TaskCalendarDbContext(DbContextOptions<TaskCalendarDbContext> options)
        : base(options)
    {
    }

    public DbSet<ScheduledTask> ScheduledTasks => Set<ScheduledTask>();
    public DbSet<UserOperatingHour> UserOperatingHours => Set<UserOperatingHour>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ScheduledTask>(entity =>
        {
            entity.ToTable("ScheduledTasks");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(2000);
            entity.HasIndex(x => new { x.UserId, x.StartAt, x.EndAt });
        });

        builder.Entity<UserOperatingHour>(entity =>
        {
            entity.ToTable("UserOperatingHours");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.UserId, x.DayOfWeek }).IsUnique();
        });
    }
}
