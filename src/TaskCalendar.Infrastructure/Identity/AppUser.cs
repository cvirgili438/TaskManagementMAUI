using Microsoft.AspNetCore.Identity;

namespace TaskCalendar.Infrastructure.Identity;

public sealed class AppUser : IdentityUser<Guid>
{
    public string DisplayName { get; set; } = string.Empty;
    public string PreferredCulture { get; set; } = "es-AR";
}
