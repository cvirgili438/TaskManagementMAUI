using System.Security.Claims;

namespace TaskCalendar.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return value is not null && Guid.TryParse(value, out var userId)
            ? userId
            : throw new InvalidOperationException("Authenticated user identifier is missing.");
    }
}
