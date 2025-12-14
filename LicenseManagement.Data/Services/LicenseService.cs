using LicenseManagement.Data.Models;
using LicenseManagement.Data.Repositories;

namespace LicenseManagement.Data.Services
{
    public class LicenseService : ILicenseService
    {
        private readonly ILicenseRepository _repository;

        public LicenseService(ILicenseRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<License>> GetAllLicensesAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<License?> GetLicenseByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid license ID");

            return await _repository.GetByIdAsync(id);
        }

        public async Task<License> CreateLicenseAsync(License license)
        {
            if (string.IsNullOrWhiteSpace(license.Name))
                throw new ArgumentException("License name is required");

            return await _repository.CreateAsync(license);
        }

        public async Task<License> UpdateLicenseAsync(int id, License license)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid license ID");

            if (string.IsNullOrWhiteSpace(license.Name))
                throw new ArgumentException("License name is required");

            return await _repository.UpdateAsync(id, license);
        }

        public async Task DeleteLicenseAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid license ID");

            await _repository.DeleteAsync(id);
        }
    }
}