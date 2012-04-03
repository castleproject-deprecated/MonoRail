namespace Castle.MonoRail.Extension.OData.Tests
{
	using System;
	using NUnit.Framework;

	public partial class SegmentBinderTestCase
	{
		[Test]
		public void ValueOperation_ForKeyInResource()
		{
			var model = new StubModel(
				m =>
				{
					m.EntitySet("catalogs", _catalog1Set);
					m.EntitySet("products", _product1Set);
					m.EntitySet("suppliers", _supplier1Set);
				});
			var segments = SegmentParser.parse("/catalogs(1)/Products(1)/Id/$value", String.Empty, model);

			SegmentBinder.bind(segments, model);

			// assert last segment is product name in raw format
		}

		[Test]
		public void ValueOperation_ForPropOfComplexType()
		{
			var model = new StubModel(
				m =>
				{
					m.EntitySet("catalogs", _catalog1Set);
					m.EntitySet("products", _product1Set);
					m.EntitySet("suppliers", _supplier1Set);
				});
			var segments = SegmentParser.parse("/suppliers(1)/Address/Street/$value", String.Empty, model);

			SegmentBinder.bind(segments, model);

			// Assert last segment value is ''
		}

		[Test]
		public void ValueOperation_ForPropertyOfResource()
		{
			var model = new StubModel(
				m =>
				{
					m.EntitySet("catalogs", _catalog1Set);
					m.EntitySet("products", _product1Set);
					m.EntitySet("suppliers", _supplier1Set);
				});
			var segments = SegmentParser.parse("/products(1)/Catalog/Name/$value", String.Empty, model);

			SegmentBinder.bind(segments, model);

			// Assert last segment value is single value = name in raw format
		}

		[Test]
		public void InvalidValueOperation_ForEntityType_()
		{
			var model = new StubModel(
				m =>
				{
					m.EntitySet("catalogs", _catalog1Set);
					m.EntitySet("products", _product1Set);
					m.EntitySet("suppliers", _supplier1Set);
				});
			var segments = SegmentParser.parse("/catalogs(1)/$value", String.Empty, model);

			SegmentBinder.bind(segments, model);
		}

		[Test]
		public void InvalidValueOperation_ForResourceMultiResult_()
		{
			var model = new StubModel(
				m =>
				{
					m.EntitySet("catalogs", _catalog1Set);
					m.EntitySet("products", _product1Set);
					m.EntitySet("suppliers", _supplier1Set);
				});
			var segments = SegmentParser.parse("/catalogs(1)/Products(1)/$value", String.Empty, model);

			SegmentBinder.bind(segments, model);

			// assert last segment is product name
		}

		[Test]
		public void InvalidValueOperation_ForResourceSingleResult_()
		{
			var model = new StubModel(
				m =>
				{
					m.EntitySet("catalogs", _catalog1Set);
					m.EntitySet("products", _product1Set);
					m.EntitySet("suppliers", _supplier1Set);
				});
			var segments = SegmentParser.parse("/products(1)/Catalog/$value", String.Empty, model);

			SegmentBinder.bind(segments, model);

			// assert for 

			//			<error xmlns="http://schemas.microsoft.com/ado/2007/08/dataservices/metadata">
			//			<code></code>
			//			<message xml:lang="en-US">Resource not found for the segment 'Invalid'.</message>
			//			</error>
		}
	}
}
