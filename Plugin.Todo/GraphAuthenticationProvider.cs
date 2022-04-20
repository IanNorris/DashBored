using DashBored.PluginApi;
using Microsoft.Graph;
using Microsoft.Identity.Client;

namespace Plugin.Todo
{
	internal class GraphAuthenticationProvider : IAuthenticationProvider
	{
		public delegate void OnAuthUpdateDelegate(string message);

		public GraphAuthenticationProvider(IPluginSecrets secrets, TodoData data, OnAuthUpdateDelegate onAuthUpdate)
		{
			_clientId = data.ClientId;
			_tenantId = data.TenantId;
			_onAuthUpdate = onAuthUpdate;
			_secrets = secrets;
		}

		public async Task AuthenticateRequestAsync(HttpRequestMessage request)
		{
			var accountName = _secrets.GetSecret("Account");

			var clientApplication = PublicClientApplicationBuilder.Create(this._clientId)
			   .WithClientId(_clientId)
			   .WithRedirectUri("http://localhost/")
			   .WithTenantId(_tenantId)
			   .Build();

			clientApplication.UserTokenCache.SetBeforeAccess(args =>
			{
				var currentValue = _secrets.GetSecret("TokenCache");

				if (currentValue != null)
				{
					var bytes = Convert.FromBase64String(currentValue);
					args.TokenCache.DeserializeMsalV3(bytes);
				}
			});

			clientApplication.UserTokenCache.SetAfterAccess(args =>
			{
				if (args.HasStateChanged)
				{
					var bytes = args.TokenCache.SerializeMsalV3();
					var dataBase64 = Convert.ToBase64String(bytes);

					_secrets.SetSecret("TokenCache", dataBase64);
				}
			});

			if (accountName != null)
			{
				try
				{
					var silentResult = await clientApplication.AcquireTokenSilent(_scopes, accountName).ExecuteAsync();

					if(request.Headers.Contains("Authorization"))
					{
						request.Headers.Remove("Authorization");
					}
					request.Headers.Add("Authorization", silentResult.AccessToken);

					_onAuthUpdate(null);

					return;
				}
				catch (MsalUiRequiredException)
				{

				}
			}

			var result = await clientApplication.AcquireTokenWithDeviceCode(_scopes, (result) =>
			{
				_onAuthUpdate(result.Message);

				return Task.CompletedTask;
			}).ExecuteAsync();

			if(result == null || result.AccessToken == null)
			{
				_onAuthUpdate("Authentication timed out.");
				return;
			}

			if(result.Account != null)
			{
				_secrets.SetSecret("Account", result.Account.Username);
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
		private readonly string[] _scopes = { "offline_access", "User.Read", "Tasks.Read" , "Tasks.Read.Shared" , "Tasks.ReadWrite", "Tasks.ReadWrite.Shared" };
		//private readonly string[] _scopes2 = { "Users.Read" /*, "Tasks.Read", "Tasks.Read.Shared", "Tasks.ReadWrite", "Tasks.ReadWrite.Shared"*/ };

		private IPluginSecrets _secrets;
		private OnAuthUpdateDelegate _onAuthUpdate;
	}
}
