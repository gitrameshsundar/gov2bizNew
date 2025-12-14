using LicenseManagement.Data.Models;

namespace LicenseManagement.Data.Services
{
    public interface ILicenseService
    {
        Task<List<License>> GetAllLicensesAsync();
        Task<License?> GetLicenseByIdAsync(int id);
        Task<License> CreateLicenseAsync(License license);
        Task<License> UpdateLicenseAsync(int id, License license);
        Task DeleteLicenseAsync(int id);
    }
}