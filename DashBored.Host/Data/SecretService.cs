using System.Reflection;
using DashBored.PluginApi;
using Microsoft.AspNetCore.DataProtection;
using Newtonsoft.Json;

namespace DashBored.Host.Data
{
	public class SecretService
	{
		public SecretService(IDataProtectionProvider dataProtectionProvider)
		{
			_dataProtectionProvider = dataProtectionProvider;

			LoadSecrets();
		}

		public IPluginSecrets CreatePluginSecrets(string pluginNamespace, PluginSecrets.OnSecretUpdatedDelegate onUpdated)
		{
			var appName = Assembly.GetExecutingAssembly().FullName;
			var protector = _dataProtectionProvider.CreateProtector(appName, new string[] { pluginNamespace });
			return new PluginSecrets(pluginNamespace, this, protector, onUpdated);
		}

		public void SetValue(string fullPath, string encryptedValue)
		{
			lock (this)
			{
				var exists = _secrets.ContainsKey(fullPath);
				if (exists && encryptedValue == null)
				{
					_secrets.Remove(fullPath);
					SaveSecrets();
				}
				else if (encryptedValue != null)
				{
					_secrets[fullPath] = encryptedValue;
					SaveSecrets();
				}
			}
		}

		public string GetValue(string fullPath)
		{
			if(_secrets.TryGetValue(fullPath, out var value))
			{
				return value;
			}

			return null;
		}

		public string GetPath()
		{
			var folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DashBored");
			var secretsPath = Path.Combine(folderPath, "Secrets.json");

			return secretsPath;
		}

		private void LoadSecrets()
		{
			var secretsPath = GetPath();

			if (!File.Exists(secretsPath))
			{
				_secrets = new Dictionary<string, string>();
				return;
			}

			var fileContent = File.ReadAllText(secretsPath);

			lock (this)
			{
				_secrets = JsonConvert.DeserializeObject<Dictionary<string, string>>(fileContent);
			}
		}

		private void SaveSecrets()
		{
			var secretsPath = GetPath();

			var fileContent = JsonConvert.SerializeObject(_secrets);
			File.WriteAllText(secretsPath, fileContent);
		}

		private IDataProtectionProvider _dataProtectionProvider;
		private Dictionary<string, string> _secrets = new Dictionary<string, string>();
	}
}
