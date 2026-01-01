using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Torch2API.DTOs.Instances;

namespace Torch2WebUI.Services.SQL
{
    public class AppDbContext : DbContext
    {
        static readonly string DatabaseName = "Torch2WebData";

        //Saved Instances
        public DbSet<ConfiguredInstance> ConfiguredInstances { get; set; }

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
