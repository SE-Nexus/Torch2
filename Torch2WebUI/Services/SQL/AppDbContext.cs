using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Torch2API.DTOs.Instances;
using Torch2WebUI.Models.Database;

namespace Torch2WebUI.Services.SQL
{
    public class AppDbContext : DbContext
    {
        static readonly string DatabaseName = "Torch2WebData";

        //Saved Instances
        public DbSet<ConfiguredInstance> ConfiguredInstances { get; set; }

        //Mod Lists and Mods
        public DbSet<ModList> ModLists { get; set; }
        public DbSet<Mod> Mods { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            this.SaveChangesFailed += AppDbContext_SaveChangesFailed;
        }

        private void AppDbContext_SaveChangesFailed(object? sender, SaveChangesFailedEventArgs e)
        {
            Console.WriteLine("SaveChangesFailed: " + e.Exception.Message);
        }


    }
}
