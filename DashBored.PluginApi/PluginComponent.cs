using Microsoft.AspNetCore.Components;

namespace DashBored.PluginApi
{
	public class PluginComponent<T> : ComponentBase where T : IPlugin
	{
		[Parameter]
		public T Model { get; set; }


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
