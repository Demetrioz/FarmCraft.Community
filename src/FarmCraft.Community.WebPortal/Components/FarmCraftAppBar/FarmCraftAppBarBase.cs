using FarmCraft.Community.WebPortal.Services;
using Microsoft.AspNetCore.Components;

namespace FarmCraft.Community.WebPortal.Components.FarmCraftAppBar
{
    public class FarmCraftAppBarBase : ComponentBase
    {
        [Inject]
        private NavigationManager NavigationManager { get; set; }
        [Inject]
        private FarmCraftAuthenticationStateProvider AuthProvider { get; set; }

        protected void HandleLogout()
        {
            NavigateHome();
            AuthProvider.Logout();
        }

        protected void NavigateHome()
        {
            NavigationManager.NavigateTo("/", true);
        }
    }
}
