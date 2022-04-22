using DashBored.PluginApi;
using Microsoft.Graph;

namespace Plugin.Todo
{
	public class Todo : IPlugin
	{
		public const string TokenCache = "TokenCache";

		public static Type DataType => typeof(TodoData);
		public Type RazorType => typeof(TodoView);
		public CardStyle CardStyle => new CardStyle
		{
			Classes = "todo",
			Padding = true,
		};

		public IDictionary<int, int> TimerFrequencies => new Dictionary<int, int>
		{
			{ 0, 5 * 60 * 1000 }, //5m
		};

		public IEnumerable<Secret> Secrets => new List<Secret>
		{
			new Secret
			{
				Name = TokenCache,
				DisplayName = "Token Cache",
				Description = "Token storage",
				UserVisible = false,
			},
			new Secret
			{
				Name = "Account",
				DisplayName = "Account Name",
				Description = "MSA used for authentication",
				UserVisible = false,
			}
		};

		public string Error { get; set; }

		public string Message { get; set; }

		public List<string> TodoItems { get; set; } = new List<string>();

		public string Title { get; set; }

		public IPlugin.OnDataChangedDelegate OnDataChanged { get; set; }

		public Todo(TodoData data, string title)
		{
			_data = data;
			Title = title;
		}

		private bool _isInitializing = false;

		public Task<bool> OnInitialize(IPluginSecrets pluginSecrets)
		{
			try
			{
				if(_isInitializing)
				{
					return Task.FromResult(true);
				}

				_isInitializing = true;

				_httpClient = GraphClientFactory.Create(new GraphAuthenticationProviderPublic(pluginSecrets, _data, async message =>
				{
					if (Message != message)
					{
						Message = message;

						if (Message != null)
						{
							OnDataChanged?.Invoke();
						}
						else
						{
							await OnTimer(0);
						}
					}
				}));

				_ = OnTimer(0);

				return Task.FromResult(true);
			}
			finally
			{
				_isInitializing = false;
			}
		}

		public async Task<bool> OnTimer(int _)
		{
			if(Message == null && Error == null && _httpClient != null)
			{
				var graphClient = new GraphServiceClient(_httpClient);
				var requestData = await graphClient.Me.Todo.Lists.Request().GetAsync();

				//var calendarData = await graphClient.Me.CalendarView.Request().GetAsync();

				foreach (var entry in requestData)
				{
					if (string.Compare(entry.DisplayName, _data.ListName, true) == 0)
					{
						var listItems = await graphClient.Me.Todo.Lists[entry.Id].Tasks.Request().GetAsync();

						lock (TodoItems)
						{
							TodoItems.Clear();

							foreach (var item in listItems)
							{
								TodoItems.Add(item.Title);
							}
						}
						break;
					}
				}

				if (!_isInitializing)
				{
					OnDataChanged?.Invoke();
				}

				return true;
			}
			else
			{
				return false;
			}
		}

		private HttpClient _httpClient;
		private TodoData _data;
	}
}
