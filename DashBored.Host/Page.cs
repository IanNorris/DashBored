using DashBored.Host.Data;
using DashBored.Host.Models;

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
				var pluginInstance = loader.CreateInstance(tile.Plugin, tile.Data, tile.Title);
				var razorType = pluginInstance.RazorType;
				var cardStyle = pluginInstance.CardStyle;

				var newColumn = new TileInstance()
				{
					Height = tile.Height,
					Width = tile.Width,
					Title = tile.Title,
					PluginInstance = pluginInstance,
					PluginNamespace = $"{tile.Plugin}.{tile.Title}",
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

		public async Task Initialize(SecretService secretService)
		{
			_timers = new List<PluginTimer>();

			foreach (var tile in Tiles)
			{
				tile.PluginSecrets = secretService.CreatePluginSecrets(tile.PluginNamespace, async ps =>
				{
					await tile.PluginInstance.OnInitialize(ps);
				});

				await tile.PluginInstance.OnInitialize(tile.PluginSecrets);

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
