using BankProject.Core.Enums;

namespace BankProject.Core.Dtos.Responses;

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public int? CustomerId { get; set; }
}
