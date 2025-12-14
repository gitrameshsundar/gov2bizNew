using LicenseManagement.Data.Models;

namespace LicenseManagement.Data.Services
{
    public interface IUserService
    {
        Task<List<User>> GetAllUsersAsync();
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<List<User>> GetUsersByTenantAsync(int tenantId);
        Task<User> CreateUserAsync(User user);
        Task<User> UpdateUserAsync(int id, User user);
        Task DeleteUserAsync(int id);
        Task AssignUserToTenantAsync(int userId, int tenantId);
    }
}