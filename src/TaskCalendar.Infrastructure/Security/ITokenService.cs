using TaskCalendar.Application.DTOs.Auth;
using TaskCalendar.Infrastructure.Identity;

namespace TaskCalendar.Infrastructure.Security;

public interface ITokenService
{
    AuthResponse CreateToken(AppUser user, IEnumerable<string> roles);
}
