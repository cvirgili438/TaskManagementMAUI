using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskCalendar.Infrastructure.Configuration;
using TaskCalendar.Infrastructure.Data;
using TaskCalendar.Infrastructure.Identity;
using TaskCalendar.Infrastructure.Security;

namespace TaskCalendar.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTaskCalendarInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(options =>
        {
            var section = configuration.GetSection(JwtOptions.SectionName);
            options.Key = section["Key"] ?? options.Key;
            options.Issuer = section["Issuer"] ?? options.Issuer;
            options.Audience = section["Audience"] ?? options.Audience;
            if (int.TryParse(section["ExpirationMinutes"], out var expirationMinutes))
            {
                options.ExpirationMinutes = expirationMinutes;
            }
        });

        services.Configure<DatabaseOptions>(options =>
        {
            options.Provider = configuration.GetSection(DatabaseOptions.SectionName)["Provider"] ?? options.Provider;
        });

        var databaseProvider = configuration.GetSection(DatabaseOptions.SectionName)["Provider"] ?? "Sqlite";
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        services.AddDbContext<TaskCalendarDbContext>(options =>
        {
            if (string.Equals(databaseProvider, "SqlServer", StringComparison.OrdinalIgnoreCase))
            {
                options.UseSqlServer(connectionString);
            }
            else
            {
                options.UseSqlite(connectionString);
            }
        });

        services
            .AddIdentityCore<AppUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<TaskCalendarDbContext>();

        services.AddScoped<ITokenService, TokenService>();

        return services;
    }
}
