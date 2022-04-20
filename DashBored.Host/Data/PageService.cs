using DashBored.Host.Models;
using Newtonsoft.Json;

namespace DashBored.Host.Data
{
	public class PageService : IDisposable
	{
		public PageService(PluginLoader pluginLoader, SecretService secretService)
		{
			_pluginLoader = pluginLoader;
			_secretService = secretService;
		}

		public async Task LoadPages()
		{
			var folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DashBored");
			var configPath = Path.Combine(folderPath, "Layout.json");
			var secretsPath = Path.Combine(folderPath, "Secrets.json");

			if (!File.Exists(configPath))
			{
				Console.Error.WriteLine($"Layout file {configPath} does not exist.");
				Environment.Exit(1);
			}

			var fileContent = File.ReadAllText(configPath);

			var layout = JsonConvert.DeserializeObject<Layout>(fileContent);

			var page = new Page(layout, _pluginLoader);

			await page.Initialize(_secretService);

			Pages = new List<Page>()
			{
				page
			};

		}

		public void Dispose()
		{
			foreach(var page in Pages)
			{
				page.Dispose();
			}
		}

		public List<Page> Pages { get; private set; }

		private PluginLoader _pluginLoader;
		private SecretService _secretService;
	}
}
