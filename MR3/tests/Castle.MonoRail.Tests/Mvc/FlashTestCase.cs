namespace Castle.MonoRail.Tests.Mvc
{
	using NUnit.Framework;

	[TestFixture]
	public class FlashTestCase
	{
		[Test]
		public void NewEntries_AfterSweep_AreKept()
		{
			var flash = new Flash();

			flash["test"] = "hello";

			flash.Sweep();

			Assert.IsTrue(flash.ContainsKey("test"));

			flash = new Flash(flash);

			Assert.IsTrue(flash.ContainsKey("test"));
		}

		[Test]
		public void WhenUsingNow_ItemsIsNotKept()
		{
			var flash = new Flash();

			flash.Now("test", "hello");

			Assert.IsTrue(flash.ContainsKey("test"));

			flash.Sweep();

			Assert.IsFalse(flash.ContainsKey("test"));
		}

		[Test]
		public void WhenUsingKeep_ItemIsKept()
		{
			var flash = new Flash();

			flash.Now("test1", "hello");
			flash.Now("test2", "hello");

			flash.Keep("test1");

			flash.Sweep();

			Assert.IsTrue(flash.ContainsKey("test1"));
			Assert.IsFalse(flash.ContainsKey("test2"));

			flash = new Flash(flash);
			flash.Sweep();

			Assert.IsTrue(flash.Count == 0);

			flash.Now("test1", "hello");
			flash.Now("test2", "hello");

			flash.Keep();

			flash.Sweep();

			Assert.IsTrue(flash.ContainsKey("test1"));
			Assert.IsTrue(flash.ContainsKey("test2"));
		}

		[Test]
		public void WhenUsingDiscard_ItemsIsNotKept()
		{
			var flash = new Flash
			{
				{ "test1", "hello" }, 
				{ "test2", "hello" }
			};

			flash.Discard("test2");

			flash.Sweep();

			Assert.IsTrue(flash.ContainsKey("test1"));
			Assert.IsFalse(flash.ContainsKey("test2"));

			flash = new Flash(flash);
			flash.Sweep();

			Assert.IsTrue(flash.Count == 0);

			flash.Add("test1", "hello");
			flash.Add("test2", "hello");

			flash.Discard();

			flash = new Flash(flash);
			flash.Sweep();

			Assert.IsFalse(flash.ContainsKey("test1"));
			Assert.IsFalse(flash.ContainsKey("test2"));

			flash = new Flash
			{
				{ "test1", "hello" }, 
				{ "test1", "hello update" }
			};

			Assert.AreEqual("hello update", flash["test1"]);

			flash.Discard("test1");

			flash.Sweep();

			Assert.IsFalse(flash.ContainsKey("test1"));
		}
	}
}
