using System.ComponentModel.DataAnnotations;
using BankProject.Core.Enums;

namespace BankProject.Core.Dtos.Requests;

public class AccountTransactionRequestDto
{
    [Required]
    public TransactionType Type { get; set; } // Deposit או Withdraw בלבד - נבדק ב-Controller

    [Range(0.01, double.MaxValue, ErrorMessage = "הסכום חייב להיות גדול מאפס")]
    public decimal Amount { get; set; }

    public string? Note { get; set; }
}
