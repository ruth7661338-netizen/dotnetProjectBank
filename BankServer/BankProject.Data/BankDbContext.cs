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

        modelBuilder.Entity<Account>()
            .HasOne(a => a.Customer)
            .WithMany(c => c.Accounts)
            .HasForeignKey(a => a.CustomerId);

        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.Account)
            .WithMany(a => a.Transactions)
            .HasForeignKey(t => t.AccountId);

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
            new User { Id = 1, FullName = "פקידת בנק", Email = "clerk@bank.local", PasswordHash = demoPasswordHash, Role = UserRole.Clerk, CustomerId = null },
            new User { Id = 2, FullName = "ישראל ישראלי", Email = "customer@bank.local", PasswordHash = demoPasswordHash, Role = UserRole.Customer, CustomerId = 1 }
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
