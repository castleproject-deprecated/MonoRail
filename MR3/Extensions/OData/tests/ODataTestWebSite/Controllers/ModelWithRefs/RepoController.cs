namespace ODataTestWebSite.Controllers.ModelWithRefs
{
	using Castle.MonoRail;

	public class RepoController : ODataController<CodeRepositoryModel>
	{
		public RepoController() : base(new CodeRepositoryModel())
		{
		}
	}
}