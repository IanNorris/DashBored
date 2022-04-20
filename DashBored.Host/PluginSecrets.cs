using DashBored.Host.Data;
using DashBored.PluginApi;
using Microsoft.AspNetCore.DataProtection;

namespace DashBored.Host
{
	public class PluginSecrets : IPluginSecrets
	{
		public delegate void OnSecretUpdatedDelegate(IPluginSecrets pluginSecrets);

		public PluginSecrets(string keyName, SecretService secretService, IDataProtector dataProtector, OnSecretUpdatedDelegate onUpdated)
		{
			_keyName = keyName;
			_secretService = secretService;
			_dataProtector = dataProtector;
			_onUpdated = onUpdated;
		}

		public string GetSecret(string name)
		{
			var encryptedValue = _secretService.GetValue($"{_keyName}.{name}");
			if (encryptedValue != null)
			{
				return _dataProtector.Unprotect(encryptedValue);
			}

			return null;
		}

		public void SetSecret(string name, string value)
		{
			var encryptedValue = value != null ? _dataProtector.Protect(value) : null;
			_secretService.SetValue($"{_keyName}.{name}", encryptedValue);

			_onUpdated?.Invoke(this);
		}

		private string _keyName;
		private SecretService _secretService;
		private IDataProtector _dataProtector;
		private OnSecretUpdatedDelegate _onUpdated;
	}
}
