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
        private FarmCraftAuthenticationStateProvider _authProvider { get; set; }
        [Inject]
        private ProtectedSessionStorage _protectedSessionStorage { get; set; }
        [Inject]
        private ISnackbar _snackBar { get; set; }

        protected string Username { get; set; }
        protected string Password { get; set; }

        protected override async Task OnInitializedAsync()
        {
            ProtectedBrowserStorageResult<string> token = await
                _protectedSessionStorage.GetAsync<string>("token");

            if (token.Success && token.Value != null)
                _authProvider.Login(token.Value);
        }

        protected async Task HandleLogin()
        {
            try
            {
                if(string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
                {
                    _snackBar.Add("Username and password required", Severity.Error);
                    return;
                }
                    
                string token = await _authProvider.Login(Username, Password);
                await _protectedSessionStorage.SetAsync("token", token);
            }
            catch (Exception ex)
            {
                _snackBar.Add(ex.Message, Severity.Error);
            }
        }
    }
}
