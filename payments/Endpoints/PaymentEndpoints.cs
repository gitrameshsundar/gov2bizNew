using LicenseManagement.Data.Models;
using LicenseManagement.Data.Results;
using LicenseManagement.Data.Services;

namespace payments.Endpoints
{
    /// <summary>
    /// Endpoint handlers for Payment API operations.
    /// 
    /// DESIGN PATTERN: Endpoint Organization Pattern (Minimal APIs)
    /// PURPOSE: Organizes all payment-related HTTP endpoint handlers
    /// 
    /// ENDPOINTS:
    /// - GET /api/payments - Get all payments
    /// - GET /api/payments/{id} - Get payment by ID
    /// - GET /api/payments/license/{licenseId} - Get payments by license
    /// - GET /api/payments/status/{status} - Get payments by status
    /// - GET /api/payments/summary - Get payment statistics
    /// - POST /api/payments - Create new payment
    /// - PUT /api/payments/{id}/status - Update payment status
    /// - PUT /api/payments/{id}/refund - Refund a payment
    /// - DELETE /api/payments/{id} - Delete payment
    /// 
    /// SECURITY:
    /// - All endpoints should be protected with [Authorize]
    /// - Payment creation should validate payment provider response
    /// - Refunds should require additional authorization
    /// </summary>
    public static class PaymentEndpoints
    {
        /// <summary>
        /// Registers all payment-related endpoints to the application.
        /// </summary>
        public static WebApplication MapPaymentEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/payments")
                .WithName("Payments")
                .WithOpenApi();

            // GET /api/payments
            group.MapGet("/", GetAllPayments)
                .WithName("GetAllPayments")
                .WithSummary("Get all payments")
                .WithDescription("Retrieves a list of all payment transactions")
                .Produces<ApiResult<List<Payment>>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status500InternalServerError)
                .WithOpenApi();

            // GET /api/payments/{paymentId}
            group.MapGet("/{paymentId}", GetPaymentById)
                .WithName("GetPaymentById")
                .WithSummary("Get payment by ID")
                .WithDescription("Retrieves a specific payment transaction")
                .Produces<ApiResult<Payment>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status400BadRequest)
                .WithOpenApi();

            // GET /api/payments/license/{licenseId}
            group.MapGet("/license/{licenseId}", GetPaymentsByLicense)
                .WithName("GetPaymentsByLicense")
                .WithSummary("Get payments by license")
                .WithDescription("Retrieves all payments for a specific license")
                .Produces<ApiResult<List<Payment>>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .WithOpenApi();

            // GET /api/payments/status/{status}
            group.MapGet("/status/{status}", GetPaymentsByStatus)
                .WithName("GetPaymentsByStatus")
                .WithSummary("Get payments by status")
                .WithDescription("Retrieves payments filtered by status")
                .Produces<ApiResult<List<Payment>>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status400BadRequest)
                .WithOpenApi();

            // GET /api/payments/summary
            group.MapGet("/summary", GetPaymentSummary)
                .WithName("GetPaymentSummary")
                .WithSummary("Get payment statistics")
                .WithDescription("Retrieves payment summary and statistics")
                .Produces<ApiResult<PaymentSummary>>(StatusCodes.Status200OK)
                .WithOpenApi();

            // POST /api/payments
            group.MapPost("/", CreatePayment)
                .WithName("CreatePayment")
                .WithSummary("Create a new payment")
                .WithDescription("Records a new payment transaction")
                .Accepts<Payment>("application/json")
                .Produces<ApiResult<Payment>>(StatusCodes.Status201Created)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status500InternalServerError)
                .WithOpenApi();

            // PUT /api/payments/{paymentId}/status
            group.MapPut("/{paymentId}/status", UpdatePaymentStatus)
                .WithName("UpdatePaymentStatus")
                .WithSummary("Update payment status")
                .WithDescription("Updates payment status (used for webhook callbacks)")
                .Accepts<PaymentStatusUpdate>("application/json")
                .Produces<ApiResult<Payment>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status400BadRequest)
                .WithOpenApi();

            // PUT /api/payments/{paymentId}/refund
            group.MapPut("/{paymentId}/refund", RefundPayment)
                .WithName("RefundPayment")
                .WithSummary("Refund a payment")
                .WithDescription("Processes a refund for a completed payment")
                .Accepts<RefundRequest>("application/json")
                .Produces<ApiResult<Payment>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status400BadRequest)
                .WithOpenApi();

            // DELETE /api/payments/{paymentId}
            group.MapDelete("/{paymentId}", DeletePayment)
                .WithName("DeletePayment")
                .WithSummary("Delete a payment")
                .WithDescription("Deletes a pending payment (completed payments cannot be deleted)")
                .Produces<ApiResponse>(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status400BadRequest)
                .WithOpenApi();

            return app;
        }

        private static async Task<IResult> GetAllPayments(IPaymentService service)
        {
            var payments = await service.GetAllPaymentsAsync();
            return Results.Ok(ApiResult<List<Payment>>.SuccessResult(
                payments,
                "Payments retrieved successfully"));
        }

        private static async Task<IResult> GetPaymentById(int paymentId, IPaymentService service)
        {
            if (paymentId <= 0)
                return Results.BadRequest(ApiResult<Payment>.FailureResult(
                    "Invalid payment ID. Must be greater than 0."));

            var payment = await service.GetPaymentByIdAsync(paymentId);
            if (payment == null)
                return Results.NotFound(ApiResult<Payment>.FailureResult(
                    $"Payment with ID {paymentId} not found."));

            return Results.Ok(ApiResult<Payment>.SuccessResult(
                payment,
                "Payment retrieved successfully"));
        }

        private static async Task<IResult> GetPaymentsByLicense(int licenseId, IPaymentService service)
        {
            if (licenseId <= 0)
                return Results.BadRequest(ApiResult<List<Payment>>.FailureResult(
                    "Invalid license ID. Must be greater than 0."));

            var payments = await service.GetPaymentsByLicenseAsync(licenseId);
            return Results.Ok(ApiResult<List<Payment>>.SuccessResult(
                payments,
                $"Payments for license {licenseId} retrieved successfully"));
        }

        private static async Task<IResult> GetPaymentsByStatus(string status, IPaymentService service)
        {
            if (string.IsNullOrWhiteSpace(status))
                return Results.BadRequest(ApiResult<List<Payment>>.FailureResult(
                    "Status is required."));

            try
            {
                var payments = await service.GetPaymentsByStatusAsync(status);
                return Results.Ok(ApiResult<List<Payment>>.SuccessResult(
                    payments,
                    $"Payments with status '{status}' retrieved successfully"));
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(ApiResult<List<Payment>>.FailureResult(ex.Message));
            }
        }

        private static async Task<IResult> GetPaymentSummary(IPaymentService service)
        {
            var summary = await service.GetPaymentSummaryAsync();
            return Results.Ok(ApiResult<PaymentSummary>.SuccessResult(
                summary,
                "Payment summary retrieved successfully"));
        }

        private static async Task<IResult> CreatePayment(Payment payment, IPaymentService service)
        {
            try
            {
                var createdPayment = await service.CreatePaymentAsync(payment);
                return Results.CreatedAtRoute(
                    "GetPaymentById",
                    new { paymentId = createdPayment.PaymentID },
                    ApiResult<Payment>.SuccessResult(
                        createdPayment,
                        "Payment created successfully"));
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(ApiResult<Payment>.FailureResult(ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ApiResult<Payment>.FailureResult(ex.Message));
            }
        }

        private static async Task<IResult> UpdatePaymentStatus(int paymentId, PaymentStatusUpdate update, IPaymentService service)
        {
            if (paymentId <= 0)
                return Results.BadRequest(ApiResult<Payment>.FailureResult(
                    "Invalid payment ID. Must be greater than 0."));

            try
            {
                var payment = await service.UpdatePaymentStatusAsync(paymentId, update.Status);
                return Results.Ok(ApiResult<Payment>.SuccessResult(
                    payment,
                    $"Payment status updated to {update.Status}"));
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(ApiResult<Payment>.FailureResult(ex.Message));
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound(ApiResult<Payment>.FailureResult(
                    $"Payment with ID {paymentId} not found."));
            }
        }

        private static async Task<IResult> RefundPayment(int paymentId, RefundRequest request, IPaymentService service)
        {
            if (paymentId <= 0)
                return Results.BadRequest(ApiResult<Payment>.FailureResult(
                    "Invalid payment ID. Must be greater than 0."));

            try
            {
                var payment = await service.RefundPaymentAsync(paymentId, request.Reason ?? "");
                return Results.Ok(ApiResult<Payment>.SuccessResult(
                    payment,
                    "Payment refunded successfully"));
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(ApiResult<Payment>.FailureResult(ex.Message));
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound(ApiResult<Payment>.FailureResult(
                    $"Payment with ID {paymentId} not found."));
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ApiResult<Payment>.FailureResult(ex.Message));
            }
        }

        private static async Task<IResult> DeletePayment(int paymentId, IPaymentService service)
        {
            if (paymentId <= 0)
                return Results.BadRequest(ApiResult<Payment>.FailureResult(
                    "Invalid payment ID. Must be greater than 0."));

            try
            {
                await service.DeletePaymentAsync(paymentId);
                return Results.NoContent();
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound(ApiResult<Payment>.FailureResult(
                    $"Payment with ID {paymentId} not found."));
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(ApiResult<Payment>.FailureResult(ex.Message));
            }
        }
    }

    // Request/Response DTOs
    public class PaymentStatusUpdate
    {
        public string Status { get; set; } = string.Empty;
    }

    public class RefundRequest
    {
        public string? Reason { get; set; }
    }
}