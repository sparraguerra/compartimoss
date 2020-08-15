using AspNetCoreMvcClient.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using AuthenticationTicket = Microsoft.AspNetCore.Authentication.AuthenticationTicket;

namespace AspNetCoreMvcClient.Infraestructure
{
    /// <summary>
    /// TicketStore
    /// </summary>
    public class TicketStore : ITicketStore
    {
        private readonly IServiceCollection services;
        private readonly IDataProtector dataProtector;

        public TicketStore(IServiceCollection services, IDataProtectionProvider dataProtectionProvider)
        {
            this.services = services;
            this.dataProtector = dataProtectionProvider.CreateProtector(GetType().FullName);
        }

        /// <summary>
        /// RemoveAsync
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task RemoveAsync(string key)
        {
            using var scope = services.BuildServiceProvider().CreateScope();
            var context = scope.ServiceProvider.GetService<DataProtectionKeysContext>();
            if (Guid.TryParse(key, out var id))
            {
                var authenticationTicket = await context.AuthenticationTickets.SingleOrDefaultAsync(x => x.Id == id);
                if (authenticationTicket != null)
                {
                    context.AuthenticationTickets.Remove(authenticationTicket);
                    await context.SaveChangesAsync();
                }
            }
        }

        /// <summary>
        /// RenewAsync
        /// </summary>
        /// <param name="key"></param>
        /// <param name="ticket"></param>
        /// <returns></returns>
        public async Task RenewAsync(string key, AuthenticationTicket ticket)
        {
            using var scope = services.BuildServiceProvider().CreateScope();
            var context = scope.ServiceProvider.GetService<DataProtectionKeysContext>();
            if (Guid.TryParse(key, out var id))
            {
                var authenticationTicket = await context.AuthenticationTickets.FindAsync(id);
                if (authenticationTicket != null)
                {
                    authenticationTicket.Value = dataProtector.Protect(SerializeToBytes(ticket));
                    authenticationTicket.LastActivity = DateTimeOffset.UtcNow;
                    authenticationTicket.Expires = ticket.Properties.ExpiresUtc;
                    await context.SaveChangesAsync();
                }
            }
        }

        /// <summary>
        /// RetrieveAsync
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<AuthenticationTicket> RetrieveAsync(string key)
        {
            using var scope = services.BuildServiceProvider().CreateScope();
            var context = scope.ServiceProvider.GetService<DataProtectionKeysContext>();
            if (Guid.TryParse(key, out var id))
            {
                var authenticationTicket = await context.AuthenticationTickets.FindAsync(id);
                if (authenticationTicket != null)
                {
                    authenticationTicket.LastActivity = DateTimeOffset.UtcNow;
                    await context.SaveChangesAsync();

                    return DeserializeFromBytes(dataProtector.Unprotect(authenticationTicket.Value));
                }
            }

            return null;
        }

        /// <summary>
        /// StoreAsync
        /// </summary>
        /// <param name="ticket"></param>
        /// <returns></returns>
        public async Task<string> StoreAsync(AuthenticationTicket ticket)
        {
            const string principalEmailType = "email";

            using var scope = services.BuildServiceProvider().CreateScope();
            var userId = ticket.Principal.FindFirst(t => t.Type == principalEmailType)?.Value;
            var context = scope.ServiceProvider.GetService<DataProtectionKeysContext>();

            var authenticationTicket = new AspNetCoreMvcClient.Data.AuthenticationTicket()
            {
                UserId = userId,
                LastActivity = DateTimeOffset.UtcNow,
                Value = dataProtector.Protect(SerializeToBytes(ticket)),
                Expires = ticket.Properties.ExpiresUtc
            };

            context.AuthenticationTickets.Add(authenticationTicket);
            await context.SaveChangesAsync();

            return authenticationTicket.Id.ToString();
        }

        private byte[] SerializeToBytes(AuthenticationTicket source) => TicketSerializer.Default.Serialize(source);

        private AuthenticationTicket DeserializeFromBytes(byte[] source) => source == null ? null : TicketSerializer.Default.Deserialize(source);
    }
}