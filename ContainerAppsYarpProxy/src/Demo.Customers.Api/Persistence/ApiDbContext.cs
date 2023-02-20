using Demo.Customers.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Demo.Customers.Api.Persistence;

public class ApiDbContext : DbContext
{
    public DbSet<Customer>? Customers { get; set; }

    public ApiDbContext(DbContextOptions<ApiDbContext> options)
        : base(options)
    { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
            entity.Property(p => p.Id).ValueGeneratedOnAdd().IsRequired()
        );
    }
}