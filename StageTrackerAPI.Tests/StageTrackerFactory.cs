using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StageTrackerAPI.Data;

namespace StageTrackerAPI.Tests
{
    // Demarre l'API en memoire avec une base SQLite vide.
    // Chaque test cree sa propre factory, donc sa propre base.
    public class StageTrackerFactory : WebApplicationFactory<Program>
    {
        private readonly SqliteConnection _connexion = new("DataSource=:memory:");

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                // La base en memoire existe tant que la connexion reste ouverte
                _connexion.Open();
                services.AddDbContext<AppDbContext>(o => o.UseSqlite(_connexion));
            });
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            var host = base.CreateHost(builder);

            using var scope = host.Services.CreateScope();
            scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.EnsureCreated();

            return host;
        }

        // Pour preparer des donnees directement en base (ex. une relance deja en retard)
        public async Task ModifierBase(Func<AppDbContext, Task> action)
        {
            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await action(db);
            await db.SaveChangesAsync();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
                _connexion.Dispose();
        }
    }
}
