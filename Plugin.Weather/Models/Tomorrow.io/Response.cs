
namespace Plugin.Weather.Models.Tomorrow.io
{
	public class Response<T>
	{
		public T Data { get; set; }
		public List<Warning> Warnings { get; set; }
	}
}
