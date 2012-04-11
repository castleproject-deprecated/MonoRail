namespace WebApplication1.Filters
{
	using System;
	using System.Web;
	using Castle.MonoRail;

	public class TestActionFilter : IActionFilter
	{
		public void BeforeAction(PreActionFilterExecutionContext context)
		{
			context.HttpContext.Response.Write("<!-- Before action filter -->");
		}

		public void AfterAction(AfterActionFilterExecutionContext context)
		{
			context.HttpContext.Response.Write("<!-- After action filter -->");
		}
	}

	public class TestAuthFilter : IAuthorizationFilter
	{
		public void AuthorizeRequest(PreActionFilterExecutionContext context)
		{
			context.HttpContext.Response.Write("<!-- auth filter -->");
		}
	}

	public class TestExceptionFilter : IExceptionFilter
	{
		public void HandleException(ExceptionFilterExecutionContext context)
		{
			// context.HttpContext.Response.Write("<!-- exception filter -->");

			context.ExceptionHandled = false;
		}
	}
}