
namespace Plugin.Portfolio.Model
{
	public class Position
	{
		public string Symbol { get; set; }
		public decimal? OpenPrice { get; set; } //For all units
		public decimal? ClosePrice { get; set; } //For all units
		public decimal? Fees { get; set; } //For all units
		public decimal? FeeUnits { get; set; } //Units consumed by the transaction
		public decimal Units { get; set; }
		public DateTime? Opened { get; set; }
		public DateTime? Closed { get; set; }
		public string Currency { get; set; }
		public string Notes { get; set; }
	}
}
