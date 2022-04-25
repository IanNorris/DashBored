using DashBored.MicrosoftGraph.Models;

namespace Plugin.Calendar
{
	public class NamedCalendar
	{
		public string Color { get; set; }
	}

	public class CalendarData
	{
		public AzureAD AzureAD { get; set; }
		public Dictionary<string, NamedCalendar> Calendars { get; set; } = new Dictionary<string, NamedCalendar>();
	}
}
