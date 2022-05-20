using Akka.Actor;
using Akka.Actor.Setup;
using Akka.Configuration;
using Akka.DependencyInjection;
using FarmCraft.Community.Api.Actors;
using FarmCraft.Community.Api.Config;
using FarmCraft.Community.Api.Policies.AdminPolicy;
using FarmCraft.Community.Core.Config;
using FarmCraft.Community.Services.Cache;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

//////////////////////////////////////////
//          Configure Settings          //
//////////////////////////////////////////

AuthenticationSettings authSettings = new AuthenticationSettings();
builder.Configuration.Bind("AuthenticationSettings", authSettings);

builder.Services.Configure<AppSettings>(
    builder.Configuration.GetSection("AppSettings"));
builder.Services.Configure<CacheSettings>(
    builder.Configuration.GetSection("CacheSettings"));

//////////////////////////////////////////
//             Actor Setup              //
//////////////////////////////////////////

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

//////////////////////////////////////////
//          Add API Components          //
//////////////////////////////////////////

builder.Services.AddControllers()
    .AddNewtonsoftJson();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//////////////////////////////////////////
//    Authentication & Authorization    //
//////////////////////////////////////////

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = authSettings.Issuer,
            ValidAudience = authSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(authSettings.SecretKey ?? ""))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.Requirements.Add(new AdminRequirement()));
});

//////////////////////////////////////////
//        Add Additional Services       //
//////////////////////////////////////////

//builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IAuthorizationHandler, AdminHandler>();
builder.Services.AddSingleton<ICacheService, FarmCraftCache>();

//////////////////////////////////////////
//      Configure Request Pipeline      //
//////////////////////////////////////////

var app = builder.Build();

// TODO: Shutdown the actor system when the app stops
// app.Lifetime.ApplicationStopping.Register(() => { });

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();