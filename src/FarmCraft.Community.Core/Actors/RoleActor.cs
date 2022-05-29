using Akka.Actor;
using FarmCraft.Community.Data.Entities.Users;
using FarmCraft.Community.Data.Messages.Roles;
using FarmCraft.Community.Data.Repositories.Roles;

namespace FarmCraft.Community.Core.Actors
{
    public class RoleActor : FarmCraftActor
    {
        private readonly IServiceScope _scope;
        private readonly IRoleRepository _roleRepo;
        private readonly ILogger<RoleActor> _logger;

        public RoleActor(IServiceProvider provider)
        {
            _scope = provider.CreateScope();
            _roleRepo = _scope.ServiceProvider
                .GetRequiredService<IRoleRepository>();
            _logger = _scope.ServiceProvider
                .GetRequiredService<ILogger<RoleActor>>();

            Receive<AskForRole>(message => HandleWith(GetRole, message));
            Receive<AskForRoles>(message => HandleGetRoles(message));

            _logger.LogInformation($"{nameof(RoleActor)} ready for messages");
        }

        protected override void PostStop()
        {
            _scope.Dispose();
            _logger.LogInformation($"{nameof(RoleActor)} scope disposed");
        }

        //////////////////////////////////////////
        //               Handlers               //
        //////////////////////////////////////////

        private void HandleGetRoles(AskForRoles message)
        {
            IActorRef sender = Sender;
            string requestId = Guid.NewGuid().ToString();

            GetAllRoles().ContinueWith(result =>
                HandleResult(result, sender, requestId));
        }

        //////////////////////////////////////////
        //                Logic                 //
        //////////////////////////////////////////
        
        private async Task<List<Role>> GetAllRoles()
        {
            return await _roleRepo.GetRoles();
        }

        private async Task<Role?> GetRole(AskForRole message)
        {
            if (message.Id == null && message.Name == null)
                throw new ArgumentNullException(nameof(message));

            return message.Id != null
                ? await _roleRepo.GetRoleById((int)message.Id)
                : await _roleRepo.GetRoleByName(message.Name ?? "");
        }
    }
}
