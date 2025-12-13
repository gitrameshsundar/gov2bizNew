using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<CustomerDbContext>(options =>
    options.UseSqlServer(connectionString));


var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// CRUD Endpoints for Customers (Authorization Required)
var customerGroup = app.MapGroup("/api/customers")
    .WithName("Customers");
    //.RequireAuthorization();

// GET all customers
customerGroup.MapGet("/", GetAllCustomers)
    .WithName("GetAllCustomers")
    .WithSummary("Get all customers")
    .Produces<List<Customer>>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status401Unauthorized);

// GET customer by ID
customerGroup.MapGet("/{customerId}", GetCustomerById)
    .WithName("GetCustomerById")
    .WithSummary("Get customer by ID")
    .Produces<Customer>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound)
    .Produces(StatusCodes.Status401Unauthorized);

// POST create customer
customerGroup.MapPost("/", CreateCustomer)
    .WithName("CreateCustomer")
    .WithSummary("Create a new customer")
    .Accepts<Customer>("application/json")
    .Produces<Customer>(StatusCodes.Status201Created)
    .Produces(StatusCodes.Status400BadRequest)
    .Produces(StatusCodes.Status401Unauthorized);

// PUT update customer
customerGroup.MapPut("/{customerId}", UpdateCustomer)
    .WithName("UpdateCustomer")
    .WithSummary("Update an existing customer")
    .Accepts<Customer>("application/json")
    .Produces<Customer>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status400BadRequest)
    .Produces(StatusCodes.Status404NotFound)
    .Produces(StatusCodes.Status401Unauthorized);

// DELETE customer
customerGroup.MapDelete("/{customerId}", DeleteCustomer)
    .WithName("DeleteCustomer")
    .WithSummary("Delete a customer")
    .Produces(StatusCodes.Status204NoContent)
    .Produces(StatusCodes.Status404NotFound)
    .Produces(StatusCodes.Status401Unauthorized);

app.Run();



// Endpoint handlers
async Task<IResult> GetAllCustomers(CustomerDbContext db)
{
    var customers = await db.Customers.ToListAsync();
    return Results.Ok(customers);
}

async Task<IResult> GetCustomerById(int customerId, CustomerDbContext db)
{
    var customer = await db.Customers.FirstOrDefaultAsync(c => c.CustomerID == customerId);
    if (customer == null)
        return Results.NotFound(new { message = "Customer not found" });

    return Results.Ok(customer);
}

async Task<IResult> CreateCustomer(Customer customer, CustomerDbContext db)
{
    if (string.IsNullOrWhiteSpace(customer.Name))
        return Results.BadRequest(new { message = "Customer name is required" });

    db.Customers.Add(customer);
    await db.SaveChangesAsync();
    return Results.CreatedAtRoute("GetCustomerById", new { customerId = customer.CustomerID }, customer);
}

async Task<IResult> UpdateCustomer(int customerId, Customer customerUpdate, CustomerDbContext db)
{
    if (string.IsNullOrWhiteSpace(customerUpdate.Name))
        return Results.BadRequest(new { message = "Customer name is required" });

    var customer = await db.Customers.FirstOrDefaultAsync(c => c.CustomerID == customerId);
    if (customer == null)
        return Results.NotFound(new { message = "Customer not found" });

    customer.Name = customerUpdate.Name;
    await db.SaveChangesAsync();
    return Results.Ok(customer);
}

async Task<IResult> DeleteCustomer(int customerId, CustomerDbContext db)
{
    var customer = await db.Customers.FirstOrDefaultAsync(c => c.CustomerID == customerId);
    if (customer == null)
        return Results.NotFound(new { message = "Customer not found" });

    db.Customers.Remove(customer);
    await db.SaveChangesAsync();
    return Results.NoContent();
}

// DbContext
class CustomerDbContext : DbContext
{
    public CustomerDbContext(DbContextOptions<CustomerDbContext> options) : base(options) { }

    public DbSet<Customer> Customers { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerID);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
        });
    }
}

// Models
class Customer
{
    public int CustomerID { get; set; }
    public string Name { get; set; } = string.Empty;
}

