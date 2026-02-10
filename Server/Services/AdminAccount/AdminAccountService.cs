using Microsoft.EntityFrameworkCore;
using Server.Data;

namespace Server.Services.AdminAccount;

public sealed class AdminAccountService : IAdminAccountService
{
    private readonly AppDbContext _db;
    private const int LoginHistoryLimit = 200;

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

    public AdminAccountDetailResult? GetAccountDetail(long id)
    {
        var account = _db.Accounts
            .AsNoTracking()
            .SingleOrDefault(a => a.Id == id);

        if (account is null)
        {
            return null;
        }

        var items = _db.AccountItems
            .AsNoTracking()
            .Include(item => item.Item)
            .Where(item => item.AccountId == id)
            .OrderBy(item => item.ItemId)
            .Select(item => new AdminAccountItemEntry(
                item.ItemId,
                item.Item.Name,
                item.Quantity))
            .ToList();

        var loginHistory = _db.AccountLoginBonusLogs
            .AsNoTracking()
            .Include(log => log.LoginBonus)
            .Include(log => log.LoginBonusDay)
            .Where(log => log.AccountId == id)
            .OrderByDescending(log => log.ClaimedAt)
            .Take(LoginHistoryLimit)
            .Select(log => new AdminAccountLoginHistoryEntry(
                log.ClaimedAt,
                log.LoginBonusId,
                log.LoginBonus != null ? log.LoginBonus.Name : "",
                log.LoginBonusDay != null ? log.LoginBonusDay.Date : "",
                log.ClaimCount))
            .ToList();

        return new AdminAccountDetailResult(
            account.Id,
            account.Name,
            account.LastLoginAt,
            items,
            loginHistory);
    }

    public bool DeleteAccount(long id)
    {
        var account = _db.Accounts
            .Include(a => a.Items)
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

        if (account.Items.Count > 0)
        {
            _db.AccountItems.RemoveRange(account.Items);
        }

        _db.Accounts.Remove(account);
        _db.SaveChanges();

        return true;
    }
}
