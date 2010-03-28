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

namespace Castle.MonoRail.Framework.Adapters
{
	using System;
	using System.Security.Principal;
	using System.Web;
	using System.Collections;
	using Castle.MonoRail.Framework;
	using Castle.MonoRail.Framework.Container;

	/// <summary>
	/// Adapter to expose a valid <see cref="IEngineContext"/> 
	/// implementation on top of <c>HttpContext</c>.
	/// </summary>
	public class DefaultEngineContext : AbstractServiceContainer, IEngineContext
	{
		private readonly IMonoRailContainer container;

		/// <summary>
		/// Initializes a new instance of the <see cref="DefaultEngineContext"/> class.
		/// </summary>
		/// <param name="container">The container.</param>
		/// <param name="urlInfo">Url information</param>
		/// <param name="context">The context.</param>
		/// <param name="server">The server.</param>
		/// <param name="request">The request.</param>
		/// <param name="response">The response.</param>
		/// <param name="trace">The trace.</param>
		/// <param name="session">The session.</param>
		public DefaultEngineContext(IMonoRailContainer container, UrlInfo urlInfo, HttpContext context, IServerUtility server,  IRequest request, IResponse response, ITrace trace,  IDictionary session)
			: base(container)
		{
			this.container = container;
			this.UnderlyingContext = context;
			this.UrlInfo = urlInfo;
			this.Request = request;
			this.Response = response;
			this.Session = session;
			this.Server = server;
			this.Trace = trace;
		}

		/// <summary>
		/// Gets the underlying context of the API being used.
		/// </summary>
		/// <value></value>
		public HttpContext UnderlyingContext { get; private set; }

		/// <summary>
		/// Gets a reference to the MonoRail services.
		/// </summary>
		/// <value>The services.</value>
		public IMonoRailServices Services
		{
			get { return container; }
		}

		/// <summary>
		/// Gets the last exception raised during
		/// the execution of an action.
		/// </summary>
		/// <value></value>
		public Exception LastException { get; set; }

		/// <summary>
		/// Access the session objects.
		/// </summary>
		/// <value></value>
		public IDictionary Session { get; set; }

		/// <summary>
		/// Gets the request object.
		/// </summary>
		/// <value></value>
		public IRequest Request { get; private set; }

		/// <summary>
		/// Gets the response object.
		/// </summary>
		/// <value></value>
		public IResponse Response { get; private set; }

		/// <summary>
		/// Gets the trace object.
		/// </summary>
		/// <value></value>
		public ITrace Trace { get; private set; }

		/// <summary>
		/// Returns an <see cref="IServerUtility"/>.
		/// </summary>
		/// <value></value>
		public IServerUtility Server { get; private set; }

		/// <summary>
		/// Access a dictionary of volative items.
		/// </summary>
		/// <value></value>
		public Flash Flash { get; set; }

		/// <summary>
		/// Gets or sets the current user.
		/// </summary>
		/// <value></value>
		public IPrincipal CurrentUser
		{
			get { return UnderlyingContext.User; }
			set { UnderlyingContext.User = value; }
		}

		/// <summary>
		/// Returns the <see cref="UrlInfo"/> of the the current request.
		/// </summary>
		/// <value></value>
		public UrlInfo UrlInfo { get; private set; }

		/// <summary>
		/// Returns the application virtual path.
		/// </summary>
		/// <value></value>
		public String ApplicationPath
		{
			get
			{
				String path;

				if (UnderlyingContext != null)
				{
					path = UnderlyingContext.Request.ApplicationPath;

					if ("/".Equals(path))
					{
						path = String.Empty;
					}
				}
				else
				{
					// Huh? That is supposed to be the virtual path
					path = AppDomain.CurrentDomain.BaseDirectory;
				}

				return path;
			}
		}

		/// <summary>
		/// Returns the physical application path.
		/// </summary>
		public String ApplicationPhysicalPath
		{
			get 
			{ 
				String path;

				if (UnderlyingContext != null)
				{
					path = UnderlyingContext.Request.PhysicalApplicationPath;
				}
				else
				{
					path = AppDomain.CurrentDomain.BaseDirectory;
				}

				return path;
			}
		}

		/// <summary>
		/// Returns the Items collection from the current HttpContext.
		/// </summary>
		/// <value></value>
		public IDictionary Items
		{
			get { return UnderlyingContext.Items; }
		}

		/// <summary>
		/// Gets or sets the current controller.
		/// </summary>
		/// <value>The current controller.</value>
		public IController CurrentController { get; set; }

		/// <summary>
		/// Gets or sets the current controller context.
		/// </summary>
		/// <value>The current controller context.</value>
		public IControllerContext CurrentControllerContext { get; set; }


	}
}