using System.Net;

namespace BankProject.API.Middleware;

/// <summary>
/// עוטף את כל שרשרת ה-pipeline. כל חריגה לא מטופלת (כזו שלא נתפסה כבר ב-Controller/Service
/// כ-InvalidOperationException/ConcurrencyConflictException עסקיים) מגיעה לכאן, נרשמת ברמת
/// Error, ומוחזרת ללקוח כ-JSON אחיד עם 500 - בלי לחשוף פרטי מימוש (stack trace וכו') ללקוח.
/// חייב להיות ה-middleware הראשון ב-pipeline כדי שיתפוס חריגות מכל מה שאחריו.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var correlationId = context.Items.TryGetValue("CorrelationId", out var cid) ? cid?.ToString() : null;

            _logger.LogError(ex,
                "חריגה לא מטופלת בעיבוד הבקשה {Method} {Path} (CorrelationId={CorrelationId})",
                context.Request.Method, context.Request.Path, correlationId);

            if (context.Response.HasStarted)
            {
                // התשובה כבר החלה להישלח - אי אפשר לשנות status code/body, רק לתעד ולזרוק הלאה
                throw;
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            await context.Response.WriteAsJsonAsync(new
            {
                message = "אירעה שגיאה לא צפויה בשרת. נסי שוב מאוחר יותר.",
                correlationId
            });
        }
    }
}
