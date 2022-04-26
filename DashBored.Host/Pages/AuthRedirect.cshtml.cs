using DashBored.PluginApi;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DashBored.Host.Pages
{
    public class AuthRedirectModel : PageModel
    {
        public AuthRedirectModel(IAuthService authService)
		{
            _authService = authService;

        }

        public void OnGet()
        {
            var newUri = new Uri($"{Request.Scheme}://{Request.Host}{Request.Path}{Request.QueryString}");
            _authService.OnCodeReceived(newUri);
        }

        IAuthService _authService;
    }
}
