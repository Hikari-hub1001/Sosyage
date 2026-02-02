using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Server.Data;
using Server.Infrastructure;

namespace Server;

public static class Application
{
    /// <summary>
    /// <code>dotnet run</code> で起動
    /// </summary>
    /// <param name="args"></param>
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers();
        builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite("Data Source=db/app.db"));
        builder.Services.AddAppServices();

        var app = builder.Build();

        var adminRoot = Path.Combine(app.Environment.WebRootPath ?? "wwwroot", "admin");
        var adminFileProvider = new PhysicalFileProvider(adminRoot);

        app.UseDefaultFiles(new DefaultFilesOptions
        {
            FileProvider = adminFileProvider,
            RequestPath = "/admin"
        });
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = adminFileProvider,
            RequestPath = "/admin"
        });

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        }

        app.MapControllers();

        app.Run("http://0.0.0.0:5000");
    }
}
