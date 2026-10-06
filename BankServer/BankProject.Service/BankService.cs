using BankProject.Core.Enums;
using BankProject.Core.Exceptions;
using BankProject.Core.Interfaces;
using BankProject.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BankProject.Service;

public class BankService : IBankService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BankService> _logger;

    public BankService(IUnitOfWork unitOfWork, ILogger<BankService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public Task<(List<Account> Items, int TotalCount)> GetAllAccountsAsync(
        int pageNumber, int pageSize, AccountType? typeFilter = null, CancellationToken ct = default) =>
        _unitOfWork.Accounts.GetPagedAsync(pageNumber, pageSize, typeFilter, ct);

    public Task<Account?> GetAccountAsync(int id, CancellationToken ct = default) =>
        _unitOfWork.Accounts.GetByIdWithCustomerAsync(id, ct);

    public Task<List<Customer>> GetAllCustomersAsync(CancellationToken ct = default) =>
        _unitOfWork.Customers.GetAllAsync(ct);

    public Task<Customer?> GetCustomerAsync(int id, CancellationToken ct = default) =>
        _unitOfWork.Customers.GetWithAccountsAsync(id, ct);

    public async Task<Account> CreateAccountAsync(int customerId, AccountType type, decimal openingBalance, CancellationToken ct = default)
    {
        if (openingBalance < 0)
            throw new InvalidOperationException("יתרת פתיחה לא יכולה להיות שלילית");

        var customer = await _unitOfWork.Customers.GetByIdAsync(customerId, ct)
            ?? throw new InvalidOperationException("לקוח לא נמצא");

        var account = new Account
        {
            AccountNumber = GenerateAccountNumber(),
            Type = type,
            Balance = openingBalance,
            CustomerId = customer.Id
        };

        _unitOfWork.Accounts.Add(account);
        await _unitOfWork.SaveChangesAsync(ct);

        if (openingBalance > 0)
        {
            _unitOfWork.Transactions.Add(new Transaction
            {
                AccountId = account.Id,
                Type = TransactionType.Deposit,
                Amount = openingBalance,
                BalanceAfter = account.Balance,
                Note = "יתרת פתיחה"
            });
            await _unitOfWork.SaveChangesAsync(ct);
        }

        return account;
    }

    public async Task DepositAsync(int accountId, decimal amount, string? note = null, CancellationToken ct = default)
    {
        if (amount <= 0)
            throw new InvalidOperationException("סכום ההפקדה חייב להיות גדול מאפס");

        var account = await _unitOfWork.Accounts.GetByIdAsync(accountId, ct)
            ?? throw new InvalidOperationException("חשבון לא נמצא");

        account.Balance += amount;

        _unitOfWork.Transactions.Add(new Transaction
        {
            AccountId = account.Id,
            Type = TransactionType.Deposit,
            Amount = amount,
            BalanceAfter = account.Balance,
            Note = note
        });

        await SaveWithConcurrencyHandlingAsync(accountId, "הפקדה", ct);
    }

    public async Task WithdrawAsync(int accountId, decimal amount, string? note = null, CancellationToken ct = default)
    {
        if (amount <= 0)
            throw new InvalidOperationException("סכום המשיכה חייב להיות גדול מאפס");

        var account = await _unitOfWork.Accounts.GetByIdAsync(accountId, ct)
            ?? throw new InvalidOperationException("חשבון לא נמצא");

        if (account.Balance < amount)
            throw new InvalidOperationException("אין מספיק יתרה בחשבון לביצוע המשיכה");

        account.Balance -= amount;

        _unitOfWork.Transactions.Add(new Transaction
        {
            AccountId = account.Id,
            Type = TransactionType.Withdraw,
            Amount = amount,
            BalanceAfter = account.Balance,
            Note = note
        });

        // כאן קורית התחרות: שתי בקשות משיכה על אותו חשבון עלולות לקרוא את אותה יתרה
        // לפני ששתיהן שומרות. ה-Version (concurrency token) על Account תופס את זה ב-SaveChangesAsync.
        await SaveWithConcurrencyHandlingAsync(accountId, "משיכה", ct);
    }

    public async Task TransferAsync(int fromAccountId, int toAccountId, decimal amount, string? note = null, CancellationToken ct = default)
    {
        if (fromAccountId == toAccountId)
            throw new InvalidOperationException("לא ניתן להעביר כסף לאותו חשבון");

        if (amount <= 0)
            throw new InvalidOperationException("סכום ההעברה חייב להיות גדול מאפס");

        var fromAccount = await _unitOfWork.Accounts.GetByIdAsync(fromAccountId, ct)
            ?? throw new InvalidOperationException("חשבון המקור לא נמצא");

        var toAccount = await _unitOfWork.Accounts.GetByIdAsync(toAccountId, ct)
            ?? throw new InvalidOperationException("חשבון היעד לא נמצא");

        if (fromAccount.Balance < amount)
            throw new InvalidOperationException("אין מספיק יתרה בחשבון המקור לביצוע ההעברה");

        fromAccount.Balance -= amount;
        toAccount.Balance += amount;

        _unitOfWork.Transactions.Add(new Transaction
        {
            AccountId = fromAccount.Id,
            Type = TransactionType.TransferOut,
            Amount = amount,
            BalanceAfter = fromAccount.Balance,
            Note = note ?? $"העברה לחשבון {toAccount.AccountNumber}"
        });

        _unitOfWork.Transactions.Add(new Transaction
        {
            AccountId = toAccount.Id,
            Type = TransactionType.TransferIn,
            Amount = amount,
            BalanceAfter = toAccount.Balance,
            Note = note ?? $"העברה מחשבון {fromAccount.AccountNumber}"
        });

        // אותה תחרות עלולה לקרות כאן על חשבון המקור וגם על חשבון היעד.
        await SaveWithConcurrencyHandlingAsync(fromAccountId, "העברה", ct);
    }

    public Task<List<Transaction>> GetTransactionsAsync(int accountId, CancellationToken ct = default) =>
        _unitOfWork.Transactions.GetByAccountIdAsync(accountId, ct);

    /// <summary>
    /// שומר שינויים, ואם EF Core מזהה שמישהו אחר עדכן את השורה בינתיים (Version לא תואם),
    /// הופך את ה-DbUpdateConcurrencyException הטכני לחריגה עסקית שה-API יודע למפות ל-409.
    /// </summary>
    private async Task SaveWithConcurrencyHandlingAsync(int accountId, string actionDescription, CancellationToken ct)
    {
        try
        {
            await _unitOfWork.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex,
                "התנגשות תחרות (optimistic concurrency) על חשבון {AccountId} בפעולת {Action}",
                accountId, actionDescription);

            throw new ConcurrencyConflictException(
                $"החשבון עודכן על ידי משתמש אחר בין הקריאה לשמירה (פעולת {actionDescription}). נסי שוב.");
        }
    }

    private static string GenerateAccountNumber()
    {
        var random = new Random();
        return $"{random.Next(1000, 9999)}-{random.Next(1000, 9999)}";
    }
}
