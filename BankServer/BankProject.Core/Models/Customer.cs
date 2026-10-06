using System.ComponentModel.DataAnnotations;

namespace BankProject.Core.Models;

public class Customer
{
    public int Id { get; set; }

    [Required(ErrorMessage = "שם מלא הוא שדה חובה")]
    [Display(Name = "שם מלא")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "אימייל הוא שדה חובה")]
    [EmailAddress(ErrorMessage = "כתובת אימייל לא תקינה")]
    [Display(Name = "אימייל")]
    public string Email { get; set; } = string.Empty;

    public ICollection<Account> Accounts { get; set; } = new List<Account>();
}
