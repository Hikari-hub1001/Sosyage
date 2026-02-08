using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using EntityLoginBonusDay = Server.Data.Entities.LoginBonusDay;
using EntityLoginBonusDayReward = Server.Data.Entities.LoginBonusDayReward;
using EntityLoginBonus = Server.Data.Entities.LoginBonus;
using EntityReward = Server.Data.Entities.Reward;

namespace Server.Services.LoginBonus;

public sealed class LoginBonusService : ILoginBonusService
{
    private static readonly TimeZoneInfo TokyoTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");

    private readonly AppDbContext _db;

    public LoginBonusService(AppDbContext db)
    {
        _db = db;
    }

    public IReadOnlyList<LoginBonusSummary> ListSummaries()
    {
        return _db.LoginBonuses
            .AsNoTracking()
            .OrderBy(bonus => bonus.Id)
            .Select(bonus => new LoginBonusSummary
            {
                Id = bonus.Id,
                Name = bonus.Name
            })
            .ToList();
    }

    public long Register(LoginBonusRegistration request)
    {
        Validate(request);

        using var transaction = _db.Database.BeginTransaction();
        try
        {
            EntityLoginBonus? loginBonus = null;
            if (request.Id > 0)
            {
                loginBonus = _db.LoginBonuses
                    .Include(bonus => bonus.Days)
                    .ThenInclude(day => day.Rewards)
                    .SingleOrDefault(bonus => bonus.Id == request.Id);

                if (loginBonus is null)
                {
                    throw new InvalidOperationException("login bonus not found");
                }
            }

            if (loginBonus is null)
            {
                loginBonus = new EntityLoginBonus
                {
                    Name = request.Name,
                    Type = request.Type,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate
                };
            }
            else
            {
                loginBonus.Name = request.Name;
                loginBonus.Type = request.Type;
                loginBonus.StartDate = request.StartDate;
                loginBonus.EndDate = request.EndDate;

                if (loginBonus.Days.Count > 0)
                {
                    var dayRewards = loginBonus.Days.SelectMany(day => day.Rewards).ToList();
                    if (dayRewards.Count > 0)
                    {
                        _db.LoginBonusDayRewards.RemoveRange(dayRewards);
                    }

                    _db.LoginBonusDays.RemoveRange(loginBonus.Days);
                    loginBonus.Days.Clear();
                }
            }

            var rewardCache = new Dictionary<(long ItemId, int Quantity), EntityReward>();

            foreach (var day in request.Days)
            {
                var dayEntity = new EntityLoginBonusDay
                {
                    Date = day.Date
                };

                foreach (var reward in day.Rewards)
                {
                    var rewardEntity = FindOrCreateReward(rewardCache, reward.ItemId, reward.Quantity);
                    dayEntity.Rewards.Add(new EntityLoginBonusDayReward
                    {
                        Reward = rewardEntity,
                        RewardId = rewardEntity.Id
                    });
                }

                loginBonus.Days.Add(dayEntity);
            }

            if (loginBonus.Id == 0)
            {
                _db.LoginBonuses.Add(loginBonus);
            }

            _db.SaveChanges();

            transaction.Commit();
            return loginBonus.Id;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public LoginBonusRegistration? FindById(long id)
    {
        if (id <= 0)
        {
            return null;
        }

        var loginBonus = _db.LoginBonuses
            .AsNoTracking()
            .Include(bonus => bonus.Days)
            .ThenInclude(day => day.Rewards)
            .ThenInclude(reward => reward.Reward)
            .SingleOrDefault(bonus => bonus.Id == id);

        if (loginBonus is null)
        {
            return null;
        }

        var days = loginBonus.Days
            .OrderBy(day => day.Date)
            .Select(day => new LoginBonusDay
            {
                Date = day.Date,
                Rewards = day.Rewards
                    .OrderBy(reward => reward.RewardId)
                    .Select(reward => new LoginBonusReward
                    {
                        ItemId = reward.Reward?.ItemId ?? 0,
                        Quantity = reward.Reward?.Quantity ?? 0
                    })
                    .ToList()
            })
            .ToList();

        return new LoginBonusRegistration
        {
            Id = loginBonus.Id,
            Name = loginBonus.Name,
            Type = loginBonus.Type,
            StartDate = loginBonus.StartDate,
            EndDate = loginBonus.EndDate,
            Days = days
        };
    }

    public bool DeleteById(long id)
    {
        if (id <= 0)
        {
            return false;
        }

        using var transaction = _db.Database.BeginTransaction();
        try
        {
            var loginBonus = _db.LoginBonuses
                .Include(bonus => bonus.Days)
                .ThenInclude(day => day.Rewards)
                .SingleOrDefault(bonus => bonus.Id == id);

            if (loginBonus is null)
            {
                return false;
            }

            if (loginBonus.Days.Count > 0)
            {
                var dayRewards = loginBonus.Days.SelectMany(day => day.Rewards).ToList();
                if (dayRewards.Count > 0)
                {
                    _db.LoginBonusDayRewards.RemoveRange(dayRewards);
                }

                _db.LoginBonusDays.RemoveRange(loginBonus.Days);
            }

            _db.LoginBonuses.Remove(loginBonus);
            _db.SaveChanges();
            transaction.Commit();
            return true;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public LoginBonusClaimResponse? ClaimForAccount(long accountId)
    {
        if (accountId <= 0)
        {
            throw new ArgumentException("accountId must be >= 1", nameof(accountId));
        }

        var jstNow = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, TokyoTimeZone);
        var today = DateOnly.FromDateTime(jstNow.DateTime);
        var todayText = today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var todayKey = today.Year * 10000 + (today.Month * 100) + today.Day;

        var loginBonusHeader = _db.LoginBonuses
            .AsNoTracking()
            .Select(bonus => new
            {
                bonus.Id,
                bonus.StartDate,
                bonus.EndDate
            })
            .ToList()
            .Where(bonus =>
                string.Compare(bonus.StartDate, todayText, StringComparison.Ordinal) <= 0 &&
                string.Compare(bonus.EndDate, todayText, StringComparison.Ordinal) >= 0)
            .OrderByDescending(bonus => bonus.StartDate)
            .FirstOrDefault();

        var loginBonus = loginBonusHeader is null
            ? null
            : _db.LoginBonuses
                .Include(bonus => bonus.Days)
                .ThenInclude(day => day.Rewards)
                .ThenInclude(reward => reward.Reward)
                .SingleOrDefault(bonus => bonus.Id == loginBonusHeader.Id);

        if (loginBonus is null)
        {
            return null;
        }

        var dayList = loginBonus.Days
            .OrderBy(day => day.Date)
            .ToList();
        var dailyBonuses = BuildDailyBonuses(dayList);

        var accountBonus = _db.AccountLoginBonuses
            .SingleOrDefault(bonus => bonus.AccountId == accountId && bonus.LoginBonusId == loginBonus.Id);

        var claimCount = accountBonus?.ClaimCount ?? 0;
        // すでに本日付与済みなら、現在の状態を返す。
        if (accountBonus?.LastClaimedDay == todayKey)
        {
            return BuildClaimResponse(loginBonus, claimCount, dailyBonuses, false);
        }

        var eligibleDays = dayList
            .Where(day => string.Compare(day.Date, todayText, StringComparison.Ordinal) <= 0)
            .ToList();

        // 付与対象がなければ現在の状態を返す。
        if (eligibleDays.Count <= claimCount)
        {
            return BuildClaimResponse(loginBonus, claimCount, dailyBonuses, false);
        }

        var nextDay = eligibleDays[claimCount];
        claimCount = PersistClaim(accountId, loginBonus, accountBonus, nextDay, claimCount + 1, todayKey, jstNow);

        return BuildClaimResponse(loginBonus, claimCount, dailyBonuses, true);
    }

    private static void Validate(LoginBonusRegistration request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ArgumentException("name is required", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.Type))
        {
            throw new ArgumentException("type is required", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.StartDate))
        {
            throw new ArgumentException("startDate is required", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.EndDate))
        {
            throw new ArgumentException("endDate is required", nameof(request));
        }

        foreach (var day in request.Days)
        {
            if (string.IsNullOrWhiteSpace(day.Date))
            {
                throw new ArgumentException("date is required", nameof(request));
            }

            foreach (var reward in day.Rewards)
            {
                if (reward.ItemId <= 0)
                {
                    throw new ArgumentException("itemId must be >= 1", nameof(request));
                }

                if (reward.Quantity <= 0)
                {
                    throw new ArgumentException("quantity must be >= 1", nameof(request));
                }
            }
        }
    }

    private static List<LoginBonusDailyBonus> BuildDailyBonuses(List<EntityLoginBonusDay> days)
    {
        var result = new List<LoginBonusDailyBonus>(days.Count);
        foreach (var day in days)
        {
            var bonuses = day.Rewards
                .OrderBy(reward => reward.RewardId)
                .Select(reward => new LoginBonusItemBonus
                {
                    Id = reward.Reward?.ItemId ?? reward.RewardId,
                    Quantity = reward.Reward?.Quantity ?? 0
                })
                .ToList();

            result.Add(new LoginBonusDailyBonus
            {
                Bonuses = bonuses
            });
        }

        return result;
    }

    private static LoginBonusClaimResponse BuildClaimResponse(
        EntityLoginBonus loginBonus,
        int claimCount,
        List<LoginBonusDailyBonus> dailyBonuses,
        bool claimedThisRequest)
    {
        return new LoginBonusClaimResponse
        {
            Period = new LoginBonusPeriod
            {
                Start = loginBonus.StartDate,
                End = loginBonus.EndDate
            },
            CurrentDay = claimCount,
            IsClaimedThisRequest = claimedThisRequest,
            DailyBonuses = dailyBonuses
        };
    }

    private int PersistClaim(
        long accountId,
        EntityLoginBonus loginBonus,
        Data.Entities.AccountLoginBonus? accountBonus,
        EntityLoginBonusDay nextDay,
        int nextClaimCount,
        int todayKey,
        DateTimeOffset jstNow)
    {
        using var transaction = _db.Database.BeginTransaction();
        try
        {
            if (accountBonus is null)
            {
                accountBonus = new Data.Entities.AccountLoginBonus
                {
                    AccountId = accountId,
                    LoginBonusId = loginBonus.Id
                };
                _db.AccountLoginBonuses.Add(accountBonus);
            }

            accountBonus.ClaimCount = nextClaimCount;
            accountBonus.LastClaimedDay = todayKey;

            var logExists = _db.AccountLoginBonusLogs.Any(log =>
                log.AccountId == accountId &&
                log.LoginBonusId == loginBonus.Id &&
                log.LoginBonusDayId == nextDay.Id);

            if (!logExists)
            {
                _db.AccountLoginBonusLogs.Add(new Data.Entities.AccountLoginBonusLog
                {
                    AccountId = accountId,
                    LoginBonusId = loginBonus.Id,
                    LoginBonusDayId = nextDay.Id,
                    ClaimCount = nextClaimCount,
                    ClaimedAt = jstNow.ToString("yyyy-MM-dd'T'HH:mm:ss", CultureInfo.InvariantCulture)
                });
            }

            _db.SaveChanges();
            transaction.Commit();
            return accountBonus.ClaimCount;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    private EntityReward FindOrCreateReward(Dictionary<(long ItemId, int Quantity), EntityReward> cache, long itemId, int quantity)
    {
        var key = (itemId, quantity);
        if (cache.TryGetValue(key, out var cached))
        {
            return cached;
        }

        var reward = _db.Rewards.SingleOrDefault(r =>
            r.Type == "item" &&
            r.ItemId == itemId &&
            r.Quantity == quantity);

        if (reward is null)
        {
            reward = new EntityReward
            {
                Type = "item",
                ItemId = itemId,
                Quantity = quantity
            };
            _db.Rewards.Add(reward);
        }

        cache[key] = reward;
        return reward;
    }
}
