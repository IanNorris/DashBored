using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace DashBored.PluginApi
{
	public class PluginComponent<T> : ComponentBase where T : IPlugin
	{
		[Parameter]
		public T Model { get; set; }

		[Inject]
		IJSRuntime JSRuntime { get; set; }

		[Inject]
		public IAuthService AuthService { get; set; }

		public async Task OpenBrowserPopup(Uri targetUri)
		{
			await JSRuntime.InvokeVoidAsync("openWindow", targetUri.ToString());
		}

		protected override void OnParametersSet()
		{
			if (Model != null)
			{
				Model.OnDataChanged += () =>
				{
					InvokeAsync(StateHasChanged);
				};
			}
		}
	}
}
