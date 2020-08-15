using System;

namespace AspNetCoreMvcClient.Data
{
    public class AuthenticationTicket
    {
        public Guid Id { get; set; }

        public string UserId { get; set; }

        public byte[] Value { get; set; }

        public DateTimeOffset? LastActivity { get; set; }

        public DateTimeOffset? Expires { get; set; }
    }
}
