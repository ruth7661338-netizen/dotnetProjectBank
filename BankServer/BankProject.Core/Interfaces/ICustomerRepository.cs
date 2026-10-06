using BankProject.Core.Models;

namespace BankProject.Core.Interfaces;

public interface ICustomerRepository
{
    Task<List<Customer>> GetAllAsync(CancellationToken ct = default);
    Task<Customer?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Customer?> GetWithAccountsAsync(int id, CancellationToken ct = default);
    void Add(Customer customer);
}
