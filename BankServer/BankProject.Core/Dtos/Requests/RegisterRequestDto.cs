using System.ComponentModel.DataAnnotations;

namespace BankProject.Core.Dtos.Requests;

public class RegisterRequestDto
{
    [Required]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8, ErrorMessage = "הסיסמה חייבת לכלול לפחות 8 תווים")]
    public string Password { get; set; } = string.Empty;
}
