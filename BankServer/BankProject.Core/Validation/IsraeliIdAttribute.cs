using System.ComponentModel.DataAnnotations;

namespace BankProject.Core.Validation;

/// <summary>
/// בודק שהערך הוא מספר תעודת זהות ישראלית תקין: 9 ספרות בדיוק, כולל ספרת ביקורת.
/// (ערך ריק לא נבדק כאן - החובה נאכפת ע"י [Required].)
/// </summary>
public class IsraeliIdAttribute : ValidationAttribute
{
    public IsraeliIdAttribute() : base("מספר תעודת זהות לא תקין") { }

    public override bool IsValid(object? value)
    {
        if (value is not string id || id.Length == 0)
            return true;

        return IsValidId(id);
    }

    public static bool IsValidId(string id)
    {
        if (id.Length != 9 || !id.All(char.IsAsciiDigit))
            return false;

        var sum = 0;
        for (var i = 0; i < 9; i++)
        {
            var product = (id[i] - '0') * (i % 2 == 0 ? 1 : 2);
            sum += product > 9 ? product - 9 : product;
        }
        return sum % 10 == 0;
    }
}
