namespace DashBored.MicrosoftGraph;

public static class Delegates
{
	public delegate void OnAuthErrorDelegate(GraphError errorType, string message);
	public delegate Task<Uri> OnLoginPromptDelegate(Uri authorizationUri, Uri redirectUri, CancellationToken cancellationToken);
}
