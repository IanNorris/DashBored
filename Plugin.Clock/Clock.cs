using DashBored.PluginApi;

namespace Plugin.Clock
{
	public class Clock : IPlugin
	{
		public static Type DataType => typeof(ClockData);
		public Type RazorType => typeof(ClockView);
		public CardStyle CardStyle => new CardStyle
		{
			Classes = "clock",
			Padding = false,
		};

		public Clock(ClockData _)
		{

		}
	}
}
