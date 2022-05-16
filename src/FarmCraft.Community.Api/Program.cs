using Akka.Actor;
using Akka.Actor.Setup;
using Akka.Configuration;
using Akka.DependencyInjection;
using FarmCraft.Community.Api.Actors;
using FarmCraft.Community.Api.Config;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.Configure<AppSettings>(
    builder.Configuration.GetSection("AppSettings"));

builder.Services.AddSingleton(provider =>
{
    Config config = ConfigurationFactory.ParseString(@"
    akka {  
        actor {
            provider = remote
        }
        remote {
            dot-netty.tcp {
		        port = 8081
		        hostname = localhost
            }
        }
    }
    ");

    BootstrapSetup bootstrap = BootstrapSetup.Create().WithConfig(config);
    DependencyResolverSetup di = DependencyResolverSetup.Create(provider);
    ActorSystemSetup actorSystemSetup = bootstrap.And(di);
    return ActorSystem.Create("ApiSystem", actorSystemSetup);
});

builder.Services.AddSingleton(provider =>
{
    var actorSystem = provider.GetRequiredService<ActorSystem>();
    Props rootProps = DependencyResolver.For(actorSystem).Props<RootActor>();
    return actorSystem.ActorOf(rootProps, "RootActor");
});

builder.Services.AddControllers()
    .AddNewtonsoftJson();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// TODO: Shutdown the actor system when the app stops
// app.Lifetime.ApplicationStopping.Register(() => { });

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();