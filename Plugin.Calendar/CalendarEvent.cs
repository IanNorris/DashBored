
namespace Plugin.Calendar
{
	public class CalendarEvent
	{
		public string Title { get; set; }
		public string Color { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public bool AllDay { get; set; }
	}
}
