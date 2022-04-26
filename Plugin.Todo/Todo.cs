using DashBored.MicrosoftGraph;
using DashBored.PluginApi;

namespace Plugin.Todo
{
	public class Todo : IPlugin
	{
		public static Type DataType => typeof(TodoData);
		public Type RazorType => typeof(TodoView);
		public CardStyle CardStyle => new CardStyle
		{
			Classes = "todo-card",
			Padding = true,
		};

		public IDictionary<int, int> TimerFrequencies => new Dictionary<int, int>
		{
			{ 0, 15 * 60 * 1000 }, //15m
		};

		public IEnumerable<Secret> Secrets => null;
		public IEnumerable<string> ScriptPaths => null;
		public IEnumerable<string> StylesheetPaths => null;

		public string Error { get; set; }

		public List<string> TodoItems { get; set; } = new List<string>();

		public string Title { get; set; }

		public GraphError GraphError { get; set; }

		public IPlugin.OnDataChangedDelegate OnDataChanged { get; set; }

		public GraphAuthenticationProviderPublic.OnLoginPromptDelegate OnLoginPrompt { get; set; }

		public Todo(TodoData data, string title)
		{
			_data = data;
			Title = title;
		}

		public async Task Login()
		{
			await _client.AcquireToken(true);

			await OnTimer(0);
		}

		public async Task<bool> OnInitialize(IPluginSecrets pluginSecrets)
		{
			_client = new GraphClient(pluginSecrets, _data.AzureAD, _scopes, (errorType, message) =>
			{
				GraphError = errorType;
				Error = message;

				OnDataChanged?.Invoke();
			},
			async (targetUri, redirectUri, cancellationToken) =>
			{
				return await OnLoginPrompt(targetUri, redirectUri, cancellationToken);
			});

			await OnTimer(0);

			return true;
		}

		public async Task<bool> OnTimer(int _)
		{
			//Don't retry on FatalError or NeedsAuth.
			if(GraphError == GraphError.Success || GraphError == GraphError.Error)
			{
				try
				{
					var requestData = await _client.Client.Me.Todo.Lists.Request().GetAsync();

					foreach (var entry in requestData)
					{
						if (string.Compare(entry.DisplayName, _data.ListName, true) == 0)
						{
							var listItems = await _client.Client.Me.Todo.Lists[entry.Id].Tasks.Request().GetAsync();

							lock (TodoItems)
							{
								TodoItems.Clear();

								foreach (var item in listItems)
								{
									TodoItems.Add(item.Title);
								}
							}

							OnDataChanged?.Invoke();
							break;
						}
					}
				}
				catch
				{
					if(GraphError == GraphError.Success)
					{
						throw;
					}
				}
			}

			return true;
		}

		private GraphClient _client;
		private TodoData _data;

		private string[] _scopes = { "email", "profile", "offline_access", "User.Read", "Tasks.Read", "Tasks.Read.Shared", "Tasks.ReadWrite", "Tasks.ReadWrite.Shared" };
	}
}
