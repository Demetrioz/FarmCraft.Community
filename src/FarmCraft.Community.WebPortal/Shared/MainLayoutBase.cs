using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace FarmCraft.Community.WebPortal.Shared
{
    public class MainLayoutBase : LayoutComponentBase
    {
        [CascadingParameter]
        private Task<AuthenticationState> authenticationStateTask { get; set; }

        //[Inject]
        //private FarmCraftAuthenticationStateProvider AuthProvider { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var user = (await authenticationStateTask).User;


            //var pause = true;
            await base.OnInitializedAsync();
        }
    }
}
