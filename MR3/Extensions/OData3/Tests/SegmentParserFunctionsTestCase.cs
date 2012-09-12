using Castle.MonoRail.OData.Internal;

namespace Castle.MonoRail.Extension.OData3.Tests
{
    using System;
    using System.Linq;
    using Microsoft.Data.Edm;
    using Microsoft.Data.Edm.Library;
    using MonoRail.Tests;
    using NUnit.Framework;
    using OData;
    using OData.Tests;

    [TestFixture]
    public class SegmentParserFunctionsTestCase
    {
        private IEdmModel _simpleModel;

        [SetUp]
        public void Init()
        {
            _simpleModel = Models.SimpleODataModel.BuildWithFunctions();
        }

        [Test]
        public void resolves_function_bound_to_singleprod_Top()
        {
            var segments = Parse("/products(1)/Top", String.Empty, _simpleModel);
            SegmentParserTestCase.Asserts.ExpectingSegmentsCount(segments, 2);
            SegmentParserTestCase.Asserts.IsSingleEntity(segments.ElementAt(0), "1", "products", GetEdmEntityType(_simpleModel, "Products"));
			SegmentParserTestCase.Asserts.IsFunctionOperation(segments.ElementAt(1), "Top", GetEdmEntityType(_simpleModel, "Products"));
		}

		[Test]
		public void resolves_function_bound_to_collection_Best()
		{
			var segments = Parse("/products/Best", String.Empty, _simpleModel);
			SegmentParserTestCase.Asserts.ExpectingSegmentsCount(segments, 2);
			SegmentParserTestCase.Asserts.IsFunctionOperation(
				segments.ElementAt(1), "Best", GetEdmEntityType(_simpleModel, "Products"), relativeUri: "/products/Best");
		}

	    [Test] 
		public void resolves_function_which_returns_single_element_and_access_property()
		{
			var segments = Parse("/products/Best/Name", String.Empty, _simpleModel);
			SegmentParserTestCase.Asserts.ExpectingSegmentsCount(segments, 3);
			SegmentParserTestCase.Asserts.IsFunctionOperation(
				segments.ElementAt(1), "Best", GetEdmEntityType(_simpleModel, "Products"), relativeUri: "/products/Best");
			SegmentParserTestCase.Asserts.IsPropertySingle(segments.ElementAt(2), "Name", EdmCoreModel.Instance.GetString(true) );
		}

        private IEdmEntityTypeReference GetEdmEntityType(IEdmModel model, string name)
        {
            var entSet = model.EntityContainers().Single().FindEntitySet(name);
            return new EdmEntityTypeReference( entSet.ElementType, false);
        }

        protected UriSegment[] Parse(string path, string qs, IEdmModel model)
        {
            var parameters = Utils.BuildFromQS(qs);

            var tuple = SegmentParser.parse(path, parameters, model, new Uri("http://localhost/base/"));

//            Segments = tuple.Item1;
//            Meta = tuple.Item2;
//            MetaQueries = tuple.Item3;

            return tuple.Item1;
        }
    }
}
