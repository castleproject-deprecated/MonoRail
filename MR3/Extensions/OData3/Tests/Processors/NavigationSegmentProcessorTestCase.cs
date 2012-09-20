using System.Linq;
using FluentAssertions;

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

			var returnType = property.Type;
			if (key != null && returnType.IsCollection())
			{
				returnType = ((returnType as IEdmCollectionTypeReference).Definition as IEdmCollectionType).ElementType;
			}

			propertyAccessInfo = new PropertyAccessInfo(
				rawPathSegment: "",
				uri: new Uri("http://localhost/"),
				edmSet: new FSharpOption<IEdmEntitySet>(null), 
				manyResult: null,
				singleResult: null,
				returnType: returnType, 
				key: key, 
				property: property,
				edmEntityType: returnType as IEdmEntityTypeReference,
				container: edmModel.EntityContainers().ElementAt(0));

			return new NavigationSegmentProcessor(
				edmModel, _odata,
				_stubCallbacks.callbacks, new List<Tuple<Type, object>>(),
				_serializer,
				_request, _response,
				propertyAccessInfo);
		}

		public ResponseToSend ForProductProperty(RequestOperation op, IEdmProperty property, object container, string key = null)
		{
			PropertyAccessInfo propertyAccessInfo;
			var processor = BuildProcessor(key, property, out propertyAccessInfo);

			return processor.Process(op,
				UriSegment.NewPropertyAccess(propertyAccessInfo),
				UriSegment.Nothing,
				hasMoreSegments: false,
				shouldContinue: _shouldContinue,
				container: container);
		}

		[Test]
		public void get_keyed_item_from_collection()
		{
			var edmType = _odata.EdmModel.FindDeclaredType("schemaNs.Product") as IEdmEntityType;
			var product = new Models.ModelWithAssociation.Product() { Id = 1, Name = "test" };
			product.Categories = new List<Models.ModelWithAssociation.Category>
				                     {
					                     new Models.ModelWithAssociation.Category() { Id = 10, Name = "cat1" }
				                     };
			var property = edmType.FindProperty("Categories");

			var toSend = ForProductProperty(RequestOperation.Get, property, product, key: "10");

			toSend.Should().NotBeNull();
			toSend.SingleResult.Should().NotBeNull();

			toSend.SingleResult.Should().Be(product.Categories.ElementAt(0));
		}

		[Test]
		public void get_keyed_item_for_non_existent_item_should_return_404()
		{
			var edmType = _odata.EdmModel.FindDeclaredType("schemaNs.Product") as IEdmEntityType;
			var product = new Models.ModelWithAssociation.Product() { Id = 1, Name = "test" };
			product.Categories = new List<Models.ModelWithAssociation.Category>
				                     {
					                     new Models.ModelWithAssociation.Category() { Id = 10, Name = "cat1" }
				                     };
			var property = edmType.FindProperty("Categories");

			var toSend = ForProductProperty(RequestOperation.Get, property, product, key: "20202020");

			_response.StatusCode.Should().Be(404);
			toSend.Should().NotBeNull();
			toSend.SingleResult.Should().BeNull();
		}


		[Test]
		public void get_whole_collection()
		{
			var edmType = _odata.EdmModel.FindDeclaredType("schemaNs.Product") as IEdmEntityType;
			var product = new Models.ModelWithAssociation.Product() { Id = 1, Name = "test" };
			product.Categories = new List<Models.ModelWithAssociation.Category>
				                     {
					                     new Models.ModelWithAssociation.Category() { Id = 10, Name = "cat1" }
				                     };
			var property = edmType.FindProperty("Categories");

			var toSend = ForProductProperty(RequestOperation.Get, property, product);
			
			toSend.Should().NotBeNull();
			toSend.QItems.Should().NotBeNull();
			toSend.QItems.Should().HaveCount(1);
		}
	}
}