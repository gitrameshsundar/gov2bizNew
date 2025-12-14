using LicenseManagement.Data.Models;

namespace LicenseManagement.Data.Services
{
    public interface ITenantService
    {
        Task<List<Tenant>> GetAllTenantsAsync();
        Task<Tenant?> GetTenantByIdAsync(int id);
        Task<Tenant> CreateTenantAsync(Tenant tenant);
        Task<Tenant> UpdateTenantAsync(int id, Tenant tenant);
        Task DeleteTenantAsync(int id);
    }
}