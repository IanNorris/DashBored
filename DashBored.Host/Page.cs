using System.Reflection;
using DashBored.Host.Models;
using DashBored.PluginApi;

namespace DashBored.Host
{
	public class Page
	{
		public const int Dimension = 12;

		public Page(Layout layout, PluginLoader loader)
		{
			var tileList = new List<TileInstance>();

			foreach(var tile in layout.Tiles)
			{
				var pluginInstance = loader.CreateInstance(tile.Plugin, tile.Data);
				var razorType = (Type)pluginInstance.GetType().GetProperty("RazorType", BindingFlags.Static | BindingFlags.Public).GetValue(null);

				var newColumn = new TileInstance()
				{
					Height = tile.Height,
					Width = tile.Width,
					PluginInstance = pluginInstance,
					RazorType = razorType,
					X = tile.X,
					Y = tile.Y,
				};

				tileList.Add(newColumn);
			}

			tileList = tileList.OrderBy(c => c.Y).ThenBy(c => c.X).ToList();

			Tiles = tileList;
		}

		public List<TileInstance> Tiles { get; private set; }
	}
}
