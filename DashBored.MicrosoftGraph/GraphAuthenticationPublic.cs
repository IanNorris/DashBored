using Microsoft.Identity.Client;
using DashBored.PluginApi;
using Microsoft.Graph;
using DashBored.MicrosoftGraph.Models;
using Microsoft.Identity.Client.Extensibility;

namespace DashBored.MicrosoftGraph
{
	public class GraphAuthenticationProviderPublic : IAuthenticationProvider
	{
		public delegate void OnAuthErrorDelegate(GraphError errorType, string message);
		public delegate Task<Uri> OnLoginPromptDelegate(Uri authorizationUri, Uri redirectUri, CancellationToken cancellationToken);

		public GraphAuthenticationProviderPublic(IPluginSecrets secrets, AzureAD azureAD, string[] scopes, OnAuthErrorDelegate onAuthError, OnLoginPromptDelegate onLoginPrompt)
		{
			_clientId = azureAD.ClientId;
			_tenantId = azureAD.TenantId;
			_onAuthError = onAuthError;
			_customWebUi = new BlazorWebUI(onLoginPrompt);
			_scopes = scopes;
			_secrets = secrets;

			_clientApplication = PublicClientApplicationBuilder.Create(_clientId)
			   .WithClientId(_clientId)
			   .WithRedirectUri("https://localhost:7058/AuthRedirect")
			   .WithTenantId(_tenantId)
			   .Build();

			_clientApplication.UserTokenCache.SetBeforeAccess(args =>
			{
				var currentValue = _secrets.GetSecret("TokenCache");

				if (currentValue != null)
				{
					var bytes = Convert.FromBase64String(currentValue);
					args.TokenCache.DeserializeMsalV3(bytes);
				}
			});

			_clientApplication.UserTokenCache.SetAfterAccess(args =>
			{
				if (args.HasStateChanged)
				{
					var bytes = args.TokenCache.SerializeMsalV3();
					var dataBase64 = Convert.ToBase64String(bytes);

					_secrets.SetSecret("TokenCache", dataBase64, false);
				}
			});
		}

		public async Task<bool> AcquireToken(bool interactive)
		{
			var accountName = _secrets.GetSecret("Account");

			if (accountName != null)
			{
				try
				{
					var silentResult = await _clientApplication.AcquireTokenSilent(_scopes, accountName).ExecuteAsync();

					_authorizationHeader = silentResult.CreateAuthorizationHeader();

					if (silentResult.Account != null)
					{
						_secrets.SetSecret("Account", silentResult.Account.Username, false);
					}

					_onAuthError?.Invoke(GraphError.Success, null);

					return true;
				}
				catch (MsalUiRequiredException)
				{
					//The only way into the second function
				}
				catch (Exception ex)
				{
					_onAuthError?.Invoke(GraphError.FatalError, ex.Message);
					return false;
				}
			}

			if (interactive)
			{
				try
				{
					var result = await _clientApplication.AcquireTokenInteractive(_scopes)
						.WithCustomWebUi(_customWebUi)
						.ExecuteAsync();

					_authorizationHeader = result.CreateAuthorizationHeader();

					_secrets.SetSecret("Account", result.Account.Username, false);

					_onAuthError?.Invoke(GraphError.Success, null);

					return true;
				}
				catch (Exception ex)
				{
					_onAuthError?.Invoke(GraphError.FatalError, ex.Message);
				}
			}
			else
			{
				_onAuthError?.Invoke(GraphError.NeedsLogin, null);
			}

			return false;
		}

		public async Task AuthenticateRequestAsync(HttpRequestMessage request)
		{
			if (await AcquireToken(false))
			{
				if (request.Headers.Contains("Authorization"))
				{
					request.Headers.Remove("Authorization");
				}
				request.Headers.Add("Authorization", _authorizationHeader);
			}
			else
			{
				throw new UnauthorizedAccessException();
			}
		}

		private readonly string _clientId;
		private readonly string _tenantId;
		private readonly string[] _scopes;

		private string _authorizationHeader;

		private IPluginSecrets _secrets;
		private OnAuthErrorDelegate _onAuthError;
		private ICustomWebUi _customWebUi;
		public IPublicClientApplication _clientApplication;
	}
}
