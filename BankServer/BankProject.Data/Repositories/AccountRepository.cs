using BankProject.Core.Enums;
using BankProject.Core.Interfaces;
using BankProject.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace BankProject.Data.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly BankDbContext _context;

    public AccountRepository(BankDbContext context)
    {
        _context = context;
    }

    public async Task<(List<Account> Items, int TotalCount)> GetPagedAsync(
        int pageNumber, int pageSize, AccountType? typeFilter = null, CancellationToken ct = default)
    {
        var query = _context.Accounts.AsNoTracking().AsQueryable();

        if (typeFilter.HasValue)
        {
            query = query.Where(a => a.Type == typeFilter.Value);
        }

        // הספירה הכוללת רצה על אותה שאילתה (אחרי הסינון, לפני Skip/Take) כדי שה-pagination יהיה נכון
        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Include(a => a.Customer)
            .Include(a => a.Tags)
            .OrderBy(a => a.AccountNumber)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    // נשאר tracked בכוונה: זו הפעולה שה-Service משתמש בה לפני Deposit/Withdraw/Transfer,
    // וה-change tracker חייב לעקוב אחרי השינוי כדי ש-SaveChangesAsync יידע מה עודכן.
    public async Task<Account?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await _context.Accounts.FindAsync(new object?[] { id }, ct);

    // קריאה בלבד (תצוגה) - AsNoTracking
    public async Task<Account?> GetByIdWithCustomerAsync(int id, CancellationToken ct = default) =>
        await _context.Accounts
            .AsNoTracking()
            .Include(a => a.Customer)
            .Include(a => a.Tags)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

    public void Add(Account account) => _context.Accounts.Add(account);
}
