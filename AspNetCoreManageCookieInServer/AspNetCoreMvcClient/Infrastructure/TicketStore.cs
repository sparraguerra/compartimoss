using AspNetCoreMvcClient.Data;
using AspNetCoreMvcClient.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Security.Claims;
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

        public TicketStore(IServiceCollection services)
        {
            this.services = services;
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
                    authenticationTicket.Value = SerializeToBytes(ticket);
                    authenticationTicket.LastActivity = DateTimeOffset.UtcNow;
                    authenticationTicket.Expires = ticket.Properties.ExpiresUtc;
                    await context.SaveChangesAsync();
                }
            }
        }

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

                    return DeserializeFromBytes(authenticationTicket.Value);
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
            const string userAgentHeader = "user-agent";

            using var scope = services.BuildServiceProvider().CreateScope();
            var userId = ticket.Principal.FindFirst(t => t.Type == principalEmailType)?.Value;
            var context = scope.ServiceProvider.GetService<DataProtectionKeysContext>();

            var authenticationTicket = new AspNetCoreMvcClient.Data.AuthenticationTicket()
            {
                UserId = userId,
                LastActivity = DateTimeOffset.UtcNow,
                Value = SerializeToBytes(ticket),
                Expires = ticket.Properties.ExpiresUtc
            };

            var httpContextAccessor = scope.ServiceProvider.GetService<IHttpContextAccessor>();
            var httpContext = httpContextAccessor?.HttpContext;
            if (httpContext != null)
            {
                var remoteIpAddress = httpContext.Request.GetClientIP();
                if (remoteIpAddress != null)
                {
                    authenticationTicket.RemoteIpAddress = remoteIpAddress;
                }

                var userAgent = httpContext.Request.Headers?.FirstOrDefault(s => s.Key.ToLower() == userAgentHeader).Value;
                if (!string.IsNullOrWhiteSpace(userAgent))
                {
                    var uaParser = UAParser.Parser.GetDefault();
                    var clientInfo = uaParser.Parse(userAgent);
                    authenticationTicket.OperatingSystem = clientInfo.OS.ToString();
                    authenticationTicket.UserAgentFamily = clientInfo.UA.Family;
                    authenticationTicket.UserAgentVersion = $"{clientInfo.UA.Major}.{clientInfo.UA.Minor}.{clientInfo.UA.Patch}";
                }
            }

            context.AuthenticationTickets.Add(authenticationTicket);
            await context.SaveChangesAsync();

            return authenticationTicket.Id.ToString();
        }

        private byte[] SerializeToBytes(AuthenticationTicket source) => TicketSerializer.Default.Serialize(source);

        private AuthenticationTicket DeserializeFromBytes(byte[] source) => source == null ? null : TicketSerializer.Default.Deserialize(source);
    }
}