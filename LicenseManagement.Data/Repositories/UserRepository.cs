using LicenseManagement.Data.Data;
using LicenseManagement.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace LicenseManagement.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserDbContext _context;

        public UserRepository(UserDbContext context)
        {
            _context = context;
        }

        public async Task<List<User>> GetAllAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.UserID == id);
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<List<User>> GetByTenantAsync(int tenantId)
        {
            return await _context.Users.Where(u => u.TenantID == tenantId).ToListAsync();
        }

        public async Task<User> CreateAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User> UpdateAsync(int id, User user)
        {
            var existing = await GetByIdAsync(id);
            if (existing == null)
                throw new KeyNotFoundException($"User {id} not found");

            existing.Email = user.Email;
            existing.Role = user.Role;
            existing.TenantID = user.TenantID;
            existing.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task DeleteAsync(int id)
        {
            var user = await GetByIdAsync(id);
            if (user == null)
                throw new KeyNotFoundException($"User {id} not found");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await _context.Users.AnyAsync(u => u.Username == username);
        }
    }
}