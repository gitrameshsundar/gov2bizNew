using LicenseManagement.Data.Models;

namespace LicenseManagement.Data.Repositories
{
    public interface IUserRepository
    {
        Task<List<User>> GetAllAsync();
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByUsernameAsync(string username);
        Task<List<User>> GetByTenantAsync(int tenantId);
        Task<User> CreateAsync(User user);
        Task<User> UpdateAsync(int id, User user);
        Task DeleteAsync(int id);
        Task<bool> UsernameExistsAsync(string username);
    }
}