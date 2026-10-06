using BankProject.Core.Enums;

namespace BankProject.Core.Dtos.Responses;

public class AccountSummaryDto
{
    public int Id { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public AccountType Type { get; set; }
    public decimal Balance { get; set; }
}
