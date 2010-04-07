namespace Castle.MonoRail.ActiveRecordSupport.Tests
{
	using System;
	using System.Collections.Generic;
	using Castle.Components.Binder;
	using Castle.MonoRail.Framework.Test;
	using NUnit.Framework;
	using TestSiteARSupport.Model;

	[TestFixture]
	public class ARFetcherTestCase : BaseARTestCase
	{ 
		private Account account1;
		private Account account2;

		protected override void CreateTestData()
		{
			account1 = new Account("AdWords", "email1@gmail.net", "abc123");
			account2 = new Account("AdWords", "email1@gmail.net", "abc123");

			account1.Create();
			account2.Create();
		}

		[Test]
		public void CanGetItemByIdFromRequest()
		{
			var fetcher = new ARFetcher(new DefaultConverter());
			var parameter = typeof(MyController).GetMethod("MyAction").GetParameters()[0];
			var attribute = (ARFetchAttribute)parameter.GetCustomAttributes(typeof(ARFetchAttribute), true)[0];
			var request = new StubRequest();
			request.Params["id"] = account1.Id.ToString();
			var record = (Account)fetcher.FetchActiveRecord(
				parameter, attribute, request, new Dictionary<string, object>());
			Assert.AreEqual(account1.Id, record.Id);
		}

		[Test]
		public void CanGetItemByIdFromActionParams()
		{
			var fetcher = new ARFetcher(new DefaultConverter());
			var parameter = typeof(MyController).GetMethod("MyAction").GetParameters()[0];
			var attribute = (ARFetchAttribute)parameter.GetCustomAttributes(typeof(ARFetchAttribute), true)[0];
			var customActionParameters = new Dictionary<string, object>();
			customActionParameters["id"] = account1.Id.ToString();
			var record = (Account)fetcher.FetchActiveRecord(
				parameter, attribute, new StubRequest(), customActionParameters);
			Assert.AreEqual(account1.Id, record.Id);
		}

		[Test]
		public void CanGetItemByIdFromRequest_UsingArray()
		{
			var fetcher = new ARFetcher(new DefaultConverter());
			var parameter = typeof(MyController).GetMethod("MyAction2").GetParameters()[0];
			var attribute = (ARFetchAttribute)parameter.GetCustomAttributes(typeof(ARFetchAttribute), true)[0];
			var request = new StubRequest();
			request.Params.Add("id", account1.Id.ToString());
			request.Params.Add("id", account2.Id.ToString());
			var records = (Account[])fetcher.FetchActiveRecord(
				parameter, attribute, request, new Dictionary<string, object>());
			Assert.AreEqual(account1.Id, records[0].Id);
			Assert.AreEqual(account2.Id, records[1].Id);
		}

		[Test]
		public void CanGetItemByIdFromActionParams_UsingArray()
		{
			var fetcher = new ARFetcher(new DefaultConverter());
			var parameter = typeof(MyController).GetMethod("MyAction2").GetParameters()[0];
			var attribute = (ARFetchAttribute)parameter.GetCustomAttributes(typeof(ARFetchAttribute), true)[0];
			var customActionParameters = new Dictionary<string, object>
			{
				{ "id", new[] { account1.Id.ToString(), account2.Id.ToString() } }
			};
			var records = (Account[])fetcher.FetchActiveRecord(
				parameter, attribute, new StubRequest(), customActionParameters);
		
			Assert.AreEqual(account1.Id, records[0].Id);
			Assert.AreEqual(account2.Id, records[1].Id);
		}

		[Test]
		public void CanFetchWithEmptyGuid()
		{
			var tag = new Tag
			{
				Id = Guid.Empty, 
				Name = "TopMovie"
			};
			tag.Create();
			
			var fetcher = new ARFetcher(new DefaultConverter());
			
			var parameter = typeof(MyController).GetMethod("MyAction3").GetParameters()[0];
			
			var attribute = (ARFetchAttribute)parameter.GetCustomAttributes(typeof(ARFetchAttribute), true)[0];
			
			var customActionParameters = new Dictionary<string, object>();
			customActionParameters["id"] = Guid.Empty;

			var record = (Tag)fetcher.FetchActiveRecord(
				parameter, attribute, new StubRequest(), customActionParameters);
			
			Assert.AreEqual(tag.Id, record.Id);
		}

		public class MyController
		{
			public void MyAction([ARFetch("id")]Account account)
			{
				
			}

			public void MyAction2([ARFetch("id")]Account[] account)
			{

			}

			public void MyAction3([ARFetch("id")]Tag tag)
			{

			}
		}
	}
}