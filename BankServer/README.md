# מערכת ניהול בנק — Web API

פרויקט גמר בקורס .NET — Web API לניהול לקוחות, חשבונות ותנועות בנקאיות, עם ארכיטקטורה בשכבות, אימות JWT, ו-optimistic concurrency על יתרת החשבון.

## תיאור המערכת

המערכת מנהלת לקוחות וחשבונות בנק. כל לקוח יכול להחזיק כמה חשבונות (עובר-ושב/חיסכון), ולבצע עליהם הפקדה, משיכה והעברה בין חשבונות. לחשבונות אפשר גם לצרף תגיות (VIP/עסקי). יש שני תפקידי משתמש: **פקידת בנק** (Clerk) שרואה ויוצרת הכל, ו-**לקוחה** (Customer) שרואה ופועלת רק על החשבונות שלה.

## המשאב המוגבל והטיפול בתחרות עליו

המשאב המוגבל הוא **יתרת החשבון** (`Account.Balance`). תחרות עליו קורית כששני משתמשים מבצעים משיכה/הפקדה/העברה על אותו חשבון בו-זמנית — למשל שני בקשות משיכה שקוראות את אותה יתרה לפני ששתיהן נשמרות, מה שעלול להוביל לעדכון שגוי אם לא מטפלים בזה.

הטיפול מבוסס **optimistic concurrency**: לישות `Account` יש שדה `Version` (`Guid`, `[ConcurrencyCheck]`), שמוחלף אוטומטית בכל שמירה (ב-`BankDbContext.SaveChangesAsync`). אם מישהו אחר עדכן את החשבון בין הקריאה לשמירה, EF Core מזהה זאת (ה-`WHERE` שנוצר ל-`UPDATE` לא מוצא שורה תואמת), זורק `DbUpdateConcurrencyException`, ושכבת ה-Service הופכת אותה ל-`ConcurrencyConflictException` עסקי — שה-API ממפה ל-**409 Conflict** עם הודעה ברורה למשתמש, במקום לאפשר עדכון יתרה שגוי. הבדיקה הייעודית שמוכיחה את זה (שני `DbContext` נפרדים מתחרים על אותה שורה) נמצאת ב-`BankProject.Tests/ConcurrencyTests.cs`.

## ארכיטקטורה

```
BankProject.sln
  BankProject.Core      // Entities, Enums, DTOs, Interfaces - לא תלוי בשום פרויקט אחר
  BankProject.Data      // BankDbContext, Repositories, Migrations
  BankProject.Service   // לוגיקה עסקית (BankService), Auth (hashing, JWT)
  BankProject.API       // Controllers, Middleware, Program.cs
  BankProject.Tests     // xUnit + Moq
```

כיוון התלויות: `Core` לא מפנה לאף פרויקט אחר. `Data` ו-`Service` מפנים ל-`Core` בלבד. `API` מפנה לכולם.

## הרצה מקומית

**נדרש:** SQL Server (LocalDB מספיק - מגיע עם Visual Studio).

הוראות מפורטות (כולל כל הפקודות המדויקות) נמצאות ב-**[SETUP.md](./SETUP.md)**. בקצרה:

1. `dotnet user-secrets init/set` עבור connection string ו-JWT secret.
2. `dotnet ef migrations add InitialCreate` + `dotnet ef database update`.
3. הרצה רגילה (F5 / `dotnet run --project BankProject.API`) — migrations נוספות יורצו אוטומטית בכל עליה.

Swagger זמין ב-`/swagger` בסביבת פיתוח, כולל כפתור "Authorize" לבדיקת endpoints מוגנים.

## משתמשי דמו

| Role     | Email                | סיסמה       |
|----------|------------------------|-------------|
| Clerk    | clerk@bank.local       | Demo1234!   |
| Customer | customer@bank.local    | Demo1234!   |

## בדיקות

`BankProject.Tests` (xUnit + Moq) — 15 test cases: הבדיקה הייעודית לתחרות על משאב (שני `DbContext`), ובדיקות Service לכל פעולות היתרה (הפקדה/משיכה/העברה/יצירת חשבון), כולל מקרי הצלחה ודחייה.

## מה לא נכלל (בכוונה)

העלאה לענן, Docker, CI, caching, SignalR — לא נדרשים בפרויקט הזה.
