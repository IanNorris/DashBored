namespace DashBored.Host
{
	public class PluginTimer : IDisposable
	{
		public delegate Task TimerDelegage(int TimerIndex);

		private Timer _timer;

		public PluginTimer(int timerIndex, int period, TimerDelegage callback)
		{
			_timer = new Timer(
				async (object _) =>
				{
					await callback(timerIndex);
				},
				new AutoResetEvent(false),
				TimeSpan.FromMilliseconds(period),
				TimeSpan.FromMilliseconds(period)
			);
		}

		public void Dispose()
		{
			_timer?.Dispose();
		}
	}
}
