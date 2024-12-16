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

		private Uri TargetPopupUri;

		public async Task OpenBrowserPopup(Uri targetUri)
		{
			TargetPopupUri = targetUri;

			_ = InvokeAsync(() =>
			{
				StateHasChanged();
			});
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

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			if(TargetPopupUri != null)
			{
				await JSRuntime.InvokeVoidAsync("openWindow", TargetPopupUri.ToString());
				TargetPopupUri = null;
			}
		}
	}
}
