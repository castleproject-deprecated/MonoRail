// Copyright 2004-2009 Castle Project - http://www.castleproject.org/
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

namespace Castle.MonoRail.Views.AspView.Tests.ViewTests
{
	using System;
	using System.Diagnostics;
	using System.IO;
	using System.Linq;
	using System.Text;
	using Configuration;
	using Framework;
	using Framework.Services;
	using NUnit.Framework;

	[TestFixture]
	public class ViewPropertiesScopingFixture : AbstractViewTestFixture
	{
		public abstract class TestView<T> : EmptyView<T>
		{
			protected void OutputKeys(string viewName)
			{
				var values = Enumerable.Range(1, 10)
					.Select(i=>Properties["key" + i])
					.Where(v=>v!=null)
					.Select(v=>v.ToString())
					.ToArray();

				OutputLine(viewName.PadRight(15) + "Keys=[" + string.Join(",", values) + "]");
			}
		}

		public interface IPrimaryLayout { }
		public interface ISecondaryLayout { }
		public interface IMainView { }
		public interface ISecondaryView { }
		public interface ITernaryView { }
		public class PrimaryLayout : TestView<IPrimaryLayout>
		{
			public override void Render()
			{
				OutputLine("Primary layout start");
				OutputLine("Top capture:"+Properties["TopCapture"]);
				OutputLine(ViewContents);
				OutputLine("Bottom capture:" + Properties["BottomCapture"]);
				OutputLine("Primary layout end");
			}
		}
		public class SecondaryLayout : TestView<ISecondaryLayout>
		{
			public override void Render()
			{
				OutputLine("Secondary layout start");
				OutputLine(ViewContents);
				OutputLine("Secondary layout end");
			}
		}

		public class MainView : TestView<IMainView>
		{
			public override void Render()
			{
				OutputLine("Main view start");
				OutputKeys("MainView");
				OutputSubView("SecondaryView",
					N("key1", "val1_S_1")
					.N("key4", "val4_S_1")
					.N("key5", "val5_S_1")
					.N("val", (int)Properties["val"] + 1)
					);
				OutputKeys("MainView");
				OutputSubView("SecondaryView",
					N("key1", "val1_S_2")
					.N("key4", "val4_S_2")
					.N("key5", "val5_S_2")
					.N("val", (int)Properties["val"] + 1)
					);
				OutputKeys("MainView");
				OutputLine("Main view end");
				InvokeViewComponent("CaptureFor", N("id", "TopCapture").N("append", "after"), () => 
					Output("MainView" + Properties["Val"]), null);
				InvokeViewComponent("CaptureFor", N("id", "BottomCapture").N("append", "before"), () =>
					Output("MainView" + Properties["Val"]), null);
			}
		}

		public class SecondaryView : TestView<ISecondaryView>
		{
			public override void Render()
			{
				OutputLine("Secondary view start");
				OutputKeys("SecondaryView");
				OutputSubView("TrenaryView",
					N("key1", "val1_T_1")
					.N("key2", "val2_T_1")
					.N("key5", "val5_T_1")
					.N("key7", "val7_T_1")
					.N("val", (int)Properties["val"] + 1)
					);
				OutputKeys("SecondaryView");
				InvokeViewComponent("CaptureFor", N("id", "TopCapture").N("append", "after"), () =>
					Output("SecondaryView" + Properties["Val"]), null);
				InvokeViewComponent("CaptureFor", N("id", "BottomCapture").N("append", "before"), () =>
					Output("SecondaryView" + Properties["Val"]), null);
			}
		}

		public class TrenaryView : TestView<ITernaryView>
		{
			public override void Render()
			{
				OutputLine("Trenary view start");
				OutputKeys("TrenaryView");
				InvokeViewComponent("CaptureFor", N("id", "TopCapture").N("append", "after"), () =>
					Output("TrenaryView" + Properties["Val"]), null);
				InvokeViewComponent("CaptureFor", N("id", "BottomCapture").N("append", "before"), () =>
					Output("TrenaryView" + Properties["Val"]), null);
				OutputLine("Trenary view end");
			}
		}

		public abstract class EmptyView<T> : AspViewBase<T>
		{
			protected override string ViewDirectory
			{
				get { return ""; }
			}

			protected override string ViewName
			{
				get { return ""; }
			}

			protected void OutputLine(object s)
			{
				Output("" + s + Environment.NewLine);
			}
		}
		
		[Test]
		public void ComplexScoping()
		{
			var viewComponentFactory = new DefaultViewComponentFactory();
			viewComponentFactory.Service(context);
			context.AddService(typeof(IViewComponentFactory), viewComponentFactory);
			engine.Options.ViewProperties = ViewPropertiesInclusionOptions.QueryString;
			controllerContext.LayoutNames = new[] { "PrimaryLayout", "SecondaryLayout" };

			AddCompilation("MainView", typeof(MainView));
			AddCompilation("Layouts_PrimaryLayout", typeof(PrimaryLayout));
			AddCompilation("Layouts_SecondaryLayout", typeof(SecondaryLayout));
			AddCompilation("SecondaryView", typeof(SecondaryView));
			AddCompilation("TrenaryView", typeof(TrenaryView));

			var outputString = new StringBuilder();
			for (var i=1; i <=6; ++i)
				controllerContext.PropertyBag["key" + i] = "val" + i+"_C_1";
			controllerContext.PropertyBag["val"] = 1;

			engine.Process("MainView", new StringWriter(outputString), context, controller, controllerContext);


			expected =
				@"Primary layout start
Top capture:TrenaryView3SecondaryView2TrenaryView3SecondaryView2MainView1
Secondary layout start
Main view start
MainView       Keys=[val1_C_1,val2_C_1,val3_C_1,val4_C_1,val5_C_1,val6_C_1]
Secondary view start
SecondaryView  Keys=[val1_S_1,val2_C_1,val3_C_1,val4_S_1,val5_S_1,val6_C_1]
Trenary view start
TrenaryView    Keys=[val1_T_1,val2_T_1,val3_C_1,val4_S_1,val5_T_1,val6_C_1,val7_T_1]
Trenary view end
SecondaryView  Keys=[val1_S_1,val2_C_1,val3_C_1,val4_S_1,val5_S_1,val6_C_1]
MainView       Keys=[val1_C_1,val2_C_1,val3_C_1,val4_C_1,val5_C_1,val6_C_1]
Secondary view start
SecondaryView  Keys=[val1_S_2,val2_C_1,val3_C_1,val4_S_2,val5_S_2,val6_C_1]
Trenary view start
TrenaryView    Keys=[val1_T_1,val2_T_1,val3_C_1,val4_S_2,val5_T_1,val6_C_1,val7_T_1]
Trenary view end
SecondaryView  Keys=[val1_S_2,val2_C_1,val3_C_1,val4_S_2,val5_S_2,val6_C_1]
MainView       Keys=[val1_C_1,val2_C_1,val3_C_1,val4_C_1,val5_C_1,val6_C_1]
Main view end

Secondary layout end

Bottom capture:MainView1SecondaryView2TrenaryView3SecondaryView2TrenaryView3
Primary layout end
";

			Assert.That(outputString.ToString(), Is.EqualTo(expected));
		}

	}

}