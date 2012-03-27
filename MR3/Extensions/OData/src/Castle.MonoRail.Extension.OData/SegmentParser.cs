namespace Castle.MonoRail.Extension.OData
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Web;

	public enum SegmentKind
	{
		Undefined,
		ServiceDirectory,           
		Resource,
		ComplexObject,
		Primitive,
		PrimitiveValue,
		Metadata,     
		VoidServiceOperation,
		Batch,
		Link,
		OpenProperty,
		OpenPropertyValue,
		MediaResource,
	}

	public class Segment
	{
		public string Identifier { get; private set; }
		public SegmentKind Kind { get; set; }

		public Segment(string identifier)
		{
			Identifier = identifier;
		}
	}

	/// <summary>
	/// This class actually does more than parsing. it also binds the segments
	/// to their corresponding tokens
	/// </summary>
	public class SegmentParser
	{
		public IEnumerable<Segment> ParseAndBind(string path, ODataModel model)
		{
			if (path == null) throw new ArgumentNullException("path");
			if (model == null) throw new ArgumentNullException("model");

			var segments = path.Split('/').Select(p => new Segment(p)).ToArray();

			if (segments.Length == 0 || segments[0].Identifier == "/")
			{
				return new[] { new Segment("/") { Kind = SegmentKind.ServiceDirectory } };
			}

			if (segments[0].Identifier == "$metadata")
			{
				if (segments.Length > 1) throw InvalidUrl("cannot have additional segments");
				segments[0].Kind = SegmentKind.Metadata;
			}
			if (segments[0].Identifier == "$batch")
			{
				throw new HttpException(501, "batch is not supported");
			}
			if (segments[0].Identifier == "$count")
			{
				if (segments.Length > 1) throw InvalidUrl("$count is not supported on a service directory segment");
			}
			// 

			return segments;
		}

		private HttpException InvalidUrl(string reason)
		{
			return new HttpException(400, reason);
		}
	}

}
