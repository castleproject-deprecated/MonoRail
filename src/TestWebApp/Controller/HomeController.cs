﻿namespace TestWebApp.Controller
{
	using Castle.MonoRail;

	public class HomeController
	{
		public object Index()
		{
			return new StringResult("Line Lanley");
		}
	}
}