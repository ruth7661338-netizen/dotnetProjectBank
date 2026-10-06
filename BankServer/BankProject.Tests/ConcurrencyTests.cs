using BankProject.Core.Enums;
using BankProject.Core.Models;
using BankProject.Data;
using BankProject.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BankProject.Tests;

/// <summary>
/// זו הבדיקה שהמסמך דורש במפורש עבור חלק ג (הלב): שני DbContext נפרדים קוראים את אותה
/// ישות, שניהם משנים אותה, הראשון שומר בהצלחה והשני מקבל DbUpdateConcurrencyException.
///
/// משתמשים כאן ב-EF Core InMemory provider (עם אותו שם database) כדי לדמות את שני
/// המשתמשים בלי תלות בהתקנת SQL Server אמיתי בזמן הרצת הבדיקות. InMemory provider
/// תומך ב-concurrency tokens: השדה Version המסומן ב-[ConcurrencyCheck] מוחלף בערך חדש
/// בכל שמירה (ב-BankDbContext.SaveChangesAsync), ו-EF Core משווה את הערך שנקרא במקור
/// מול הערך העדכני בזמן ה-SaveChanges של הקורא השני.
/// </summary>
public class ConcurrencyTests
{
    private static BankDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<BankDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new BankDbContext(options);
    }

    [Fact]
    public async Task TwoContextsCompetingOnSameAccount_FirstSaveSucceeds_SecondThrowsConcurrencyException()
    {
        var dbName = Guid.NewGuid().ToString();

        // הכנה: יוצרים לקוח וחשבון בסיסי במסד "משותף" (אותו dbName). שימי לב: ה-seed data
        // (HasData) תמיד קיים גם ב-InMemory - משתמשים כאן ב-Id-ים גבוהים ומפורשים כדי
        // שלא יתנגשו עם הלקוחות/חשבונות/משתמשים שכבר קיימים מה-seed.
        const int testAccountId = 9001;
        using (var setupContext = CreateContext(dbName))
        {
            var customer = new Customer { Id = 9001, FullName = "בדיקה", Email = "test@example.com" };
            setupContext.Customers.Add(customer);
            var account = new Account
            {
                Id = testAccountId,
                AccountNumber = "TEST-0001",
                Type = AccountType.Checking,
                Balance = 1000m,
                Customer = customer
            };
            setupContext.Accounts.Add(account);
            await setupContext.SaveChangesAsync();
        }
        var accountId = testAccountId;

        // שני "משתמשים" - שני DbContext נפרדים - קוראים את אותו חשבון *לפני* ששניהם שומרים
        using var context1 = CreateContext(dbName);
        using var context2 = CreateContext(dbName);

        var account1 = await context1.Accounts.FindAsync(accountId);
        var account2 = await context2.Accounts.FindAsync(accountId);

        Assert.NotNull(account1);
        Assert.NotNull(account2);

        // שניהם "מושכים" כסף מהיתרה שקראו
        account1!.Balance -= 100m;
        account2!.Balance -= 200m;

        // הראשון שומר בהצלחה
        await context1.SaveChangesAsync();

        // השני מנסה לשמור על בסיס Version שכבר לא תואם למה שבמסד הנתונים -> חריגה
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => context2.SaveChangesAsync());

        // ולידציה נוספת: היתרה הסופית משקפת רק את העדכון של המשתמש הראשון, לא של שניהם
        using var verifyContext = CreateContext(dbName);
        var finalAccount = await verifyContext.Accounts.FindAsync(accountId);
        Assert.Equal(900m, finalAccount!.Balance);
    }
}
