using DashBored.PluginApi;

namespace Plugin.Clock
{
	public class Clock : IPlugin
	{
		public static Type DataType => typeof(ClockData);
		public static Type RazorType => typeof(ClockView);

		public Clock(ClockData data)
		{

		}
	}
}
