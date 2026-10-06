using BankProject.Core.Interfaces;
using BankProject.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace BankProject.Data.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly BankDbContext _context;

    public CustomerRepository(BankDbContext context)
    {
        _context = context;
    }

    public async Task<List<Customer>> GetAllAsync(CancellationToken ct = default) =>
        await _context.Customers
            .AsNoTracking()
            .OrderBy(c => c.FullName)
            .ToListAsync(ct);

    // היחיד שקורא לזה (CreateAccountAsync) רק מוודא שהלקוח קיים - קריאה בלבד, לכן AsNoTracking.
    // FindAsync לא תומך ב-AsNoTracking, אז עוברים לשאילתה מפורשת.
    public async Task<Customer?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<Customer?> GetWithAccountsAsync(int id, CancellationToken ct = default) =>
        await _context.Customers
            .AsNoTracking()
            .Include(c => c.Accounts)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    public void Add(Customer customer) => _context.Customers.Add(customer);
}
