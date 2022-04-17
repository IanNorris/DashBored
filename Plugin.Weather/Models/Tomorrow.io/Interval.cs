
namespace Plugin.Weather.Models.Tomorrow.io
{
	public class Interval
	{
		public DateTime StartTime { get; set; }
		public Dictionary<string, decimal> Values { get; set; }
	}
}
