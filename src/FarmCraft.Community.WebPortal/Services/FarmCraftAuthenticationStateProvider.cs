using FarmCraft.Community.Core.Config;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace FarmCraft.Community.WebPortal.Services
{
    // https://docs.microsoft.com/en-us/aspnet/core/blazor/security/?view=aspnetcore-6.0

    public class FarmCraftAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly AuthenticationSettings _authSettings;
        private readonly FarmCraftApiService _apiService;
        private JwtSecurityToken? _token;

        public FarmCraftAuthenticationStateProvider(
            IOptions<AuthenticationSettings> authSettings,
            FarmCraftApiService apiService
        )
        {
            _authSettings = authSettings.Value;
            _apiService = apiService;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            ClaimsIdentity identity = _token != null
                ? new ClaimsIdentity(_token.Claims, "ApiAuth")
                : new();

            return new AuthenticationState(new ClaimsPrincipal(identity));
        }

        public void HydrateToken(string token)
        {
            try
            {
                DateTimeOffset now = DateTimeOffset.Now;

                JwtSecurityTokenHandler handler = new();
                _token = handler.ReadJwtToken(token);

                if (
                    _token.ValidFrom > now
                    || _token.ValidTo <= now
                    || _token.Issuer != _authSettings.Issuer
                    || _token.Audiences.First() != _authSettings.Audience
                )
                    throw new UnauthorizedAccessException();
            }
            catch (Exception ex) 
            {
                _token = null;
            }
            finally
            {
                NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
            }
        }

        public async Task<string> Login(string username, string password)
        {
            _token = await _apiService.Login(username, password);
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

            JwtSecurityTokenHandler handler = new();
            string token = handler.WriteToken(_token);
            return token;
        }

        public void Logout()
        {
            _token = null;
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}
