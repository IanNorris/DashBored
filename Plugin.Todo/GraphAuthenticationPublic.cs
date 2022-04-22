using DashBored.PluginApi;
using Microsoft.Graph;
using Microsoft.Identity.Client;

namespace Plugin.Todo
{
	internal class GraphAuthenticationProviderPublic : IAuthenticationProvider
	{
		public delegate void OnAuthUpdateDelegate(string message);

		public GraphAuthenticationProviderPublic(IPluginSecrets secrets, TodoData data, OnAuthUpdateDelegate onAuthUpdate)
		{
			_clientId = data.ClientId;
			_tenantId = data.TenantId;
			_onAuthUpdate = onAuthUpdate;
			_secrets = secrets;

			_clientApplication = PublicClientApplicationBuilder.Create(_clientId)
			   .WithClientId(_clientId)
			   .WithRedirectUri("http://localhost/")
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

		public async Task AuthenticateRequestAsync(HttpRequestMessage request)
		{
			var accountName = _secrets.GetSecret("Account");

			if (accountName != null)
			{
				try
				{
					var silentResult = await _clientApplication.AcquireTokenSilent(_scopes, accountName).ExecuteAsync();

					if(request.Headers.Contains("Authorization"))
					{
						request.Headers.Remove("Authorization");
					}
					request.Headers.Add("Authorization", silentResult.CreateAuthorizationHeader());

					if (silentResult.Account != null)
					{
						_secrets.SetSecret("Account", silentResult.Account.Username, false);
					}

					_onAuthUpdate(null);

					return;
				}
				catch (MsalUiRequiredException)
				{

				}
			}

			var result = await _clientApplication.AcquireTokenInteractive(_scopes).ExecuteAsync();

			if(result == null || result.AccessToken == null)
			{
				_onAuthUpdate("Authentication timed out.");
				return;
			}

			if(result.Account != null)
			{
				_secrets.SetSecret("Account", result.Account.Username, false);
			}

			if (request.Headers.Contains("Authorization"))
			{
				request.Headers.Remove("Authorization");
			}
			request.Headers.Add("Authorization", result.CreateAuthorizationHeader());

			_onAuthUpdate(null);
		}

		private readonly string _clientId;
		private readonly string _tenantId;
		private readonly string[] _scopes = { "email", "profile", "offline_access", "User.Read", "Tasks.Read" , "Tasks.Read.Shared" , "Tasks.ReadWrite", "Tasks.ReadWrite.Shared", "Calendars.Read", "Calendars.Read.Shared" };
		//private readonly string[] _scopes2 = { "Users.Read" /*, "Tasks.Read", "Tasks.Read.Shared", "Tasks.ReadWrite", "Tasks.ReadWrite.Shared"*/ };

		private IPluginSecrets _secrets;
		private OnAuthUpdateDelegate _onAuthUpdate;
		public IPublicClientApplication _clientApplication;
	}
}
