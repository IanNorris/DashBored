
namespace DashBored.PluginApi
{
	public interface ISettingsService
	{
		string GetSettingsPath();

		string GetSettingsContent(string filename, bool optional);
		T GetSettingsObject<T>(string filename, bool optional);

		void WriteSettingsContent(string filename, string content);
		void WriteSettingsObject(string filename, object content);
	}
}
