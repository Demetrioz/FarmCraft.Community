using FarmCraft.Community.Core;
using FarmCraft.Community.Data.Context;
using FarmCraft.Community.Data.Repositories.Users;
using FarmCraft.Community.Migrations;
using FarmCraft.Community.Migrations.Release_0001;
using Microsoft.EntityFrameworkCore;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        string dbConnection = context.Configuration
            .GetConnectionString("FarmCraftContext");

        //////////////////////////////////////////
        //            Database Setup            //
        //////////////////////////////////////////

        services.AddDbContext<FarmCraftContext>(options =>
            options.UseNpgsql(dbConnection));

        services.AddMigrations(
            dbConnection,
            new List<Type>() { typeof(Release_0001) }
        );

        services.AddTransient<IUserRepository, UserRepository>();

        //////////////////////////////////////////
        //            Add the Worker            //
        //////////////////////////////////////////

        services.AddHostedService<FarmCraftCore>();
    })
    .Build();

await host.RunAsync();
