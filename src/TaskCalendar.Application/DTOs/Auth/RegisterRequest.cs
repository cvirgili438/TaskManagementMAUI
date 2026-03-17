namespace TaskCalendar.Application.DTOs.Auth;

public sealed class RegisterRequest
{
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string PreferredCulture { get; set; } = "es-AR";
}
