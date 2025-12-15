using LicenseManagement.Data.Models;

namespace LicenseManagement.Data.Services
{
    /// <summary>
    /// Interface for payment business logic operations.
    /// 
    /// DESIGN PATTERN: Service Layer Pattern
    /// PURPOSE: Defines contract for payment business logic
    /// 
    /// RESPONSIBILITIES:
    /// - Input validation
    /// - Business rule enforcement
    /// - Payment processing workflows
    /// - Exception handling
    /// </summary>
    public interface IPaymentService
    {
        /// <summary>
        /// Gets all payments.
        /// </summary>
        Task<List<Payment>> GetAllPaymentsAsync();

        /// <summary>
        /// Gets a payment by ID.
        /// </summary>
        Task<Payment?> GetPaymentByIdAsync(int id);

        /// <summary>
        /// Gets all payments for a license.
        /// </summary>
        Task<List<Payment>> GetPaymentsByLicenseAsync(int licenseId);

        /// <summary>
        /// Gets payments by status.
        /// </summary>
        Task<List<Payment>> GetPaymentsByStatusAsync(string status);

        /// <summary>
        /// Gets payment summary/statistics.
        /// </summary>
        Task<PaymentSummary> GetPaymentSummaryAsync();

        /// <summary>
        /// Creates a new payment.
        /// </summary>
        Task<Payment> CreatePaymentAsync(Payment payment);

        /// <summary>
        /// Updates payment status (for webhook callbacks from payment providers).
        /// </summary>
        Task<Payment> UpdatePaymentStatusAsync(int id, string newStatus);

        /// <summary>
        /// Records a refund for a payment.
        /// </summary>
        Task<Payment> RefundPaymentAsync(int id, string reason);

        /// <summary>
        /// Deletes a payment (typically for Pending payments only).
        /// </summary>
        Task DeletePaymentAsync(int id);
    }

    /// <summary>
    /// Payment summary with statistics.
    /// </summary>
    public class PaymentSummary
    {
        public decimal TotalAmount { get; set; }
        public int TotalPayments { get; set; }
        public int CompletedPayments { get; set; }
        public int PendingPayments { get; set; }
        public int FailedPayments { get; set; }
        public int RefundedPayments { get; set; }
    }
}