﻿
namespace Plugin.Weather
{
	public class WeatherData
	{
		public string ApiKey { get; set; }
		public string Units { get; set; }
		public string Timezone { get; set; }
		public double Latitude { get; set; }
		public double Longitude { get; set; }
	}
}
