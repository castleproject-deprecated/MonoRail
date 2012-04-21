namespace ODataTestWebSite.Controllers.AggRootModel
{
	using System.Collections.Generic;
	using System.ComponentModel.DataAnnotations;
	using System.Linq;
	using Castle.MonoRail;

	#region Model

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
	
	#endregion

	public class CodeRepositoryModel : ODataModel
	{
		public CodeRepositoryModel() : base("ns", "container")
		{
			var source = new List<Repository>()
			             	{
			             		new Repository() { Id = 1, Name = "repo1", 
									Branches = new List<Branch>{ 
										new Branch() { Id = 100, 
											Revisions = new List<Revision>()
										            	{
										            		new Revision() { FileName = "File1", Id = 3000, UserId = 102 }, 
															new Revision() { FileName = "File2", Id = 3001, UserId = 102 },
															new Revision() { FileName = "File1", Id = 3002, UserId = 101 },
										            	}},
										new Branch() { Id = 101, 
											Revisions = new List<Revision>()
										            	{
										            		new Revision() { FileName = "File31", Id = 4000, UserId = 102 }, 
															new Revision() { FileName = "File21", Id = 4001, UserId = 102 },
															new Revision() { FileName = "File11", Id = 4002, UserId = 101 },
										            	}}} },
			             	};
			this.EntitySet("Repositories", source.AsQueryable());
		}
	}

	public class AcceptableMedia
	{
		public IEnumerable<string> Preferred { get; private set; }
		public IEnumerable<string> Accept { get; private set; }

		public bool Prefers(string media)
		{
			return false;
		}
		
		public bool PrefersOver(string media, string overThisMedia)
		{
			return false;
		}

		public bool Accepts(string media)
		{
			return false;
		}
	}

	public class AggRootModelController : ODataController<CodeRepositoryModel>
	{
		public AggRootModelController() : base(new CodeRepositoryModel())
		{
		}
	}

	public class CodeRepositorySubController : ODataEntitySubController<Repository>
	{
		public ActionResult Access(Repository repos, AcceptableMedia accepts, bool isLastResourceInPath)
		{
			if (isLastResourceInPath && accepts.Prefers(MediaTypes.Html))
			{
				return new ViewResult();
			}

			return EmptyResult.Instance;
		}

		// not meaningful for odata
		public ActionResult New()
		{
			return new ViewResult();
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

	public class BranchRepositorySubController : ODataEntitySubController<Branch>
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

	public class RevisionRepositorySubController : ODataEntitySubController<Revision>
	{
		public ActionResult Create(Repository repos, Branch branch, Model<Revision> revision)
		{
			return EmptyResult.Instance;
		}
	}

}