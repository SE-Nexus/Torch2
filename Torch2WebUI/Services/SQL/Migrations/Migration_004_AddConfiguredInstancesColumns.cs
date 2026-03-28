using FluentMigrator;

namespace Torch2WebUI.Services.SQL.Migrations
{
    [Migration(4, "Migration_004_AddConfiguredInstancesColumns")]
    public class Migration_004_AddConfiguredInstancesColumns : Migration
    {
        public override void Up()
        {
            Alter.Table("ConfiguredInstances")
                .AddColumn("ProfileName").AsString().Nullable().WithDefaultValue(string.Empty)
                .AddColumn("TargetWorld").AsString().Nullable().WithDefaultValue(string.Empty)
                .AddColumn("TorchVersion").AsString().Nullable()
                .AddColumn("LastUpdate").AsDateTime().Nullable();
        }

        public override void Down()
        {
            Delete.Column("ProfileName").FromTable("ConfiguredInstances");
            Delete.Column("TargetWorld").FromTable("ConfiguredInstances");
            Delete.Column("TorchVersion").FromTable("ConfiguredInstances");
            Delete.Column("LastUpdate").FromTable("ConfiguredInstances");
        }
    }
}
