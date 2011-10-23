using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using Castle.Core;
using Castle.MonoRail.Framework.Container;
using Castle.MonoRail.Framework.Routing;
using Castle.MonoRail.Framework.Services;

namespace Castle.MonoRail.Framework.Helpers
{
	/// <summary>
	/// Helper to call actions from inside views.
	/// </summary>
	public class ActionHelper : AbstractHelper, IServiceEnabledComponent
	{
		private IMonoRailContainer container;

		/// <summary>
		/// Initializes a new instance of the <see cref="ActionHelper"/> class.
		/// </summary>
		public ActionHelper() { }
		/// <summary>
		/// Initializes a new instance of the <see cref="ActionHelper"/> class.
		/// setting the Controller, Context and ControllerContext.
		/// </summary>
		/// <param name="engineContext">The engine context.</param>
		public ActionHelper(IEngineContext engineContext) : base(engineContext) { }

		#region IServiceEnabledComponent Members

		/// <inheritdoc/>
		public void Service(IServiceProvider provider)
		{
			container = provider as IMonoRailContainer;
		}

		#endregion

		/// <summary>
		/// Invokes the specified child action and returns the result as an HTML string.
		/// </summary>
		/// <example>
		/// The following code uses nvelocity syntax:
		/// 
		/// <code>
		///  $action.render("%{action='RecommendedProducts'}")
		///  $action.render("%{action='ProductSummary', querystring='id=1'}")
		/// </code>
		/// 
		/// </example>
		/// 
		/// <param name="parameters">The parameters.</param>
		/// <returns>The child action result as an HTML string.</returns>
		public virtual string Render(IDictionary parameters)
		{
			using (var writer = new StringWriter(CultureInfo.InvariantCulture))
			{
				ActionRender(parameters, writer);
				return writer.ToString();
			}
		}

		/// <summary>
		/// Invokes the specified child action and renders the result inline in the parent view.
		/// </summary>
		/// <example>
		/// The following code uses nvelocity syntax:
		/// 
		/// <code>
		///  $action.renderinline("%{action='RecommendedProducts'}")
		///  $action.renderinline("%{action='ProductSummary', querystring='id=1'}")
		/// </code>
		/// 
		/// </example>
		/// 
		/// <param name="parameters">The parameters.</param>
		public virtual void RenderInline(IDictionary parameters)
		{
			ActionRender(parameters, Context.Response.Output);
		}

		private void ActionRender(IDictionary parameters, TextWriter writer)
		{
			UrlBuilderParameters urlBuilderParameters = UrlBuilderParameters.From(parameters);

			var urlInfo = new UrlInfo(urlBuilderParameters.Area, urlBuilderParameters.Controller, urlBuilderParameters.Action);
			IController controller = Context.Services.ControllerFactory.CreateController(urlBuilderParameters.Area,
																						 urlBuilderParameters.Controller);
			IControllerContext controllerContext = Context.Services.ControllerContextFactory.Create(urlBuilderParameters.Area,
																									urlBuilderParameters.
																										Controller,
																									urlBuilderParameters.Action,
																									Context.Services.
																										ControllerDescriptorProvider
																										.BuildDescriptor(
																											controller));

			IEngineContext engineContext = Context.Services.EngineContextFactory.Create(container, urlInfo,
																						Context.UnderlyingContext,
																						new RouteMatch());

			engineContext.CurrentController = controller;
			engineContext.CurrentControllerContext = controllerContext;

			var childMonoRailHttpHandler = new ChildMonoRailHttpHandler(engineContext, controller, controllerContext);

			Context.UnderlyingContext.Server.Execute(new HttpHandlerWrapper(childMonoRailHttpHandler), writer, true);
		}

		#region Nested type: ChildMonoRailHttpHandler

		internal class ChildMonoRailHttpHandler : BaseHttpHandler, IRequiresSessionState
		{
			/// <summary>
			/// Initializes a new instance of the <see cref="ChildMonoRailHttpHandler"/> class.
			/// </summary>
			/// <param name="engineContext">The engine context.</param>
			/// <param name="controller">The controller.</param>
			/// <param name="context">The context.</param>
			public ChildMonoRailHttpHandler(IEngineContext engineContext, IController controller, IControllerContext context)
				: base(engineContext, controller, context, false)
			{
			}
		}

		#endregion

		#region Nested type: HttpHandlerWrapper

		/// <summary>
		/// Server.Execute() requires that the provided IHttpHandler subclasses from <see cref="Page"/>.
		/// </summary>
		internal class HttpHandlerWrapper : Page
		{
			private readonly IHttpHandler httpHandler;

			/// <summary>
			/// Initializes a new instance of the <see cref="HttpHandlerWrapper"/> class.
			/// </summary>
			/// <param name="httpHandler">The child <see cref="IHttpHandler"/>.</param>
			public HttpHandlerWrapper(IHttpHandler httpHandler)
			{
				this.httpHandler = httpHandler;
			}

			/// <inheritdoc/>
			public override void ProcessRequest(HttpContext context)
			{
				try
				{
					httpHandler.ProcessRequest(context);
				}
				catch (HttpException ex)
				{
					if (ex.GetHttpCode() == 500)
					{
						throw; // doesn't need to be wrapped
					}

					throw new HttpException(500,
											"An exception occurred while trying to execute either ActionHelper.Render or ActionHelper.RenderInline.",
											ex);
				}
			}
		}

		#endregion
	}
}