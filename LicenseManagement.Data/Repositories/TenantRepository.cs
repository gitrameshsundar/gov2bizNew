using LicenseManagement.Data.Data;
using LicenseManagement.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace LicenseManagement.Data.Repositories
{
    public class TenantRepository : ITenantRepository
    {
        private readonly TenantDbContext _context;

        public TenantRepository(TenantDbContext context)
        {
            _context = context;
        }

        public async Task<List<Tenant>> GetAllAsync()
        {
            return await _context.Tenants.ToListAsync();
        }

        public async Task<Tenant?> GetByIdAsync(int id)
        {
            return await _context.Tenants.FirstOrDefaultAsync(t => t.TenantID == id);
        }

        public async Task<Tenant?> GetByNameAsync(string name)
        {
            return await _context.Tenants.FirstOrDefaultAsync(t => t.Name == name);
        }

        public async Task<Tenant> CreateAsync(Tenant tenant)
        {
            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync();
            return tenant;
        }

        public async Task<Tenant> UpdateAsync(int id, Tenant tenant)
        {
            var existing = await GetByIdAsync(id);
            if (existing == null)
                throw new KeyNotFoundException($"Tenant {id} not found");

            existing.Name = tenant.Name;
           // existing.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task DeleteAsync(int id)
        {
            var tenant = await GetByIdAsync(id);
            if (tenant == null)
                throw new KeyNotFoundException($"Tenant {id} not found");

            _context.Tenants.Remove(tenant);
            await _context.SaveChangesAsync();
        }
    }
}