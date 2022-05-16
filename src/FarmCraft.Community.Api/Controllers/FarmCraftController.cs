using Akka.Actor;
using FarmCraft.Community.Api.Config;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FarmCraft.Community.Api.Controllers
{
    [ApiController]
    [Produces("application/json")]
    public abstract class FarmCraftController : ControllerBase
    {
        protected readonly IActorRef _rootActor;
        protected readonly int _defaultWait;

        public FarmCraftController(IActorRef rootActor, IOptions<AppSettings> settings)
        {
            _rootActor = rootActor;
            _defaultWait = settings.Value.DefaultActorWaitSeconds;
        }
    }
}
