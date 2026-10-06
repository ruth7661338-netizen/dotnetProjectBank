using System.ComponentModel.DataAnnotations;

namespace BankProject.Core.Dtos.Requests;

public class TransferRequestDto
{
    [Required]
    public int ToAccountId { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "הסכום חייב להיות גדול מאפס")]
    public decimal Amount { get; set; }

    public string? Note { get; set; }
}
