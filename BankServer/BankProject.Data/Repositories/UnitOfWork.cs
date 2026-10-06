using BankProject.Core.Interfaces;

namespace BankProject.Data.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly BankDbContext _context;

    public UnitOfWork(BankDbContext context,
        ICustomerRepository customers,
        IAccountRepository accounts,
        ITransactionRepository transactions,
        IUserRepository users)
    {
        _context = context;
        Customers = customers;
        Accounts = accounts;
        Transactions = transactions;
        Users = users;
    }

    public ICustomerRepository Customers { get; }
    public IAccountRepository Accounts { get; }
    public ITransactionRepository Transactions { get; }
    public IUserRepository Users { get; }

    public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        _context.SaveChangesAsync(ct);
}
