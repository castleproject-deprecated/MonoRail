namespace Castle.MonoRail.Extension.OData3.Tests.Processors
{
	using System;
	using System.Linq;
	using System.Collections.Generic;
	using Castle.MonoRail.OData.Internal;
	using NUnit.Framework;
	using FluentAssertions;
	using Microsoft.Data.Edm;
	using Microsoft.Data.Edm.Library;

	[TestFixture]
	public class EntitySegmentProcessorTestCase : ProcessorTestCaseBase
	{
		public EntitySegmentProcessor BuildProcessor(string key, out EntityAccessInfo entityAccessInfo)
		{
			var edmModel = _odata.EdmModel;

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
				edmModel, _odata,
				_stubCallbacks.callbacks, new List<Tuple<Type, object>>(),
				_serializer,
				_request, _response,
				entityAccessInfo);
		}

		public ResponseToSend ForProductsWithKey(RequestOperation op, string key)
		{
			EntityAccessInfo entityAccessInfo;
			var processor = BuildProcessor(key, out entityAccessInfo);

			return processor.Process(op,
				UriSegment.NewEntitySet(entityAccessInfo),
				UriSegment.Nothing, 
				hasMoreSegments: false,
				shouldContinue: _shouldContinue, 
				container: null);
		}

		public ResponseToSend ForProducts(RequestOperation op)
		{
			EntityAccessInfo entityAccessInfo;
			var processor = BuildProcessor(null, out entityAccessInfo);

			return processor.Process(op,
				UriSegment.NewEntitySet(entityAccessInfo),
				UriSegment.Nothing,
				hasMoreSegments: false,
				shouldContinue: _shouldContinue,
				container: null);
		}

		[Test]
		public void get_entity_by_Key_should_return_requested_instance()
		{
			var toSend = ForProductsWithKey(RequestOperation.Get, "1");

			toSend.Should().NotBeNull();
			toSend.QItems.Should().BeNull();
			toSend.SingleResult.Should().NotBeNull();
			(toSend.SingleResult as Models.ModelWithAssociation.Product).Id.Should().Be(1);

			_response.StatusCode.Should().Be(200);
		}

		[Test]
		public void get_entity_by_Key_for_non_existent_id_should_return_404()
		{
			var toSend = ForProductsWithKey(RequestOperation.Get, "100000");

			toSend.Should().NotBeNull();
			toSend.QItems.Should().BeNull();
			toSend.SingleResult.Should().BeNull();

			_response.StatusCode.Should().Be(404);
		}

		[Test]
		public void get_all_entities()
		{
			var toSend = ForProducts(RequestOperation.Get);

			toSend.Should().NotBeNull();
			toSend.QItems.Should().NotBeNull();
			toSend.SingleResult.Should().BeNull();

			var products = (IEnumerable<Models.ModelWithAssociation.Product>) toSend.QItems;
			products.Should().NotBeNull();

			_response.StatusCode.Should().Be(200);
		}

		[Test, ExpectedException(typeof(Exception), ExpectedMessage = "Unsupported operation Create at this level")]
		public void create_entity_with_key_is_not_supported()
		{
			var toSend = ForProductsWithKey(RequestOperation.Create, "1");
		}

		[Test, ExpectedException(typeof(Exception), ExpectedMessage = "Unsupported operation for entity set segment Delete")]
		public void delete_collections_is_not_supported()
		{
			var toSend = ForProducts(RequestOperation.Delete);

		}

		[Test, ExpectedException(typeof(Exception), ExpectedMessage = "Unsupported operation for entity set segment Update")]
		public void update_collections_is_not_supported()
		{
			var toSend = ForProducts(RequestOperation.Update);

		}

		[Test, ExpectedException(typeof(Exception), ExpectedMessage = "Unsupported operation for entity set segment Merge")]
		public void merge_collections_is_not_supported()
		{
			var toSend = ForProducts(RequestOperation.Merge);
		}

		[Test]
		public void create_for_collections_invokes_callback_and_returns_200()
		{
			_serializer.ObjectToReturn = new Models.ModelWithAssociation.Product();

			var toSend = ForProducts(RequestOperation.Create);

			_stubCallbacks.CreateWasCalled(1);
			toSend.SingleResult.Should().BeSameAs(_serializer.ObjectToReturn);
			_response.StatusCode.Should().Be(200);
		}

		[Test]
		public void update_for_single_invokes_callback_and_returns_204()
		{
			var prods = (IQueryable<Models.ModelWithAssociation.Product>) _odata.GetQueryable(
				_odata.EdmModel.FindDeclaredEntityContainer("schemaNs.containerName").FindEntitySet("Products"));

			var toSend = ForProductsWithKey(RequestOperation.Update, "1");

			_stubCallbacks.UpdateWasCalled(1);
			toSend.SingleResult.Should().BeSameAs(prods.Single(p => p.Id == 1));
			_response.StatusCode.Should().Be(204);
		}

		[Test]
		public void merge_for_single_invokes_callback_and_returns_204()
		{
			var prods = (IQueryable<Models.ModelWithAssociation.Product>)_odata.GetQueryable(
				_odata.EdmModel.FindDeclaredEntityContainer("schemaNs.containerName").FindEntitySet("Products"));

			var toSend = ForProductsWithKey(RequestOperation.Merge, "1");

			_stubCallbacks.UpdateWasCalled(1);
			toSend.SingleResult.Should().BeSameAs(prods.Single(p => p.Id == 1));
			_response.StatusCode.Should().Be(204);
		}

		[Test]
		public void delete_for_single_invokes_callback_and_returns_204()
		{
			var prods = (IQueryable<Models.ModelWithAssociation.Product>)_odata.GetQueryable(
				_odata.EdmModel.FindDeclaredEntityContainer("schemaNs.containerName").FindEntitySet("Products"));

			var toSend = ForProductsWithKey(RequestOperation.Delete, "1");

			_stubCallbacks.RemoveWasCalled(1);
			toSend.SingleResult.Should().BeSameAs(prods.Single(p => p.Id == 1));
			_response.StatusCode.Should().Be(204);
		}
	}
}
