using LicenseManagement.Data.Models;

namespace LicenseManagement.Data.Repositories
{
    public interface ICustomerRepository
    {
        Task<List<Customer>> GetAllAsync();
        Task<Customer?> GetByIdAsync(int id);
        Task<Customer> CreateAsync(Customer customer);
        Task<Customer> UpdateAsync(int id, Customer customer);
        Task DeleteAsync(int id);
    }
}