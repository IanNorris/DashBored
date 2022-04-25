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
			Padding = false,
		};

		public IDictionary<int, int> TimerFrequencies => new Dictionary<int, int>
		{
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
			_webSocket = new TVWebSocket();
			var sessionId = _webSocket.CreateQuoteSession((data) =>
			{
				lock (StockData)
				{
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
			}, new string[] { "lp", "ch", "chp", "description", "status", "short_name" });

			if (_data.Stocks != null)
			{
				foreach (var stock in _data.Stocks)
				{
					_webSocket.AddSymbol(sessionId, stock);
				}
			}

			await UpdateData();

			Error = null;
			return true;
		}

		public Task<bool> OnTimer(int _)
		{
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

		public Dictionary<string, Quote> StockData { get; set; } = new Dictionary<string, Quote>();
	}
}
