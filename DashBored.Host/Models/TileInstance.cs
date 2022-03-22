using DashBored.PluginApi;

namespace DashBored.Host.Models
{
	public class TileInstance
	{
		public int X { get; set; }
		public int Y { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }

		public IPlugin PluginInstance { get; set; }
		public Type RazorType { get; set; }
		public CardStyle CardStyle { get; set; }
	}
}
