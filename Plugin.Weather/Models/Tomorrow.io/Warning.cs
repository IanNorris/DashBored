
namespace Plugin.Weather.Models.Tomorrow.io
{
	public class Warning
	{
		public int Code { get; set; }
		public string Type { get; set; }
		public string Message { get; set; }
		public Dictionary<string, string> Meta { get; set; }
	}
}
