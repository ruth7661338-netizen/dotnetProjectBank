using BankProject.Core.Enums;

namespace BankProject.Core.Dtos.Responses;

public class AccountResponseDto
{
    public int Id { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public AccountType Type { get; set; }
    public decimal Balance { get; set; }
    public int CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public List<string> Tags { get; set; } = new();
}
