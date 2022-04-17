
namespace DashBored.PluginApi
{
	public interface IPlugin
	{
		public delegate void OnDataChangedDelegate();

		static Type DataType { get; }
		Type RazorType { get; }
		public CardStyle CardStyle { get; }
		public Dictionary<int, int> TimerFrequencies { get; }
		public string Error { get; set; }

		public Task<bool> OnInitialize();
		public Task<bool> OnTimer(int Timer);
		public OnDataChangedDelegate OnDataChanged { get; set; }
	}
}
