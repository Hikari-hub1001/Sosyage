using Microsoft.AspNetCore.StaticFiles;
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

        var logFilePath = Path.Combine(builder.Environment.ContentRootPath, "Log", "app.log");
        builder.Logging.ClearProviders();
        builder.Logging.AddSimpleConsole(options =>
        {
            options.SingleLine = true;
            options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
        });
        builder.Logging.AddProvider(new FileLoggerProvider(new FileLoggerOptions
        {
            FilePath = logFilePath
        }));

        builder.Services.AddControllers(options =>
        {
            options.Filters.Add<ApiVersionResponseFilter>();
        });
        builder.Services.AddScoped<ApiVersionResponseFilter>();
        builder.Services.Configure<VersionOptions>(builder.Configuration.GetSection("Version"));
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite("Data Source=db/app.db"));
        builder.Services.AddAppServices();

        var app = builder.Build();

        app.UseSwagger();
        app.UseSwaggerUI();

        var adminRoot = Path.Combine(app.Environment.WebRootPath ?? "wwwroot", "admin");
        var adminFileProvider = new PhysicalFileProvider(adminRoot);
        var staticRoot = Path.Combine(app.Environment.WebRootPath ?? "wwwroot", "static");
        var staticFileProvider = new PhysicalFileProvider(staticRoot);
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
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = staticFileProvider,
            RequestPath = "/static"
        });

        var contentTypeProvider = new FileExtensionContentTypeProvider();
        contentTypeProvider.Mappings[".bundle"] = "application/octet-stream";
        contentTypeProvider.Mappings[".hash"] = "text/plain";

        app.UseStaticFiles(new StaticFileOptions
        {
            ContentTypeProvider = contentTypeProvider
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
