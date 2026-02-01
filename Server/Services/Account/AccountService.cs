using System.Security.Cryptography;
using Microsoft.Data.Sqlite;

namespace Server.Services.Account;

public sealed class AccountService : IAccountService
{
    private static readonly TimeZoneInfo TokyoTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");

    private readonly SqliteConnection _connection;

    public AccountService(SqliteConnection connection)
    {
        _connection = connection;
    }

    public long Register(string name)
    {
        _connection.Open();
        try
        {
            const int maxAttempts = 5;
            long id = 0;

            for (var attempt = 0; attempt < maxAttempts; attempt++)
            {
                id = NextInt32Range(1_100_000_000, int.MaxValue);

                using var command = _connection.CreateCommand();
                command.CommandText = "INSERT INTO Account (id, name, last_login_at) VALUES ($id, $name, $lastLoginAt);";
                command.Parameters.AddWithValue("$id", id);
                command.Parameters.AddWithValue("$name", name);
                command.Parameters.AddWithValue("$lastLoginAt", FormatJstNow());

                try
                {
                    command.ExecuteNonQuery();
                    break;
                }
                catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
                {
                    if (attempt == maxAttempts - 1)
                    {
                        throw;
                    }
                }
            }

            return id;
        }
        finally
        {
            _connection.Close();
        }
    }

    public string? Login(long id)
    {
        _connection.Open();
        try
        {
            string? name;

            using (var selectCommand = _connection.CreateCommand())
            {
                selectCommand.CommandText = "SELECT name FROM Account WHERE id = $id;";
                selectCommand.Parameters.AddWithValue("$id", id);
                name = selectCommand.ExecuteScalar() as string;
            }

            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            using (var updateCommand = _connection.CreateCommand())
            {
                updateCommand.CommandText = "UPDATE Account SET last_login_at = $lastLoginAt WHERE id = $id;";
                updateCommand.Parameters.AddWithValue("$id", id);
                updateCommand.Parameters.AddWithValue("$lastLoginAt", FormatJstNow());
                updateCommand.ExecuteNonQuery();
            }

            return name;
        }
        finally
        {
            _connection.Close();
        }
    }

    private static string FormatJstNow()
    {
        var jstNow = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, TokyoTimeZone);
        return jstNow.ToString("yyyy-MM-dd'T'HH:mm:ss");
    }

    private static long NextInt32Range(int minInclusive, int maxInclusive)
    {
        if (minInclusive > maxInclusive)
        {
            throw new ArgumentOutOfRangeException(nameof(minInclusive));
        }

        var maxExclusive = (long)maxInclusive + 1;
        var range = (ulong)(maxExclusive - minInclusive);
        var limit = ulong.MaxValue - (ulong.MaxValue % range);
        Span<byte> buffer = stackalloc byte[8];
        ulong value;

        do
        {
            RandomNumberGenerator.Fill(buffer);
            value = BitConverter.ToUInt64(buffer);
        } while (value >= limit);

        return (long)(value % range) + minInclusive;
    }
}
