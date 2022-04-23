namespace Plugin.Calendar.Models
{
	public class FullCalendarEvent
	{
		public string Title { get; set; } = "Unknown";
		public DateTime Start { get; set; }
		public DateTime End { get; set; }
		public bool AllDay { get; set; }
		public string ClassName { get; set; } = "";
	}
}
