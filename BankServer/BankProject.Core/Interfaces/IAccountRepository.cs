using BankProject.Core.Enums;
using BankProject.Core.Models;

namespace BankProject.Core.Interfaces;

public interface IAccountRepository
{
    /// <summary>Pagination אמיתי (Skip/Take בתוך השאילתה), עם סינון אופציונלי לפי סוג חשבון.</summary>
    Task<(List<Account> Items, int TotalCount)> GetPagedAsync(
        int pageNumber, int pageSize, AccountType? typeFilter = null, CancellationToken ct = default);

    Task<Account?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Account?> GetByIdWithCustomerAsync(int id, CancellationToken ct = default);
    void Add(Account account);
}
