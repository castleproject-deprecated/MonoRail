namespace ODataTestWebSite.Controllers.AggRootModel
{
	using System.Collections.Generic;
	using System.ComponentModel.DataAnnotations;
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

	public partial class AggRootModelController : ODataController<CodeRepositoryModel>
	{
		public AggRootModelController() : base(new CodeRepositoryModel())
		{
		}
	}

	// /repositories(1)/branches(2)/revisions
}