namespace Castle.MonoRail.Extension.OData
{
	using System;
	using System.Collections.Generic;
	using System.Data.Services.Providers;
	using System.Web;

	[Flags]
	public enum EntitySetPermission
	{
		ReadOnly = 0
//		None = 0,
//		ReadSingle = 1,
//		ReadMultiple = 2,
//		AllRead = ReadMultiple | ReadSingle,
//		WriteAppend = 4,
//		WriteReplace = 8,
//		WriteDelete = 16,
//		WriteMerge = 32,
//		AllWrite = WriteMerge | WriteDelete | WriteReplace | WriteAppend,
//		All = AllWrite | AllRead,
	}

	public abstract class ODataController<T> where T : ODataModel
	{
		public T Model { get; protected set; }

		protected ODataController(T model)
		{
			Model = model;
		}

		public ActionResult Process(string GreedyMatch, HttpRequestBase requestBase)
		{
			// TODO: pass this along
			var query = requestBase.Url.Query;

			// Parse and bind segments
			var segments = new SegmentParser().ParseAndBind(GreedyMatch, this.Model);

			// Process query

			// Render output

			return new JsonResult(new { Test = GreedyMatch });
		}
	}
}
