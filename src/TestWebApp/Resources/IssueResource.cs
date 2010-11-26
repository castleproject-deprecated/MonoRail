namespace TestWebApp.Resources
{
	using System;
	using Castle.MonoRail;
	using Model;

	public class IssueResource : Resource<Issue>
	{
		public IssueResource(Issue value) : base(value)
		{
			AddRelation(new ResourceRelation("self", new Uri("/get")));
		}
	}
}