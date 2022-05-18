using Akka.Actor;
using FarmCraft.Community.Api.Config;
using FarmCraft.Community.Data.DTOs;
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
            try
            {
                object result = _server.Ask(
                    message,
                    TimeSpan.FromSeconds(_settings.ForwarderActorWaitSeconds)
                ).Result;

                Sender.Tell(result);
            }
            catch(Exception ex)
            {
                Sender.Tell(ActorResponse.Failure(
                    Guid.NewGuid().ToString(),
                    ex.Message
                ));
            }
        }
    }
}
