using LicenseManagement.Data.Models;
using LicenseManagement.Data.Repositories;

namespace LicenseManagement.Data.Services
{
    public class TenantService : ITenantService
    {
        private readonly ITenantRepository _repository;

        public TenantService(ITenantRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<Tenant>> GetAllTenantsAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<Tenant?> GetTenantByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid tenant ID");

            return await _repository.GetByIdAsync(id);
        }

        public async Task<Tenant> CreateTenantAsync(Tenant tenant)
        {
            if (string.IsNullOrWhiteSpace(tenant.Name))
                throw new ArgumentException("Tenant name is required");

            return await _repository.CreateAsync(tenant);
        }

        public async Task<Tenant> UpdateTenantAsync(int id, Tenant tenant)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid tenant ID");

            if (string.IsNullOrWhiteSpace(tenant.Name))
                throw new ArgumentException("Tenant name is required");

            return await _repository.UpdateAsync(id, tenant);
        }

        public async Task DeleteTenantAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid tenant ID");

            await _repository.DeleteAsync(id);
        }
    }
}