# הרצה מקומית — SQL Server + Migrations

הערה: לא היה לי SDK של dotnet בסביבת העבודה שלי, אז לא הרצתי את הפקודות האלה בפועל.
בצעי אותן פעם אחת אצלך (ב-Visual Studio Developer PowerShell, או בטרמינל רגיל משורש ה-solution שבו יושב `BankProject.sln`).

## 1. ודאי שכלי ה-EF מותקן

```
dotnet tool install --global dotnet-ef
```
(אם כבר מותקן, הפקודה תגיד לך את זה - זה בסדר.)

## 2. הגדירי את ה-connection string וה-JWT secret ב-User Secrets

```
dotnet user-secrets init --project BankProject.API
dotnet user-secrets set "ConnectionStrings:Default" "Server=(localdb)\MSSQLLocalDB;Database=BankProjectDb;Trusted_Connection=True;TrustServerCertificate=True" --project BankProject.API
dotnet user-secrets set "Jwt:SecretKey" "ltkLUq3YCFpf8k-lRZ34-IlZM-KfkXuOQyvFUB2n732k3ldMZv-pPzpNXhl2d-Ec" --project BankProject.API
```

ה-`Jwt:SecretKey` שלמעלה הוא ערך אקראי לדוגמה שנוצר בשבילך — אפשר להשתמש בו, או ליצור ערך אקראי אחר (32+ תווים) בעצמך. **חשוב: אף פעם לא לשים את הערך הזה ב-`appsettings.json`** — Issuer/Audience/ExpiryMinutes כן נמצאים שם כי הם לא סוד, רק ה-SecretKey חייב להישאר ב-User Secrets.

אם יש לך SQL Server Express מותקן במקום LocalDB, השתמשי בזה במקום עבור ה-connection string:
```
Server=localhost\SQLEXPRESS;Database=BankProjectDb;Trusted_Connection=True;TrustServerCertificate=True
```

## 3. צרי את ה-migration הראשונה

```
dotnet ef migrations add InitialCreate -p BankProject.Data -s BankProject.API
```

הדגלים חשובים: `-p` מציין איפה יושב ה-`DbContext` (`BankProject.Data`), ו-`-s` מציין את פרויקט ההרצה (`BankProject.API`). בלעדיהם הפקודה תיכשל כי אלה שני פרויקטים נפרדים.

זה ייצור תיקיית `Migrations/` בתוך `BankProject.Data` עם הקובץ `InitialCreate.cs` (וקובץ Designer נלווה). **אל תמחקי אותם** — הם חלק מהדרישה של "לפחות שתי migrations בהיסטוריה".

## 4. עדכני את בסיס הנתונים

```
dotnet ef database update -p BankProject.Data -s BankProject.API
```

זה ייצור בפועל את מסד הנתונים `BankProjectDb` על ה-LocalDB שלך, כולל ה-seed data (שני לקוחות, שלושה חשבונות, ושני משתמשי דמו להתחברות).

## משתמשי דמו (להתחברות דרך `/api/auth/login`)

| Role     | Email                | סיסמה       | הערה                                    |
|----------|-----------------------|-------------|-------------------------------------------|
| Clerk    | clerk@bank.local      | Demo1234!   | רואה/יוצרת הכל                            |
| Customer | customer@bank.local   | Demo1234!   | מקושרת ללקוח Id=1 (ישראל ישראלי, חשבונות 1+2) |

ב-Swagger UI, אחרי login, לחצי על כפתור "Authorize" למעלה והדביקי שם רק את ה-token עצמו (בלי המילה "Bearer").

## 5. הרצה רגילה

מכאן והלאה, בכל הרצה של `BankProject.API` (F5 בוויז'ואל סטודיו, או `dotnet run --project BankProject.API`), הקוד ב-`Program.cs` מריץ אוטומטית `db.Database.Migrate()` — כך שכל migration חדשה שתוסיפי בעתיד תיושם אוטומטית גם היא, בלי צורך בהתערבות ידנית.

## עדכון: שדה ה-concurrency token הוא `Version` (Guid), לא `RowVersion`

שימי לב: המודל `Account` כולל כרגע שדה `Version` מסוג `Guid` עם `[ConcurrencyCheck]` (במקום `[Timestamp] byte[] RowVersion` שהוזכר בגרסה מוקדמת יותר). הסיבה: הגישה הזו עובדת זהה על כל provider (כולל EF Core InMemory ששימש לבדיקות ה-concurrency), בלי תלות בהתנהגות auto-generation שספציפית ל-SQL Server. הערך מוחלף אוטומטית בכל שמירה בקוד (`BankDbContext.SaveChangesAsync`), כך שלא צריך לזכור לעדכן אותו ידנית באף `service`.

כל עוד עדיין לא יצרת אף migration (סעיף 3 למעלה), אין לך מה לתקן — פשוט תריצי `dotnet ef migrations add InitialCreate` כרגיל והשדה `Version` ייכנס אליה מההתחלה.
