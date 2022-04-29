using System.Reflection;
using DashBored.PluginApi;
using Microsoft.AspNetCore.DataProtection;

namespace DashBored.Host.Data
{
	public class SecretService
	{
		public SecretService(ISettingsService settingsService, IDataProtectionProvider dataProtectionProvider)
		{
			_settingsService = settingsService;
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

		private void LoadSecrets()
		{
			var newSecrets = _settingsService.GetSettingsObject<Dictionary<string, string>>(SecretsName, true);

			lock (this)
			{
				if (newSecrets == null)
				{
					_secrets = new Dictionary<string, string>();
				}
				else
				{
					_secrets = newSecrets;
				}
			}
		}

		private void SaveSecrets()
		{
			_settingsService.WriteSettingsObject(SecretsName, _secrets);
		}

		private const string SecretsName = "Secrets.json";

		private ISettingsService _settingsService;
		private IDataProtectionProvider _dataProtectionProvider;
		private Dictionary<string, string> _secrets = new Dictionary<string, string>();
	}
}
