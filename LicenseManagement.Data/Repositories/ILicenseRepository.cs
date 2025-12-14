using LicenseManagement.Data.Models;

namespace LicenseManagement.Data.Repositories
{
    public interface ILicenseRepository
    {
        Task<List<License>> GetAllAsync();
        Task<License?> GetByIdAsync(int id);
        Task<License?> GetByNameAsync(string name);
        Task<License> CreateAsync(License license);
        Task<License> UpdateAsync(int id, License license);
        Task DeleteAsync(int id);
    }
}