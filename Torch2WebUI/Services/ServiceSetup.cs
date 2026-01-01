using Microsoft.EntityFrameworkCore; // Add this using directive
using Torch2WebUI.Services.SQL;
using FluentMigrator.Runner;
using System.Threading.Tasks; // Add this using directive

namespace Torch2WebUI.Services
{
    public static class ServiceSetup
    {
        static readonly string DatabaseFileName = "Torch2";

        public static void SetupSQL(this IServiceCollection Services)
        {
            string basePath = AppContext.BaseDirectory; // or Directory.GetCurrentDirectory()
            string directoryPath = Path.Combine(basePath, "Data");

            //Create Base Directory
            Directory.CreateDirectory(directoryPath);

            string databasePath = Path.Combine(directoryPath, $"{DatabaseFileName}.db");
            string SQLiteConnectionString = $"Data Source={databasePath}";

            Services.AddDbContext<AppDbContext>(options => options.UseSqlite(SQLiteConnectionString));
            Services.AddFluentMigratorCore().ConfigureRunner(rb => rb.AddSQLite().WithGlobalConnectionString(SQLiteConnectionString).ScanIn(typeof(ServiceSetup).Assembly).For.Migrations());
        }

        public static async Task MigrateDatabase(this IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                //Development only: Ensure database is created
                await dbContext.Database.EnsureDeletedAsync();

                runner.MigrateUp();

                //Development only: Seed initial data
                //await dbContext.SaveChangesAsync();
            }
        }

    }
}
