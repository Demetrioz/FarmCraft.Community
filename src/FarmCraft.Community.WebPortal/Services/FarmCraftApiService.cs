using FarmCraft.Community.Data.DTOs;
using FarmCraft.Community.Data.DTOs.Requests;
using FarmCraft.Community.Services.Encryption;
using FarmCraft.Community.WebPortal.Config;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace FarmCraft.Community.WebPortal.Services
{
    public class FarmCraftApiService
    {
        private readonly IEncryptionService _encryptor;
        private readonly HttpClient _httpClient;
        private readonly ILogger<FarmCraftApiService> _logger;
        private JwtSecurityToken? _apiToken { get; set; }

        public FarmCraftApiService(
            IEncryptionService encryptor,
            HttpClient httpClient,
            IOptions<AppSettings> settings,
            ILogger<FarmCraftApiService> logger
        )
        {
            _encryptor = encryptor;
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(settings.Value.ApiUri);
            _logger = logger;
        }

        public JwtSecurityToken? UserToken { get { return _apiToken; } }

        public void Login(string token)
        {
            try
            {
                JwtSecurityTokenHandler handler = new();
                _apiToken = handler.ReadJwtToken(token);
                //return _apiToken;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Invalid token: {token}");
                throw new Exception(ex.Message);
            }
        }

        public async Task Login(string username, string password)
        {
            string requestBody = JsonConvert.SerializeObject(new LoginRequest
            {
                Username = _encryptor.Encrypt(username),
                Password = _encryptor.Encrypt(password)
            });

            StringContent content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PostAsync("authentication/login", content);

            if(response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                FarmCraftApiResponse? responseObject = JsonConvert.DeserializeObject<FarmCraftApiResponse>(responseContent);
                if (responseObject != null && responseObject.Status == ResponseStatus.Success && responseObject.Data != null)
                {
                    string jwtString = (string)responseObject.Data;
                    JwtSecurityTokenHandler handler = new();
                    _apiToken = handler.ReadJwtToken(jwtString);
                    //return _apiToken;
                }
                else
                {
                    _logger.LogError($"Unsuccessful login: {responseObject?.Status} || {responseObject?.Error}");
                    throw new Exception(responseObject?.Error);
                }
            }
            else
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Unsuccessful login: {response.StatusCode} || {responseContent}");
                throw new Exception(responseContent);
            }
        }

        public void Logout()
        {
            _apiToken = null;
        }
    }
}
