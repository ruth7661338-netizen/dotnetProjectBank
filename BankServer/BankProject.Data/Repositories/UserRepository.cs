using BankProject.Core.Interfaces;
using BankProject.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace BankProject.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly BankDbContext _context;

    public UserRepository(BankDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email, ct);

    public async Task<User?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, ct);

    public void Add(User user) => _context.Users.Add(user);
}
