using BankProject.Core.Enums;
using BankProject.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace BankProject.Data;

public class BankDbContext : DbContext
{
    public BankDbContext(DbContextOptions<BankDbContext> options) : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<User> Users => Set<User>();
    public DbSet<AccountTag> AccountTags => Set<AccountTag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Account>(e =>
        {
            e.Property(a => a.AccountNumber).HasMaxLength(20);
            e.HasIndex(a => a.AccountNumber).IsUnique();
        });

        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.Account)
            .WithMany(a => a.Transactions)
            .HasForeignKey(t => t.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Customer>(e =>
        {
            e.Property(c => c.FullName).HasMaxLength(100);
            e.Property(c => c.Email).HasMaxLength(256);
        });

        modelBuilder.Entity<User>(e =>
        {
            e.Property(u => u.FullName).HasMaxLength(100);
            e.Property(u => u.Email).HasMaxLength(256);
            e.Property(u => u.IdNumber).HasMaxLength(9).IsFixedLength();
            e.HasIndex(u => u.IdNumber).IsUnique();     // filtered index - משתמשים ישנים בלי ת"ז (NULL) לא מתנגשים
            e.HasIndex(u => u.CustomerId).IsUnique();   // SQL Server makes this a filtered index (NULLs allowed)
        });

        modelBuilder.Entity<AccountTag>().Property(t => t.Name).HasMaxLength(50);
        modelBuilder.Entity<Account>()
            .HasOne(a => a.Customer)
            .WithMany(c => c.Accounts)
            .HasForeignKey(a => a.CustomerId);

        modelBuilder.Entity<User>()
            .HasOne(u => u.Customer)
            .WithMany()
            .HasForeignKey(u => u.CustomerId)
            .IsRequired(false);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // קשר many-to-many: Account <-> AccountTag, דרך טבלת קישור מוגדרת ב-Fluent API
        modelBuilder.Entity<Account>()
            .HasMany(a => a.Tags)
            .WithMany(t => t.Accounts)
            .UsingEntity<Dictionary<string, object>>(
                "AccountAccountTag",
                j => j.HasOne<AccountTag>().WithMany().HasForeignKey("AccountTagId"),
                j => j.HasOne<Account>().WithMany().HasForeignKey("AccountId"),
                j =>
                {
                    j.ToTable("AccountAccountTags");
                    j.HasData(
                        new { AccountId = 1, AccountTagId = 1 }, // חשבון 1 -> VIP
                        new { AccountId = 1, AccountTagId = 2 }, // חשבון 1 -> עסקי
                        new { AccountId = 3, AccountTagId = 1 }  // חשבון 3 -> VIP
                    );
                });

        // נתוני דמו ראשוניים כדי שהאפליקציה תעבוד מיד לאחר ההרצה
        modelBuilder.Entity<Customer>().HasData(
            new Customer { Id = 1, FullName = "ישראל ישראלי", Email = "israel@example.com" },
            new Customer { Id = 2, FullName = "רחל כהן", Email = "rachel@example.com" }
        );

        modelBuilder.Entity<Account>().HasData(
            new Account { Id = 1, AccountNumber = "1000-0001", Type = AccountType.Checking, Balance = 5000m, CustomerId = 1, Version = new Guid("11111111-1111-1111-1111-111111111111") },
            new Account { Id = 2, AccountNumber = "1000-0002", Type = AccountType.Savings, Balance = 15000m, CustomerId = 1, Version = new Guid("22222222-2222-2222-2222-222222222222") },
            new Account { Id = 3, AccountNumber = "2000-0001", Type = AccountType.Checking, Balance = 3200m, CustomerId = 2, Version = new Guid("33333333-3333-3333-3333-333333333333") }
        );

        modelBuilder.Entity<AccountTag>().HasData(
            new AccountTag { Id = 1, Name = "VIP" },
            new AccountTag { Id = 2, Name = "עסקי" }
        );

        // משתמשי דמו לכל role - סיסמה לשניהם: Demo1234!
        // ה-hash חושב מראש (PBKDF2-HMACSHA256, 100000 איטרציות, salt קבוע) כדי ש-HasData יהיה דטרמיניסטי.
        const string demoPasswordHash = "100000.AAECAwQFBgcICQoLDA0ODw==.oWtrAeSs7kbOgbjzpTpGVXeHiOWTIZMNxApazrC9MAk=";

        modelBuilder.Entity<User>().HasData(
            new User { Id = 1, FullName = "פקידת בנק", IdNumber = "000000018", Email = "clerk@bank.local", PasswordHash = demoPasswordHash, Role = UserRole.Clerk, CustomerId = null },
            new User { Id = 2, FullName = "ישראל ישראלי", IdNumber = "000000026", Email = "customer@bank.local", PasswordHash = demoPasswordHash, Role = UserRole.Customer, CustomerId = 1 },
            // Id גבוה בכוונה: Ids נמוכים עלולים להיות תפוסים ע"י משתמשים שנרשמו ב-DB מקומי קיים
            new User { Id = 100, FullName = "רחל כהן", IdNumber = "000000034", Email = "rachel@bank.local", PasswordHash = demoPasswordHash, Role = UserRole.Customer, CustomerId = 2 }
        );

        // היסטוריית תנועות דמו. ה-BalanceAfter של התנועה האחרונה בכל חשבון שווה ל-Balance שלו למעלה,
        // ותאריכים קבועים (לא DateTime.UtcNow) כדי ש-HasData יהיה דטרמיניסטי ולא ייצר migration חדש בכל פעם.
        modelBuilder.Entity<Transaction>().HasData(
            // חשבון 1 (1000-0001): 8000 -> 3000 -> 5500 -> 5000
            new Transaction { Id = 1, AccountId = 1, Type = TransactionType.Deposit, Amount = 8000m, BalanceAfter = 8000m, Date = new DateTime(2026, 9, 1, 9, 0, 0, DateTimeKind.Utc), Note = "יתרת פתיחה" },
            new Transaction { Id = 2, AccountId = 1, Type = TransactionType.TransferOut, Amount = 5000m, BalanceAfter = 3000m, Date = new DateTime(2026, 9, 5, 10, 30, 0, DateTimeKind.Utc), Note = "העברה לחשבון 1000-0002" },
            new Transaction { Id = 3, AccountId = 1, Type = TransactionType.Deposit, Amount = 2500m, BalanceAfter = 5500m, Date = new DateTime(2026, 9, 10, 8, 15, 0, DateTimeKind.Utc), Note = "משכורת" },
            new Transaction { Id = 4, AccountId = 1, Type = TransactionType.Withdraw, Amount = 500m, BalanceAfter = 5000m, Date = new DateTime(2026, 9, 18, 17, 45, 0, DateTimeKind.Utc), Note = "משיכה מכספומט" },

            // חשבון 2 (1000-0002): 10000 -> 15000 (צד הקבלה של ההעברה מחשבון 1)
            new Transaction { Id = 5, AccountId = 2, Type = TransactionType.Deposit, Amount = 10000m, BalanceAfter = 10000m, Date = new DateTime(2026, 9, 1, 9, 5, 0, DateTimeKind.Utc), Note = "יתרת פתיחה" },
            new Transaction { Id = 6, AccountId = 2, Type = TransactionType.TransferIn, Amount = 5000m, BalanceAfter = 15000m, Date = new DateTime(2026, 9, 5, 10, 30, 0, DateTimeKind.Utc), Note = "העברה מחשבון 1000-0001" },

            // חשבון 3 (2000-0001): 4000 -> 3200
            new Transaction { Id = 7, AccountId = 3, Type = TransactionType.Deposit, Amount = 4000m, BalanceAfter = 4000m, Date = new DateTime(2026, 9, 2, 11, 0, 0, DateTimeKind.Utc), Note = "יתרת פתיחה" },
            new Transaction { Id = 8, AccountId = 3, Type = TransactionType.Withdraw, Amount = 800m, BalanceAfter = 3200m, Date = new DateTime(2026, 9, 20, 14, 20, 0, DateTimeKind.Utc), Note = "תשלום חשבון חשמל" }
        );
    }

    // ה-Version מוחלף בעצמנו בכל שמירה, כדי שאף אחד לא יצטרך לזכור לעשות את זה בכל service בנפרד.
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        RegenerateConcurrencyTokens();
        return await base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        RegenerateConcurrencyTokens();
        return base.SaveChanges();
    }

    private void RegenerateConcurrencyTokens()
    {
        foreach (var entry in ChangeTracker.Entries<Account>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.Version = Guid.NewGuid();
            }
        }
    }
}
