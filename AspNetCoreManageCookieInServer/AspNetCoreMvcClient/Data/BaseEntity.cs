using System;

namespace AspNetCoreMvcClient.Data
{
    public abstract  class BaseEntity
    {
        public virtual Guid Id { get; set; }
    }
}
