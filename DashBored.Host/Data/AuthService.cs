using DashBored.PluginApi;

namespace DashBored.Host.Data
{
	public class AuthService : IAuthService
	{
		public Task<Uri> CreateNewReceiptTask()
		{
			var newTaskSource = new TaskCompletionSource<Uri>();
			lock (_existingTasks)
			{
				_existingTasks.Add(newTaskSource);
			}

			return newTaskSource.Task;
		}

		public void OnCodeReceived(Uri uri)
		{
			TaskCompletionSource<Uri> newTask = null;
			lock (_existingTasks)
			{
				var task = _existingTasks.LastOrDefault();
				if (task != null)
				{
					_existingTasks.Clear();
					newTask = task;
				}
			}

			if(newTask != null)
			{
				newTask.SetResult(uri);
			}
		}

		private List<TaskCompletionSource<Uri>> _existingTasks = new List<TaskCompletionSource<Uri>>();
	}
}
