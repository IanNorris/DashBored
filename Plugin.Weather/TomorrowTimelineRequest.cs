
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using Plugin.Weather.Models.Tomorrow.io;

namespace Plugin.Weather
{
	public class TomorrowTimelineRequest
	{
		public string Error { get; private set; }

		public TomorrowTimelineRequest(string apiKey, double latitude, double longitude, string units, string timezone)
		{
			_client = new HttpClient();
			_apiKey = apiKey;
			_latitude = latitude;
			_longitude = longitude;
			_units = units;
			_timezone = timezone;
		}

		public async Task<Response<TimelineArray>> CreateRequest()
		{
			var currentTime = DateTime.UtcNow;

			var queryParameters = new Dictionary<string, string>
			{
				{ "apikey", _apiKey },
				{ "units", _units },
				{ "timezone", _timezone },
				{ "startTime", currentTime.ToString("O") },
				{ "endTime", currentTime.AddDays(1).ToString("O") },
				{ "location", $"{_latitude},{_longitude}" },
				{ "fields", string.Join(',', _fields ) },
				{ "timesteps", string.Join(',', _timesteps) },
			};

			var query = new Uri(QueryHelpers.AddQueryString("https://data.climacell.co/v4/timelines", queryParameters));

			var result = await _client.GetAsync(query);

			if(result.IsSuccessStatusCode)
			{
				var jsonContent = await result.Content.ReadAsStringAsync();
				return JsonConvert.DeserializeObject<Response<TimelineArray>>(jsonContent);
			}
			else
			{
				var jsonContent = await result.Content.ReadAsStringAsync();
				var error = JsonConvert.DeserializeObject<Error>(jsonContent);

				Error = error.Message;
				return null;
			}
		}

		HttpClient _client;

		string _apiKey;
		double _latitude;
		double _longitude;
		string _units;
		string _timezone;

		static readonly string[] _fields =
		{
			"precipitationIntensity",
			"precipitationProbability",
			"precipitationType",
			"temperature",
			"temperatureApparent",
			"humidity",
			"uvHealthConcern",
			"cloudCover",
			"weatherCode",
			"weatherCodeFullDay",
			"treeIndex",
			"grassIndex"
		};

		static readonly string[] _timesteps =
		{
			"1d"
		};
	}
}
