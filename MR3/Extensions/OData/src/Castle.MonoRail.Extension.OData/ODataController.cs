namespace Castle.MonoRail.Extension.OData
{
	using System;

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
		public ActionResult Process(string GreedyMatch)
		{
			// Parse and bind segments
			// var segments = ParseAndBindSegments(GreedyMatch);

			// Process query

			// Render output

			return new JsonResult(new { Test = GreedyMatch });
		}
	}
}
