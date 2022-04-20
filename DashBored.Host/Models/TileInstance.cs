using DashBored.PluginApi;

namespace DashBored.Host.Models
{
	public class TileInstance
	{
		public string Id = Guid.NewGuid().ToString();
		public int X { get; set; }
		public int Y { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
		public string Title { get; set; }
		public string PluginNamespace { get; set; }
		public IPlugin PluginInstance { get; set; }
		public Type RazorType { get; set; }
		public CardStyle CardStyle { get; set; }
		public IPluginSecrets PluginSecrets { get; set; }
	}
}
