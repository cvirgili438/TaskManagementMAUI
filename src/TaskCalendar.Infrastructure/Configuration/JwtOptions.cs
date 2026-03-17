namespace TaskCalendar.Infrastructure.Configuration;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Key { get; set; } = "ReplaceThisWithAStrongKeyForProduction123!";
    public string Issuer { get; set; } = "TaskCalendar.Api";
    public string Audience { get; set; } = "TaskCalendar.App";
    public int ExpirationMinutes { get; set; } = 480;
}
