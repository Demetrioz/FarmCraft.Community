using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace FarmCraft.Community.WebPortal.Services
{
    // https://docs.microsoft.com/en-us/aspnet/core/blazor/security/?view=aspnetcore-6.0

    public class FarmCraftAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly FarmCraftApiService _apiService;
        //private JwtSecurityToken? _userToken { get; set; }

        public FarmCraftAuthenticationStateProvider(FarmCraftApiService apiService)
        {
            _apiService = apiService;
            //_apiService.TokenUpdated += HandleTokenChange;
        }

        //private void HandleTokenChange(object? sender, JwtSecurityToken token)
        //{
        //    NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        //}

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            //ClaimsIdentity identity = new();
            //JwtSecurityToken? token = _apiService.UserToken;

            ClaimsIdentity identity = _apiService.UserToken != null
                ? new ClaimsIdentity(_apiService.UserToken.Claims, "ApiAuth")
                : new();

            //if(_apiService.UserToken != null)
            //{
            //    // Do some logic
            //    try
            //    {
            //        var claims = new[]
            //        {
            //            new Claim("sub", "Something"),
            //            new Claim("phone", "SomethingEsle")
            //        };
            //        identity = new ClaimsIdentity(claims, "ApiAuth");
            //    }
            //    catch(Exception ex)
            //    {

            //    }
            //}

            //return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));

            return new AuthenticationState(new ClaimsPrincipal(identity));
        }

        public async Task Login()
        {
            await _apiService.Login("", "");
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public async Task Logout()
        {
            _apiService.Logout();
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}
