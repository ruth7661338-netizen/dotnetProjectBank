using System.ComponentModel.DataAnnotations;
using BankProject.Core.Enums;

namespace BankProject.Core.Dtos.Requests;

public class CreateAccountRequestDto
{
    [Required]
    public int CustomerId { get; set; }

    [Required]
    public AccountType Type { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "יתרת פתיחה לא יכולה להיות שלילית")]
    public decimal OpeningBalance { get; set; }
}
