using Akka.Actor;
using FarmCraft.Community.Api.Config;
using Microsoft.Extensions.Options;

namespace FarmCraft.Community.Api.Actors
{
    public class RootActor : ReceiveActor
    {
        private readonly ActorSelection _server;
        private readonly AppSettings _settings;

        public RootActor(IOptions<AppSettings> settings)
        {
            _settings = settings.Value;
            _server = Context.ActorSelection(_settings.CoreActorUri);

            ReceiveAny(message => ForwardToCore(message));
        }

        private void ForwardToCore(object message)
        {
            object result = _server.Ask(
                message, 
                TimeSpan.FromSeconds(_settings.DefaultActorWaitSeconds)
            ).Result;

            Sender.Tell(result);
        }
    }
}
