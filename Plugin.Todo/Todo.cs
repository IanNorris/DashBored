using DashBored.PluginApi;
using Microsoft.Graph;

namespace Plugin.Todo
{
	public class Todo : IPlugin
	{
		public static Type DataType => typeof(TodoData);
		public Type RazorType => typeof(TodoView);
		public CardStyle CardStyle => new CardStyle
		{
			Classes = "todo",
			Padding = true,
		};

		public Dictionary<int, int> TimerFrequencies => new Dictionary<int, int>
		{
			{ 0, 5 * 60 * 1000 }, //5m
		};
		public string Error { get; set; }

		public IPlugin.OnDataChangedDelegate OnDataChanged { get; set; }

		public Todo(TodoData data)
		{
			/*var webClient = GraphClientFactory.Create(new GraphAuthenticationProvider(data));
			var graphClient = new GraphServiceClient(webClient);
			graphClient.Me.Todo.Lists.Request().GetAsync().ContinueWith(data =>
			{
				foreach (var entry in data.Result)
				{
					Console.Out.Write(entry.DisplayName);
				}
			});
			*/
		}

		public Task<bool> OnInitialize()
		{
			return Task.FromResult(true);
		}

		public Task<bool> OnTimer(int _)
		{
			return Task.FromResult(true);
		}
	}
}
