using Demo.Products.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Demo.Products.Api.Persistence;

public class ApiDbContext : DbContext
{
    public DbSet<Product>? Customers { get; set; }

    public ApiDbContext(DbContextOptions<ApiDbContext> options)
        : base(options)
    { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
            entity.Property(p => p.Id).ValueGeneratedOnAdd().IsRequired()
        );
    }
}