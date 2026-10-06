using System.ComponentModel.DataAnnotations;

namespace BankProject.Core.Enums;

public enum AccountType
{
    [Display(Name = "עובר ושב")]
    Checking,

    [Display(Name = "חיסכון")]
    Savings
}
