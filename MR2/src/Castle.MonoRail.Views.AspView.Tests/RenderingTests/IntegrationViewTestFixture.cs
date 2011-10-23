// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Configuration;

namespace Castle.MonoRail.Views.AspView.Tests.RenderingTests
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.IO;
	using System.Reflection;
	using Configuration;
	using Core.Logging;
	using AspView.Compiler;
	using Framework.Descriptors;
	using Framework.Helpers;
	using Framework.Resources;
	using Framework.Services;
	using Framework.Test;
	using Framework;
	using NUnit.Framework;

	public abstract class IntegrationViewTestFixture
	{
		protected ControllerContext ControllerContext;
		protected HelperDictionary Helpers;
		private string lastOutput;
		protected string Layout;
		protected StubEngineContext StubEngineContext;
		protected Hashtable PropertyBag;
		protected string Area = null;
		protected string ControllerName = "test_controller";
		protected string Action = "test_action";
		protected DefaultViewComponentFactory ViewComponentFactory;
		protected AspViewEngine viewEngine;

		protected IntegrationViewTestFixture()
		{
			SetUp();
		}

		[SetUp]
		public void SetUp()
		{
			var siteRoot = GetSiteRoot();
			var viewPath = Path.Combine(siteRoot, "RenderingTests\\Views");
			Layout = null;
			PropertyBag = new Hashtable(StringComparer.InvariantCultureIgnoreCase);
			Helpers = new HelperDictionary();
			var services = new StubMonoRailServices
			{
				UrlBuilder = new DefaultUrlBuilder(new StubServerUtility(), new StubRoutingEngine()), 
				UrlTokenizer = new DefaultUrlTokenizer()
			};
			var urlInfo = new UrlInfo(
				"example.org", "test", "/TestBrail", "http", 80,
				"http://test.example.org/test_area/test_controller/test_action.tdd",
				Area, ControllerName, Action, "tdd", "no.idea");
			StubEngineContext = new StubEngineContext(new StubRequest(), new StubResponse(), services,
													  urlInfo);
			StubEngineContext.AddService<IUrlBuilder>(services.UrlBuilder);
			StubEngineContext.AddService<IUrlTokenizer>(services.UrlTokenizer);
			StubEngineContext.AddService<IViewComponentFactory>(ViewComponentFactory);
			StubEngineContext.AddService<ILoggerFactory>(new ConsoleFactory());
			StubEngineContext.AddService<IViewSourceLoader>(new FileAssemblyViewSourceLoader(viewPath));
			

			ViewComponentFactory = new DefaultViewComponentFactory();
			ViewComponentFactory.Service(StubEngineContext);

			ControllerContext = new ControllerContext
			{
				Helpers = Helpers, 
				PropertyBag = PropertyBag
			};
			StubEngineContext.CurrentControllerContext = ControllerContext;


			Helpers["urlhelper"] = Helpers["url"] = new UrlHelper(StubEngineContext);
			Helpers["dicthelper"] = Helpers["dict"] = new DictHelper(StubEngineContext);
			Helpers["DateFormatHelper"] = Helpers["DateFormat"] = new DateFormatHelper(StubEngineContext);


			//FileAssemblyViewSourceLoader loader = new FileAssemblyViewSourceLoader(viewPath);
//			loader.AddAssemblySource(
//				new AssemblySourceInfo(Assembly.GetExecutingAssembly().FullName,
//									   "Castle.MonoRail.Views.Brail.Tests.ResourcedViews"));

			viewEngine = new AspViewEngine();
			var options = new AspViewEngineOptions();
			options.CompilerOptions.AutoRecompilation = true;
			options.CompilerOptions.KeepTemporarySourceFiles = false;
			((IAspViewEngineTestAccess)viewEngine).SetOptions(options);
			ICompilationContext context =
				new CompilationContext(
					new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory),
					new DirectoryInfo(siteRoot),
					new DirectoryInfo(Path.Combine(siteRoot, "RenderingTests\\Views")),
					new DirectoryInfo(siteRoot));

			var compilationContexts = new List<ICompilationContext> { context };
			((IAspViewEngineTestAccess)viewEngine).SetCompilationContext(compilationContexts);
			viewEngine.Service(StubEngineContext);
		}

		public string ProcessView(string templatePath)
		{
			var sw = new StringWriter();
			if (string.IsNullOrEmpty(Layout) == false)
				ControllerContext.LayoutNames = Layout.Split(',');
			StubEngineContext.CurrentControllerContext = ControllerContext;
			viewEngine.Process(templatePath, sw, StubEngineContext, null, ControllerContext);
			lastOutput = sw.ToString();
			return lastOutput;
		}

		private const string AppPathTests = "tests.src";
		protected virtual string GetSiteRoot()
		{
			var webAppPath = ConfigurationManager.AppSettings[AppPathTests];
			if (Directory.Exists(webAppPath))
				return new DirectoryInfo(webAppPath).FullName;

			throw new ConfigurationErrorsException("Unable to find site root. Check the key " + AppPathTests + " in app.config/appSettings");
		}

		protected void AddResource(string name, string resourceName, Assembly asm)
		{
			IResourceFactory resourceFactory = new DefaultResourceFactory();
			var descriptor = new ResourceDescriptor(
				null,
				name,
				resourceName,
				null,
				null);
			var resource = resourceFactory.Create(
				descriptor,
				asm);
			ControllerContext.Resources.Add(name, resource);
		}

		/*
		protected string RenderStaticWithLayout(string staticText)
		{
			if (string.IsNullOrEmpty(Layout) == false)
				ControllerContext.LayoutNames = new string[] { Layout, };
			StubEngineContext.CurrentControllerContext = ControllerContext;

			BooViewEngine.RenderStaticWithinLayout(staticText, StubEngineContext, null, ControllerContext);
			lastOutput = ((StringWriter)StubEngineContext.Response.Output)
				.GetStringBuilder().ToString();
			return lastOutput;
		}
		*/
		public void AssertReplyEqualTo(string expected)
		{
			Assert.AreEqual(expected, lastOutput);
		}

		public void AssertReplyContains(string contained)
		{
			Assert.IsTrue(lastOutput.Contains(contained));
		}
	}
}
