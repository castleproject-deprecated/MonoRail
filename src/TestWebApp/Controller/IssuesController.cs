#region License
//  Copyright 2004-2010 Castle Project - http://www.castleproject.org/
//  
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//  
//      http://www.apache.org/licenses/LICENSE-2.0
//  
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
// 
#endregion
namespace TestWebApp.Controller
{
	using Castle.MonoRail;
	using Castle.MonoRail.Mvc;
	using Castle.MonoRail.Mvc.Rest;
	using Model;

	// optional [RespondTo()]
	public class IssuesController
	{
		private readonly ContentNegotiator _contentNegotiator;
		private readonly ControllerContext _ctx;

		public IssuesController(ContentNegotiator contentNegotiator, ControllerContext controllerContext)
		{
			_contentNegotiator = contentNegotiator;
			_ctx = controllerContext;
			// _contentNegotiator.Allow();
		}

		// [HttpVerbs()]
		public ActionResult Index(int id)
		{
			var issue = new Issue() { Id = id, Title = "Some error"} ;
			_ctx.Data.MainModel = new Resource<Issue>(issue);

			return _contentNegotiator.Respond(format =>
												  {
													  format.Html();
													  format.JSon();
													  format.Xml();
												  });
		}
	}
}
