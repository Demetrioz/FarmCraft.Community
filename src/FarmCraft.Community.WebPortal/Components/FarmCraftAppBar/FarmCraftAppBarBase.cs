using FarmCraft.Community.WebPortal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace FarmCraft.Community.WebPortal.Components.FarmCraftAppBar
{
    public class FarmCraftAppBarBase : ComponentBase
    {
        [Inject]
        private NavigationManager _navigationManager { get; set; }
        [Inject]
        private FarmCraftAuthenticationStateProvider _authProvider { get; set; }
        [Inject]
        private ProtectedSessionStorage _protectedSessionStorage { get; set; }

        protected async Task HandleLogout()
        {
            await _protectedSessionStorage.DeleteAsync("token");
            NavigateHome();
            _authProvider.Logout();
        }

        protected void NavigateHome()
        {
            _navigationManager.NavigateTo("/", true);
        }
    }
}
