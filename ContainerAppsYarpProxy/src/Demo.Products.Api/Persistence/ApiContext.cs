using Demo.Products.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Demo.Products.Api.Persistence;

public class ApiContext : DbContext
{
    public DbSet<Product>? Customers { get; set; }

    public ApiContext(DbContextOptions<ApiContext> options)
        : base(options)
    { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
            entity.Property(p => p.Id).ValueGeneratedOnAdd().IsRequired()
        );
    }
}