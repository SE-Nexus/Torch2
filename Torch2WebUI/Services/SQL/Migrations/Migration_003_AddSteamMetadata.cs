using FluentMigrator;

namespace Torch2WebUI.Services.SQL.Migrations
{
    [Migration(3, "Migration_003_AddSteamMetadata")]
    public class Migration_003_AddSteamMetadata : Migration
    {
        public override void Up()
        {
            Alter.Table("Mods")
                .AddColumn("FileSize").AsInt64().Nullable().WithDefaultValue(0)
                .AddColumn("PreviewUrl").AsString(500).Nullable()
                .AddColumn("Title").AsString(255).Nullable()
                .AddColumn("Description").AsString(int.MaxValue).Nullable()
                .AddColumn("TimeCreated").AsInt64().Nullable().WithDefaultValue(0)
                .AddColumn("TimeUpdated").AsInt64().Nullable().WithDefaultValue(0)
                .AddColumn("Subscriptions").AsInt32().Nullable().WithDefaultValue(0)
                .AddColumn("Favorites").AsInt32().Nullable().WithDefaultValue(0)
                .AddColumn("Views").AsInt32().Nullable().WithDefaultValue(0)
                .AddColumn("Tags").AsString(1000).Nullable()
                .AddColumn("SteamMetadataUpdatedAt").AsDateTime().Nullable();
        }

        public override void Down()
        {
            Delete.Column("FileSize").FromTable("Mods");
            Delete.Column("PreviewUrl").FromTable("Mods");
            Delete.Column("Title").FromTable("Mods");
            Delete.Column("Description").FromTable("Mods");
            Delete.Column("TimeCreated").FromTable("Mods");
            Delete.Column("TimeUpdated").FromTable("Mods");
            Delete.Column("Subscriptions").FromTable("Mods");
            Delete.Column("Favorites").FromTable("Mods");
            Delete.Column("Views").FromTable("Mods");
            Delete.Column("Tags").FromTable("Mods");
            Delete.Column("SteamMetadataUpdatedAt").FromTable("Mods");
        }
    }
}
