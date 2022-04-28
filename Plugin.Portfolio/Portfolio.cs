using DashBored.PluginApi;
using DashBored.TradingView;
using DashBored.TradingView.Models;

namespace Plugin.Portfolio
{
	public class Portfolio : IPlugin
	{
		public static Type DataType => typeof(PortfolioData);
		public Type RazorType => typeof(PortfolioView);
		public CardStyle CardStyle => new CardStyle
		{
			Classes = "portfolio-card",
			Padding = true,
		};

		public IDictionary<int, int> TimerFrequencies => new Dictionary<int, int>
		{
			{ 0, 1 * 1000 } // 1s
		};

		public IEnumerable<Secret> Secrets => new List<Secret>
		{
			
		};

		public IEnumerable<string> ScriptPaths => null;
		public IEnumerable<string> StylesheetPaths => null;

		public string Error { get; set; }

		public IPlugin.OnDataChangedDelegate OnDataChanged { get; set; }

		PortfolioData _data;

		public Portfolio(PortfolioData data, string title)
		{
			_data = data;
		}

		public async Task<bool> OnInitialize(IPluginSecrets pluginSecrets)
		{
			_hasSession = false;
			_webSocket = new TVWebSocket();
			StartQuote();

			await UpdateData();

			Error = null;
			return true;
		}

		private void StartQuote()
		{
			_hasSession = true;
			_lastUpdate = DateTime.UtcNow;

			var sessionId = _webSocket.CreateQuoteSession((data) =>
			{
				lock (StockData)
				{
					_hasSession = true;
					_lastUpdate = DateTime.UtcNow;
					_updatesThisPeriod++;

					if (StockData.TryGetValue(data.Name, out var existing))
					{
						existing.Data.UpdateFrom(data.Data);
					}
					else
					{
						StockData.Add(data.Name, data);
					}
				}

				OnDataChanged?.Invoke();
			}, new string[] { "chp", "description", "status", "short_name", "logoid", "rchp" });

			if (_data.Stocks != null)
			{
				foreach (var stock in _data.Stocks)
				{
					_webSocket.AddSymbol(sessionId, stock);
				}
			}
		}

		public Task<bool> OnTimer(int _)
		{
			var currentTime = DateTime.UtcNow;
			var lastUpdate = _lastUpdate;
			var delta = currentTime - lastUpdate;
			if (_hasSession && delta > TimeSpan.FromSeconds(10))
			{
				Console.Out.WriteLine($"[TradingView] {delta.TotalSeconds} elapsed without updates, restarting quote.");

				_hasSession = false;

				StartQuote();
			}
			UpdatesThisPeriod = _updatesThisPeriod;
			_updatesThisPeriod = 0;

			OnDataChanged?.Invoke();

			return Task.FromResult(true);
		}

		private Task<bool> UpdateData()
		{
			return Task.FromResult(true);
		}

		public List<QuoteData> GetStockData()
		{
			lock(StockData)
			{
				var newData = new List<QuoteData>(StockData.Count);

				foreach(var item in StockData)
				{
					newData.Add(item.Value.Data.Clone());
				}

				return newData.OrderBy(s => s.ShortName).ToList();
			}
		}

		private TVWebSocket _webSocket;
		private DateTime _lastUpdate;
		private bool _hasSession;
		private int _updatesThisPeriod;

		public int UpdatesThisPeriod { get; set; }

		public Dictionary<string, Quote> StockData { get; set; } = new Dictionary<string, Quote>();
	}
}
