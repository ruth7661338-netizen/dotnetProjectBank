using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BankProject.Core.Enums;

namespace BankProject.Core.Models;

public class Account
{
    public int Id { get; set; }

    [Display(Name = "מספר חשבון")]
    public string AccountNumber { get; set; } = string.Empty;

    [Display(Name = "סוג חשבון")]
    public AccountType Type { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "יתרה")]
    public decimal Balance { get; set; }

    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    // קשר many-to-many: חשבון יכול לשאת כמה תגיות (VIP, עסקי וכו'), ותגית משויכת לכמה חשבונות.
    public ICollection<AccountTag> Tags { get; set; } = new List<AccountTag>();

    // Concurrency token: אנחנו מנהלות אותו בעצמנו (מוחלף בכל שמירה, ב-BankDbContext.SaveChangesAsync).
    // נבחרה הגישה הזו (ולא [Timestamp] rowversion הפרטי ל-SQL Server) כי היא עובדת זהה
    // על כל provider - כולל EF Core InMemory שמשמש את בדיקות ה-concurrency - בלי תלות
    // בהתנהגות auto-generation ספציפית ל-DB.
    [ConcurrencyCheck]
    public Guid Version { get; set; } = Guid.NewGuid();
}
