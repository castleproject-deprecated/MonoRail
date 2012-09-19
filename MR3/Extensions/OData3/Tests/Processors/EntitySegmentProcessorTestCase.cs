using System;
using System.Collections.Generic;
using Castle.MonoRail.Extension.OData3.Tests.Stubs;
using Castle.MonoRail.OData.Internal;
using Castle.MonoRail.Tests;
using Microsoft.FSharp.Core;
using NUnit.Framework;

namespace Castle.MonoRail.Extension.OData3.Tests.Processors
{
	using FluentAssertions;
	using Microsoft.Data.Edm;
	using Microsoft.Data.Edm.Library;

	[TestFixture]
	public class EntitySegmentProcessorTestCase
	{
		private StubODataRequest _request;
		private StubODataResponse _response;
		private FSharpRef<bool> _shouldContinue;

		[SetUp]
		public void Init()
		{
			_request = new StubODataRequest();
			_response = new StubODataResponse();
			_shouldContinue = new FSharpRef<bool>(true);
		}

		public EntitySegmentProcessor BuildProcessor(string key, out EntityAccessInfo entityAccessInfo)
		{
			var stubCallbacks = new StubCallbacks();
			var odata = new Models.ModelWithAssociation();
			odata.InitializeModels(new StubServiceRegistry());
			var edmModel = odata.EdmModel;
			var serializer = new StubPayloadSerializer();

			var entSet = edmModel.FindDeclaredEntityContainer("schemaNs.containerName").FindEntitySet("Products");
			var entType = new EdmEntityTypeReference(edmModel.FindDeclaredType("schemaNs.Product") as IEdmEntityType, false); ;

			entityAccessInfo = new EntityAccessInfo(
				rawPathSegment: "", 
				uri: new Uri("http://localhost/"),
				manyResult: null, 
				singleResult: null,
				edmSet: entSet,
				edmEntityType: entType,
				returnType: null,
				container: null,
				name: "products", key: key);

			return new EntitySegmentProcessor(
				edmModel, odata,
				stubCallbacks.callbacks, new List<Tuple<Type, object>>(),
				serializer,
				_request, _response,
				entityAccessInfo);
		}

		public ResponseToSend GetProductByKey(string key)
		{
			EntityAccessInfo entityAccessInfo;
			var processor = BuildProcessor(key, out entityAccessInfo);

			return processor.Process(RequestOperation.Get,
				UriSegment.NewEntitySet(entityAccessInfo),
				UriSegment.Nothing, 
				hasMoreSegments: false,
				shouldContinue: _shouldContinue, 
				container: null);
		}

		public ResponseToSend GetProducts()
		{
			EntityAccessInfo entityAccessInfo;
			var processor = BuildProcessor(null, out entityAccessInfo);

			return processor.Process(RequestOperation.Get,
				UriSegment.NewEntitySet(entityAccessInfo),
				UriSegment.Nothing,
				hasMoreSegments: false,
				shouldContinue: _shouldContinue,
				container: null);
		}

		[Test]
		public void get_entity_by_Key_should_return_requested_instance()
		{
			var toSend = GetProductByKey("1");

			toSend.Should().NotBeNull();
			toSend.QItems.Should().BeNull();
			toSend.SingleResult.Should().NotBeNull();
			(toSend.SingleResult as Models.ModelWithAssociation.Product).Id.Should().Be(1);

			_response.StatusCode.Should().Be(200);
		}

		[Test]
		public void get_entity_by_Key_for_non_existent_id_should_return_404()
		{
			var toSend = GetProductByKey("10000");

			toSend.Should().NotBeNull();
			toSend.QItems.Should().BeNull();
			toSend.SingleResult.Should().BeNull();

			_response.StatusCode.Should().Be(404);
		}

		[Test]
		public void get_all_entities()
		{
			var toSend = GetProducts();

			toSend.Should().NotBeNull();
			toSend.QItems.Should().NotBeNull();
			toSend.SingleResult.Should().BeNull();

			var products = (IEnumerable<Models.ModelWithAssociation.Product>) toSend.QItems;
			products.Should().NotBeNull();

			_response.StatusCode.Should().Be(200);
		}
	}
}
