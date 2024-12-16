using DashBored.MicrosoftGraph;
using DashBored.PluginApi;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Graph.Models;
using static DashBored.MicrosoftGraph.Delegates;

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
            { 0, 15 * 60 * 1000 }, // 15 minutes
        };

        public IEnumerable<Secret> Secrets => null;
        public IEnumerable<string> ScriptPaths => null;
        public IEnumerable<string> StylesheetPaths => null;

        public string Error { get; set; }

        public List<string> TodoItems { get; set; } = new List<string>();

        public string Title { get; set; }

        public GraphError GraphError { get; set; } = GraphError.NeedsLogin;

        private readonly IPluginServerEnvironment _server;

        public IPlugin.OnDataChangedDelegate OnDataChanged { get; set; }

        public OnLoginPromptDelegate OnLoginPrompt { get; set; }

        public Todo(IPluginServerEnvironment server, TodoData data, string title)
        {
            _server = server;
            _data = data;
            Title = title;
        }

        public async Task Login()
        {
            if (_client == null)
            {
                throw new InvalidOperationException("GraphClient is not initialized.");
            }

            GraphError = await _client.AcquireToken(true) ? GraphError.Success : GraphError.NeedsLogin;
            await OnTimer(0);
        }

        public async Task<bool> OnInitialize(IPluginSecrets pluginSecrets)
        {
            _client = new GraphClient(pluginSecrets, _data.AzureAD, _server, _scopes,
            (errorType, message) =>
            {
                GraphError = errorType;
                Error = message;
                OnDataChanged?.Invoke();
            },
            async (targetUri, redirectUri, cancellationToken) =>
            {
                return OnLoginPrompt != null
                    ? await OnLoginPrompt(targetUri, redirectUri, cancellationToken)
                    : throw new InvalidOperationException("OnLoginPrompt delegate is not set.");
            });

            GraphError = await _client.AcquireToken(false) ? GraphError.Success : GraphError.NeedsLogin;

            await OnTimer(0);

            return true;
        }

        public async Task<bool> OnTimer(int _)
        {
            if (GraphError is GraphError.Success or GraphError.Error)
            {
                try
                {
                    var requestData = await _client.Client.Me.Todo.Lists.GetAsync();

                    foreach (var entry in requestData.Value)
                    {
                        if (string.Equals(entry.DisplayName, _data.ListName, StringComparison.OrdinalIgnoreCase))
                        {
                            var listItems = await _client.Client.Me.Todo.Lists[entry.Id].Tasks.GetAsync();

                            lock (TodoItems)
                            {
                                TodoItems.Clear();
                                TodoItems.AddRange(listItems.Value.Where(item => item.Status != Microsoft.Graph.Models.TaskStatus.Completed).Select(item => item.Title));
                            }

                            OnDataChanged?.Invoke();
                            break;
                        }
                    }
                }
                catch when (GraphError == GraphError.Success)
                {
                    throw;
                }
            }

            return true;
        }

        private GraphClient _client;
        private readonly TodoData _data;

        private readonly string[] _scopes = { "email", "profile", "offline_access", "User.Read", "Tasks.Read", "Tasks.Read.Shared", "Tasks.ReadWrite", "Tasks.ReadWrite.Shared" };
    }
}
