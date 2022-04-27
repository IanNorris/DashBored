using DashBored.PluginApi;
using Plugin.Weather.Models.Tomorrow.io;

namespace Plugin.Weather
{
	public class Weather : IPlugin
	{
		public static Type DataType => typeof(WeatherData);
		public Type RazorType => typeof(WeatherView);
		public CardStyle CardStyle => new CardStyle
		{
			Classes = "weather-card",
			Padding = false,
		};

		public IDictionary<int, int> TimerFrequencies => new Dictionary<int, int>
		{
			{ 0, 30 * 60 * 1000 }, //30 mins
		};

		public IEnumerable<Secret> Secrets => new List<Secret>
		{
			new Secret
			{
				Name = ApiKeySecret, 
				DisplayName = "Api Key",
				Description = "A Tomorrow.io Api Key",
				UserVisible = true,
			}
		};

		public IEnumerable<string> ScriptPaths => null;
		public IEnumerable<string> StylesheetPaths => null;

		public string Error { get; set; }

		public IPlugin.OnDataChangedDelegate OnDataChanged { get; set; }

		private TomorrowTimelineRequest _requestClient;

		public Response<TimelineArray> CurrentWeather { get; set; }

		WeatherData _data;

		public Weather(WeatherData data, string title)
		{
			_data = data;
		}

		public async Task<bool> OnInitialize(IPluginSecrets pluginSecrets)
		{
			var apiKey = pluginSecrets.GetSecret(ApiKeySecret);

			if(apiKey == null)
			{
				Error = $"No Api Key specified";
				await UpdateData();
				return false;
			}

			_requestClient = new TomorrowTimelineRequest(apiKey, _data.Latitude, _data.Longitude, _data.Units, _data.Timezone);

			await UpdateData();

			Error = null;
			return true;
		}

		public async Task<bool> OnTimer(int _)
		{
			await UpdateData();

			return true;
		}

		private async Task<bool> UpdateData()
		{
			if (_requestClient != null)
			{
				var response = await _requestClient.CreateRequest();

				if (response != null)
				{
					CurrentWeather = response;
				}
				else
				{
					Error = "Error!";
					return false;
				}

				if (OnDataChanged != null)
				{
					OnDataChanged.Invoke();
				}

				return true;
			}

			return false;
		}

		public string GetWeatherIcon(Dictionary<string, decimal> day, bool large)
		{
			if(CurrentWeather == null)
			{
				return "unknown.png";
			}

			var sizeString = large ? "large" : "small";

			var code = (int)day["weatherCodeFullDay"];
			if(code < 10000)
			{
				code *= 10;
			}

			var fullName = $"{code}_{sizeString}.png";

			return $"_content/Plugin.Weather/images/weatherCodes/{sizeString}/png/{fullName}";
		}

		public string GetWeatherDescription(Dictionary<string, decimal> day)
		{
			var code = (int)day["weatherCodeFullDay"];
			if (code < 10000)
			{
				code *= 10;
			}

			if(WeatherCodes.TryGetValue(code, out var description))
			{
				return description;
			}
			else
			{
				return "Unknown";
			}
		}

		public string GetTemperatureUnit()
		{
			if(string.Compare(_data.Units, "metric", true) == 0)
			{
				return "°C";
			}
			else
			{
				return "°F";
			}
		}

		public string GetWeatherValue(Dictionary<string, decimal> day, string name)
		{
			if(day == null)
			{
				return string.Empty;
			}

			return day[name].ToString("N0");
		}

		public Dictionary<string, decimal> GetToday()
		{
			if(CurrentWeather == null)
			{
				return null;
			}

			var today = CurrentWeather.Data.Timelines[0].Intervals[0].Values;

			return today;
		}

		public Dictionary<string, decimal> GetTomorrow()
		{
			if (CurrentWeather == null)
			{
				return null;
			}

			var tomorrow = CurrentWeather.Data.Timelines[0].Intervals[1].Values;

			return tomorrow;
		}

		public void GetStatTable(ref IDictionary<string, List<string>> output, Dictionary<string, decimal> today)
		{
			string precipitationType;

			var percent = today["precipitationProbability"].ToString("N0");
			var precipitationIntensity = today["precipitationIntensity"].ToString("N0");
			var precipitationUnits = (string.Compare(_data.Units, "metric", true) == 0) ? "mm/h" : "in/h";

			var precipitationValue = $"{percent}%, {precipitationIntensity}{precipitationUnits}";

			var warningIcon = "⚠️";

			AddRowValue(ref output, "Weather", GetWeatherDescription(today));

			var temperature = GetWeatherValue(today, "temperature");
			var temperatureApparent = GetWeatherValue(today, "temperatureApparent");
			AddRowValue(ref output, "Temperature", $"{temperature}{GetTemperatureUnit()} (feels {temperatureApparent}{GetTemperatureUnit()})");

			switch ((int)today["precipitationType"])
			{
				case 0:
					{
						precipitationType = "None";
						break;
					}

				case 1:
					{
						precipitationType = "Rain";
						break;
					}

				case 2:
					{
						precipitationType = $"{warningIcon} Snow";
						break;
					}

				case 3:
					{
						precipitationType = $"{warningIcon} Sleet";
						break;
					}

				case 4:
					{
						precipitationType = $"{warningIcon} Hail";
						break;
					}

				default:
					{
						precipitationType = $"{warningIcon} Unknown";
						break;
					}
			}

			AddRowValue(ref output, "Precipitation", $"{precipitationType} {precipitationValue}");

			var humidity = today["humidity"].ToString("N0");
			AddRowValue(ref output, "Humidity", $"{humidity}%");

			var grassIndex = (int)today["grassIndex"];
			AddRowValue(ref output, "Grass Pollen", GetBadge(grassIndex));

			var treeIndex = (int)today["treeIndex"];
			AddRowValue(ref output, "Tree Pollen", GetBadge(treeIndex));

			var uvHealth = (int)today["uvHealthConcern"];
			AddRowValue(ref output, "UV Health", GetBadge(uvHealth));

			var cloudCover = today["cloudCover"].ToString("N0");
			AddRowValue(ref output, "Cloud Cover", $"{cloudCover}%");
		}

		public void AddRowValue(ref IDictionary<string, List<string>> output, string key, string value)
		{
			if (output.TryGetValue(key, out var existing))
			{
				existing.Add(value);
			}
			else
			{
				output.Add(key, new List<string>() { value });
			}
		}

		public string GetBadge(int value)
		{
			var warningIcon = "⚠️";

			var colour = "success";
			var label = "None";

			switch(value)
			{
				case 0:
					{
						colour = "success";
						label = "None";

						break;
					}

				case 1:
					{
						colour = "success";
						label = "Low";
						break;
					}

				case 2:
					{
						colour = "success";
						label = "Low";
						break;
					}

				case 3:
					{
						colour = "warning text-dark";
						label = "Moderate";
						break;
					}

				case 4:
					{
						colour = "warning text-dark";
						label = "Moderate";
						break;
					}

				case 5:
					{
						colour = "warning text-dark";
						label = "Moderate";
						break;
					}

				case 6:
					{
						colour = "danger";
						label = "{warningIcon} High";
						break;
					}

				case 7:
					{
						colour = "danger";
						label = "{warningIcon} High";
						break;
					}

				case 8:
					{
						colour = "danger";
						label = "{warningIcon} V High";
						break;
					}

				case 9:
					{
						colour = "danger";
						label = "{warningIcon} V High";
						break;
					}

				default:
					{
						colour = "danger";
						label = $"{warningIcon} Extreme";
						break;
					}
			}

			label += $" ({value})";

			return $@"<span class=""badge bg-{colour}"">{label}</span>";
		}

		private const string ApiKeySecret = "ApiKey";

		public Dictionary<int, string> WeatherCodes = new Dictionary<int, string>
		{
			{ 1000, "Sunny" },
			{ 1100, "Clear" },
			{ 1101, "Partly Cloudy" },
			{ 1102, "Mostly Cloudy" },
			{ 1001, "Cloudy" },
			{ 1103, "Mostly Clear" },
			{ 2100, "Light Fog" },
			{ 2101, "Light Fog" },
			{ 2102, "Light Fog" },
			{ 2103, "Light Fog" },
			{ 2106, "Fog" },
			{ 2107, "Fog" },
			{ 2108, "Fog" },
			{ 2000, "Fog" },
			{ 4204, "Rain" },
			{ 4203, "Some Rain" },
			{ 4205, "Some Rain" },
			{ 4000, "Rain" },
			{ 4200, "Light Rain" },
			{ 4213, "Light Rain" },
			{ 4214, "Light Rain" },
			{ 4215, "Light Rain" },
			{ 4209, "Rain" },
			{ 4208, "Rain" },
			{ 4210, "Rain" },
			{ 4001, "Rain" },
			{ 4211, "Heavy Rain" },
			{ 4202, "Heavy Rain" },
			{ 4212, "Heavy Rain" },
			{ 4201, "Heavy Rain" },
			{ 5115, "Snow Flurries" },
			{ 5116, "Snow Flurries" },
			{ 5117, "Snow Flurries" },
			{ 5001, "Snow Flurries" },
			{ 5100, "Light Snow" },
			{ 5102, "Light Snow" },
			{ 5103, "Light Snow" },
			{ 5104, "Light Snow" },
			{ 5122, "Light Snow &amp; Rain" },
			{ 5105, "Snow" },
			{ 5106, "Snow" },
			{ 5107, "Snow" },
			{ 5000, "Snow" },
			{ 5101, "Heavy Snow" },
			{ 5119, "Heavy Snow" },
			{ 5120, "Heavy Snow" },
			{ 5121, "Heavy Snow" },
			{ 5110, "Rain &amp; Snow" },
			{ 5108, "Snow &amp; Rain" },
			{ 5114, "Snow &amp; Sleet" },
			{ 5112, "Snow &amp; Hail" },
			{ 6000, "Sleet" },
			{ 6003, "Sleet" },
			{ 6002, "Sleet" },
			{ 6004, "Sleet" },
			{ 6204, "Sleet" },
			{ 6206, "Sleet &amp; Rain" },
			{ 6205, "Sleet" },
			{ 6203, "Sleet" },
			{ 6209, "Sleet" },
			{ 6200, "Light Sleet" },
			{ 6213, "Sleet" },
			{ 6214, "Sleet" },
			{ 6215, "Sleet" },
			{ 6001, "Sleet" },
			{ 6212, "Sleet &amp; Rain" },
			{ 6220, "Sleet" },
			{ 6222, "Sleet &amp; Rain" },
			{ 6207, "Heavy Sleet" },
			{ 6202, "Heavy Sleet" },
			{ 6208, "Heavy Sleet" },
			{ 6201, "Heavy Sleet" },
			{ 7110, "Hail" },
			{ 7111, "Hail" },
			{ 7112, "Hail" },
			{ 7102, "Hail" },
			{ 7108, "Hail" },
			{ 7107, "Hail" },
			{ 7109, "Hail" },
			{ 7000, "Hail" },
			{ 7105, "Hail" },
			{ 7106, "Hail &amp; Sleet" },
			{ 7115, "Hail" },
			{ 7117, "Hail" },
			{ 7103, "Heavy Hail &amp Sleet;" },
			{ 7113, "Heavy Hail" },
			{ 7114, "Heavy Hail" },
			{ 7116, "Heavy Hail" },
			{ 7101, "Heavy Hail" },
			{ 8001, "Thunderstorm" },
			{ 8003, "Thunderstorm" },
			{ 8002, "Thunderstorm" },
			{ 8000, "Thunderstorm" },
			{ 10000, "Clear, Sunny" },
			{ 11000, "Mostly Clear" },
			{ 11010, "Partly Cloudy" },
			{ 11020, "Mostly Cloudy" },
			{ 10010, "Cloudy" },
			{ 11030, "Mostly Clear" },
			{ 21000, "Light Fog" },
			{ 21010, "Light Fog" },
			{ 21020, "Light Fog" },
			{ 21030, "Light Fog" },
			{ 21060, "Fog" },
			{ 21070, "Fog" },
			{ 21080, "Fog" },
			{ 20000, "Fog" },
			{ 42040, "Rain" },
			{ 42030, "Rain" },
			{ 42050, "Rain" },
			{ 40000, "Rain" },
			{ 42000, "Light Rain" },
			{ 42130, "Light Rain" },
			{ 42140, "Light Rain" },
			{ 42150, "Light Rain" },
			{ 42090, "Rain" },
			{ 42080, "Rain" },
			{ 42100, "Rain" },
			{ 40010, "Rain" },
			{ 42110, "Heavy Rain" },
			{ 42020, "Heavy Rain" },
			{ 42120, "Heavy Rain" },
			{ 42010, "Heavy Rain" },
			{ 51150, "Flurries" },
			{ 51160, "Flurries" },
			{ 51170, "Flurries" },
			{ 50010, "Flurries" },
			{ 51000, "Light Snow" },
			{ 51020, "Light Snow" },
			{ 51030, "Light Snow" },
			{ 51040, "Light Snow" },
			{ 51220, "Light Snow &amp; Rain" },
			{ 51050, "Snow" },
			{ 51060, "Snow" },
			{ 51070, "Snow" },
			{ 50000, "Snow" },
			{ 51010, "Heavy Snow" },
			{ 51190, "Heavy Snow" },
			{ 51200, "Heavy Snow" },
			{ 51210, "Heavy Snow" },
			{ 51100, "Snow &amp; Rain" },
			{ 51080, "Rain &amp; Snow" },
			{ 51140, "Snow &amp; Sleet" },
			{ 51120, "Snow &amp; Hail" },
			{ 60000, "Sleet" },
			{ 60030, "Sleet" },
			{ 60020, "Sleet" },
			{ 60040, "Sleet" },
			{ 62040, "Rain &amp; Sleet" },
			{ 62060, "Sleet" },
			{ 62050, "Light Sleet" },
			{ 62030, "Light Sleet" },
			{ 62090, "Light Sleet" },
			{ 62000, "Light Sleet" },
			{ 62130, "Sleet" },
			{ 62140, "Sleet" },
			{ 62150, "Sleet" },
			{ 60010, "Sleet" },
			{ 62120, "Rain &amp; Sleet" },
			{ 62200, "Sleet" },
			{ 62220, "Rain &amp; Sleet" },
			{ 62070, "Heavy Sleet" },
			{ 62020, "Heavy Sleet" },
			{ 62080, "Heavy Sleet" },
			{ 62010, "Heavy Sleet" },
			{ 71100, "Hail" },
			{ 71110, "Hail" },
			{ 71120, "Hail" },
			{ 71020, "Hail" },
			{ 71080, "Hail" },
			{ 71070, "Hail" },
			{ 71090, "Hail" },
			{ 70000, "Hail" },
			{ 71050, "Rain &amp; Hail" },
			{ 71060, "Sleet &amp; Hail" },
			{ 71150, "Hail" },
			{ 71170, "Rain &amp; Hail" },
			{ 71030, "Sleet &amp; Heavy Hail" },
			{ 71130, "Heavy Hail" },
			{ 71140, "Heavy Hail" },
			{ 71160, "Heavy Hail" },
			{ 71010, "Heavy Hail" },
			{ 80010, "Thunderstorm" },
			{ 80030, "Thunderstorm" },
			{ 80020, "Thunderstorm" },
			{ 80000, "Thunderstorm" },
			{ 10001, "Clear" },
			{ 11001, "Mostly Clear" },
			{ 11011, "Partly Cloudy" },
			{ 11021, "Mostly Cloudy" },
			{ 10011, "Cloudy" },
			{ 11031, "Mostly Clear" },
			{ 21001, "Light Fog" },
			{ 21011, "Light Fog" },
			{ 21021, "Light Fog" },
			{ 21031, "Light Fog" },
			{ 21061, "Fog" },
			{ 21071, "Fog" },
			{ 21081, "Fog" },
			{ 20001, "Fog" },
			{ 42041, "Rain" },
			{ 42031, "Rain" },
			{ 42051, "Rain" },
			{ 40001, "Rain" },
			{ 42001, "Light Rain" },
			{ 42131, "Light Rain" },
			{ 42141, "Light Rain" },
			{ 42151, "Light Rain" },
			{ 42091, "Rain" },
			{ 42081, "Rain" },
			{ 42101, "Rain" },
			{ 40011, "Rain" },
			{ 42111, "Heavy Rain" },
			{ 42021, "Heavy Rain" },
			{ 42121, "Heavy Rain" },
			{ 42011, "Heavy Rain" },
			{ 51151, "Flurries" },
			{ 51161, "Flurries" },
			{ 51171, "Flurries" },
			{ 50011, "Flurries" },
			{ 51001, "Light Snow" },
			{ 51021, "Light Snow" },
			{ 51031, "Light Snow" },
			{ 51041, "Light Snow" },
			{ 51221, "Light Snow &amp; Rain" },
			{ 51051, "Snow" },
			{ 51061, "Snow" },
			{ 51071, "Snow" },
			{ 50001, "Snow" },
			{ 51011, "Heavy Snow" },
			{ 51191, "Heavy Snow" },
			{ 51201, "Heavy Snow" },
			{ 51211, "Heavy Snow" },
			{ 51101, "Rain &amp; Snow" },
			{ 51081, "Rain &amp; Snow" },
			{ 51141, "Snow &amp; Sleet" },
			{ 51121, "Snow &amp; Hail" },
			{ 60001, "Sleet" },
			{ 60031, "Sleet" },
			{ 60021, "Sleet" },
			{ 60041, "Sleet" },
			{ 62041, "Rain &amp; Sleet" },
			{ 62061, "Sleet" },
			{ 62051, "Light Sleet" },
			{ 62031, "Light Sleet" },
			{ 62091, "Light Sleet" },
			{ 62001, "Light Sleet" },
			{ 62131, "Sleet" },
			{ 62141, "Sleet" },
			{ 62151, "Sleet" },
			{ 60011, "Sleet" },
			{ 62121, "Sleet" },
			{ 62201, "Sleet" },
			{ 62221, "Rain &amp; Sleet" },
			{ 62071, "Heavy Sleet" },
			{ 62021, "Heavy Sleet" },
			{ 62081, "Heavy Sleet" },
			{ 62011, "Heavy Sleet" },
			{ 71101, "Hail" },
			{ 71111, "Hail" },
			{ 71121, "Hail" },
			{ 71021, "Hail" },
			{ 71081, "Hail" },
			{ 71071, "Hail" },
			{ 71091, "Hail" },
			{ 70001, "Hail" },
			{ 71051, "Rain &amp; Hail" },
			{ 71061, "Sleet &amp; Hail" },
			{ 71151, "Hail" },
			{ 71171, "Rain &amp; Hail" },
			{ 71031, "Heavy Hail" },
			{ 71131, "Heavy Hail" },
			{ 71141, "Heavy Hail" },
			{ 71161, "Heavy Hail" },
			{ 71011, "Heavy Hail" },
			{ 80011, "Thunderstorm" },
			{ 80031, "Thunderstorm" },
			{ 80021, "Thunderstorm" },
			{ 80001, "Thunderstorm" },
		};
	}
}
