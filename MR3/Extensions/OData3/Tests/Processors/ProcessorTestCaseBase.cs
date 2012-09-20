namespace Castle.MonoRail.Extension.OData3.Tests.Processors
{
	using Microsoft.FSharp.Core;
	using MonoRail.Tests;
	using NUnit.Framework;
	using Stubs;

	public abstract class ProcessorTestCaseBase
	{
		internal StubODataRequest _request;
		internal StubODataResponse _response;
		internal FSharpRef<bool> _shouldContinue;
		internal StubPayloadSerializer _serializer;
		internal StubCallbacks _stubCallbacks;
		internal ODataModel _odata;

		[SetUp]
		public void Init()
		{
			_request = new StubODataRequest();
			_response = new StubODataResponse();
			_shouldContinue = new FSharpRef<bool>(true);
			_serializer = new StubPayloadSerializer();
			_stubCallbacks = new StubCallbacks();
			_odata = new Models.ModelWithAssociation();
			_odata.InitializeModels(new StubServiceRegistry());
		}
	}
}