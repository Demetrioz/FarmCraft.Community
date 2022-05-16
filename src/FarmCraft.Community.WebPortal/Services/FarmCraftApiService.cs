using FarmCraft.Community.WebPortal.Config;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;

namespace FarmCraft.Community.WebPortal.Services
{
    public class FarmCraftApiService
    {
        private readonly HttpClient _httpClient;
        private JwtSecurityToken? _apiToken { get; set; }

        public FarmCraftApiService(HttpClient httpClient, IOptions<AppSettings> settings)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(settings.Value.ApiUri);
        }

        public JwtSecurityToken? UserToken { get { return _apiToken; } }

        //public event EventHandler<JwtSecurityToken> TokenUpdated;

        public async Task<JwtSecurityToken> Login(string username, string password)
        {
            Thread.Sleep(TimeSpan.FromSeconds(2));
            // jwt.io
            string jwtString = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
            JwtSecurityTokenHandler jwtHandler = new ();
            _apiToken = jwtHandler.ReadJwtToken(jwtString);

            //if (TokenUpdated != null)
            //    TokenUpdated(this, _apiToken);

            return _apiToken;
        }

        public void Logout()
        {
            _apiToken = null;
        }
    }
}
