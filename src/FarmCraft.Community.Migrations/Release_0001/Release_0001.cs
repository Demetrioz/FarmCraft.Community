using FluentMigrator;

namespace FarmCraft.Community.Migrations.Release_0001
{
    [Migration(0001)]
    public class Release_0001 : Migration
    {
        public override void Up()
        {
            Create.Table("role")
                .WithColumn("id").AsInt32().NotNullable().PrimaryKey().Identity()
                .WithColumn("name").AsString(255).NotNullable()
                .WithColumn("label").AsString(255).NotNullable()
                .WithFarmCraftBase();

            Create.Table("user")
                .WithColumn("id").AsGuid().NotNullable().PrimaryKey()
                .WithColumn("username").AsString(255).NotNullable()
                .WithColumn("password").AsString(255).NotNullable()
                .WithColumn("email").AsString(255).Nullable()
                .WithColumn("phone").AsString(20).Nullable()
                .WithColumn("reset_required").AsBoolean().NotNullable()
                .WithColumn("last_login").AsDateTimeOffset().Nullable()
                .WithColumn("role_id").AsInt32().NotNullable()
                    .ForeignKey("role", "id")
                    .OnDelete(System.Data.Rule.None)
                .WithFarmCraftBase();

            DateTimeOffset now = DateTimeOffset.Now;

            AutoIncrementer roleIncrementer = new();
            int adminId = roleIncrementer.Get();
            int memberId = roleIncrementer.Get();
            int guestId = roleIncrementer.Get();

            Insert.IntoTable("role")
                .Row(new 
                { 
                    id = adminId, 
                    name = "admin", 
                    label = "Admin",
                    created = now,
                    modified = now,
                    is_deleted = false
                })
                .Row(new 
                { 
                    id = memberId, 
                    name = "member", 
                    label = "Memmber",
                    created = now,
                    modified = now,
                    is_deleted = false
                })
                .Row(new 
                { 
                    id = guestId, 
                    name = "guest",
                    label = "Guest",
                    created = now,
                    modified = now,
                    is_deleted = false
                });

            Guid adminGuid = Guid.NewGuid();

            Insert.IntoTable("user")
                .Row(new 
                { 
                    id = adminGuid, 
                    username = "admin", 
                    password = "nBFcyVdbEX+eSsoybmuYiVtuzFBG1GdFlTVloi+WTkwsmps5", 
                    reset_required = false, 
                    role_id = adminId,
                    created = now,
                    modified = now,
                    is_deleted = false
                });
        }

        public override void Down()
        {
            Delete.Table("user");
            Delete.Table("role");
        }
    }
}
