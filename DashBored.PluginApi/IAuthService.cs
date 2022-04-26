
namespace DashBored.PluginApi
{
	public interface IAuthService
	{
		public Task<Uri> CreateNewReceiptTask();
		public void OnCodeReceived(Uri uri);
	}
}
