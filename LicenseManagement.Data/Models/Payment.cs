namespace LicenseManagement.Data.Models
{
    /// <summary>
    /// Represents a payment transaction in the system.
    /// 
    /// DESIGN PATTERN: Entity Pattern
    /// PURPOSE: Domain model for payment entities
    /// 
    /// PROPERTIES:
    /// - PaymentID: Unique identifier for the payment
    /// - LicenseID: Reference to the license being paid for
    /// - Amount: Payment amount in currency
    /// - Currency: Currency code (USD, EUR, etc.)
    /// - PaymentDate: When the payment was made
    /// - Status: Payment status (Pending, Completed, Failed, Refunded)
    /// - PaymentMethod: How payment was made (Credit Card, Bank Transfer, etc.)
    /// - TransactionReference: External payment provider reference
    /// - CreatedDate: When payment record was created
    /// - UpdatedDate: Last modification timestamp
    /// </summary>
    public class Payment
    {
        /// <summary>
        /// Gets or sets the unique identifier for the payment.
        /// Primary key for the payment record.
        /// </summary>
        public int PaymentID { get; set; }

        /// <summary>
        /// Gets or sets the license ID this payment is associated with.
        /// Foreign key reference to License table.
        /// </summary>
        public int LicenseID { get; set; }

        /// <summary>
        /// Gets or sets the payment amount.
        /// 
        /// REQUIREMENTS:
        /// - Must be greater than 0
        /// - Stored with 2 decimal places (currency precision)
        /// - Represents the actual amount paid
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the currency code (ISO 4217).
        /// 
        /// EXAMPLES:
        /// - USD: United States Dollar
        /// - EUR: Euro
        /// - GBP: British Pound
        /// - AUD: Australian Dollar
        /// 
        /// DEFAULT: USD
        /// MAXIMUM LENGTH: 3 characters
        /// </summary>
        public string Currency { get; set; } = "USD";

        /// <summary>
        /// Gets or sets the payment date.
        /// When the payment was actually processed/received.
        /// </summary>
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the payment status.
        /// 
        /// VALID VALUES:
        /// - Pending: Awaiting processing or payment confirmation
        /// - Completed: Successfully processed and confirmed
        /// - Failed: Payment processing failed
        /// - Refunded: Payment has been refunded to customer
        /// - Disputed: Payment is under dispute
        /// 
        /// DEFAULT: Pending
        /// MAXIMUM LENGTH: 50 characters
        /// </summary>
        public string Status { get; set; } = "Pending";

        /// <summary>
        /// Gets or sets the payment method used.
        /// 
        /// EXAMPLES:
        /// - CreditCard: Visa, MasterCard, Amex, Discover
        /// - DebitCard: Debit card payment
        /// - BankTransfer: Direct bank transfer (ACH, SWIFT, etc.)
        /// - PayPal: PayPal payment service
        /// - DigitalWallet: Apple Pay, Google Pay, etc.
        /// - Wire: Wire transfer
        /// - Check: Mailed check
        /// 
        /// MAXIMUM LENGTH: 50 characters
        /// </summary>
        public string PaymentMethod { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the transaction reference from payment provider.
        /// 
        /// PURPOSE:
        /// - Track payment across multiple systems
        /// - Reference for payment gateway reconciliation
        /// - Customer service reference for support
        /// - Dispute resolution evidence
        /// 
        /// FORMAT:
        /// - Payment provider dependent (Stripe, PayPal, etc.)
        /// - Often alphanumeric identifier
        /// - Unique within payment gateway
        /// 
        /// EXAMPLE:
        /// - Stripe: pi_1234567890abcdef
        /// - PayPal: EC-1234567890ABCDEF
        /// 
        /// MAXIMUM LENGTH: 100 characters
        /// </summary>
        public string TransactionReference { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets optional description/notes for the payment.
        /// 
        /// USE CASES:
        /// - Invoice number reference
        /// - Order details
        /// - Custom notes from admin
        /// - Reason for refund
        /// 
        /// MAXIMUM LENGTH: 500 characters
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the date when the payment record was created.
        /// Automatically set to current UTC time on creation.
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the date when the payment record was last updated.
        /// Automatically updated when payment status or amount changes.
        /// </summary>
        public DateTime? UpdatedDate { get; set; }
    }
}