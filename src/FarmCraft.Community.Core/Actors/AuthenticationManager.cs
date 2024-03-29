﻿using FarmCraft.Community.Core.Config;
using FarmCraft.Community.Data.Entities.Users;
using FarmCraft.Community.Data.Messages.Authentication;
using FarmCraft.Community.Data.Repositories.Users;
using FarmCraft.Community.Services.Encryption;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace FarmCraft.Community.Core.Actors
{
    public class AuthenticationManager : FarmCraftActor
    {
        private readonly AuthenticationSettings _settings;
        private readonly IServiceScope _scope;
        private readonly IUserRepository _userRepo;
        private readonly IEncryptionService _encryptor;
        private readonly ILogger<AuthenticationManager> _logger;

        public AuthenticationManager(IServiceProvider provider)
        {
            _scope = provider.CreateScope();
            _settings = _scope.ServiceProvider
                .GetRequiredService<IOptions<AuthenticationSettings>>().Value;
            _userRepo = _scope.ServiceProvider
                .GetRequiredService<IUserRepository>();
            _logger = _scope.ServiceProvider
                .GetRequiredService<ILogger<AuthenticationManager>>();
            _encryptor = _scope.ServiceProvider
                .GetRequiredService<IEncryptionService>();

            Receive<AskToLogin>(message => HandleWith(Login, message));
            Receive<AskToCreateUser>(message => HandleWith(CreateUser, message));

            _logger.LogInformation($"{nameof(AuthenticationManager)} ready for messages");
        }

        protected override void PostStop()
        {
            _scope.Dispose();
            _logger.LogInformation($"{nameof(AuthenticationManager)} scope disposed");
        }

        //////////////////////////////////////////
        //                Logic                 //
        //////////////////////////////////////////

        private async Task<string> Login(AskToLogin message)
        {
            if (
                string.IsNullOrEmpty(message.Username)
                || string.IsNullOrEmpty(message.Password)
            )
                throw new ArgumentNullException(nameof(message));

            string plainTextUsername = _encryptor.Decrypt(message.Username);
            string plainTextPassword = _encryptor.Decrypt(message.Password);

            User? user = await _userRepo.FindUserByName(plainTextUsername);
            if (user == null || !ValidateHash(user.Password, plainTextPassword))
                throw new UnauthorizedAccessException("Unauthorized");

            await _userRepo.SetLastLogin(user.Id, DateTimeOffset.Now);
            return GenerateJWT(user);
        }

        private async Task<User> CreateUser(AskToCreateUser message)
        {
            if(
                string.IsNullOrEmpty(message.Username)
                || string.IsNullOrEmpty(message.Password)
                || message.RoleId == 0
            )
                throw new ArgumentNullException(nameof(message));

            string plainTextUser = _encryptor.Decrypt(message.Username);
            string plainTextPassword = _encryptor.Decrypt(message.Password);

            User? existingUser = await _userRepo.FindUserByName(plainTextUser);
            if (existingUser != null)
                throw new ArgumentException($"Cannot create user {message.Username}");

            User newUser = await _userRepo.CreateNewUser(new()
            {
                Id = Guid.NewGuid(),
                Username = plainTextUser,
                Password = GenerateHash(plainTextPassword),
                ResetRequired = true,
                RoleId = message.RoleId,
            });

            newUser.Password = null;

            return newUser;
        }

        //////////////////////////////////////////
        //                Helpers               //
        //////////////////////////////////////////

        private string GenerateJWT(User user)
        {
            SymmetricSecurityKey secretKey = new(Encoding.ASCII.GetBytes(_settings.SecretKey ?? ""));
            SigningCredentials signingCredentials = new(secretKey, SecurityAlgorithms.HmacSha256);

            List<Claim> claims = new()
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("sub", user.Id.ToString()),
                new Claim("name", user.Username),
                new Claim("role", user.RoleId.ToString()),
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
                expires: DateTime.UtcNow.AddMinutes(_settings.TokenDurationMinutes),
                signingCredentials: signingCredentials
            );

            JwtSecurityTokenHandler handler = new();
            return handler.WriteToken(tokenOptions);
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
    }
}
