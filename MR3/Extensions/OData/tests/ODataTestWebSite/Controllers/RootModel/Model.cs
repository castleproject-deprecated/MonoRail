namespace ODataTestWebSite.Controllers.AggRootModel
{
	using System.Collections.Generic;
	using System.ComponentModel.DataAnnotations;

	public class Revision
	{
		[Key]
		public int Id { get; set; }
		public string FileName { get; set; }
		public int UserId { get; set; }
	}

	public class Branch
	{
		[Key]
		public int Id { get; set; }
		public string Name { get; set; }
		public IList<Revision> Revisions { get; set; }
	}

    public enum RepositoryKind
    {
        Unset,
        Private,
        Public
    }

	public class Repository
	{
		[Key]
		public int Id { get; set; }
		public string Name { get; set; }
        public RepositoryKind Kind { get; set; }
        public Address Add { get; set; }
        public IList<Address> Addresses { get; set; } 
		public IList<Branch> Branches { get; set; }
	}

    public class Address
    {
        public string City { get; set; }
        public string State { get; set; }
    }
}