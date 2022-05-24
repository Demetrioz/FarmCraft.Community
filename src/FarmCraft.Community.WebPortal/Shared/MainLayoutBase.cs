using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;

namespace FarmCraft.Community.WebPortal.Shared
{
    public class MainLayoutBase : LayoutComponentBase
    {
        [CascadingParameter]
        private Task<AuthenticationState> AuthenticationStateTask { get; set; }

        protected MudTheme FarmCraftTheme = new MudTheme
        {
            Palette = new Palette
            {
                Primary = "#306B2B",
                Secondary = "#FF9F1D"//"#F39412",
            },
            //PaletteDark = new Palette
            //{

            //},
            LayoutProperties = new LayoutProperties
            {
                AppbarHeight = "64px"
            }
        };

        //[Inject]
        //private FarmCraftAuthenticationStateProvider AuthProvider { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var user = (await AuthenticationStateTask).User;


            //var pause = true;
            await base.OnInitializedAsync();
        }
    }
}
