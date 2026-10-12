using System.ComponentModel.DataAnnotations;
using BankProject.Core.Enums;

namespace BankProject.Core.Models;

public class User
{
    public int Id { get; set; }

    [Required]
    public string FullName { get; set; } = string.Empty;

    // מספר תעודת זהות (9 ספרות). חובה בכל משתמש חדש - נאכף ב-RegisterRequestDto.
    // ב-DB העמודה nullable רק כדי שמשתמשים שנוצרו לפני השדה הזה לא ישברו את ה-migration.
    public string? IdNumber { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    // מקשר משתמש בתפקיד Customer ללקוח הבנק שלו. ריק עבור משתמשי Clerk.
    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }
}
