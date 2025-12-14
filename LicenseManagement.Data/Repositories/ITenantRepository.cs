using LicenseManagement.Data.Models;

namespace LicenseManagement.Data.Repositories
{
    public interface ITenantRepository
    {
        Task<List<Tenant>> GetAllAsync();
        Task<Tenant?> GetByIdAsync(int id);
        Task<Tenant?> GetByNameAsync(string name);
        Task<Tenant> CreateAsync(Tenant tenant);
        Task<Tenant> UpdateAsync(int id, Tenant tenant);
        Task DeleteAsync(int id);
    }
}