namespace ODataTestWebSite.Controllers.AggRootModel
{
	using System.Collections.Generic;
	using System.ComponentModel.DataAnnotations;
	using System.Linq;
	using Castle.MonoRail;

	public class Revision
	{
		[Key]
		public int Id { get; set; }
		public string FileName { get; set; }
		public int UserId { get; set; }
	}

	public class Branch
	{
		[Key]
		public int Id { get; set; }
		public IList<Revision> Revisions { get; set; }
	}

	public class Repository
	{
		[Key]
		public int Id { get; set; }
		public string Name { get; set; }
		public IList<Branch> Branches { get; set; }
	}

	public class CodeRepositoryModel : ODataModel
	{
		public CodeRepositoryModel()
			: base("ns", "container")
		{
			var source = new List<Repository>();
			this.EntitySet("Repositories", source.AsQueryable());
		}
	}

	public class AggRootModelController : ODataController<CodeRepositoryModel>
	{
		public AggRootModelController() : base(new CodeRepositoryModel())
		{
		}
	}

	public class CodeRepositorySubController : ODataEntitySubController<CodeRepositoryModel>
	{
		public ActionResult Create(Model<CodeRepositoryModel> repos)
		{
			return EmptyResult.Instance;
		}
	}

	public class BranchRepositorySubController : ODataEntitySubController<CodeRepositoryModel>
	{
		public ActionResult Create(CodeRepositoryModel repos, Model<Branch> branch)
		{
			return EmptyResult.Instance;
		}
	}

	public class RevisionRepositorySubController : ODataEntitySubController<CodeRepositoryModel>
	{
		public ActionResult Create(CodeRepositoryModel repos, Branch branch, Model<Revision> revision)
		{
			return EmptyResult.Instance;
		}
	}

}