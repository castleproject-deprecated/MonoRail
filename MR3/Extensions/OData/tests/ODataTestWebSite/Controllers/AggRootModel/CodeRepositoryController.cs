namespace ODataTestWebSite.Controllers.AggRootModel
{
	using System.Collections.Generic;
	using System.Security.Principal;
	using System.Web;
	using Castle.MonoRail;

	public partial class CodeRepositoryController : ODataEntitySubController<Repository>
	{
		public ActionResult Authorize(Repository repos, IPrincipal user, HttpRequestBase requestBase)
		{
			return EmptyResult.Instance;
		}

		public ActionResult AuthorizeMany(IEnumerable<Repository> repos, IPrincipal user, HttpRequestBase requestBase)
		{
			return EmptyResult.Instance;
		}

		// not meaningful for odata
		public ActionResult New()
		{
			return new ViewResult();
		}

		public ActionResult Get_View(Repository repos)
		{
			return EmptyResult.Instance;
		}

		public ActionResult ViewMany(IEnumerable<Repository> repos)
		{
			return EmptyResult.Instance;
		}

		public ActionResult Post_Create(Model<Repository> repos)
		{
			var newModel = repos.Value;
			newModel.Id = 1000;

			return new ContentNegotiatedResult<Repository>(newModel);
		}

		// not meaningful for odata
		public ActionResult Edit(int id)
		{
			var repo = new Repository();

			return new ViewResult<Repository>(repo) { };
		}

		public ActionResult Put_Update(Model<Repository> repos)
		{
			// pretend to save it
			return EmptyResult.Instance;
		}

		[HttpMethod(HttpVerb.Delete)]
		public ActionResult Remove(Repository repos)
		{
			// pretend to delete it
			return EmptyResult.Instance;
		}
	}
}