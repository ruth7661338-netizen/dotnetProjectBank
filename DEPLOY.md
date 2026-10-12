# העלאה ל-Render

הפרויקט מוגדר להעלאה כשני שירותים, שמוגדרים יחד בקובץ `render.yaml` (Blueprint):

| שירות | סוג | מקור |
|---|---|---|
| `bank-api` | Web Service (Docker) | `BankServer/Dockerfile` |
| `bank-client` | Static Site | `BankClient` (`npm ci && npm run build` → `dist`) |

מסד הנתונים **לא** מאוחסן ב-Render (Render מציע רק PostgreSQL, והפרויקט עובד עם SQL Server). צריך SQL Server חיצוני.

---

## 1. מסד נתונים: SQL Server חיצוני

אפשר להשתמש בכל SQL Server שנגיש מהאינטרנט, למשל:
- **Azure SQL Database** – יש מסלול חינמי (serverless, עם השהיה אוטומטית כשאין שימוש).
- **MonsterASP.NET / somee.com** – אחסון SQL Server חינמי.

צרי מסד ריק ושמרי את ה-connection string. דוגמה ל-Azure SQL:

```
Server=tcp:<server>.database.windows.net,1433;Database=<db>;User ID=<user>;Password=<password>;Encrypt=True;TrustServerCertificate=False;Connection Timeout=60;
```

ב-Azure צריך גם לאפשר גישה מהאינטרנט: **Networking → Allow Azure services and resources to access this server**, או להוסיף כלל firewall ל-IP-ים היוצאים של Render (מופיעים בעמוד השירות ב-Render, תחת *Outbound IPs*).

> אין צורך להריץ migrations ידנית: השרת מריץ `Database.Migrate()` בעלייה, ויוצר את כל הטבלאות + נתוני הדמו במסד הריק.

## 2. העלאת הקוד ל-GitHub

Render מושך את הקוד מ-GitHub. ודאי שהענף שרוצים להעלות (למשל `main`) נדחף ל-`origin`.

## 3. יצירת השירותים ב-Render

1. ב-Render: **New → Blueprint**, ובחרי את ה-repository.
2. Render יקרא את `render.yaml` ויבקש ערכים למשתנים המסומנים `sync: false`:

| משתנה | שירות | ערך |
|---|---|---|
| `ConnectionStrings__Default` | `bank-api` | ה-connection string משלב 1 |
| `ClientOrigins__0` | `bank-api` | כתובת הלקוח, למשל `https://bank-client.onrender.com` |
| `VITE_API_BASE_URL` | `bank-client` | כתובת ה-API, למשל `https://bank-api.onrender.com` |

`Jwt__SecretKey` נוצר אוטומטית ע"י Render – אין צורך להזין.

> הכתובות הסופיות (`*.onrender.com`) ידועות רק אחרי יצירת השירותים, ולפעמים Render מוסיף להן סיומת. אם הכתובת שונה ממה שהזנת:
> עדכני את `ClientOrigins__0` ב-`bank-api` ואת `VITE_API_BASE_URL` ב-`bank-client`, ואז **Manual Deploy** ללקוח
> (הכתובת נצרבת בקוד הלקוח בזמן ה-build, אז שינוי שלה מחייב build מחדש).
> שימי לב: בלי `/` בסוף הכתובות.

## 4. בדיקה

| מה | איך |
|---|---|
| השרת חי | `https://<api>/health` מחזיר `{"status":"ok"}` |
| Swagger | `https://<api>/swagger` |
| התחברות | בלקוח: `clerk@bank.local` / `Demo1234!` |
| לוגים | בעמוד השירות ב-Render → **Logs** |

## הערות

- **המסלול החינמי של Render** מרדים את ה-API אחרי 15 דקות בלי תעבורה. הבקשה הראשונה אחרי זה לוקחת כדקה.
- **Azure SQL serverless** מושהה כשאין שימוש, והחיבור הראשון אחרי השהיה יכול להיכשל. השרת מוגדר עם `EnableRetryOnFailure`, כך שהוא מנסה שוב אוטומטית.
- **קבצי הלוג של NLog** בתוך ה-container נמחקים בכל deploy. הלוגים שנשארים זמינים הם אלה שב-Render → Logs.
- **סודות** (`ConnectionStrings__Default`, `Jwt__SecretKey`) נשמרים רק ב-Render, לא ב-git.
- **בדיקה מקומית של ה-image** (אם Docker מותקן): מתוך `BankServer`:
  ```
  docker build -t bank-api .
  docker run -p 8080:8080 -e ConnectionStrings__Default="..." -e Jwt__SecretKey="..." bank-api
  ```
