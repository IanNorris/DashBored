using DashBored.PluginApi;
using DashBored.TradingView;
using DashBored.TradingView.Models;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using Plugin.Portfolio.Model;

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

		public Portfolio(ISettingsService settingsService, PortfolioData data, string title)
		{
			_settingsService = settingsService;
			_data = data;
		}

		public async Task<bool> OnInitialize(IPluginSecrets pluginSecrets)
		{
			var root = _settingsService.GetSettingsPath();
			var positionsPath = Path.Combine(root, "PortfolioPositions.csv");

			var futureDate = DateTime.UtcNow.AddYears(10);

			if(File.Exists(positionsPath))
			{
				using var reader = new StreamReader(positionsPath) as TextReader;
				using var csvReader = new CsvReader(reader, CultureInfo.CurrentCulture);
				Positions = csvReader.GetRecords<Position>().ToList();

				foreach(var position in Positions)
				{
					if(position.Opened.HasValue)
					{
						if(!position.OpenPrice.HasValue)
						{
							throw new InvalidDataException($"Open position {position.Opened.Value} {position.Symbol} has no open price.");
						}
					}
					else if (position.Closed.HasValue)
					{
						if (!position.ClosePrice.HasValue)
						{
							throw new InvalidDataException($"Close position {position.Closed.Value} {position.Symbol} has no close price.");
						}
					}
					else
					{
						throw new InvalidDataException($"Position {position.Symbol} has no open or close date.");
					}

					var key = position.Symbol.Trim().ToUpperInvariant();
					if(!string.IsNullOrEmpty(key))
					{
						if(PortfolioPositions.TryGetValue(key, out var existingSymbol))
						{
							existingSymbol.Add(position);
						}
						else
						{
							PortfolioPositions[key] = new List<Position> { position };
						}
					}
				}

				//Order them
				foreach(var symbol in PortfolioPositions)
				{
					PortfolioPositions[symbol.Key] = symbol.Value
						.OrderBy(p => p.Opened.GetValueOrDefault())
						.ThenBy(p => p.Closed.GetValueOrDefault(futureDate))
						.ToList();
				}
				
				//Don't care about the numbers, just want the validation we do
				_ = GetGains();
			}

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
						existing.UpdateFrom(data.Data);
					}
					else
					{
						if (_data.Stocks.Contains(data.Name))
						{
							StockData.Add(data.Name, data.Data);
						}
					}

					if (PortfolioStockData.TryGetValue(data.Name, out var existingPortfolio))
					{
						existingPortfolio.UpdateFrom(data.Data);
					}
					else
					{
						PortfolioStockData.Add(data.Name, data.Data.Clone());
					}
				}

				OnDataChanged?.Invoke();
			}, new string[] { "chp", "description", "status", "short_name", "logoid", "rchp", "lp" });

			var stockSet = new HashSet<string>();

			if (_data.Stocks != null)
			{
				foreach (var stock in _data.Stocks)
				{
					if (!stockSet.Contains(stock))
					{
						_webSocket.AddSymbol(sessionId, stock);
						stockSet.Add(stock);
					}
				}
			}

			foreach (var stock in PortfolioPositions.Keys)
			{
				if (!stockSet.Contains(stock))
				{
					_webSocket.AddSymbol(sessionId, stock);
					stockSet.Add(stock);
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
					newData.Add(item.Value.Clone());
				}

				return newData.OrderBy(s => s.ShortName).ToList();
			}
		}

		public List<Gain> GetGains()
		{
			var gains = new List<Gain>();

			lock (StockData)
			{
				var amounts = new Dictionary<string, Holding>();

				foreach(var position in Positions)
				{
					if (position.Opened.HasValue)
					{
						if (amounts.TryGetValue(position.Symbol, out var holding))
						{
							holding.Units += position.Units;
							if(holding.Currency != position.Currency)
							{
								throw new InvalidDataException($"Currency of position for {position.Symbol} is {holding.Currency} but a new position has currency {holding.Currency}");
							}
							holding.Spent += position.OpenPrice.Value;
						}
						else
						{
							amounts[position.Symbol] = new Holding
							{
								Currency = position.Currency,
								Spent = position.OpenPrice.Value,
								Units = position.Units,
							};
						}
					}
					else if(position.Closed.HasValue)
					{
						if(amounts.TryGetValue(position.Symbol,out var holding))
						{
							if(holding.Units >= position.Units + position.FeeUnits.GetValueOrDefault())
							{
								var avgPerUnit = holding.Spent / holding.Units;
								var positionPerUnit = position.ClosePrice.Value / position.Units;
								
								var deltaPerUnit = (positionPerUnit - avgPerUnit) * (position.Units - position.FeeUnits.GetValueOrDefault());

								holding.Units -= position.Units;

								gains.Add(new Gain
								{
									Symbol = position.Symbol,
									Currency = position.Currency,
									Date = position.Closed.Value,
									Value = deltaPerUnit,
									IsOpen = false,
								});
							}
							else
							{
								throw new InvalidDataException($"Sold {position.Units} of {position.Symbol} on {position.Closed.Value} but only holding {position.Units}.");
							}
						}
						else
						{
							throw new InvalidDataException($"Sold {position.Units} of {position.Symbol} on {position.Closed.Value} but not currently holding. Shorting is not supported.");
						}
					}
				}

				var currentDate = DateTime.UtcNow;

				foreach(var holding in amounts)
				{
					if (PortfolioStockData.TryGetValue(holding.Key, out var currentPrice))
					{
						if (currentPrice.Price.HasValue)
						{
							gains.Add(new Gain
							{
								Symbol = holding.Key,
								Currency = holding.Value.Currency,
								Date = currentDate,
								Value = (currentPrice.Price.Value * holding.Value.Units) - holding.Value.Spent,
								IsOpen = true,
							});
						}
					}
				}
			}

			return gains;
		}

		private ISettingsService _settingsService;
		private TVWebSocket _webSocket;
		private DateTime _lastUpdate;
		private bool _hasSession;
		private int _updatesThisPeriod;

		public int UpdatesThisPeriod { get; set; }

		public List<Position> Positions { get; private set; } = new List<Position>();
		public Dictionary<string, QuoteData> StockData { get; private set; } = new Dictionary<string, QuoteData>();
		public Dictionary<string, QuoteData> PortfolioStockData { get; private set; } = new Dictionary<string, QuoteData>();

		public Dictionary<string, List<Position>> PortfolioPositions { get; private set; } = new Dictionary<string, List<Position>>();
	}
}
