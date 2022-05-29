using Akka.Actor;
using FarmCraft.Community.Data.DTOs;

namespace FarmCraft.Community.Core.Actors
{
    public abstract class FarmCraftActor : ReceiveActor
    {
        protected void HandleWith<T, O>(Func<T, Task<O>> func, T message)
        {
            IActorRef sender = Sender;
            string requestId = Guid.NewGuid().ToString();

            func(message).ContinueWith(result =>
                HandleResult(result, sender, requestId));
        }

        protected void HandleResult<T>(
            Task<T> result, 
            IActorRef sender, 
            string requestId
        )
        {
            if (result.Exception != null)
                sender.Tell(ActorResponse.Failure(requestId, result.Exception.Message));
            else
                sender.Tell(ActorResponse.Success(requestId, result.Result));
        }
    }
}
