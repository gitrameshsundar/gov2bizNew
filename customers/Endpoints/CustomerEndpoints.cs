using LicenseManagement.Data.Models;
using LicenseManagement.Data.Results;
using LicenseManagement.Data.Services;

namespace customers.Endpoints
{
    /// <summary>
    /// Endpoint handlers for Customer API operations.
    /// 
    /// DESIGN PATTERN: Endpoint Organization Pattern (Minimal APIs)
    /// PURPOSE: Organizes all customer-related HTTP endpoint handlers in a single class.
    /// 
    /// BENEFITS:
    /// - Keeps Program.cs clean and focused on configuration
    /// - Groups related endpoints together
    /// - Easy to test endpoint logic
    /// - Follows Single Responsibility Principle
    /// - Better code organization and maintainability
    /// - Scalable architecture
    /// 
    /// BEST PRACTICES:
    /// 1. Static class with extension methods for registering routes
    /// 2. All endpoint handlers as static methods
    /// 3. Dependency injection through method parameters
    /// 4. Clear method naming following HTTP verbs
    /// 5. Comprehensive error handling
    /// 6. Consistent response format using ApiResult<T>
    /// 
    /// USAGE: In Program.cs:
    /// app.MapCustomerEndpoints();
    /// </summary>
    public static class CustomerEndpoints
    {
        /// <summary>
        /// Registers all customer-related endpoints to the application.
        /// 
        /// EXECUTION FLOW:
        /// 1. Creates a route group for /api/customers
        /// 2. Maps all CRUD operations (GET, POST, PUT, DELETE)
        /// 3. Configures endpoint metadata (name, summary, responses)
        /// 4. Returns the application for fluent chaining
        /// 
        /// CLEAN ARCHITECTURE PRINCIPLE:
        /// This extension method keeps Program.cs readable while maintaining
        /// all customer endpoint configuration in one logical place.
        /// </summary>
        /// <param name="app">The WebApplication instance</param>
        /// <returns>The WebApplication for method chaining</returns>
        public static WebApplication MapCustomerEndpoints(this WebApplication app)
        {
            // Create a route group for all customer endpoints
            // GROUP BENEFITS:
            // - Centralized route prefix (/api/customers)
            // - Reusable endpoint configuration
            // - Easier to apply shared policies (authorization, etc.)
            var group = app.MapGroup("/api/customers")
                .WithName("Customers")
                .WithOpenApi();  // Auto-generate OpenAPI documentation

            // GET /api/customers   
            group.MapGet("/", GetAllCustomers)
                .WithName("GetAllCustomers")
                .WithSummary("Get all customers")
                .WithDescription("Retrieves a list of all customers in the system")
                .Produces<ApiResult<List<Customer>>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status500InternalServerError)
                .WithOpenApi();

            // GET /api/customers/{customerId}
            group.MapGet("/{customerId}", GetCustomerById)
                .WithName("GetCustomerById")
                .WithSummary("Get customer by ID")
                .WithDescription("Retrieves a specific customer by their ID")
                .Produces<ApiResult<Customer>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status400BadRequest)
                .WithOpenApi();

            // POST /api/customers
            group.MapPost("/", CreateCustomer)
                .WithName("CreateCustomer")
                .WithSummary("Create a new customer")
                .WithDescription("Creates a new customer record in the system")
                .Accepts<Customer>("application/json")
                .Produces<ApiResult<Customer>>(StatusCodes.Status201Created)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status500InternalServerError)
                .WithOpenApi();

            // PUT /api/customers/{customerId}
            group.MapPut("/{customerId}", UpdateCustomer)
                .WithName("UpdateCustomer")
                .WithSummary("Update an existing customer")
                .WithDescription("Updates customer information by ID")
                .Accepts<Customer>("application/json")
                .Produces<ApiResult<Customer>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status500InternalServerError)
                .WithOpenApi();

            // DELETE /api/customers/{customerId}
            group.MapDelete("/{customerId}", DeleteCustomer)
                .WithName("DeleteCustomer")
                .WithSummary("Delete a customer")
                .WithDescription("Removes a customer record from the system")
                .Produces<ApiResponse>(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status500InternalServerError)
                .WithOpenApi();

            return app;
        }

        /// <summary>
        /// Handles GET /api/customers - Retrieve all customers.
        /// 
        /// HTTP METHOD: GET (Safe, Idempotent)
        /// RESPONSE CODE: 200 OK
        /// PARAMETERS: None
        /// 
        /// BEST PRACTICES:
        /// - Service injection via parameter
        /// - Consistent error handling
        /// - Always wrap response in ApiResult<T>
        /// - Exceptions handled by middleware
        /// </summary>
        /// <param name="service">The customer service (injected)</param>
        /// <returns>ApiResult containing list of customers or error details</returns>
        private static async Task<IResult> GetAllCustomers(ICustomerService service)
        {
            var customers = await service.GetAllCustomersAsync();
            return Results.Ok(ApiResult<List<Customer>>.SuccessResult(
                customers,
                "Customers retrieved successfully"));
        }

        /// <summary>
        /// Handles GET /api/customers/{customerId} - Retrieve a specific customer.
        /// 
        /// HTTP METHOD: GET (Safe, Idempotent)
        /// RESPONSE CODE: 200 OK on success, 404 Not Found if customer doesn't exist
        /// PARAMETERS: customerId (int) - The customer ID
        /// 
        /// ERROR CASES:
        /// - 400 Bad Request: Invalid customer ID format
        /// - 404 Not Found: Customer with given ID doesn't exist
        /// - 500 Internal Server Error: Unexpected server error
        /// </summary>
        /// <param name="customerId">The ID of the customer to retrieve</param>
        /// <param name="service">The customer service (injected)</param>
        /// <returns>ApiResult containing the customer or error details</returns>
        private static async Task<IResult> GetCustomerById(int customerId, ICustomerService service)
        {
            if (customerId <= 0)
                return Results.BadRequest(ApiResult<Customer>.FailureResult(
                    "Invalid customer ID. Must be greater than 0."));

            var customer = await service.GetCustomerByIdAsync(customerId);

            if (customer == null)
                return Results.NotFound(ApiResult<Customer>.FailureResult(
                    $"Customer with ID {customerId} not found."));

            return Results.Ok(ApiResult<Customer>.SuccessResult(
                customer,
                "Customer retrieved successfully"));
        }

        /// <summary>
        /// Handles POST /api/customers - Create a new customer.
        /// 
        /// HTTP METHOD: POST (Not Safe, Not Idempotent)
        /// RESPONSE CODE: 201 Created on success
        /// REQUEST BODY: Customer object with required fields
        /// 
        /// IMPORTANT: POST is not idempotent - multiple identical requests
        /// will create multiple resources. Consider:
        /// - Providing idempotency key support
        /// - Implementing duplicate detection
        /// - Using database constraints
        /// 
        /// ERROR CASES:
        /// - 400 Bad Request: Invalid or missing required fields
        /// - 409 Conflict: Resource already exists (e.g., duplicate name)
        /// - 500 Internal Server Error: Database error
        /// </summary>
        /// <param name="customer">The customer object to create</param>
        /// <param name="service">The customer service (injected)</param>
        /// <returns>201 Created with the new customer or error details</returns>
        private static async Task<IResult> CreateCustomer(Customer customer, ICustomerService service)
        {
            var createdCustomer = await service.CreateCustomerAsync(customer);

            return Results.CreatedAtRoute(
                "GetCustomerById",
                new { customerId = createdCustomer.CustomerID },
                ApiResult<Customer>.SuccessResult(
                    createdCustomer,
                    "Customer created successfully"));
        }

        /// <summary>
        /// Handles PUT /api/customers/{customerId} - Update an existing customer.
        /// 
        /// HTTP METHOD: PUT (Not Safe, Idempotent)
        /// RESPONSE CODE: 200 OK on success
        /// PARAMETERS: customerId (int) - The customer ID
        /// REQUEST BODY: Customer object with updated fields
        /// 
        /// IDEMPOTENCY NOTE: PUT is idempotent - calling it multiple times
        /// with the same data should produce the same result. No duplicate
        /// side effects occur.
        /// 
        /// ERROR CASES:
        /// - 400 Bad Request: Invalid customer ID or request body
        /// - 404 Not Found: Customer doesn't exist
        /// - 409 Conflict: Constraint violation (e.g., duplicate unique field)
        /// - 500 Internal Server Error: Database error
        /// </summary>
        /// <param name="customerId">The ID of the customer to update</param>
        /// <param name="customerUpdate">The updated customer data</param>
        /// <param name="service">The customer service (injected)</param>
        /// <returns>200 OK with updated customer or error details</returns>
        private static async Task<IResult> UpdateCustomer(int customerId, Customer customerUpdate, ICustomerService service)
        {
            if (customerId <= 0)
                return Results.BadRequest(ApiResult<Customer>.FailureResult(
                    "Invalid customer ID. Must be greater than 0."));

            var updatedCustomer = await service.UpdateCustomerAsync(customerId, customerUpdate);

            return Results.Ok(ApiResult<Customer>.SuccessResult(
                updatedCustomer,
                "Customer updated successfully"));
        }

        /// <summary>
        /// Handles DELETE /api/customers/{customerId} - Delete a customer.
        /// 
        /// HTTP METHOD: DELETE (Not Safe, Idempotent)
        /// RESPONSE CODE: 204 No Content on success
        /// PARAMETERS: customerId (int) - The customer ID
        /// 
        /// IDEMPOTENCY NOTE: DELETE is idempotent - calling it multiple times
        /// on the same resource is safe. The first call deletes it, subsequent
        /// calls return 404 (or 204 depending on implementation).
        /// 
        /// IMPORTANT CONSIDERATIONS:
        /// - Consider soft deletes (mark deleted_at) instead of hard deletes
        /// - Check for referential integrity (licenses, users linked to customer)
        /// - Consider audit logging for compliance
        /// - Consider approval workflows for sensitive deletions
        /// 
        /// ERROR CASES:
        /// - 400 Bad Request: Invalid customer ID
        /// - 404 Not Found: Customer doesn't exist
        /// - 409 Conflict: Cannot delete (foreign key constraints)
        /// - 500 Internal Server Error: Database error
        /// </summary>
        /// <param name="customerId">The ID of the customer to delete</param>
        /// <param name="service">The customer service (injected)</param>
        /// <returns>204 No Content on success or error details</returns>
        private static async Task<IResult> DeleteCustomer(int customerId, ICustomerService service)
        {
            if (customerId <= 0)
                return Results.BadRequest(ApiResult<Customer>.FailureResult(
                    "Invalid customer ID. Must be greater than 0."));

            await service.DeleteCustomerAsync(customerId);

            return Results.NoContent();
        }
    }
}