# --- שלב 1: build + publish עם ה-SDK ---
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# קודם רק קבצי ה-csproj, כדי ש-restore יישמר ב-cache של Docker כל עוד התלויות לא השתנו
COPY BankProject.API/BankProject.API.csproj BankProject.API/
COPY BankProject.Core/BankProject.Core.csproj BankProject.Core/
COPY BankProject.Data/BankProject.Data.csproj BankProject.Data/
COPY BankProject.Service/BankProject.Service.csproj BankProject.Service/
RUN dotnet restore BankProject.API/BankProject.API.csproj

COPY BankProject.API/ BankProject.API/
COPY BankProject.Core/ BankProject.Core/
COPY BankProject.Data/ BankProject.Data/
COPY BankProject.Service/ BankProject.Service/
RUN dotnet publish BankProject.API/BankProject.API.csproj -c Release -o /app/publish --no-restore

# --- שלב 2: runtime בלבד (image קטן יותר, בלי SDK) ---
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# NLog כותב ל-/app/logs; התיקייה צריכה להיות כתיבה למשתמש הלא-root שה-image רץ איתו
RUN mkdir -p /app/logs && chown "$APP_UID" /app/logs
USER $APP_UID

ENV ASPNETCORE_ENVIRONMENT=Production
# Render מעביר את הפורט במשתנה PORT (ברירת מחדל 10000); מקומית נופלים ל-8080
EXPOSE 8080
ENTRYPOINT ["sh", "-c", "ASPNETCORE_HTTP_PORTS=${PORT:-8080} exec dotnet BankProject.API.dll"]
