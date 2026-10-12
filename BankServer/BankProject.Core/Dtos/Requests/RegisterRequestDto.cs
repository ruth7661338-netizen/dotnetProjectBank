using System.ComponentModel.DataAnnotations;
using BankProject.Core.Validation;

namespace BankProject.Core.Dtos.Requests;

public class RegisterRequestDto
{
    [Required]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "חובה להזין מספר תעודת זהות")]
    [IsraeliId]
    public string IdNumber { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8, ErrorMessage = "הסיסמה חייבת לכלול לפחות 8 תווים")]
    public string Password { get; set; } = string.Empty;
}
