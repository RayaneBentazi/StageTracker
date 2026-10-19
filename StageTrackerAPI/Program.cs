using Microsoft.EntityFrameworkCore;
using StageTrackerAPI.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Les tests d'integration remplacent SQL Server par SQLite en memoire,
// donc on n'enregistre pas SQL Server dans l'environnement "Testing".
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<AppDbContext>(o =>
        o.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

// Permet au projet de tests d'utiliser WebApplicationFactory<Program>
public partial class Program { }
