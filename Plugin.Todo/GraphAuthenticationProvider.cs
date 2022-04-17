using Microsoft.Graph;
using Microsoft.Identity.Client;

namespace Plugin.Todo
{
	internal class GraphAuthenticationProvider : IAuthenticationProvider
	{
		public GraphAuthenticationProvider(TodoData data)
		{
			_clientId = data.ClientId;
			_tenantId = data.TenantId;
			_clientSecret = data.ClientSecret; //TODO: Move this
		}

		public async Task AuthenticateRequestAsync(HttpRequestMessage request)
		{
			var clientApplication = PublicClientApplicationBuilder.Create(this._clientId)
			   //.WithClientSecret(this._clientSecret)
			   .WithClientId(this._clientId)
			   .WithRedirectUri("http://localhost:7058/")
			   //.WithA
			   //.WithTenantId(/*this._tenantId*/"consumers") // <---- Seems to work with this?
			   .WithTenantId(this._tenantId)
			   //.With
			   .Build();

			//var result = await clientApplication.AcquireTokenForClient(this._scopes).ExecuteAsync();
			var result = await clientApplication.AcquireTokenWithDeviceCode(this._scopes, async (result) =>
			{
				//TODO: This spits out a URL and code for you to enter
			}).ExecuteAsync();

			//TODO
			//TokenCacheHelper.EnableSerialization(PublicClientApp.UserTokenCache);

			request.Headers.Add("Authorization", result.CreateAuthorizationHeader());
		}

		private readonly string _clientId;
		private readonly string _clientSecret;
		private readonly string _tenantId;
		private readonly string[] _scopes = { "Tasks.Read" /*, "Tasks.Read.Shared", "Tasks.ReadWrite", "Tasks.ReadWrite.Shared"*/ };
		//private readonly string[] _scopes2 = { "Users.Read" /*, "Tasks.Read", "Tasks.Read.Shared", "Tasks.ReadWrite", "Tasks.ReadWrite.Shared"*/ };
	}
}
