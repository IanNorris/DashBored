
namespace DashBored.PluginApi
{
	public interface IPlugin
	{
		static Type DataType { get; }
		Type RazorType { get; }
		public CardStyle CardStyle { get; }
	}
}
