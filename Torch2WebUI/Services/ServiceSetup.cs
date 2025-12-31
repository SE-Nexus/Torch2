using Microsoft.EntityFrameworkCore; // Add this using directive
using Torch2WebUI.Services.SQL;
using FluentMigrator.Runner; // Add this using directive

namespace Torch2WebUI.Services
{
    public static class ServiceSetup
    {
        static readonly string DatabaseName = "NexusData";

        public static void SetupSQL(this IServiceCollection Services)
        {
            string basePath = AppContext.BaseDirectory; // or Directory.GetCurrentDirectory()
            string directoryPath = Path.Combine(basePath, "Data");

            //Create Base Directory
            Directory.CreateDirectory(directoryPath);

            string databasePath = Path.Combine(directoryPath, $"{DatabaseName}.db");
            string SQLiteConnectionString = $"Data Source={databasePath}";

            Services.AddDbContext<AppDbContext>(options => options.UseSqlite(SQLiteConnectionString));
            Services.AddFluentMigratorCore().ConfigureRunner(rb => rb.AddSQLite().WithGlobalConnectionString(SQLiteConnectionString).ScanIn(typeof(ServiceSetup).Assembly).For.Migrations());
        }

    }
}
