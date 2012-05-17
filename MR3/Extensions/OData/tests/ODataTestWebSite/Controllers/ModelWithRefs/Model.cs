namespace ODataTestWebSite.Controllers.ModelWithRefs
{
	using System.Collections.Generic;
	using System.ComponentModel.DataAnnotations;

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
		public string Name { get; set; }
		// public Ref<Repository> Repository { get; set; }
		public IList<Ref<Revision>> Revisions { get; set; }
	}

	public class Repository
	{
		[Key]
		public int Id { get; set; }
		public string Name { get; set; }
		public IList<Ref<Branch>> Branches { get; set; }
	}

	public class Ref<T> where T : class
	{
		private T _entity;

		public Ref(T entity)
		{
			_entity = entity;
		}

		public T Entity
		{
			get { return _entity; }
			set { _entity = value;  }
		}

		public int Id { get; set; }
	}
}