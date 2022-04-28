using DashBored.PluginApi;
using Newtonsoft.Json;

namespace DashBored.Host.Data
{
	public class SettingsService : ISettingsService
	{
		public SettingsService()
		{
			_root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), SettingsFolder);
			Directory.CreateDirectory(_root);
		}

		public string GetSettingsPath()
		{
			return _root;
		}

		public string GetSettingsContent(string filename, bool optional)
		{
			var path = Path.Combine(_root, filename);

			if (File.Exists(path))
			{
				var fileContent = File.ReadAllText(path);

				return fileContent;
			}
			else
			{
				if (!optional)
				{
					Console.Error.WriteLine($"File {filename} was not found.");
					Environment.Exit(1);
				}

				return null;
			}
		}

		public T GetSettingsObject<T>(string filename, bool optional)
		{
			var content = GetSettingsContent(filename, optional);

			if(content == null)
			{
				return default;
			}

			return JsonConvert.DeserializeObject<T>(content);
		}

		public void WriteSettingsContent(string filename, string content)
		{
			var path = Path.Combine(_root, filename);
			File.WriteAllText(path, content);
		}

		public void WriteSettingsObject(string filename, object content)
		{
			var fileContent = JsonConvert.SerializeObject(content);
			WriteSettingsContent(filename, fileContent);
		}

		private const string SettingsFolder = "DashBored";

		private readonly string _root;
	}
}
