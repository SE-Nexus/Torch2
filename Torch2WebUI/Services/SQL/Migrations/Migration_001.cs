using FluentMigrator;
using System.Diagnostics.CodeAnalysis;

namespace Torch2WebUI.Services.SQL.Migrations
{
    [Migration(1, "Migration_001")]
    public class Migration_001 : Migration
    {
        public override void Up()
        {
            Create.Table("ConfiguredInstances")
                .WithColumn("InstanceID").AsString().PrimaryKey().NotNullable()
                .WithColumn("Name").AsString().NotNullable()
                .WithColumn("MachineName").AsString().Nullable()
                .WithColumn("IPAddress").AsString().Nullable()
                .WithColumn("GamePort").AsInt32().NotNullable();



            //throw new NotImplementedException();
        }

        public override void Down()
        {
            //throw new NotImplementedException();
            
        }

    }
}
