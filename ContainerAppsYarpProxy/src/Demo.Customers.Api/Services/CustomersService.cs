using Demo.Customers.Api.Models;
using Demo.Customers.Api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Demo.Customers.Api.Services;

public interface ICustomersService
{
    Task<IEnumerable<CustomerResponse?>> GetCustomers();
    Task<CustomerResponse?> GetCustomerById(int id);
    Task<CustomerResponse?> CreateCustomer(CustomerRequest request);
    Task<CustomerResponse?> UpdateCustomer(int id, CustomerRequest request);
    Task DeleteCustomer(int id);
}
public class CustomersService : ICustomersService
{
    private readonly ApiDbContext dbContext;

    public CustomersService(ApiDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<CustomerResponse?> CreateCustomer(CustomerRequest request)
    {
        var customer = new Customer()
        {
            Name = request.Name,
            Surname = request.Surname,
            Email = request.Email,
            DateOfBirth = request.DateOfBirth
        };

        await dbContext.Set<Customer>().AddAsync(customer);
        await dbContext.SaveChangesAsync(); 
        return new CustomerResponse(customer.Id, customer.Name, customer.Surname, customer.Email, customer.DateOfBirth);
    }

    public async Task DeleteCustomer(int id)
    {
        var customer = await dbContext.Set<Customer>().FindAsync(id);
        if (customer is not null)
        {
            dbContext.Set<Customer>().Remove(customer);
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task<CustomerResponse?> GetCustomerById(int id)
    {
        var customer = await dbContext.Set<Customer>().FindAsync(id);
        if (customer is null)
        {
            return default;            
        }

        return new CustomerResponse(customer.Id, customer.Name, customer.Surname, customer.Email, customer.DateOfBirth);
    }

    public async Task<IEnumerable<CustomerResponse?>> GetCustomers()
    {
        var customers = await dbContext.Set<Customer>().AsNoTracking().ToListAsync();

        return FetchItems(customers);

        static IEnumerable<CustomerResponse> FetchItems(IEnumerable<Customer> customers)
        {
            var items = new List<CustomerResponse>();
            foreach (var customer in customers)
            {
                items.Add(new CustomerResponse(customer.Id, customer.Name, customer.Surname, customer.Email, customer.DateOfBirth));
            }

            return items;
        }
    }

    public async Task<CustomerResponse?> UpdateCustomer(int id, CustomerRequest request)
    {
        var customer = await dbContext.Set<Customer>().FindAsync(id);
        if (customer is null)
        {
            return default;
        }
        customer.Name = request.Name;
        customer.Surname = request.Surname;
        customer.Email = request.Email;
        customer.DateOfBirth = request.DateOfBirth;

        dbContext.Set<Customer>().Update(customer);
        await dbContext.SaveChangesAsync();
        return new CustomerResponse(customer.Id, customer.Name, customer.Surname, customer.Email, customer.DateOfBirth);
    }
}
