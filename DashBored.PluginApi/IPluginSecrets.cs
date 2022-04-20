
namespace DashBored.PluginApi
{
	public interface IPluginSecrets
	{
		public void SetSecret(string name, string value);
		public string GetSecret(string name);
	}
}
