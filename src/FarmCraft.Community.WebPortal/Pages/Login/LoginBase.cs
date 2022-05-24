using FarmCraft.Community.WebPortal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using MudBlazor;

namespace FarmCraft.Community.WebPortal.Pages
{
    // https://docs.microsoft.com/en-us/aspnet/core/blazor/state-management?view=aspnetcore-6.0&pivots=server#browser-storage-server

    public class LoginBase : ComponentBase
    {
        [Inject]
        private FarmCraftAuthenticationStateProvider AuthProvider { get; set; }
        [Inject]
        private ProtectedSessionStorage ProtectedSessionStorage { get; set; }
        [Inject]
        private ISnackbar SnackBar { get; set; }

        protected string Username { get; set; }
        protected string Password { get; set; }

        protected override async Task OnInitializedAsync()
        {
            ProtectedBrowserStorageResult<string> token = await
                ProtectedSessionStorage.GetAsync<string>("token");

            if (token.Success && token.Value != null)
                AuthProvider.Login(token.Value);
        }

        protected void HandleKeyDown(KeyboardEventArgs args)
        {
            if (args.Key == "Enter")
                HandleLogin();
        }

        protected async Task HandleLogin()
        {
            try
            {
                if(string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
                {
                    SnackBar.Add("Username and password required", Severity.Error);
                    return;
                }
                    
                string token = await AuthProvider.Login(Username, Password);
                await ProtectedSessionStorage.SetAsync("token", token);
            }
            catch (Exception ex)
            {
                SnackBar.Add(ex.Message, Severity.Error);
            }
        }
    }
}
