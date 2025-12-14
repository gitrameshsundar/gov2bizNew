using LicenseManagement.Data.Data;
using LicenseManagement.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace LicenseManagement.Data.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly CustomerDbContext _context;

        public CustomerRepository(CustomerDbContext context)
        {
            _context = context;
        }

        public async Task<List<Customer>> GetAllAsync()
        {
            return await _context.Customers.ToListAsync();
        }

        public async Task<Customer?> GetByIdAsync(int id)
        {
            return await _context.Customers.FirstOrDefaultAsync(c => c.CustomerID == id);
        }

        public async Task<Customer> CreateAsync(Customer customer)
        {
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            return customer;
        }

        public async Task<Customer> UpdateAsync(int id, Customer customer)
        {
            var existing = await GetByIdAsync(id);
            if (existing == null)
                throw new KeyNotFoundException($"Customer {id} not found");

            existing.Name = customer.Name;
            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task DeleteAsync(int id)
        {
            var customer = await GetByIdAsync(id);
            if (customer == null)
                throw new KeyNotFoundException($"Customer {id} not found");

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
        }
    }
}