using DashBored.Host.Models;
using DashBored.PluginApi;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace DashBored.Host.Data
{
	public class PageService : IDisposable
	{
		public PageService(ISettingsService settingsService, PluginLoader pluginLoader, SecretService secretService)
		{
			_settingsService = settingsService;
			_pluginLoader = pluginLoader;
			_secretService = secretService;
		}

		public async Task LoadPages(IServiceProvider serviceProvider)
		{
			var layout = _settingsService.GetSettingsObject<Layout>("Layout.json", false);

			var page = new Page(layout, _pluginLoader, serviceProvider);

			await page.Initialize(_secretService);

			Pages = new List<Page>()
			{
				page
			};

		}

		public List<string> Scripts
		{
			get
			{
				var set = new HashSet<string>();
				foreach(var page in Pages)
				{
					foreach(var tile in page.Tiles)
					{
						if(tile.PluginInstance.ScriptPaths != null)
						{
							foreach(var script in tile.PluginInstance.ScriptPaths)
							{
								set.Add($"_content/{tile.PluginInstance.GetType().Namespace}/{script}");
							}
						}
					}
				}

				return set.ToList();
			}
		}

		public List<string> Stylesheets
		{
			get
			{
				var set = new HashSet<string>();
				foreach (var page in Pages)
				{
					foreach (var tile in page.Tiles)
					{
						if (tile.PluginInstance.StylesheetPaths != null)
						{
							foreach (var stylesheet in tile.PluginInstance.StylesheetPaths)
							{
								set.Add($"_content/{tile.PluginInstance.GetType().Namespace}/{stylesheet}");
							}
						}
					}
				}

				return set.ToList();
			}
		}

		public void Dispose()
		{
			foreach(var page in Pages)
			{
				page.Dispose();
			}
		}

		public List<Page> Pages { get; private set; } = new List<Page>();

		private ISettingsService _settingsService;
		private PluginLoader _pluginLoader;
		private SecretService _secretService;
	}
}
