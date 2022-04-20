using DashBored.PluginApi;

namespace Plugin.Clock
{
	public class Clock : IPlugin
	{
		public static Type DataType => typeof(ClockData);
		public Type RazorType => typeof(ClockView);
		public IDictionary<int, int> TimerFrequencies => new Dictionary<int, int>
		{
			{ 0, 1000 },
		};

		public IEnumerable<Secret> Secrets => new List<Secret>
		{

		};

		public CardStyle CardStyle => new CardStyle
		{
			Classes = "clock",
			Padding = false,
		};
		public string Error { get; set; }

		public IPlugin.OnDataChangedDelegate OnDataChanged { get; set; }

		public Task<bool> OnInitialize(IPluginSecrets pluginSecrets)
		{
			Error = null;
			return Task.FromResult(true);
		}

		public Task<bool> OnTimer(int _)
		{
			PrettyTime = DateTime.Now.ToString("HH:mm:ss");

			if (OnDataChanged != null)
			{
				OnDataChanged.Invoke();
			}

			return Task.FromResult(true);
		}

		public Clock(ClockData _, string title)
		{

		}

		public string PrettyTime { get; set; }
	}
}
