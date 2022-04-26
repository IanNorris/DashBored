using Microsoft.Identity.Client;
using DashBored.PluginApi;
using Microsoft.Graph;
using DashBored.MicrosoftGraph.Models;

namespace DashBored.MicrosoftGraph
{
	public class GraphClient
	{
		public GraphClient(
			IPluginSecrets pluginSecrets, 
			AzureAD azureAD, 
			string[] scopes, 
			GraphAuthenticationProviderPublic.OnAuthErrorDelegate onAuthenticationError,
			GraphAuthenticationProviderPublic.OnLoginPromptDelegate onLoginPrompt)
		{
			_onAuthenticationError = onAuthenticationError;

			_authProvider = new GraphAuthenticationProviderPublic(pluginSecrets, azureAD, scopes, _onAuthenticationError, onLoginPrompt);

			_httpClient = GraphClientFactory.Create(_authProvider);
			_client = new GraphServiceClient(_httpClient);
		}

		public async Task<bool> AcquireToken(bool interactive)
		{
			return await _authProvider.AcquireToken(interactive);
		}

		public GraphServiceClient Client { get => _client; }

		private GraphServiceClient _client;
		private HttpClient _httpClient;
		private GraphAuthenticationProviderPublic _authProvider;
		private GraphAuthenticationProviderPublic.OnAuthErrorDelegate _onAuthenticationError;
	}
}
