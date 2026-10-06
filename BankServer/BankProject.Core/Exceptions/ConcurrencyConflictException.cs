namespace BankProject.Core.Exceptions;

/// <summary>
/// נזרקת ע"י שכבת ה-Service כשמתגלה שמשתמש אחר עדכן את אותו משאב בין הקריאה לשמירה
/// (optimistic concurrency conflict). ה-API ממפה אותה ל-409 Conflict.
/// </summary>
public class ConcurrencyConflictException : Exception
{
    public ConcurrencyConflictException(string message) : base(message)
    {
    }
}
