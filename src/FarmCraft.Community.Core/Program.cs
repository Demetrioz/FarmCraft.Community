using FarmCraft.Community.Core;
using FarmCraft.Community.Core.Config;
using FarmCraft.Community.Data.Context;
using FarmCraft.Community.Data.Repositories.Users;
using FarmCraft.Community.Migrations;
using FarmCraft.Community.Migrations.Release_0001;
using FarmCraft.Community.Services.Encryption;
using Microsoft.EntityFrameworkCore;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        string dbConnection = context.Configuration
            .GetConnectionString("FarmCraftContext");

        //////////////////////////////////////////
        //          Configure Settings          //
        //////////////////////////////////////////

        services.Configure<EncryptionSettings>(
            context.Configuration.GetSection("EncryptionSettings"));
        services.Configure<AuthenticationSettings>(
            context.Configuration.GetSection("AuthenticationSettings"));

        //////////////////////////////////////////
        //            Database Setup            //
        //////////////////////////////////////////

        services.AddDbContext<FarmCraftContext>(options =>
            options.UseNpgsql(dbConnection));

        services.AddMigrations(
            dbConnection,
            new List<Type>() { typeof(Release_0001) }
        );

        //////////////////////////////////////////
        //             Add Services             //
        //////////////////////////////////////////

        services.AddTransient<IUserRepository, UserRepository>();
        services.AddTransient<IEncryptionService, EncryptionService>();

        //////////////////////////////////////////
        //            Add the Worker            //
        //////////////////////////////////////////

        services.AddHostedService<FarmCraftCore>();
    })
    .Build();

await host.RunAsync();
