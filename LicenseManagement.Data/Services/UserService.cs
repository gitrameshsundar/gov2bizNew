using LicenseManagement.Data.Models;
using LicenseManagement.Data.Repositories;

namespace LicenseManagement.Data.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;

        public UserService(IUserRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid user ID");

            return await _repository.GetByIdAsync(id);
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username is required");

            return await _repository.GetByUsernameAsync(username);
        }

        public async Task<List<User>> GetUsersByTenantAsync(int tenantId)
        {
            if (tenantId <= 0)
                throw new ArgumentException("Invalid tenant ID");

            return await _repository.GetByTenantAsync(tenantId);
        }

        public async Task<User> CreateUserAsync(User user)
        {
            ValidateUser(user);

            var exists = await _repository.UsernameExistsAsync(user.Username);
            if (exists)
                throw new ArgumentException("Username already exists");

            return await _repository.CreateAsync(user);
        }

        public async Task<User> UpdateUserAsync(int id, User user)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid user ID");

            ValidateUser(user);
            return await _repository.UpdateAsync(id, user);
        }

        public async Task DeleteUserAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid user ID");

            await _repository.DeleteAsync(id);
        }

        public async Task AssignUserToTenantAsync(int userId, int tenantId)
        {
            if (userId <= 0)
                throw new ArgumentException("Invalid user ID");

            if (tenantId <= 0)
                throw new ArgumentException("Invalid tenant ID");

            var user = await _repository.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            user.TenantID = tenantId;
            user.UpdatedDate = DateTime.UtcNow;
            await _repository.UpdateAsync(userId, user);
        }

        private static void ValidateUser(User user)
        {
            if (string.IsNullOrWhiteSpace(user.Username))
                throw new ArgumentException("Username is required");

            if (string.IsNullOrWhiteSpace(user.Email))
                throw new ArgumentException("Email is required");

            if (string.IsNullOrWhiteSpace(user.Password))
                throw new ArgumentException("Password is required");
        }
    }
}