namespace Castle.MonoRail.Extension.OData.Tests
{
	using System;
	using NUnit.Framework;

	public partial class SegmentBinderTestCase
	{
		[Test]
		public void aaaaaaaaaa()
		{
			var model = new StubModel(
				m =>
				{
					m.EntitySet("catalogs", _catalog1Set);
					m.EntitySet("products", _product1Set);
					m.EntitySet("suppliers", _supplier1Set);
				});
			var segments = SegmentParser.parse("/catalogs/", String.Empty, model);

			SegmentBinder.bind(segments, model);
		}

		[Test]
		public void aaaaaaaaaa2()
		{
			var model = new StubModel(
				m =>
				{
					m.EntitySet("catalogs", _catalog1Set);
					m.EntitySet("products", _product1Set);
					m.EntitySet("suppliers", _supplier1Set);
				});
			var segments = SegmentParser.parse("/catalogs(1)/", String.Empty, model);

			SegmentBinder.bind(segments, model);
		}

		[Test]
		public void aaaaaaaaaa12()
		{
			var model = new StubModel(
				m =>
				{
					m.EntitySet("catalogs", _catalog1Set);
					m.EntitySet("products", _product1Set);
					m.EntitySet("suppliers", _supplier1Set);
				});
			var segments = SegmentParser.parse("/catalogs(1)", String.Empty, model);

			SegmentBinder.bind(segments, model);
		}

		[Test]
		public void InvalidId_ForEntityType_()
		{
			var model = new StubModel(
				m =>
				{
					m.EntitySet("catalogs", _catalog1Set);
					m.EntitySet("products", _product1Set);
					m.EntitySet("suppliers", _supplier1Set);
				});
			var segments = SegmentParser.parse("/catalogs(1000)/", String.Empty, model);

			SegmentBinder.bind(segments, model);
		}
	}
}
