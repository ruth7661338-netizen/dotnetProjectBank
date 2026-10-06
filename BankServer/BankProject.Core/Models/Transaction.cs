using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BankProject.Core.Enums;

namespace BankProject.Core.Models;

public class Transaction
{
    public int Id { get; set; }

    public int AccountId { get; set; }
    public Account? Account { get; set; }

    [Display(Name = "סוג פעולה")]
    public TransactionType Type { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "סכום")]
    public decimal Amount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "יתרה לאחר פעולה")]
    public decimal BalanceAfter { get; set; }

    [Display(Name = "תאריך")]
    public DateTime Date { get; set; } = DateTime.UtcNow;

    [Display(Name = "הערה")]
    public string? Note { get; set; }
}
