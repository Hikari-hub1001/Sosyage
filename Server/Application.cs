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
        var app = BuildApp(args);

        ConfigureSwagger(app);
        ConfigureStaticFiles(app);
        EnsureDatabase(app);

        app.MapControllers();
        app.Run("http://0.0.0.0:5000");
    }

    private static WebApplication BuildApp(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        ConfigureLogging(builder);
        ConfigureServices(builder);

        return builder.Build();
    }

    private static void ConfigureLogging(WebApplicationBuilder builder)
    {
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
    }

    private static void ConfigureServices(WebApplicationBuilder builder)
    {
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
    }

    private static void ConfigureSwagger(WebApplication app)
    {
        // if (app.Environment.IsDevelopment()) // テスト段階なので常に有効化
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
    }

    private static void ConfigureStaticFiles(WebApplication app)
    {
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
    }

    private static void EnsureDatabase(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();
    }
}
