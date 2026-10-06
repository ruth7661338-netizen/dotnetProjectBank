namespace BankProject.API.Middleware;

/// <summary>
/// יוצר (או משאיר, אם כבר קיים) מזהה ייחודי לכל בקשה, מחזיר אותו ב-response header,
/// ודוחף אותו ל-logging scope - כך שכל שורת לוג שנכתבת במהלך הבקשה (גם בתוך ה-Service)
/// כוללת אותו, ואפשר לשחזר בקשה שלמה מתוך קובץ לוג משותף.
/// </summary>
public class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-ID";

    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue(HeaderName, out var existing) && !string.IsNullOrWhiteSpace(existing)
            ? existing.ToString()
            : Guid.NewGuid().ToString();

        context.Items["CorrelationId"] = correlationId;
        context.Response.Headers[HeaderName] = correlationId;

        using (_logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId }))
        {
            _logger.LogInformation("בקשה נכנסת: {Method} {Path}", context.Request.Method, context.Request.Path);
            await _next(context);
        }
    }
}
