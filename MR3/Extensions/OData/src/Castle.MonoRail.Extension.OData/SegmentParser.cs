namespace Castle.MonoRail.Extension.OData
{
	using System;
	using System.Collections.Generic;
	using System.Data.Services.Providers;
	using System.Linq;
	using System.Text.RegularExpressions;
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
		public Segment(string identifier)
		{
			Identifier = identifier;
		}

		public string Identifier { get; private set; }
		public SegmentKind Kind { get; set; }
		public ResourceSet Container { get; set; }
		public ResourceProperty ProjectedProperty { get; set; }
		// public ServiceOperationWrapper Operation { get; set; }
		public string Key { get; set; }
		public bool IsCollectionResult { get; set; }
	}

	/// <summary>
	/// This class actually does more than parsing. it also binds the segments
	/// to their corresponding tokens
	/// </summary>
	public class SegmentParser
	{
		private static readonly Regex KeyInIdentifierPattern = new Regex("(.+)\\((.+)\\)$",RegexOptions.Compiled);

		public IEnumerable<Segment> ParseAndBind(string path, ODataModel model)
		{
			if (path == null) throw new ArgumentNullException("path");
			if (model == null) throw new ArgumentNullException("model");

			if (path.StartsWith("/", StringComparison.Ordinal) )
			{
				path = path.Substring(1);
			}

			var segments = path.Split(new [] {'/'}, StringSplitOptions.None).Select(p => new Segment(p)).ToArray();

			if (segments.Length == 1 && segments[0].Identifier == String.Empty)
			{
				return new[] { new Segment("/") { Kind = SegmentKind.ServiceDirectory } };
			}

			if (segments[0].Identifier == "$metadata")
			{
				if (segments.Length > 1) throw InvalidUrl("cannot have additional segments");
				segments[0].Kind = SegmentKind.Metadata;
				return segments;
			}
			if (segments[0].Identifier == "$batch")
			{
				throw new HttpException(501, "batch is not supported");
			}
			if (segments[0].Identifier == "$count")
			{
				if (segments.Length > 1) throw InvalidUrl("$count is not supported on a service directory segment");
			}
			if (segments[0].Identifier.IndexOf("$") != -1)
			{
				// exception, since we already checked the known cases
			}

			var entityName = "";
			string key;
			if (!TryExtractKeys(segments[0], out key, out entityName))
			{
				entityName = segments[0].Identifier;
			}

			var resourceSet = model.ResourceSets.FirstOrDefault(rt => StringComparer.OrdinalIgnoreCase.Equals(rt.Name, entityName));

			if (resourceSet == null)
			{
				// either it's custom operation, or wrong url. 
				throw InvalidUrl(segments[0].Identifier + " does not map to a known entity");
			}

			segments[0].Kind = SegmentKind.Resource;
			segments[0].Container = resourceSet;
			segments[0].Key = key;
			segments[0].IsCollectionResult = key == null;

			RecursiveParseAdditionalSegments(segments[0], segments, 1);

			return segments;
		}

		private static bool TryExtractKeys(Segment segment, out string keyVal, out string entityName)
		{
			keyVal = entityName = null;
			var m = KeyInIdentifierPattern.Match(segment.Identifier);
			if (m.Success)
			{
				entityName = m.Groups[1].Captures[0].Value;
				keyVal = m.Groups[2].Captures[0].Value.Trim(new [] { '(', ')' });
				return true;
			}
			return false;
		}

		private void RecursiveParseAdditionalSegments(Segment parent, Segment[] segments, int index)
		{
			if (index == segments.Length)
				return;

			if (parent.Kind == SegmentKind.Batch || parent.Kind == SegmentKind.Metadata ||
				parent.Kind == SegmentKind.PrimitiveValue || parent.Kind == SegmentKind.VoidServiceOperation ||
				parent.Kind == SegmentKind.OpenPropertyValue || parent.Kind == SegmentKind.MediaResource)
			{
				throw new HttpException(400, "bad request");
			}

			// need to figure out if it's
			// - an operation
			// - an property
			//   - complex
			//   - reference to another entity (single)
			//   - reference to another entity (many)
			// - filtering

			var segment = segments[index];
			var identifier = segment.Identifier;
			var container = parent.Container;

			if (container != null &&
			   (container.ResourceType.ResourceTypeKind == ResourceTypeKind.EntityType ||
				container.ResourceType.ResourceTypeKind == ResourceTypeKind.ComplexType))
			{
				var prop = container.ResourceType.Properties.FirstOrDefault(p => p.Name == identifier);

				if (prop != null)
				{
					segment.ProjectedProperty = prop;

					if (prop.IsOfKind(ResourcePropertyKind.Primitive))
					{
						segment.Kind = SegmentKind.Primitive;
					}
				}
			}
			else
			{
				if (identifier == "$value")
				{
					if (parent.Kind == SegmentKind.Primitive)
					{
						segment.Kind = SegmentKind.PrimitiveValue;
					}
				}
			}

			RecursiveParseAdditionalSegments(segment, segments, index + 1);
		}

		private HttpException InvalidUrl(string reason)
		{
			return new HttpException(400, reason);
		}
	}

}
