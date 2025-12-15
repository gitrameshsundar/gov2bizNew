using LicenseManagement.Data.Models;

namespace LicenseManagement.Data.Repositories
{
    /// <summary>
    /// Interface for payment data access operations.
    /// 
    /// DESIGN PATTERN: Repository Pattern
    /// PURPOSE: Abstracts data access logic from business logic
    /// 
    /// RESPONSIBILITIES:
    /// - CRUD operations for payments
    /// - Payment filtering and searching
    /// - Transaction history retrieval
    /// - Payment status queries
    /// </summary>
    public interface IPaymentRepository
    {
        /// <summary>
        /// Gets all payments from the database.
        /// </summary>
        Task<List<Payment>> GetAllAsync();

        /// <summary>
        /// Gets a specific payment by ID.
        /// </summary>
        /// <param name="id">Payment ID</param>
        /// <returns>Payment if found, null otherwise</returns>
        Task<Payment?> GetByIdAsync(int id);

        /// <summary>
        /// Gets all payments for a specific license.
        /// </summary>
        /// <param name="licenseId">License ID</param>
        /// <returns>List of payments for the license</returns>
        Task<List<Payment>> GetByLicenseIdAsync(int licenseId);

        /// <summary>
        /// Gets payments by status.
        /// </summary>
        /// <param name="status">Payment status (Pending, Completed, Failed, etc.)</param>
        /// <returns>List of payments matching the status</returns>
        Task<List<Payment>> GetByStatusAsync(string status);

        /// <summary>
        /// Gets a payment by transaction reference from payment provider.
        /// </summary>
        /// <param name="transactionReference">External transaction reference</param>
        /// <returns>Payment if found, null otherwise</returns>
        Task<Payment?> GetByTransactionReferenceAsync(string transactionReference);

        /// <summary>
        /// Gets payments within a date range.
        /// </summary>
        /// <param name="startDate">Start date (UTC)</param>
        /// <param name="endDate">End date (UTC)</param>
        /// <returns>List of payments within the date range</returns>
        Task<List<Payment>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Creates a new payment in the database.
        /// </summary>
        /// <param name="payment">Payment to create</param>
        /// <returns>Created payment with ID</returns>
        Task<Payment> CreateAsync(Payment payment);

        /// <summary>
        /// Updates an existing payment in the database.
        /// </summary>
        /// <param name="id">Payment ID</param>
        /// <param name="payment">Updated payment data</param>
        /// <returns>Updated payment</returns>
        Task<Payment> UpdateAsync(int id, Payment payment);

        /// <summary>
        /// Deletes a payment from the database.
        /// </summary>
        /// <param name="id">Payment ID</param>
        Task DeleteAsync(int id);

        /// <summary>
        /// Checks if a transaction reference already exists.
        /// Prevents duplicate payments.
        /// </summary>
        /// <param name="transactionReference">Transaction reference to check</param>
        /// <returns>True if exists, false otherwise</returns>
        Task<bool> TransactionReferenceExistsAsync(string transactionReference);
    }
}