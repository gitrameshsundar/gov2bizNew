using Microsoft.EntityFrameworkCore;
using LicenseManagement.Data.Models;
using LicenseManagement.Data.Data;
using LicenseManagement.Data.Repositories;
using LicenseManagement.Data.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<CustomerDbContext>(options =>
    options.UseSqlServer(connectionString));

// Register Repository & Service
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ICustomerService, CustomerService>();

// Add logging
builder.Services.AddLogging();

    var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// CRUD Endpoints for Customers 
var customerGroup = app.MapGroup("/api/customers")
    .WithName("Customers");

// GET all customers
customerGroup.MapGet("/", GetAllCustomers)
    .WithName("GetAllCustomers")
    .WithSummary("Get all customers")
    .Produces<List<Customer>>(StatusCodes.Status200OK);

// GET customer by ID
customerGroup.MapGet("/{customerId}", GetCustomerById)
    .WithName("GetCustomerById")
    .WithSummary("Get customer by ID")
    .Produces<Customer>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound);

// POST create customer
customerGroup.MapPost("/", CreateCustomer)
    .WithName("CreateCustomer")
    .WithSummary("Create a new customer")
    .Accepts<Customer>("application/json")
    .Produces<Customer>(StatusCodes.Status201Created)
    .Produces(StatusCodes.Status400BadRequest);

// PUT update customer
customerGroup.MapPut("/{customerId}", UpdateCustomer)
    .WithName("UpdateCustomer")
    .WithSummary("Update an existing customer")
    .Accepts<Customer>("application/json")
    .Produces<Customer>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status400BadRequest)
    .Produces(StatusCodes.Status404NotFound);

// DELETE customer
customerGroup.MapDelete("/{customerId}", DeleteCustomer)
    .WithName("DeleteCustomer")
    .WithSummary("Delete a customer")
    .Produces(StatusCodes.Status204NoContent)
    .Produces(StatusCodes.Status404NotFound);

app.Run();



// Endpoint handlers (now using service layer)
async Task<IResult> GetAllCustomers(ICustomerService service)
{
    try
    {
        var customers = await service.GetAllCustomersAsync();
        return Results.Ok(customers);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message, statusCode: 500);
    }
}

async Task<IResult> GetCustomerById(int customerId, ICustomerService service)
{
    try
    {
        var customer = await service.GetCustomerByIdAsync(customerId);
        if (customer == null)
            return Results.NotFound(new { message = "Customer not found" });

        return Results.Ok(customer);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message, statusCode: 500);
    }
}

async Task<IResult> CreateCustomer(Customer customer, ICustomerService service)
{
    try
    {
        var createdCustomer = await service.CreateCustomerAsync(customer);
        return Results.CreatedAtRoute("GetCustomerById", new { customerId = createdCustomer.CustomerID }, createdCustomer);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message, statusCode: 500);
    }
}

async Task<IResult> UpdateCustomer(int customerId, Customer customerUpdate, ICustomerService service)
{
    try
    {
        var customer = await service.UpdateCustomerAsync(customerId, customerUpdate);
        return Results.Ok(customer);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(new { message = ex.Message });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message, statusCode: 500);
    }
}

async Task<IResult> DeleteCustomer(int customerId, ICustomerService service)
{
    try
    {
        await service.DeleteCustomerAsync(customerId);
        return Results.NoContent();
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
    catch (KeyNotFoundException ex)
    {
        return Results.NotFound(new { message = ex.Message });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message, statusCode: 500);
    }
}


