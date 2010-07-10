// Copyright 2004-2010 Castle Project - http://www.castleproject.org/
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

using System;

namespace TestSiteNVelocity.Controllers
{
	using Castle.MonoRail.Framework;

	[Filter(ExecuteWhen.BeforeAction, typeof(FilterBadHeader))]	
	public class FilterController : Controller
	{
		public delegate string Output();

		private Output output;

		public void Index()
		{
		}

		public IAsyncResult BeginIndex2()
		{
			return CallAsync();
		}

		public void EndIndex2()
		{
			var s = output.EndInvoke(ControllerContext.Async.Result);
			RenderText(s);
		}

		private IAsyncResult CallAsync()
		{
			output = GetString;
			return output.BeginInvoke(
				ControllerContext.Async.Callback,
				ControllerContext.Async.State);
		}

		public string GetString()
		{
			return "value from async task";
		}
	}

	public class FilterBadHeader : IFilter
	{
		#region IFilter Members

		public bool Perform(ExecuteWhen exec, IEngineContext context, IController controller, IControllerContext controllerContext)
		{
			if (context.Request.Headers["mybadheader"] != null)
			{
				context.Response.Write("Denied!");

				return false;
			}

			context.Response.Write("Allowed!");

			return true;
		}

		#endregion
	}
}
