using BankProject.Core.Interfaces;
using BankProject.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace BankProject.Data.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly BankDbContext _context;

    public TransactionRepository(BankDbContext context)
    {
        _context = context;
    }

    public async Task<List<Transaction>> GetByAccountIdAsync(int accountId, CancellationToken ct = default) =>
        await _context.Transactions
            .AsNoTracking()
            .Where(t => t.AccountId == accountId)
            .OrderByDescending(t => t.Date)
            .ToListAsync(ct);

    public void Add(Transaction transaction) => _context.Transactions.Add(transaction);
}
