namespace Castle.MonoRail.Extension.OData.Tests
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel.DataAnnotations;
	using System.Data.Services.Providers;
	using System.IO;
	using System.Linq;
	using System.Text;
	using FluentAssertions;
	using NUnit.Framework;

	public partial class SegmentProcessorTestCase
	{
		// naming convention for testing methods
		// [EntitySet|EntityType|PropSingle|PropCollection|Complex|Primitive]_[Operation]_[InputFormat]_[OutputFormat]__[Success|Failure]

		[Test]
		public void EntityType_PropertySingle_View_Atom_Xml__Success()
		{
			Process("/catalogs(1)/Id", SegmentOp.View, _model, accept: "application/xml");

			_response.contentType.Should().Be("application/xml");
			_body.Should().Be("");

			Process("/catalogs(1)/Id/", SegmentOp.View, _model, accept: "application/xml");
		}


	}
}
