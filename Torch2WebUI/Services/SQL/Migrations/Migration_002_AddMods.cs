using FluentMigrator;

namespace Torch2WebUI.Services.SQL.Migrations
{
    [Migration(2, "Migration_002_AddMods")]
    public class Migration_002_AddMods : Migration
    {
        public override void Up()
        {
            Create.Table("ModLists")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("Name").AsString(255).NotNullable()
                .WithColumn("Description").AsString(1000).Nullable()
                .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime)
                .WithColumn("UpdatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime);

            Create.Table("Mods")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("ModListId").AsInt32().NotNullable().ForeignKey("FK_Mods_ModListId", "ModLists", "Id")
                .WithColumn("Name").AsString(255).NotNullable()
                .WithColumn("ModId").AsString(255).NotNullable()
                .WithColumn("Url").AsString(500).NotNullable()
                .WithColumn("Source").AsString(50).NotNullable().WithDefaultValue("SteamWorkshop")
                .WithColumn("CreatedAt").AsDateTime().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime);

            Create.Index("IX_Mods_ModListId").OnTable("Mods").OnColumn("ModListId").Ascending();
        }

        public override void Down()
        {
            Delete.Table("Mods");
            Delete.Table("ModLists");
        }
    }
}
