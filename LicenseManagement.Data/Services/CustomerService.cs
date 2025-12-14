using LicenseManagement.Data.Models;
using LicenseManagement.Data.Repositories;

namespace LicenseManagement.Data.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _repository;

        public CustomerService(ICustomerRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<Customer>> GetAllCustomersAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<Customer?> GetCustomerByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid customer ID");

            return await _repository.GetByIdAsync(id);
        }

        public async Task<Customer> CreateCustomerAsync(Customer customer)
        {
            if (string.IsNullOrWhiteSpace(customer.Name))
                throw new ArgumentException("Customer name is required");

            return await _repository.CreateAsync(customer);
        }

        public async Task<Customer> UpdateCustomerAsync(int id, Customer customer)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid customer ID");

            if (string.IsNullOrWhiteSpace(customer.Name))
                throw new ArgumentException("Customer name is required");

            return await _repository.UpdateAsync(id, customer);
        }

        public async Task DeleteCustomerAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid customer ID");

            await _repository.DeleteAsync(id);
        }
    }
}