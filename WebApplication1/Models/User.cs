namespace WebApplication1.Models
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    public class Permission
    {
        public int Id { get; set; }
    }

    public class City
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class Address
    {
        public string Add1 { get; set; }
        public string Add2 { get; set; }
        public City City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Zip { get; set; }
    }

    public class SiteUser
    {
        public int Id { get; set; }
        [DefaultValue("enter your name")]
        public string Name { get; set; }
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        public Address Address { get; set; }

        public List<Permission> Permissions { get; set; }

    }
}