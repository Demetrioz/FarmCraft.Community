using Akka.Actor;
using FarmCraft.Community.Core.Config;
using FarmCraft.Community.Core.Utilities;
using FarmCraft.Community.Data.DTOs;
using FarmCraft.Community.Data.Entities.Users;
using FarmCraft.Community.Data.Messages.Authentication;
using FarmCraft.Community.Data.Repositories.Users;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace FarmCraft.Community.Core.Actors
{
    public class AuthenticationManager : ReceiveActor
    {
        private readonly AuthenticationSettings _settings;
        private readonly IServiceScope _serviceScope;
        private readonly IUserRepository _userRepo;
        private readonly ILogger<AuthenticationManager> _logger;

        public AuthenticationManager(IServiceProvider provider)
        {
            _serviceScope = provider.CreateScope();
            _settings = _serviceScope.ServiceProvider
                .GetRequiredService<IOptions<AuthenticationSettings>>().Value;
            _userRepo = _serviceScope.ServiceProvider
                .GetRequiredService<IUserRepository>();
            _logger = _serviceScope.ServiceProvider
                .GetRequiredService<ILogger<AuthenticationManager>>();

            Receive<AskToLogin>(message => HandleLogin(message));

            _logger.LogInformation($"{nameof(AuthenticationManager)} ready for messages");
        }

        protected override void PostStop()
        {
            _serviceScope.Dispose();
            _logger.LogInformation($"{nameof(AuthenticationManager)} scope disposed");
        }

        //////////////////////////////////////////
        //               Handlers               //
        //////////////////////////////////////////

        private void HandleLogin(AskToLogin message)
        {
            IActorRef sender = Sender;
            string requestId = Guid.NewGuid().ToString();

            Login(message)
                .ContinueWith(result =>
                {
                    if (result.Exception != null)
                        sender.Tell(ActorResponse.Failure(requestId, result.Exception.Message));
                    else
                        sender.Tell(ActorResponse.Success(requestId, result.Result));
                });
        }

        //////////////////////////////////////////
        //                Logic                 //
        //////////////////////////////////////////

        private async Task<string> Login(AskToLogin message)
        {
            if (string.IsNullOrEmpty(message.Username))
                throw new ArgumentNullException(message.Username);

            if (string.IsNullOrEmpty(message.Password))
                throw new ArgumentNullException(message.Password);

            string plainTextUsername = Decrypt(message.Username);
            string plainTextPassword = Decrypt(message.Password);

            User? user = await _userRepo.FindUserByName(plainTextUsername);
            if (user == null || !ValidateHash(user.Password, plainTextPassword))
                throw new UnauthorizedAccessException("Unauthorized");

            await _userRepo.SetLastLogin(user.Id, DateTimeOffset.Now);
            return GenerateJWT(user);
        }

        //////////////////////////////////////////
        //                Helpers               //
        //////////////////////////////////////////

        private string GeneratePublicKey()
        {
            using (RSACryptoServiceProvider publicProvider = new())
            {
                RSAUtility.FromXmlString(publicProvider, _settings.PublicKey ?? "");
                StringWriter publicPemKey = new();
                RSAUtility.ExportPublicKey(publicProvider, publicPemKey);

                return publicPemKey.ToString();
            }
        }

        private string GenerateJWT(User user)
        {
            SymmetricSecurityKey secretKey = new(Encoding.ASCII.GetBytes(_settings.SecretKey ?? ""));
            SigningCredentials signingCredentials = new(secretKey, SecurityAlgorithms.HmacSha256);

            List<Claim> claims = new()
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("sub", user.Id.ToString()),
                new Claim("name", user.Username),
                new Claim("authentication_method", "JWT"),
                new Claim("reset_required", user.ResetRequired.ToString())
            };

            if (!string.IsNullOrEmpty(user.Email))
                claims.Add(new Claim("email", user.Email));

            if (!string.IsNullOrEmpty(user.Phone))
                claims.Add(new Claim("phone", user.Phone));

            JwtSecurityToken tokenOptions = new(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: signingCredentials
            );

            JwtSecurityTokenHandler handler = new();
            return handler.WriteToken(tokenOptions);
        }

        private JwtSecurityToken DecodeJwt(string jwt)
        {
            JwtSecurityTokenHandler handler = new();
            return handler.ReadJwtToken(jwt);
        }

        private string GenerateHash(string key)
        {
            // Generate salt
            byte[] salt;

            // replaced RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);
            salt = RandomNumberGenerator.GetBytes(16);

            byte[] pbkdf2 = KeyDerivation.Pbkdf2(key, salt, KeyDerivationPrf.HMACSHA256, 10000, 20);

            // Combine the two 
            byte[] hashBytes = new byte[36];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(pbkdf2, 0, hashBytes, 16, 20);

            return Convert.ToBase64String(hashBytes);
        }

        private bool ValidateHash(string hashedKey, string plainKey)
        {
            byte[] hashedBytes = Convert.FromBase64String(hashedKey);

            byte[] salt = new byte[16];
            Array.Copy(hashedBytes, 0, salt, 0, 16);

            var pbkdf2 = KeyDerivation.Pbkdf2(plainKey, salt, KeyDerivationPrf.HMACSHA256, 10000, 20);

            for (int i = 0; i < 20; i++)
                if (hashedBytes[i + 16] != pbkdf2[i])
                    return false;

            return true;
        }

        private string Decrypt(string key)
        {
            using (RSACryptoServiceProvider privateProvider = new())
            {
                byte[] encryptedKey = Convert.FromBase64String(key);

                RSAUtility.FromXmlString(privateProvider, _settings.PrivateKey ?? "");

                byte[] decryptedKey = privateProvider.Decrypt(encryptedKey, false);
                return Encoding.ASCII.GetString(decryptedKey);
            }
        }
    }
}
