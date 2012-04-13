namespace Castle.MonoRail.Extension.OData.Tests
{
	using FluentAssertions;
	using NUnit.Framework;

	public partial class SegmentProcessorTestCase
	{
		// naming convention for testing methods
		// [EntitySet|EntityType|PropSingle|PropCollection|Complex|Primitive]_[Operation]_[InputFormat]_[OutputFormat]__[Success|Failure]

		[Test]
		public void EntityType_PropertySingle_View_None_Xml__Success()
		{
			Process("/catalogs(1)/Id", SegmentOp.View, _model, accept: "application/xml");

			_response.contentType.Should().Be("application/xml");
			_body.Should().Be(
@"<?xml version=""1.0"" encoding=""utf-8"" standalone=""yes""?>
<Id p1:type=""Edm.Int32"" xmlns:p1=""http://schemas.microsoft.com/ado/2007/08/dataservices"">1</Id>");
		}

		[Test]
		public void EntityType_PropertySingle_View_None_Xml__Success_2()
		{
			Process("/catalogs(1)/Name", SegmentOp.View, _model, accept: "application/xml");

			_response.contentType.Should().Be("application/xml");
			_body.Should().Be(
@"<?xml version=""1.0"" encoding=""utf-8"" standalone=""yes""?>
<Name xmlns=""http://schemas.microsoft.com/ado/2007/08/dataservices"">name</Name>");
		}

		
	}
}
