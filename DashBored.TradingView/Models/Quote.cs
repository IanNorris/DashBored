
using Newtonsoft.Json;

namespace DashBored.TradingView.Models
{
	public class QuoteData
	{
		private T UpdateIf<T>(T output, T input)
		{
			return input != null ? input : output;
		}

		public void UpdateFrom(QuoteData newData)
		{
			ShortName = UpdateIf(ShortName, newData.ShortName);
			Description = UpdateIf(Description, newData.Description);
			Price = UpdateIf(Price, newData.Price);
			ChangePercentage = UpdateIf(ChangePercentage, newData.ChangePercentage);
			Change = UpdateIf(Change, newData.Change);
			LogoId = UpdateIf(LogoId, newData.LogoId);
		}

		public QuoteData Clone()
		{
			return new QuoteData
			{
				ShortName = ShortName,
				Change = Change,
				ChangePercentage = ChangePercentage,
				Description = Description,
				Price = Price,
				LogoId = LogoId,
			};
		}

		[JsonProperty(PropertyName = "short_name")]
		public string ShortName { get; set; } = null;

		[JsonProperty(PropertyName = "description")]
		public string Description { get; set; } = null;
		[JsonProperty(PropertyName = "logoid")]
		public string LogoId { get; set; } = null;

		[JsonProperty(PropertyName = "lp")]
		public decimal? Price { get; set; } = null;

		[JsonProperty(PropertyName = "chp")]
		public decimal? ChangePercentage { get; set; } = null;

		[JsonProperty(PropertyName = "ch")]
		public decimal? Change { get; set; } = null;
	}

	public class Quote
	{
		[JsonProperty(PropertyName = "n")]
		public string Name { get; set; }

		[JsonProperty(PropertyName = "s")]
		public string Status { get; set; }

		[JsonProperty(PropertyName = "v")]
		public QuoteData Data { get; set; }
	}
}
