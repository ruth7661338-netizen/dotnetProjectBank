using System.ComponentModel.DataAnnotations;

namespace BankProject.Core.Enums;

public enum TransactionType
{
    [Display(Name = "הפקדה")]
    Deposit,

    [Display(Name = "משיכה")]
    Withdraw,

    [Display(Name = "העברה יוצאת")]
    TransferOut,

    [Display(Name = "העברה נכנסת")]
    TransferIn
}
