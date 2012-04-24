namespace ODataTestWebSite.Controllers.AggRootModel
{
	using Castle.MonoRail;

	public partial class BranchRepositoryController : ODataEntitySubController<Branch>
	{


		// not meaningful for odata, but we need the context
		public ActionResult New(Repository parent)
		{
			return new ViewResult();
		}

		public ActionResult Post_Create(Repository repos, Model<Branch> branch)
		{
			return EmptyResult.Instance;
		}
	}
}