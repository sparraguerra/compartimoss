using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AspNetCoreMvcClient.Data
{
    public class DataProtectionKeysContext : DbContext, IDataProtectionKeyContext
    { 
        public DataProtectionKeysContext(DbContextOptions<DataProtectionKeysContext> options)
            : base(options) { } 
        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
        public DbSet<AuthenticationTicket> AuthenticationTickets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AuthenticationTicket>().ToTable("AuthenticationTicket").HasKey(t => new { t.Id });

            base.OnModelCreating(modelBuilder);
        }
    }
}
