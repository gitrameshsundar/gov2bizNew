using LicenseManagement.Data.Data;
using LicenseManagement.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace LicenseManagement.Data.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly PaymentDbContext _context;

        public PaymentRepository(PaymentDbContext context)
        {
            _context = context;
        }

        public async Task<List<Payment>> GetAllAsync()
        {
            return await _context.Payments.ToListAsync();
        }

        public async Task<Payment?> GetByIdAsync(int id)
        {
            return await _context.Payments.FirstOrDefaultAsync(p => p.PaymentID == id);
        }

        public async Task<List<Payment>> GetByLicenseIdAsync(int licenseId)
        {
            return await _context.Payments.Where(p => p.LicenseID == licenseId).ToListAsync();
        }

        public async Task<List<Payment>> GetByStatusAsync(string status)
        {
            return await _context.Payments.Where(p => p.Status == status).ToListAsync();
        }

        public async Task<Payment?> GetByTransactionReferenceAsync(string transactionReference)
        {
            return await _context.Payments.FirstOrDefaultAsync(p => p.TransactionReference == transactionReference);
        }

        public async Task<List<Payment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Payments.Where(p => p.PaymentDate >= startDate && p.PaymentDate <= endDate).ToListAsync();
        }

        public async Task<Payment> CreateAsync(Payment payment)
        {
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            return payment;
        }

        public async Task<Payment> UpdateAsync(int id, Payment payment)
        {
            var existing = await GetByIdAsync(id);
            if (existing == null)
                throw new KeyNotFoundException($"Payment {id} not found");

            existing.Amount = payment.Amount;
            existing.Status = payment.Status;
            existing.PaymentMethod = payment.PaymentMethod;
            existing.TransactionReference = payment.TransactionReference;
            existing.PaymentDate = payment.PaymentDate;
            existing.Currency = payment.Currency;
            existing.Description = payment.Description;
            existing.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task DeleteAsync(int id)
        {
            var payment = await GetByIdAsync(id);
            if (payment == null)
                throw new KeyNotFoundException($"Payment {id} not found");

            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> TransactionReferenceExistsAsync(string transactionReference)
        {
            return await _context.Payments.AnyAsync(p => p.TransactionReference == transactionReference);
        }
    }
}