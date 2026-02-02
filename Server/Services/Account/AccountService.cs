using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Data.Entities;

namespace Server.Services.Account;

public sealed class AccountService : IAccountService
{
    private static readonly TimeZoneInfo TokyoTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");

    private readonly AppDbContext _db;

    public AccountService(AppDbContext db)
    {
        _db = db;
    }

    public long Register(string name)
    {
        const int maxAttempts = 5;
        long id = 0;

        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            id = NextInt32Range(1_000_000_000, int.MaxValue);

            _db.Accounts.Add(new Data.Entities.Account
            {
                Id = id,
                Name = name,
                LastLoginAt = FormatJstNow()
            });

            try
            {
                _db.SaveChanges();
                return id;
            }
            catch (DbUpdateException) when (attempt < maxAttempts - 1)
            {
                _db.ChangeTracker.Clear();
            }
        }

        throw new InvalidOperationException("Failed to generate unique account id.");
    }

    public string? Login(long id)
    {
        var account = _db.Accounts.SingleOrDefault(a => a.Id == id);
        if (account is null)
        {
            return null;
        }

        account.LastLoginAt = FormatJstNow();
        _db.SaveChanges();

        return account.Name;
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
