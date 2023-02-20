using Demo.Products.Api.Models;
using Demo.Products.Api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Demo.Products.Api.Services;

public interface IProductsService
{
    Task<IEnumerable<ProductResponse?>> GetProducts();
    Task<ProductResponse?> GetProductById(int id);
    Task<ProductResponse?> CreateProduct(ProductRequest request);
    Task<ProductResponse?> UpdateProduct(int id, ProductRequest request);
    Task DeleteProduct(int id);
}
public class ProductsService : IProductsService
{
    private readonly ApiDbContext dbContext;

    public ProductsService(ApiDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<ProductResponse?> CreateProduct(ProductRequest request)
    {
        var product = new Product()
        {
            Name = request.Name,
            Surname = request.Surname,
            Email = request.Email,
            DateOfBirth = request.DateOfBirth
        };

        _ = await dbContext.Set<Product>().AddAsync(product);
        await dbContext.SaveChangesAsync(); 
        return new ProductResponse(product.Id, product.Name, product.Surname, product.Email, product.DateOfBirth);
    }

    public async Task DeleteProduct(int id)
    {
        var product = await dbContext.Set<Product>().FindAsync(id);
        if (product is not null)
        {
            dbContext.Set<Product>().Remove(product);
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task<ProductResponse?> GetProductById(int id)
    {
        var product = await dbContext.Set<Product>().FindAsync(id);
        if (product is null)
        {
            return default;            
        }

        return new ProductResponse(product.Id, product.Name, product.Surname, product.Email, product.DateOfBirth);
    }

    public async Task<IEnumerable<ProductResponse?>> GetProducts()
    {
        var products = await dbContext.Set<Product>().AsNoTracking().ToListAsync();

        return FetchItems(products);

        static IEnumerable<ProductResponse> FetchItems(IEnumerable<Product> products)
        {
            var items = new List<ProductResponse>();
            foreach (var product in products)
            {
                items.Add(new ProductResponse(product.Id, product.Name, product.Surname, product.Email, product.DateOfBirth));
            }

            return items;
        }
    }

    public async Task<ProductResponse?> UpdateProduct(int id, ProductRequest request)
    {
        var product = await dbContext.Set<Product>().FindAsync(id);
        if (product is null)
        {
            return default;
        }
        product.Name = request.Name;
        product.Surname = request.Surname;
        product.Email = request.Email;
        product.DateOfBirth = request.DateOfBirth;

        dbContext.Set<Product>().Update(product);
        await dbContext.SaveChangesAsync();
        return new ProductResponse(product.Id, product.Name, product.Surname, product.Email, product.DateOfBirth);
    }
}
