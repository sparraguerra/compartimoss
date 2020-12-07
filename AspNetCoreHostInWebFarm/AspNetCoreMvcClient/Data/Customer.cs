using System;

namespace AspNetCoreMvcClient.Data
{
    public class Customer : BaseEntity
    {        
        public string Name { get; set; }
        public string Surname { get; set; } 
        public DateTime BirthDay { get; set; }
    }
}
