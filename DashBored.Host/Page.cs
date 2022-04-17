using System.Reflection;
using DashBored.Host.Models;
using DashBored.PluginApi;
using Microsoft.AspNetCore.Components;

namespace DashBored.Host
{
	public class Page : IDisposable
	{
		public const int Dimension = 12;

		public Page(Layout layout, PluginLoader loader)
		{
			var tileList = new List<TileInstance>();

			foreach(var tile in layout.Tiles)
			{
				var pluginInstance = loader.CreateInstance(tile.Plugin, tile.Data);
				var razorType = pluginInstance.RazorType;
				var cardStyle = pluginInstance.CardStyle;

				var newColumn = new TileInstance()
				{
					Height = tile.Height,
					Width = tile.Width,
					PluginInstance = pluginInstance,
					RazorType = razorType,
					CardStyle = cardStyle,
					X = tile.X,
					Y = tile.Y,
				};

				tileList.Add(newColumn);
			}

			tileList = tileList.OrderBy(c => c.Y).ThenBy(c => c.X).ToList();

			Tiles = tileList;
		}

		public async Task Initialize()
		{
			_timers = new List<PluginTimer>();

			foreach (var tile in Tiles)
			{
				await tile.PluginInstance.OnInitialize();

				foreach(var timer in tile.PluginInstance.TimerFrequencies)
				{
					_timers.Add(new PluginTimer(timer.Key, timer.Value, async (int timerIndex) => {
						var result = await tile.PluginInstance.OnTimer(timerIndex);
					}));
				}
			}
		}

		public void Dispose()
		{
			foreach(var timer in _timers)
			{
				timer.Dispose();
			}
		}

		public List<TileInstance> Tiles { get; private set; }

		private List<PluginTimer> _timers;
	}
}
