using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace FarmCraft.Community.WebPortal.Services
{
    // https://docs.microsoft.com/en-us/aspnet/core/blazor/security/?view=aspnetcore-6.0

    public class FarmCraftAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly FarmCraftApiService _apiService;

        public FarmCraftAuthenticationStateProvider(FarmCraftApiService apiService)
        {
            _apiService = apiService;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            ClaimsIdentity identity = _apiService.UserToken != null
                ? new ClaimsIdentity(_apiService.UserToken.Claims, "ApiAuth")
                : new();

            return new AuthenticationState(new ClaimsPrincipal(identity));
        }

        public void Login(string token)
        {
            _apiService.Login(token);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public async Task<string> Login(string username, string password)
        {
            await _apiService.Login(username, password);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

            JwtSecurityTokenHandler handler = new();
            string token = handler.WriteToken(_apiService.UserToken);
            return token;
        }

        public void Logout()
        {
            _apiService.Logout();
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}
