namespace TaskCalendar.Application.DTOs.Auth;

public sealed class UserProfileResponse
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PreferredCulture { get; set; } = "es-AR";
}
