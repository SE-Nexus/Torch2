using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Torch2WebUI.Services.SQL
{
    public class AppDbContext : DbContext
    {
        static readonly string DatabaseName = "Torch2WebData";


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
