using Akka.Actor;
using FarmCraft.Community.Api.Config;
using FarmCraft.Community.Data.DTOs;
using FarmCraft.Community.Data.Entities.Users;
using FarmCraft.Community.Data.Messages.Roles;
using FarmCraft.Community.Services.Cache;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace FarmCraft.Community.Api.Policies.AdminPolicy
{
    public class AdminHandler : AuthorizationHandler<AdminRequirement>
    {
        private readonly ICacheService _cache;
        private readonly IActorRef _root;
        private readonly AppSettings _settings;

        public AdminHandler(ICacheService cache, IActorRef root, IOptions<AppSettings> settings)
        {
            _cache = cache;
            _root = root;
            _settings = settings.Value;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context, 
            AdminRequirement requirement
        )
        {
            string? role = context.User.Claims
                .Where(c => c.Type == "role")
                .Select(c => c.Value)
                .FirstOrDefault();

            if (role == null)
            {
                context.Fail();
                return Task.CompletedTask;
            }

            Role? adminRole = _cache.GetAndSetItem("admin_role", GetAdminRole) as Role;
            
            if (role == adminRole?.Id.ToString())
                context.Succeed(requirement);
            else
                context.Fail();

            return Task.CompletedTask;
        }

        private Role GetAdminRole()
        {
            FarmCraftActorResponse? response = _root.Ask(
                new AskForRole(null, "admin"),
                TimeSpan.FromSeconds(_settings.DefaultActorWaitSeconds)
            ).Result as FarmCraftActorResponse;

            return response != null && response.Data as Role != null
                ? (Role)response.Data
                : new Role();
        }
    }
}
