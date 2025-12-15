using LicenseManagement.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace LicenseManagement.Data.Data
{
    /// <summary>
    /// Database context for payment entities.
    /// 
    /// DESIGN PATTERN: DbContext Pattern
    /// PURPOSE: Manages payment entity interactions with database
    /// 
    /// CONFIGURATION:
    /// - Entity mapping to database tables
    /// - Primary key definitions
    /// - Index configurations
    /// - Constraint definitions
    /// </summary>
    public class PaymentDbContext : DbContext
    {
        public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options) { }

        /// <summary>
        /// Gets or sets the Payments DbSet.
        /// Represents the Payments table in the database.
        /// </summary>
        public DbSet<Payment> Payments { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Payment entity
            modelBuilder.Entity<Payment>(entity =>
            {
                // Primary Key
                entity.HasKey(e => e.PaymentID);

                // Properties
                entity.Property(e => e.PaymentID)
                    .ValueGeneratedOnAdd()
                    .HasComment("Primary key for payments");

                entity.Property(e => e.LicenseID)
                    .IsRequired()
                    .HasComment("Reference to the license being paid for");

                entity.Property(e => e.Amount)
                    .IsRequired()
                    .HasColumnType("decimal(18,2)")
                    .HasComment("Payment amount with 2 decimal places for currency");

                entity.Property(e => e.Currency)
                    .IsRequired()
                    .HasMaxLength(3)
                    .HasDefaultValue("USD")
                    .HasComment("ISO 4217 currency code");

                entity.Property(e => e.PaymentDate)
                    .IsRequired()
                    .HasComment("When the payment was made");

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasDefaultValue("Pending")
                    .HasComment("Payment status: Pending, Completed, Failed, Refunded, Disputed");

                entity.Property(e => e.PaymentMethod)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasComment("Payment method used: CreditCard, BankTransfer, PayPal, etc.");

                entity.Property(e => e.TransactionReference)
                    .HasMaxLength(100)
                    .HasComment("External transaction reference from payment provider");

                entity.Property(e => e.Description)
                    .HasMaxLength(500)
                    .HasComment("Optional description or notes about the payment");

                entity.Property(e => e.CreatedDate)
                    .IsRequired()
                    .HasDefaultValueSql("GETUTCDATE()")
                    .HasComment("When the payment record was created");

                entity.Property(e => e.UpdatedDate)
                    .HasComment("When the payment record was last updated");

                // Indexes
                //entity.HasIndex(e => e.LicenseID)
                //    .HasDatabaseName("IX_Payment_LicenseID")
                //    .HasComment("Index for finding payments by license");

                //entity.HasIndex(e => e.Status)
                //    .HasDatabaseName("IX_Payment_Status")
                //    .HasComment("Index for finding payments by status");

                //entity.HasIndex(e => e.PaymentDate)
                //    .HasDatabaseName("IX_Payment_PaymentDate")
                //    .HasComment("Index for sorting and filtering by date");

                //entity.HasIndex(e => e.TransactionReference)
                //    .IsUnique()
                //    .HasDatabaseName("UX_Payment_TransactionReference")
                //    .HasComment("Unique index to prevent duplicate transaction references");

                // Table Configuration
                entity.ToTable("Payments", tb =>
                {
                    tb.HasComment("Stores payment transaction records");
                });
            });
        }
    }
}