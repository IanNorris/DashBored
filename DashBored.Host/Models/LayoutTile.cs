using Newtonsoft.Json.Linq;

namespace DashBored.Host.Models
{
	public class LayoutTile
	{
		public string? Title { get; set; }

		public int X { get; set; }
		public int Y { get; set; }

		public int Width { get; set; }
		public int Height { get; set; }

		public string? Plugin { get; set; }
		public JObject Data { get; set; }
	}
}
