using BankProject.Core.Models;

namespace BankProject.Core.Interfaces;

public interface ITransactionRepository
{
    Task<List<Transaction>> GetByAccountIdAsync(int accountId, CancellationToken ct = default);
    void Add(Transaction transaction);
}
