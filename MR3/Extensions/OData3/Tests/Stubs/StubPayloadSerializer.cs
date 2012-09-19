using System;
using System.Collections.Generic;
using System.Linq;
using Castle.MonoRail.OData.Internal;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;

namespace Castle.MonoRail.Extension.OData3.Tests.Stubs
{
	class StubPayloadSerializer : PayloadSerializer
	{
		public object ObjectToReturn { get; set; }

		public override void SerializeMetadata(IODataRequestMessage request, IODataResponseMessage response)
		{
			throw new NotImplementedException();
		}

		public override void SerializeServiceDoc(IODataRequestMessage request, IODataResponseMessage response)
		{
			throw new NotImplementedException();
		}

		public override void SerializeFeed(IQueryable models, IEdmEntitySet edmEntSet, IEdmEntityTypeReference edmEntType, ODataFormat formatOverride, HashSet<IEdmProperty> expandList, IODataRequestMessage request, IODataResponseMessage response)
		{
			throw new NotImplementedException();
		}

		public override void SerializeEntry(object model, IEdmEntitySet edmEntSet, IEdmEntityTypeReference edmEntType, ODataFormat formatOverride, HashSet<IEdmProperty> expandList, IODataRequestMessage request, IODataResponseMessage response)
		{
			throw new NotImplementedException();
		}

		public override void SerializeCollection(IQueryable models, IEdmTypeReference edmType, ODataFormat formatOverride, HashSet<IEdmProperty> expandList, IODataRequestMessage request, IODataResponseMessage response)
		{
			throw new NotImplementedException();
		}

		public override void SerializeProperty(object model, IEdmTypeReference edmType, ODataFormat formatOverride, HashSet<IEdmProperty> expandList, IODataRequestMessage request, IODataResponseMessage response)
		{
			throw new NotImplementedException();
		}

		public override void SerializeValue(object value, IEdmTypeReference edmType, ODataFormat formatOverride, IODataRequestMessage request, IODataResponseMessage response)
		{
			throw new NotImplementedException();
		}

		public override void SerializeError(Exception exception, IODataRequestMessage request, IODataResponseMessage response)
		{
			throw new NotImplementedException();
		}

		public override object Deserialize(IEdmTypeReference edmType, IODataRequestMessage request)
		{
			return ObjectToReturn;
		}
	}
}
