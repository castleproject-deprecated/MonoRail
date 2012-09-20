using System.Linq;

namespace Castle.MonoRail.Extension.OData3.Tests.Processors
{
	using System;
	using System.Collections.Generic;
	using Microsoft.Data.Edm;
	using Microsoft.FSharp.Core;
	using MonoRail.OData.Internal;
	using NUnit.Framework;

	[TestFixture]
	public class NavigationSegmentProcessorTestCase : ProcessorTestCaseBase
	{
		public NavigationSegmentProcessor BuildProcessor(string key, IEdmProperty property, out PropertyAccessInfo propertyAccessInfo)
		{
			var edmModel = _odata.EdmModel;

			// var entSet = edmModel.FindDeclaredEntityContainer("schemaNs.containerName").FindEntitySet("Products");
			// var entType = new EdmEntityTypeReference(edmModel.FindDeclaredType("schemaNs.Product") as IEdmEntityType, false); ;

			propertyAccessInfo = new PropertyAccessInfo(
				rawPathSegment: "",
				uri: new Uri("http://localhost/"),
				// edmSet: new FSharpOption<IEdmEntitySet>(entSet), 
				edmSet: new FSharpOption<IEdmEntitySet>(null), 
				manyResult: null,
				singleResult: null,
				returnType: property.Type, 
				key: key, 
				property: property, 
				container: edmModel.EntityContainers().ElementAt(0));

			return new NavigationSegmentProcessor(
				edmModel, _odata,
				_stubCallbacks.callbacks, new List<Tuple<Type, object>>(),
				_serializer,
				_request, _response,
				propertyAccessInfo);
		}

		public ResponseToSend ForProductProperty(RequestOperation op, IEdmProperty property, object container)
		{
			PropertyAccessInfo propertyAccessInfo;
			var processor = BuildProcessor(null, property, out propertyAccessInfo);

			return processor.Process(op,
				UriSegment.NewPropertyAccess(propertyAccessInfo),
				UriSegment.Nothing,
				hasMoreSegments: false,
				shouldContinue: _shouldContinue,
				container: container);
		}

		[Test]
		public void aaaaaa()
		{
			var edmType = _odata.EdmModel.FindDeclaredType("schemaNs.Product") as IEdmEntityType;
			var product = new Models.ModelWithAssociation.Product() { Id = 1, Name = "test" };
			product.Categories = new List<Models.ModelWithAssociation.Category>
				                     {
					                     new Models.ModelWithAssociation.Category() { Id = 10, Name = "cat1" }
				                     };
			var property = edmType.FindProperty("Categories");

			ForProductProperty(RequestOperation.Get, property, product);
		}
	}
}