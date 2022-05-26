using FarmCraft.Community.WebPortal.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace FarmCraft.Community.WebPortal.Components.FarmCraftDrawer
{
    public class FarmCraftDrawerBase : ComponentBase
    {
        [Inject]
        private IJSRuntime _js { get; set; }

        protected bool Open { get; set; } = false;
        protected string DrawerHeight { get; set; } = "";

        protected override async Task OnInitializedAsync()
        {
            DrawerHeight = await GetDrawerHeight();
            StateHasChanged();
        }

        private async Task<string> GetDrawerHeight()
        {
            WindowDimensions dimensions = await _js
                .InvokeAsync<WindowDimensions>("getDimensions");
            return $"height: {dimensions.Height - 64}px";
        }
    }
}
