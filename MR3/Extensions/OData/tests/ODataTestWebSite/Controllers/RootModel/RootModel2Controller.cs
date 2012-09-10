namespace ODataTestWebSite.Controllers.AggRootModel
{
    using Castle.MonoRail;

    [Area("models")]
    public partial class RootModel2Controller : ODataController<CodeRepositoryModel2>
    {
        public RootModel2Controller()
            : base(new CodeRepositoryModel2())
        {
        }

        public ActionResult Index()
        {
            return EmptyResult.Instance;
        }
    }
}