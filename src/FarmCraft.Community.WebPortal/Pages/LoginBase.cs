using FarmCraft.Community.WebPortal.Services;
using Microsoft.AspNetCore.Components;

namespace FarmCraft.Community.WebPortal.Pages
{
    public class LoginBase : ComponentBase
    {
        [Inject]
        private FarmCraftAuthenticationStateProvider AuthProvider { get; set; }

        protected async Task HandleLogin()
        {
            await AuthProvider.Login();
        }
    }
}
