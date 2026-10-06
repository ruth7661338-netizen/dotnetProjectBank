namespace BankProject.Core.Interfaces;

/// <summary>
/// מאגד את כל ה-repositories ומאפשר לשמור שינויים בטרנזקציה אחת דרך DbContext יחיד.
/// ה-Service תלוי רק בממשק הזה, ולא מכיר את EF Core בכלל.
/// </summary>
public interface IUnitOfWork
{
    ICustomerRepository Customers { get; }
    IAccountRepository Accounts { get; }
    ITransactionRepository Transactions { get; }
    IUserRepository Users { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
