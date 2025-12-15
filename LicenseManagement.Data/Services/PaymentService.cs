using LicenseManagement.Data.Models;
using LicenseManagement.Data.Repositories;

namespace LicenseManagement.Data.Services
{
    /// <summary>
    /// Service implementation for payment business logic.
    /// 
    /// DESIGN PATTERN: Service Layer Pattern
    /// PURPOSE: Encapsulates payment business logic
    /// 
    /// RESPONSIBILITIES:
    /// - Payment validation
    /// - Business rule enforcement
    /// - Status transitions
    /// - Error handling
    /// </summary>
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _repository;

        public PaymentService(IPaymentRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<List<Payment>> GetAllPaymentsAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<Payment?> GetPaymentByIdAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid payment ID", nameof(id));

            return await _repository.GetByIdAsync(id);
        }

        public async Task<List<Payment>> GetPaymentsByLicenseAsync(int licenseId)
        {
            if (licenseId <= 0)
                throw new ArgumentException("Invalid license ID", nameof(licenseId));

            return await _repository.GetByLicenseIdAsync(licenseId);
        }

        public async Task<List<Payment>> GetPaymentsByStatusAsync(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                throw new ArgumentException("Status is required", nameof(status));

            var validStatuses = new[] { "Pending", "Completed", "Failed", "Refunded", "Disputed" };
            if (!validStatuses.Contains(status))
                throw new ArgumentException($"Invalid status: {status}", nameof(status));

            return await _repository.GetByStatusAsync(status);
        }

        public async Task<PaymentSummary> GetPaymentSummaryAsync()
        {
            var allPayments = await _repository.GetAllAsync();

            return new PaymentSummary
            {
                TotalAmount = allPayments
                    .Where(p => p.Status == "Completed")
                    .Sum(p => p.Amount),
                TotalPayments = allPayments.Count,
                CompletedPayments = allPayments.Count(p => p.Status == "Completed"),
                PendingPayments = allPayments.Count(p => p.Status == "Pending"),
                FailedPayments = allPayments.Count(p => p.Status == "Failed"),
                RefundedPayments = allPayments.Count(p => p.Status == "Refunded")
            };
        }

        public async Task<Payment> CreatePaymentAsync(Payment payment)
        {
            // Validate input
            if (payment == null)
                throw new ArgumentException("Payment cannot be null", nameof(payment));

            if (payment.LicenseID <= 0)
                throw new ArgumentException("Valid license ID is required", nameof(payment.LicenseID));

            if (payment.Amount <= 0)
                throw new ArgumentException("Payment amount must be greater than 0", nameof(payment.Amount));

            if (string.IsNullOrWhiteSpace(payment.Currency))
                throw new ArgumentException("Currency is required", nameof(payment.Currency));

            if (payment.Currency.Length != 3)
                throw new ArgumentException("Currency must be a 3-character ISO code", nameof(payment.Currency));

            if (string.IsNullOrWhiteSpace(payment.PaymentMethod))
                throw new ArgumentException("Payment method is required", nameof(payment.PaymentMethod));

            // Check for duplicate transaction reference
            if (!string.IsNullOrWhiteSpace(payment.TransactionReference))
            {
                if (await _repository.TransactionReferenceExistsAsync(payment.TransactionReference))
                    throw new InvalidOperationException("A payment with this transaction reference already exists");
            }

            payment.CreatedDate = DateTime.UtcNow;
            return await _repository.CreateAsync(payment);
        }

        public async Task<Payment> UpdatePaymentStatusAsync(int id, string newStatus)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid payment ID", nameof(id));

            if (string.IsNullOrWhiteSpace(newStatus))
                throw new ArgumentException("Status is required", nameof(newStatus));

            var validStatuses = new[] { "Pending", "Completed", "Failed", "Refunded", "Disputed" };
            if (!validStatuses.Contains(newStatus))
                throw new ArgumentException($"Invalid status: {newStatus}", nameof(newStatus));

            var payment = await GetPaymentByIdAsync(id);
            if (payment == null)
                throw new KeyNotFoundException($"Payment {id} not found");

            payment.Status = newStatus;
            payment.UpdatedDate = DateTime.UtcNow;

            return await _repository.UpdateAsync(id, payment);
        }

        public async Task<Payment> RefundPaymentAsync(int id, string reason)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid payment ID", nameof(id));

            var payment = await GetPaymentByIdAsync(id);
            if (payment == null)
                throw new KeyNotFoundException($"Payment {id} not found");

            if (payment.Status != "Completed")
                throw new InvalidOperationException("Only completed payments can be refunded");

            payment.Status = "Refunded";
            payment.Description = reason ?? "Refund processed";
            payment.UpdatedDate = DateTime.UtcNow;

            return await _repository.UpdateAsync(id, payment);
        }

        public async Task DeletePaymentAsync(int id)
        {
            if (id <= 0)
                throw new ArgumentException("Invalid payment ID", nameof(id));

            var payment = await GetPaymentByIdAsync(id);
            if (payment == null)
                throw new KeyNotFoundException($"Payment {id} not found");

            if (payment.Status != "Pending")
                throw new InvalidOperationException("Only pending payments can be deleted");

            await _repository.DeleteAsync(id);
        }
    }
}