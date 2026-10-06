using BankProject.Core.Enums;

namespace BankProject.Core.Dtos.Responses;

public class TransactionResponseDto
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public decimal BalanceAfter { get; set; }
    public DateTime Date { get; set; }
    public string? Note { get; set; }
}
