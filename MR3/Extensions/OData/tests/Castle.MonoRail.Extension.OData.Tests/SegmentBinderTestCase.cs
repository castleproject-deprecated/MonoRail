namespace Castle.MonoRail.Extension.OData.Tests
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel.DataAnnotations;
	using System.Linq;
	using NUnit.Framework;

	[TestFixture]
	public partial class SegmentBinderTestCase
	{
		private IQueryable<Catalog1> _catalog1Set;
		private IQueryable<Product1> _product1Set;
		private IQueryable<Supplier1> _supplier1Set;

		[SetUp]
		public void Init()
		{
			_product1Set = new List<Product1>
			               	{
			               		new Product1() { Id = 1, Name = "Product1" },
								new Product1() { Id = 2, Name = "Product2" },
			               	}.AsQueryable();
			_catalog1Set = new List<Catalog1>
			               	{
			               		new Catalog1() { Id = 1, Name = "Cat1"}, 
								new Catalog1() { Id = 2, Name = "Cat2" }
			               	}.AsQueryable();
			
			_supplier1Set = new List<Supplier1>
							{
								new Supplier1() { Id = 1, Address = new Address1() { Street = "wilson ave", Zip = "vxxxx", Country = "canada"} },
								new Supplier1() { Id = 2, Address = new Address1() { Street = "kingsway ave", Zip = "zxxxx", Country = "canada"} },
							}.AsQueryable();

			_catalog1Set.ElementAt(0).Products = new List<Product1>(_product1Set);
			_catalog1Set.ElementAt(1).Products = new List<Product1>(_product1Set);
			_product1Set.ElementAt(0).Catalog = _catalog1Set.ElementAt(0);
			_product1Set.ElementAt(1).Catalog = _catalog1Set.ElementAt(1);
		}


		[Test]
		public void aaaaaaaaaa3()
		{
			var model = new StubModel(
				m =>
				{
					m.EntitySet("catalogs", _catalog1Set);
					m.EntitySet("products", _product1Set);
					m.EntitySet("suppliers", _supplier1Set);
				});
			var segments = SegmentParser.parse("/catalogs(1)/Id", String.Empty, model);

			SegmentBinder.bind(segments, model);
		}

		[Test]
		public void aaaaaaaaaa3999()
		{
			var model = new StubModel(
				m =>
				{
					m.EntitySet("catalogs", _catalog1Set);
					m.EntitySet("products", _product1Set);
					m.EntitySet("suppliers", _supplier1Set);
				});
			var segments = SegmentParser.parse("/catalogs(1)/Id/", String.Empty, model);

			SegmentBinder.bind(segments, model);
		}

		[Test]
		public void aaaaaaaaaa4()
		{
			var model = new StubModel(
				m =>
				{
					m.EntitySet("catalogs", _catalog1Set);
					m.EntitySet("products", _product1Set);
					m.EntitySet("suppliers", _supplier1Set);
				});
			var segments = SegmentParser.parse("/catalogs(1)/Products", String.Empty, model);

			SegmentBinder.bind(segments, model);

			// assert last segment is list of products
		}

		[Test]
		public void aaaaaaaaaa4_()
		{
			var model = new StubModel(
				m =>
				{
					m.EntitySet("catalogs", _catalog1Set);
					m.EntitySet("products", _product1Set);
					m.EntitySet("suppliers", _supplier1Set);
				});
			var segments = SegmentParser.parse("/catalogs(1)/Products(1)", String.Empty, model);

			SegmentBinder.bind(segments, model);

			// assert last segment is single product
		}

		[Test]
		public void aaaaaaaaaa4__()
		{
			var model = new StubModel(
				m =>
				{
					m.EntitySet("catalogs", _catalog1Set);
					m.EntitySet("products", _product1Set);
					m.EntitySet("suppliers", _supplier1Set);
				});
			var segments = SegmentParser.parse("/catalogs(1)/Products(1)/Name", String.Empty, model);

			SegmentBinder.bind(segments, model);

			// assert last segment is product name
		}

		[Test]
		public void aaaaaaaaaa4aa__()
		{
			var model = new StubModel(
				m =>
				{
					m.EntitySet("catalogs", _catalog1Set);
					m.EntitySet("products", _product1Set);
					m.EntitySet("suppliers", _supplier1Set);
				});
			var segments = SegmentParser.parse("/catalogs(1)/Products(1)/Id", String.Empty, model);

			SegmentBinder.bind(segments, model);

			// assert last segment is product name
		}

		

		[Test]
		public void aaaaaaaaaa5()
		{
			var model = new StubModel(
				m =>
				{
					m.EntitySet("catalogs", _catalog1Set);
					m.EntitySet("products", _product1Set);
					m.EntitySet("suppliers", _supplier1Set);
				});
			var segments = SegmentParser.parse("/suppliers(1)/Address", String.Empty, model);

			SegmentBinder.bind(segments, model);

			// Assert last segment value is 'complex value with 3 nodes'
		}

		[Test]
		public void aaaaaaaaaa6()
		{
			var model = new StubModel(
				m =>
				{
					m.EntitySet("catalogs", _catalog1Set);
					m.EntitySet("products", _product1Set);
					m.EntitySet("suppliers", _supplier1Set);
				});
			var segments = SegmentParser.parse("/suppliers(1)/Address/Street", String.Empty, model);

			SegmentBinder.bind(segments, model);

			// Assert last segment value is ''
		}

		

		[Test]
		public void aaaaaaaaaa7()
		{
			var model = new StubModel(
				m =>
				{
					m.EntitySet("catalogs", _catalog1Set);
					m.EntitySet("products", _product1Set);
					m.EntitySet("suppliers", _supplier1Set);
				});
			var segments = SegmentParser.parse("/products(1)/Catalog/", String.Empty, model);

			SegmentBinder.bind(segments, model);

			// Assert last segment value is single catalog
		}

		[Test]
		public void aaaaaaaaaa8()
		{
			var model = new StubModel(
				m =>
				{
					m.EntitySet("catalogs", _catalog1Set);
					m.EntitySet("products", _product1Set);
					m.EntitySet("suppliers", _supplier1Set);
				});
			var segments = SegmentParser.parse("/products(1)/Catalog/Name", String.Empty, model);

			SegmentBinder.bind(segments, model);

			// Assert last segment value is single value = name
		}


		[Test]
		public void InvalidId_ForResourceMultiResult_()
		{
			var model = new StubModel(
				m =>
				{
					m.EntitySet("catalogs", _catalog1Set);
					m.EntitySet("products", _product1Set);
					m.EntitySet("suppliers", _supplier1Set);
				});
			var segments = SegmentParser.parse("/catalogs(1)/Products(10000)/", String.Empty, model);

			SegmentBinder.bind(segments, model);

			// assert last segment is product name
		}

		

		[Test]
		public void InvalidId_ForResourceMultiResultPlusPrimitiveProperty_()
		{
			var model = new StubModel(
				m =>
				{
					m.EntitySet("catalogs", _catalog1Set);
					m.EntitySet("products", _product1Set);
					m.EntitySet("suppliers", _supplier1Set);
				});
			var segments = SegmentParser.parse("/catalogs(1)/Products(10000)/Name", String.Empty, model);

			SegmentBinder.bind(segments, model);

			// assert last segment is product name
		}

		[Test]
		public void InvalidPropertyName_ForResourceSingleResult_()
		{
			var model = new StubModel(
				m =>
				{
					m.EntitySet("catalogs", _catalog1Set);
					m.EntitySet("products", _product1Set);
					m.EntitySet("suppliers", _supplier1Set);
				});
			var segments = SegmentParser.parse("/products(1)/Catalog/Invalid", String.Empty, model);

			SegmentBinder.bind(segments, model);

			// assert for 

//			<error xmlns="http://schemas.microsoft.com/ado/2007/08/dataservices/metadata">
//			<code></code>
//			<message xml:lang="en-US">Resource not found for the segment 'Invalid'.</message>
//			</error>
		}

		

		[Test]
		public void InvalidPropertyName_ForComplexType_()
		{
			var model = new StubModel(
				m =>
				{
					m.EntitySet("catalogs", _catalog1Set);
					m.EntitySet("products", _product1Set);
					m.EntitySet("suppliers", _supplier1Set);
				});
			var segments = SegmentParser.parse("/suppliers(1)/Address/Invalid", String.Empty, model);

			SegmentBinder.bind(segments, model);

			// assert for 

//			<error xmlns="http://schemas.microsoft.com/ado/2007/08/dataservices/metadata">
//			<code></code>
//			<message xml:lang="en-US">Resource not found for the segment 'Invalid'.</message>
//			</error>
		}

		// -------------------------------------

		public class Address1
		{
			public string Street { get; set; }
			public string Zip { get; set; }
			public string Country { get; set; }
		}

		public class Supplier1
		{
			[Key]
			public int Id { get; set; }
			public Address1 Address { get; set; }
		}

		public class Catalog1
		{
			[Key]
			public int Id { get; set; }
			public string Name { get; set; }
			public IList<Product1> Products { get; set; }
		}
		public class Product1
		{
			[Key]
			public int Id { get; set; }
			public string Name { get; set; }
			public Catalog1 Catalog { get; set; }
		}
	}
}
