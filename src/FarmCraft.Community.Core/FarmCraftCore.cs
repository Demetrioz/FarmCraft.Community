using Akka.Actor;
using Akka.Actor.Setup;
using Akka.Configuration;
using Akka.DependencyInjection;
using FarmCraft.Community.Data.Context;
using FluentMigrator.Runner;

namespace FarmCraft.Community.Core
{
    public class FarmCraftCore : BackgroundService
    {
        private ActorSystem? _actorSystem;
        private IActorRef? _root;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<FarmCraftCore> _logger;


        public FarmCraftCore(IServiceProvider provider, ILogger<FarmCraftCore> logger)
        {
            _serviceProvider = provider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (IServiceScope scope = _serviceProvider.CreateScope())
            {
                IMigrationRunner runner = scope.ServiceProvider
                    .GetRequiredService<IMigrationRunner>();

                runner.MigrateUp();
            }

            _logger.LogInformation("Migrations Completed");

            FarmCraftContext.SetTriggers();

            _logger.LogInformation("Triggers activated");

            Akka.Configuration.Config hocon = ConfigurationFactory.ParseString(@"
            akka {  
                actor {
                    provider = remote
                }
                remote {
                    dot-netty.tcp {
                        port = 8080
                        hostname = 0.0.0.0
                        public-hostname = core
                    }
                }
            }
            ");

            BootstrapSetup bootStrap = BootstrapSetup.Create().WithConfig(hocon);
            DependencyResolverSetup di = DependencyResolverSetup.Create(_serviceProvider);
            ActorSystemSetup actorSystemSetup = bootStrap.And(di);
            _actorSystem = ActorSystem.Create("FarmCraftCore", actorSystemSetup);

            _logger.LogInformation("Actor System Created");

            var props = DependencyResolver.For(_actorSystem).Props<FarmCraftManager>();
            _root = _actorSystem.ActorOf(props, "FarmCraftManager");

            _logger.LogInformation("Root Actor Created");
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await CoordinatedShutdown.Get(_actorSystem).Run(CoordinatedShutdown.ClrExitReason.Instance);
            _logger.LogInformation("Actor System Shutdown");
        }
    }
}