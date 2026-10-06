using System.ComponentModel.DataAnnotations;
using BankProject.Core.Enums;

namespace BankProject.Core.Models;

public class User
{
    public int Id { get; set; }

    [Required]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    // מקשר משתמש בתפקיד Customer ללקוח הבנק שלו. ריק עבור משתמשי Clerk.
    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }
}
