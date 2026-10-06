using System.Text;
using BankProject.API.Middleware;
using BankProject.Core.Interfaces;
using BankProject.Core.Settings;
using BankProject.Data;
using BankProject.Data.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NLog;
using NLog.Web;

var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // --- NLog כ-logging provider יחיד (מחליף את ה-console logger הבסיסי) ---
    builder.Logging.ClearProviders();
    builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
    builder.Host.UseNLog();

    // --- EF Core: SQL Server ---
    // ה-connection string נטען מ-User Secrets בפיתוח (לא מ-appsettings.json, כדי שלא ייכנס ל-repo).
    // הגדרה חד-פעמית מקומית:
    //   dotnet user-secrets init --project BankProject.API
    //   dotnet user-secrets set "ConnectionStrings:Default" "Server=(localdb)\MSSQLLocalDB;Database=BankProjectDb;Trusted_Connection=True;TrustServerCertificate=True" --project BankProject.API
    var connectionString = builder.Configuration.GetConnectionString("Default")
        ?? throw new InvalidOperationException(
            "Connection string 'Default' לא נמצא. הגדירי אותו ב-User Secrets - ראי הוראות ב-README/SETUP.md.");

    builder.Services.AddDbContext<BankDbContext>(options =>
        options.UseSqlServer(connectionString));

    // --- Repositories + Unit of Work ---
    builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
    builder.Services.AddScoped<IAccountRepository, AccountRepository>();
    builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

    // --- Business logic ---
    builder.Services.AddScoped<IBankService, BankProject.Service.BankService>();

    // --- Auth: hashing, JWT generation, ורישום/התחברות ---
    builder.Services.AddScoped<IPasswordHasher, BankProject.Service.Auth.PasswordHasher>();
    builder.Services.AddScoped<IJwtTokenGenerator, BankProject.Service.Auth.JwtTokenGenerator>();
    builder.Services.AddScoped<IAuthService, BankProject.Service.Auth.AuthService>();

    // --- AutoMapper: ממיר entities ל-response DTOs לפני שהם חוצים את גבול ה-API ---
    builder.Services.AddAutoMapper(typeof(BankProject.API.MappingProfiles.BankMappingProfile));

    // --- JWT ---
    // ה-secret נטען מ-User Secrets (לא מ-appsettings.json). הגדרה חד-פעמית מקומית:
    //   dotnet user-secrets set "Jwt:SecretKey" "<מפתח אקראי ארוך>" --project BankProject.API
    builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
    var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
        ?? throw new InvalidOperationException("הגדרות Jwt לא נמצאו ב-configuration.");
    if (string.IsNullOrWhiteSpace(jwtSettings.SecretKey))
        throw new InvalidOperationException(
            "Jwt:SecretKey לא הוגדר. הגדירי אותו ב-User Secrets - ראי הוראות ב-SETUP.md.");

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(30)
            };
        });
    builder.Services.AddAuthorization();

    // --- CORS: מאפשר ללקוח React (שרץ על פורט נפרד ב-dev) לקרוא ל-API ---
    const string ClientCorsPolicy = "ClientCorsPolicy";
    builder.Services.AddCors(options =>
    {
        options.AddPolicy(ClientCorsPolicy, policy =>
        {
            policy.WithOrigins(
                    builder.Configuration.GetSection("ClientOrigins").Get<string[]>()
                    ?? new[] { "http://localhost:5173", "http://localhost:3000" })
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
    });

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "Bank Project API",
            Version = "v1",
            Description = "API לניהול לקוחות, חשבונות ותנועות בנקאיות"
        });

        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath);
        }

        // מוסיף כפתור "Authorize" ב-Swagger UI כדי שאפשר יהיה לבדוק endpoints מוגנים
        options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Description = "הדביקי כאן רק את ה-token (בלי המילה Bearer) שהתקבל מ-/api/auth/login"
        });
        options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    var app = builder.Build();

    // --- מריץ אוטומטית migrations שממתינות + Seed data, כדי שהאפליקציה תעלה מקומית בלי התערבות ידנית ---
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<BankDbContext>();
        db.Database.Migrate();
    }

    // --- סדר ה-pipeline חשוב: טיפול בשגיאות קודם כל (תופס הכל שמתחתיו),
    //     CorrelationId אחריו, ו-Authentication לפני Authorization ---
    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseMiddleware<CorrelationIdMiddleware>();
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    else
    {
        app.UseHttpsRedirection();
    }
    app.UseCors(ClientCorsPolicy);
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    logger.Error(ex, "השרת נעצר בגלל שגיאה בהפעלה");
    throw;
}
finally
{
    LogManager.Shutdown();
}
