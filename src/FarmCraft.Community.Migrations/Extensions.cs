using FluentMigrator.Builders.Create.Table;
using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace FarmCraft.Community.Migrations
{
    public static class Extensions
    {
        public static ICreateTableColumnOptionOrWithColumnSyntax WithFarmCraftBase(
            this ICreateTableWithColumnSyntax table
        )
        {
            return table
                .WithColumn("created").AsDateTimeOffset().NotNullable()
                .WithColumn("modified").AsDateTimeOffset().NotNullable()
                .WithColumn("is_deleted").AsBoolean().NotNullable();
        } 

        public static IServiceCollection AddMigrations(
            this IServiceCollection services,
            string connectionString,
            List<Type> assemblyTypes
        )
        {
            Assembly[] assemblies = assemblyTypes
                .Select(t => t.Assembly)
                .ToArray();

            return services.AddFluentMigratorCore()
                .ConfigureRunner(builder =>
                    builder.AddPostgres()
                    .WithGlobalConnectionString(connectionString)
                    .ScanIn(assemblies).For.Migrations()
                )
                .AddLogging(builder => builder.AddFluentMigratorConsole())
                .Configure<FluentMigratorLoggerOptions>(options =>
                {
                    options.ShowSql = true;
                    options.ShowElapsedTime = true;
                });
        }
    }
}
