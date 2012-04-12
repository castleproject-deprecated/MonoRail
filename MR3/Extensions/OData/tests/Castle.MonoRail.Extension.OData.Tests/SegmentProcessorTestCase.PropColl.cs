namespace Castle.MonoRail.Extension.OData.Tests
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel.DataAnnotations;
	using System.Data.Services.Providers;
	using System.IO;
	using System.Linq;
	using System.ServiceModel.Syndication;
	using System.Text;
	using System.Xml;
	using FluentAssertions;
	using NUnit.Framework;

	public partial class SegmentProcessorTestCase
	{
		// naming convention for testing methods
		// [EntitySet|EntityType|PropSingle|PropCollection|Complex|Primitive]_[Operation]_[InputFormat]_[OutputFormat]__[Success|Failure]

		[Test]
		public void PropCollection_View_Atom_Atom_Success()
		{
			Process("/catalogs(1)/Products/", SegmentOp.View, _model);

			Console.WriteLine(_body.ToString());
			var feed = SyndicationFeed.Load(XmlReader.Create(new StringReader(_body.ToString())));
			feed.Should().NotBeNull();

			feed.Id.Should().BeEquivalentTo("http://localhost/base/catalogs(1)/products");
		}


	}
}
