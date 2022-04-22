
namespace DashBored.PluginApi
{
	public interface IPluginSecrets
	{
		public void SetSecret(string name, string value, bool reinitializePlugin);
		public string GetSecret(string name);
	}
}
