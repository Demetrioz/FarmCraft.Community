using Akka.Actor;
using Akka.TestKit.NUnit;
using FarmCraft.Community.Core.Actors;
using FarmCraft.Community.Core.Config;
using FarmCraft.Community.Data.Context;
using FarmCraft.Community.Data.DTOs;
using FarmCraft.Community.Data.Entities.Users;
using FarmCraft.Community.Data.Messages.Authentication;
using FarmCraft.Community.Data.Repositories.Users;
using FarmCraft.Community.Migrations;
using FarmCraft.Community.Migrations.Release_0001;
using FarmCraft.Community.Services.Encryption;
using FarmCraft.Community.Tests.Config;
using FluentMigrator.Runner;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;

namespace FarmCraft.Community.Tests
{
    public class AuthenticationTests : TestKit
    {
        private IServiceProvider? _serviceProvider { get; set; }
        private TestSettings? _settings;

        [OneTimeSetUp]
        public void Init()
        {
            DirectoryInfo? cwd = Directory.GetParent(AppContext.BaseDirectory);

            if (cwd != null)
            {
                IConfiguration config = new ConfigurationBuilder()
                    .SetBasePath(cwd.FullName)
                    .AddJsonFile("appsettings.json")
                    .Build();

                TestSettings settings = new();
                config.Bind("TestSettings", settings);
                _settings = settings;

                _serviceProvider = new ServiceCollection()
                    .Configure<EncryptionSettings>(config.GetSection("EncryptionSettings"))
                    .Configure<AuthenticationSettings>(config.GetSection("AuthenticationSettings"))
                    .AddDbContext<FarmCraftContext>(options =>
                        options.UseNpgsql(settings.ConnectionStrings["TestContext"]))
                    .AddMigrations(
                        settings.ConnectionStrings["TestContext"],
                        new List<Type>() { typeof(Release_0001) }
                    )
                    .AddTransient<IUserRepository, UserRepository>()
                    .AddTransient<IEncryptionService, EncryptionService>()
                    .BuildServiceProvider();

                using (IServiceScope scope = _serviceProvider.CreateScope())
                {
                    IMigrationRunner runner = scope.ServiceProvider
                        .GetRequiredService<IMigrationRunner>();

                    runner.MigrateUp();
                }

                FarmCraftContext.SetTriggers();
            }
        }

        [OneTimeTearDown]
        public void Teardown()
        {
            if (_serviceProvider == null)
                return;

            using (IServiceScope scope = _serviceProvider.CreateScope())
            {
                FarmCraftContext dbContext = scope.ServiceProvider
                    .GetRequiredService<FarmCraftContext>();

                List<User> users = dbContext.Users.ToList();

                dbContext.RemoveRange(users);
                dbContext.SaveChanges();
            }
        }

        [Order(100)]
        [TestCase("MyUser", "SomePassword", 1)]
        [TestCase("Billy Bob", "Nev3r!!", 2)]
        [TestCase("WildThang!", "Random1z3r", 3)]
        public void CanCreateUsers(string username, string password, int roleId)
        {
            if (_serviceProvider == null || _settings == null)
            {
                Assert.Fail("Missing Dependencies");
                return;
            }

            using (var scope = _serviceProvider.CreateScope())
            {
                IEncryptionService encryptor = scope.ServiceProvider
                    .GetRequiredService<IEncryptionService>();

                IActorRef authActor = Sys.ActorOf(
                    Props.Create(() => new AuthenticationManager(_serviceProvider)));

                string encryptedUser = encryptor.Encrypt(username);
                string encryptedPassword = encryptor.Encrypt(password);

                authActor.Tell(new AskToCreateUser(encryptedUser, encryptedPassword, roleId));

                FarmCraftActorResponse response = ExpectMsg<FarmCraftActorResponse>(
                    TimeSpan.FromSeconds(_settings.DefaultActorWaitSeconds));

                Assert.IsNotNull(response);
                Assert.AreEqual(response?.Status, ResponseStatus.Success);

                User? newUser = response?.Data as User;
                Assert.IsNotNull(newUser);

                Assert.AreEqual(newUser?.Username, username);
                Assert.AreEqual(newUser?.RoleId, roleId);
                Assert.AreEqual(newUser?.ResetRequired, true);
                Assert.IsNull(newUser?.Password);
                Assert.IsNull(newUser?.LastLogin);
            }
        }

        [Order(101)]
        [TestCase("MyUser", "SomePassword", 1)]
        [TestCase("Billy Bob", "Nev3r!!", 2)]
        [TestCase("WildThang!", "Random1z3r", 3)]
        public void WillNotCreateDuplicateUsers(string username, string password, int roleId)
        {
            if(_serviceProvider == null || _settings == null)
            {
                Assert.Fail("Missing Dependencies");
                return;
            }
                

            using (IServiceScope scope = _serviceProvider.CreateScope())
            {
                IEncryptionService encryptor = scope.ServiceProvider
                    .GetRequiredService<IEncryptionService>();

                IActorRef authActor = Sys.ActorOf(
                    Props.Create(() => new AuthenticationManager(_serviceProvider)));

                string encryptedUser = encryptor.Encrypt(username);
                string encryptedPassword = encryptor.Encrypt(password);

                authActor.Tell(new AskToCreateUser(encryptedUser, encryptedPassword, roleId));

                FarmCraftActorResponse response = ExpectMsg<FarmCraftActorResponse>(
                    TimeSpan.FromSeconds(_settings.DefaultActorWaitSeconds));

                Assert.IsNotNull(response);
                Assert.AreEqual(response?.Status, ResponseStatus.Failure);
            }
        }

        [Order(102)]
        [TestCase("MyUser", "SomePassword")]
        [TestCase("Billy Bob", "Nev3r!!")]
        [TestCase("WildThang!", "Random1z3r")]
        public void CanLogin(string username, string password)
        {
            if (_serviceProvider == null || _settings == null)
            {
                Assert.Fail("Missing Dependencies");
                return;
            }

            using (IServiceScope scope = _serviceProvider.CreateScope())
            {
                IEncryptionService encryptor = scope.ServiceProvider
                    .GetRequiredService<IEncryptionService>();

                IActorRef authActor = Sys.ActorOf(
                    Props.Create(() => new AuthenticationManager(_serviceProvider)));

                string encryptedUser = encryptor.Encrypt(username);
                string encryptedPassword = encryptor.Encrypt(password);

                authActor.Tell(new AskToLogin(encryptedUser, encryptedPassword));

                FarmCraftActorResponse response = ExpectMsg<FarmCraftActorResponse>(
                    TimeSpan.FromSeconds(_settings.DefaultActorWaitSeconds));

                Assert.IsNotNull(response);
                Assert.AreEqual(response?.Status, ResponseStatus.Success);

                string? tokenString = response?.Data as string;
                Assert.IsNotNull(tokenString);

                JwtSecurityTokenHandler handler = new();
                JwtSecurityToken token = handler.ReadJwtToken(tokenString);

                DateTimeOffset tokenExpiration = DateTime.UtcNow
                    .AddMinutes(_settings.TokenMinuteDuration);
                DateTimeOffset lowerBound = tokenExpiration.Subtract(TimeSpan.FromMinutes(1));
                DateTimeOffset upperBound = tokenExpiration.AddMinutes(1);

                Claim? sub = null;
                Claim? name = null;
                Claim? role = null;
                foreach (Claim claim in token.Claims)
                {
                    if (claim.Type == "sub")
                        sub = claim;
                    else if (claim.Type == "name")
                        name = claim;
                    else if (claim.Type == "role")
                        role = claim;
                }

                Assert.IsNotNull(token);
                Assert.AreEqual(token.Issuer, _settings.TokenIssuer);
                Assert.IsTrue(token.ValidTo >= lowerBound);
                Assert.IsTrue(token.ValidTo <= upperBound);
                Assert.IsNotNull(sub);
                Assert.IsNotNull(name);
                Assert.IsNotNull(role);
            }
        }
    }
}