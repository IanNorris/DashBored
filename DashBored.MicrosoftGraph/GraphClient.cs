using Microsoft.Identity.Client;
using DashBored.PluginApi;
using Microsoft.Graph;
using DashBored.MicrosoftGraph.Models;
using static DashBored.MicrosoftGraph.Delegates;

namespace DashBored.MicrosoftGraph
{
	public class GraphClient
	{
		public GraphClient(
			IPluginSecrets pluginSecrets, 
			AzureAD azureAD,
            IPluginServerEnvironment server,
            string[] scopes,
			OnAuthErrorDelegate onAuthenticationError,
			OnLoginPromptDelegate onLoginPrompt)
		{
			_onAuthenticationError = onAuthenticationError;

			_authProvider = new GraphAuthenticationProviderPublic(pluginSecrets, azureAD, server, scopes, _onAuthenticationError, onLoginPrompt);

			_httpClient = GraphClientFactory.Create();
			_client = new GraphServiceClient(_httpClient, _authProvider);
		}

		public async Task<bool> AcquireToken(bool interactive)
		{
			return await _authProvider.AcquireToken(interactive);
		}

		public GraphServiceClient Client { get => _client; }

		private GraphServiceClient _client;
		private HttpClient _httpClient;
		private GraphAuthenticationProviderPublic _authProvider;
		private OnAuthErrorDelegate _onAuthenticationError;
	}
}
