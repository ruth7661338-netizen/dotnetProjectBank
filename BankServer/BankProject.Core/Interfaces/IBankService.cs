using BankProject.Core.Enums;
using BankProject.Core.Models;

namespace BankProject.Core.Interfaces;

public interface IBankService
{
    /// <summary>Pagination אמיתי מול ה-DB, עם סינון אופציונלי לפי סוג חשבון.</summary>
    Task<(List<Account> Items, int TotalCount)> GetAllAccountsAsync(
        int pageNumber, int pageSize, AccountType? typeFilter = null, CancellationToken ct = default);
    Task<Account?> GetAccountAsync(int id, CancellationToken ct = default);
    Task<List<Customer>> GetAllCustomersAsync(CancellationToken ct = default);
    Task<Customer?> GetCustomerAsync(int id, CancellationToken ct = default);

    Task<Account> CreateAccountAsync(int customerId, AccountType type, decimal openingBalance, CancellationToken ct = default);

    Task DepositAsync(int accountId, decimal amount, string? note = null, CancellationToken ct = default);
    Task WithdrawAsync(int accountId, decimal amount, string? note = null, CancellationToken ct = default);
    Task TransferAsync(int fromAccountId, int toAccountId, decimal amount, string? note = null, CancellationToken ct = default);

    Task<List<Transaction>> GetTransactionsAsync(int accountId, CancellationToken ct = default);
}
