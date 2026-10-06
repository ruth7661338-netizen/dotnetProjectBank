using System.Security.Claims;
using BankProject.Core.Enums;

namespace BankProject.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static UserRole? GetRole(this ClaimsPrincipal principal)
    {
        var roleClaim = principal.FindFirstValue(ClaimTypes.Role);
        return Enum.TryParse<UserRole>(roleClaim, out var role) ? role : null;
    }

    public static int? GetCustomerId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue("CustomerId");
        return int.TryParse(value, out var customerId) ? customerId : null;
    }

    /// <summary>true אם המשתמש הנוכחי הוא Customer שמנסה לגעת בלקוח/חשבון שאינו שלו.</summary>
    public static bool IsForbiddenCustomerAccess(this ClaimsPrincipal principal, int targetCustomerId) =>
        principal.GetRole() == UserRole.Customer && principal.GetCustomerId() != targetCustomerId;
}
