using Microsoft.EntityFrameworkCore;
using Server.Data;

namespace Server.Services.AdminAccount;

public sealed class AdminAccountService : IAdminAccountService
{
    private readonly AppDbContext _db;

    public AdminAccountService(AppDbContext db)
    {
        _db = db;
    }

    public AdminAccountListResult GetAccounts(int offset, int limit)
    {
        var total = _db.Accounts.LongCount();
        var accounts = _db.Accounts
            .AsNoTracking()
            .OrderBy(account => account.Id)
            .Skip(offset)
            .Take(limit)
            .Select(account => new AdminAccountItem(
                account.Id,
                account.Name,
                account.LastLoginAt))
            .ToList();

        return new AdminAccountListResult(offset, limit, total, accounts);
    }

    public bool DeleteAccount(long id)
    {
        var account = _db.Accounts
            .Include(a => a.LoginBonuses)
            .Include(a => a.LoginBonusLogs)
            .SingleOrDefault(a => a.Id == id);

        if (account is null)
        {
            return false;
        }

        if (account.LoginBonusLogs.Count > 0)
        {
            _db.AccountLoginBonusLogs.RemoveRange(account.LoginBonusLogs);
        }

        if (account.LoginBonuses.Count > 0)
        {
            _db.AccountLoginBonuses.RemoveRange(account.LoginBonuses);
        }

        _db.Accounts.Remove(account);
        _db.SaveChanges();

        return true;
    }
}
