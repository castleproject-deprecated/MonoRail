namespace TestWebApp.Model
{
	using System;

	// domain remains isolated from representations

	public class Issue
    {
		public DateTime CreatedAt { get; set; }

		public int Id { get; set; }

		public string Title { get; set; }
    }
}