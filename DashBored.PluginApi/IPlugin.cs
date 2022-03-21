
namespace DashBored.PluginApi
{
	public interface IPlugin
	{
		static Type DataType { get; }
		static Type RazorType { get; }
	}
}
