namespace TestSiteARSupport.Model
{
	using Castle.ActiveRecord;

	[ActiveRecord]
	public class Customer : ActiveRecordBase<Customer>
	{
		[PrimaryKey]
		public int Id { get; set; }

		[Property]
		public string Name { get; set; }

		[Nested]
		public Address HomeAddress { get; set; }
	}

	public class Address
	{
		[Property]
		public string Street { get; set; }
	}
}
