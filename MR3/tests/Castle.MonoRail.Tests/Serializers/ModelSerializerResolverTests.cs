#region License
//  Copyright 2004-2012 Castle Project - http://www.castleproject.org/
//  Hamilton Verissimo de Oliveira and individual contributors as indicated. 
//  See the committers.txt/contributors.txt in the distribution for a 
//  full listing of individual contributors.
// 
//  This is free software; you can redistribute it and/or modify it
//  under the terms of the GNU Lesser General Public License as
//  published by the Free Software Foundation; either version 3 of
//  the License, or (at your option) any later version.
// 
//  You should have received a copy of the GNU Lesser General Public
//  License along with this software; if not, write to the Free
//  Software Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA
//  02110-1301 USA, or see the FSF site: http://www.fsf.org.
#endregion

namespace Castle.MonoRail.Tests.Serializers
{
	using System.IO;
	using System.Web;
	using Castle.MonoRail.Serialization;
	using FluentAssertions;
	using NUnit.Framework;

	[TestFixture]
	public class ModelSerializerResolverTests
	{
		private IModelSerializerResolver resolver;

		[SetUp]
		public void Init()
		{
			resolver = new ModelSerializerResolver() as IModelSerializerResolver;
		}

		[Test]
		public void CreateSerializer_DefaultSerializers_AlwaysCreatesSerializerForJson()
		{
			var serializer = resolver.CreateSerializer<Customer>(MimeType.JSon);
			Assert.IsNotNull(serializer);
		}

		[Test]
		public void CreateSerializer_DefaultSerializers_AlwaysCreatesSerializerForXml()
		{
			var serializer = resolver.CreateSerializer<Customer>(MimeType.Xml);
			Assert.IsNotNull(serializer);
		}

		[Test]
		public void CreateSerializer_DefaultSerializers_AlwaysCreatesSerializerForFormData()
		{
			var serializer = resolver.CreateSerializer<Customer>(MimeType.FormUrlEncoded);
			Assert.IsNotNull(serializer);
		}

		[Test, ExpectedException(typeof(MonoRailException))]
		public void CreateSerializer_DefaultSerializers_NoSerializerForJS()
		{
			resolver.CreateSerializer<Customer>(MimeType.Js);
		}

		[Test]
		public void Register_OfJsonSerializer_TakesPrecedenceOverDefault()
		{
			resolver.Register<Customer>(MimeType.JSon, typeof(StubSerializer<Customer>));
			var serializer = resolver.CreateSerializer<Customer>(MimeType.JSon);
			Assert.IsNotNull(serializer);
			Assert.AreEqual(typeof(StubSerializer<Customer>), serializer.GetType());
		}

		[Test]
		public void Register_OfJsonSerializer_TakesPrecedenceOverDefault_DiffSignature()
		{
			resolver.Register<Customer>(MimeType.JSon, typeof(StubSerializer<Customer>));
			var serializer = resolver.CreateSerializer(typeof(Customer), MimeType.JSon);
			Assert.IsNotNull(serializer);
			Assert.AreEqual(typeof(NonGenericSerializerAdapter), serializer.GetType());
		}

		[Test]
		public void CreateSerializer_ForUntypedSerializer_ReturnsFunctionalAdapter()
		{
			resolver.Register<Customer>(MimeType.JSon, typeof(StubSerializer<Customer>));
			var serializer = resolver.CreateSerializer(typeof(Customer), MimeType.JSon);
			var writer = new StringWriter();
			serializer.Serialize(new Customer(), "application/json", writer, null);
			writer.GetStringBuilder().ToString().Should().Be("hello");
		}

		[Test]
		public void Register_OfJsSerializer_IsReturnedWhenRequested()
		{
			resolver.Register<Customer>(MimeType.Js, typeof(StubSerializer<Customer>));
			var serializer = resolver.CreateSerializer<Customer>(MimeType.Js);
			Assert.IsNotNull(serializer);
			Assert.AreEqual(typeof(StubSerializer<Customer>), serializer.GetType());
		}

		[Test]
		public void Register_OfSerializerForModel_DoesNotAffectOtherModel()
		{
			resolver.Register<Customer>(MimeType.JSon, typeof(StubSerializer<Customer>));
			var serializer = resolver.CreateSerializer<Customer>(MimeType.JSon);
			Assert.AreEqual(typeof(StubSerializer<Customer>), serializer.GetType());

			var serializer2 = resolver.CreateSerializer<Supplier>(MimeType.JSon);
			Assert.AreNotEqual(typeof(StubSerializer<Customer>), serializer2.GetType());
		}

		class Customer {}
		class Supplier { }

		class StubSerializer<T> : IModelSerializer<T>
		{
			public void Serialize(T model, string contentType, TextWriter writer, ModelMetadataProvider metadataProvider)
			{
				writer.Write("hello");
			}

			public T Deserialize(string prefix, string contentType, HttpRequestBase request, ModelMetadataProvider metadataProvider)
			{
				throw new System.NotImplementedException();
			}
		}
	}
}
