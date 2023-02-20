using Demo.Customers.Api.Models;
using Demo.Customers.Api.Persistence;
using Demo.Customers.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDaprClient();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ApiDbContext>(options => options.UseInMemoryDatabase("api"));
builder.Services.AddScoped<ICustomersService, CustomersService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/api/customers", async (ICustomersService customerService) =>
       await customerService.GetCustomers())
.Produces<IEnumerable<CustomerResponse>>(StatusCodes.Status200OK)
.WithName("GetCustomers");

app.MapGet("/api/customers/{id}", async (int id, ICustomersService customerService) =>
{
    var customer = await customerService.GetCustomerById(id);
    if (customer is null) return Results.NotFound();
    return Results.Ok(customer);
})
.Produces<CustomerResponse>(StatusCodes.Status200OK).Produces(StatusCodes.Status404NotFound)
.WithName("GetCustomerById");

app.MapPost("/api/customers", async (CustomerRequest request, ICustomersService customerService) => 
        await customerService.CreateCustomer(request))
.Produces<CustomerResponse>(StatusCodes.Status200OK)
.WithName("CreateCustomer");

app.MapPut("/api/customers/{id}", async (int id, CustomerRequest request, ICustomersService customerService) =>
{   
    var customer = await customerService.UpdateCustomer(id, request);
    if (customer is null) return Results.NotFound();
    return Results.Ok(customer);
})
.Produces<CustomerResponse>(StatusCodes.Status200OK).Produces(StatusCodes.Status404NotFound)
.WithName("UpdateCustomer");

app.MapDelete("/api/customers/{id}", async (int id, ICustomersService customerService) =>
{
     await customerService.DeleteCustomer(id);
     return Results.Ok();
})
.Produces<CustomerResponse>(StatusCodes.Status200OK).Produces(StatusCodes.Status404NotFound)
.WithName("DeleteCustomer");

app.Run();
