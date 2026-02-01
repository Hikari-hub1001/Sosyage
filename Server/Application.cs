using Server.Services.Account;

namespace Server;

public static class Application
{
    /// <summary>
    /// <code>dotnet run</code> で起動
    /// </summary>
    /// <param name="args"></param>
    public static void Main(string[] args)
    {
        //Directory.CreateDirectory("db");

        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers();
        builder.Services.AddScoped(_ => new Microsoft.Data.Sqlite.SqliteConnection("Data Source=db/app.db"));
        builder.Services.AddScoped<IAccountService, AccountService>();

        var app = builder.Build();

        using (var connection = new Microsoft.Data.Sqlite.SqliteConnection("Data Source=db/app.db"))
        {
            connection.Open();
        }

        app.MapControllers();

        app.Run("http://0.0.0.0:5000");
    }
}
