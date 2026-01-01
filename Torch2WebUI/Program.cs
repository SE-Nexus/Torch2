using MudBlazor.Services;
using Torch2WebUI.Components;
using Torch2WebUI.Services;

namespace Torch2WebUI
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddControllers();

            // Add MudBlazor services
            builder.Services.AddMudServices();

            // Add services to the container.
            builder.Services.AddRazorComponents().AddInteractiveServerComponents();


            builder.Services.AddSingleton<InstanceManager>();
            builder.Services.AddMemoryCache();
            builder.Services.SetupSQL();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error", createScopeForErrors: true);
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            await app.Services.MigrateDatabase();

            app.UseAntiforgery();
            app.UseWebSockets();
            app.MapControllers();

            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.Run();

        }
    }
}


