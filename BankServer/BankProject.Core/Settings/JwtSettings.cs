namespace BankProject.Core.Settings;

public class JwtSettings
{
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;

    // הערך עצמו נטען מ-User Secrets, לא מ-appsettings.json - ראי SETUP.md
    public string SecretKey { get; set; } = string.Empty;

    public int ExpiryMinutes { get; set; } = 60;
}
