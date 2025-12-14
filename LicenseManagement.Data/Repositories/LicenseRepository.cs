using LicenseManagement.Data.Data;
using LicenseManagement.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace LicenseManagement.Data.Repositories
{
    public class LicenseRepository : ILicenseRepository
    {
        private readonly LicenseDbContext _context;

        public LicenseRepository(LicenseDbContext context)
        {
            _context = context;
        }

        public async Task<List<License>> GetAllAsync()
        {
            return await _context.Licenses.ToListAsync();
        }

        public async Task<License?> GetByIdAsync(int id)
        {
            return await _context.Licenses.FirstOrDefaultAsync(l => l.LicenseID == id);
        }

        public async Task<License?> GetByNameAsync(string name)
        {
            return await _context.Licenses.FirstOrDefaultAsync(l => l.Name == name);
        }

        public async Task<License> CreateAsync(License license)
        {
            _context.Licenses.Add(license);
            await _context.SaveChangesAsync();
            return license;
        }

        public async Task<License> UpdateAsync(int id, License license)
        {
            var existing = await GetByIdAsync(id);
            if (existing == null)
                throw new KeyNotFoundException($"License {id} not found");

            existing.Name = license.Name;
            existing.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task DeleteAsync(int id)
        {
            var license = await GetByIdAsync(id);
            if (license == null)
                throw new KeyNotFoundException($"License {id} not found");

            _context.Licenses.Remove(license);
            await _context.SaveChangesAsync();
        }
    }
}