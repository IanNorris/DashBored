using Newtonsoft.Json;

namespace DashBored.TradingView.Models
{
	public class Method
	{
		public Method(string name, params string[] parameters)
		{
			Name = name;
			Parameters = parameters;
		}

		[JsonProperty(PropertyName = "m")]
		public string Name { get; set; }

		[JsonProperty(PropertyName = "p")]
		public string[] Parameters { get; set; }
	}
}
