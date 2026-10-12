using System.ComponentModel.DataAnnotations;

namespace BankProject.Core.Dtos.Requests;

public class TransferRequestDto
{
    // מספר החשבון המלא (למשל "1000-0002") - זה מה שהמשתמש רואה, לא ה-Id הפנימי
    [Required(ErrorMessage = "חובה להזין מספר חשבון יעד")]
    public string ToAccountNumber { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue, ErrorMessage = "הסכום חייב להיות גדול מאפס")]
    public decimal Amount { get; set; }

    public string? Note { get; set; }
}
