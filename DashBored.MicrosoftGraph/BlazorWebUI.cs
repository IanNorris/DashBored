using Microsoft.Identity.Client.Extensibility;
using static DashBored.MicrosoftGraph.GraphAuthenticationProviderPublic;

namespace DashBored.MicrosoftGraph
{
	internal class BlazorWebUI : ICustomWebUi
	{
		public BlazorWebUI(OnLoginPromptDelegate onLoginPrompt)
		{
			_onLoginPrompt = onLoginPrompt;
		}

		public async Task<Uri> AcquireAuthorizationCodeAsync(Uri authorizationUri, Uri redirectUri, CancellationToken cancellationToken)
		{
			return await _onLoginPrompt(authorizationUri, redirectUri, cancellationToken);
		}

		private OnLoginPromptDelegate _onLoginPrompt;
	}
}
