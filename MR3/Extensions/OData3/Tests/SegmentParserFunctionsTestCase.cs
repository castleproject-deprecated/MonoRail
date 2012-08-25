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
//        private IEdmModel _modelWithAssociations;

        [SetUp]
        public void Init()
        {
            _simpleModel = Models.SimpleODataModel.BuildWithFunctions();

        }

        [Test]
        public void aaaaaaaaaaaaa()
        {
            var segments = Parse("/products/Top", String.Empty, _simpleModel);
            SegmentParserTestCase.Asserts.ExpectingSegmentsCount(segments, 1);
            SegmentParserTestCase.Asserts.IsEntitySet(segments.ElementAt(0), "products", GetEdmEntityType(_simpleModel, "Products"));
        }

        private IEdmEntityType GetEdmEntityType(IEdmModel model, string name)
        {
            var entSet = model.EntityContainers().Single().FindEntitySet(name);
            return entSet.ElementType;
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
