using System;
using System.Collections.Generic;
using Castle.MonoRail.Extension.OData3.Tests.Stubs;
using Castle.MonoRail.OData.Internal;
using Castle.MonoRail.Tests;
using Microsoft.FSharp.Core;
using NUnit.Framework;

namespace Castle.MonoRail.Extension.OData3.Tests.Processors
{
	[TestFixture]
	public class EntitySegmentProcessorTestCase
	{
		private StubODataRequest _request;
		private StubODataResponse _response;

		[SetUp]
		public void Init()
		{
			_request = new StubODataRequest();
			_response = new StubODataResponse();
		}

		[Test]
		public void aaa()
		{
			var stubCallbacks = new StubCallbacks();
			var odata = new Models.ModelWithAssociation();
			odata.InitializeModels(new StubServiceRegistry());
			var edmModel = odata.EdmModel;
			var serializer = new StubPayloadSerializer();

			var processor = new EntitySegmentProcessor(
				edmModel, odata, 
				stubCallbacks.callbacks, new List<Tuple<Type, object>>(), 
				serializer,
				_request, _response, 
				entityAccessInfo);

			var shouldContinue = new FSharpRef<bool>(true);

			// var toSend = processor.Process(RequestOperation.Get, segment, previous, false, shouldContinue, null);
		}
	}
}
