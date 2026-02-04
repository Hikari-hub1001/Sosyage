using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using EntityLoginBonusDay = Server.Data.Entities.LoginBonusDay;
using EntityLoginBonusDayReward = Server.Data.Entities.LoginBonusDayReward;
using EntityLoginBonusMonth = Server.Data.Entities.LoginBonusMonth;

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

    public long Register(LoginBonusRegistration request)
    {
        Validate(request);

        using var transaction = _db.Database.BeginTransaction();
        try
        {
            var month = _db.LoginBonusMonths
                .Include(m => m.Days)
                .ThenInclude(d => d.Rewards)
                .SingleOrDefault(m => m.Month == request.Month);

            if (month is null)
            {
                month = new EntityLoginBonusMonth
                {
                    Month = request.Month,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate
                };
            }
            else
            {
                month.StartDate = request.StartDate;
                month.EndDate = request.EndDate;

                if (month.Days.Count > 0)
                {
                    var rewards = month.Days.SelectMany(d => d.Rewards).ToList();
                    if (rewards.Count > 0)
                    {
                        _db.LoginBonusDayRewards.RemoveRange(rewards);
                    }

                    _db.LoginBonusDays.RemoveRange(month.Days);
                    month.Days.Clear();
                }
            }

            foreach (var day in request.Days)
            {
                var dayEntity = new EntityLoginBonusDay
                {
                    DayNumber = day.DayNumber
                };

                foreach (var reward in day.Rewards)
                {
                    dayEntity.Rewards.Add(new EntityLoginBonusDayReward
                    {
                        RewardId = reward.RewardId,
                        Quantity = reward.Quantity
                    });
                }

                month.Days.Add(dayEntity);
            }

            if (month.Id == 0)
            {
                _db.LoginBonusMonths.Add(month);
            }

            _db.SaveChanges();

            transaction.Commit();
            return month.Id;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public LoginBonusRegistration? FindByMonth(string month)
    {
        if (string.IsNullOrWhiteSpace(month))
        {
            return null;
        }

        var monthEntity = _db.LoginBonusMonths
            .AsNoTracking()
            .Include(m => m.Days)
            .ThenInclude(d => d.Rewards)
            .SingleOrDefault(m => m.Month == month);

        if (monthEntity is null)
        {
            return null;
        }

        var days = monthEntity.Days
            .OrderBy(day => day.DayNumber)
            .Select(day => new LoginBonusDay
            {
                DayNumber = day.DayNumber,
                Rewards = day.Rewards
                    .OrderBy(reward => reward.RewardId)
                    .Select(reward => new LoginBonusReward
                    {
                        RewardId = reward.RewardId,
                        Quantity = reward.Quantity
                    })
                    .ToList()
            })
            .ToList();

        return new LoginBonusRegistration
        {
            Month = monthEntity.Month,
            StartDate = monthEntity.StartDate,
            EndDate = monthEntity.EndDate,
            Days = days
        };
    }

    public bool DeleteByMonth(string month)
    {
        if (string.IsNullOrWhiteSpace(month))
        {
            return false;
        }

        using var transaction = _db.Database.BeginTransaction();
        try
        {
            var monthEntity = _db.LoginBonusMonths
                .Include(m => m.Days)
                .ThenInclude(d => d.Rewards)
                .SingleOrDefault(m => m.Month == month);

            if (monthEntity is null)
            {
                return false;
            }

            if (monthEntity.Days.Count > 0)
            {
                var rewards = monthEntity.Days.SelectMany(day => day.Rewards).ToList();
                if (rewards.Count > 0)
                {
                    _db.LoginBonusDayRewards.RemoveRange(rewards);
                }

                _db.LoginBonusDays.RemoveRange(monthEntity.Days);
            }

            _db.LoginBonusMonths.Remove(monthEntity);
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
        var monthKey = new DateOnly(today.Year, today.Month, 1).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        var monthEntity = _db.LoginBonusMonths
            .Include(m => m.Days)
            .ThenInclude(d => d.Rewards)
            .SingleOrDefault(m => m.Month == monthKey);

        if (monthEntity is null)
        {
            return null;
        }

        var dayList = monthEntity.Days
            .OrderBy(day => day.DayNumber)
            .ToList();
        var maxDayNumber = dayList.LastOrDefault()?.DayNumber ?? 0;
        var dailyBonuses = BuildDailyBonuses(dayList, maxDayNumber);

        var accountBonus = _db.AccountLoginBonuses
            .SingleOrDefault(b => b.AccountId == accountId && b.MonthId == monthEntity.Id);

        var currentDay = accountBonus?.CurrentDay ?? 0;
        // 期間外なら付与せず、データのみ返す。
        if (!IsWithinPeriod(today, monthEntity.StartDate, monthEntity.EndDate))
        {
            return BuildClaimResponse(monthEntity, currentDay, dailyBonuses, false);
        }

        // すでに本日付与済みなら、現在の状態を返す。
        if (accountBonus?.LastClaimedDay == today.Day)
        {
            return BuildClaimResponse(monthEntity, currentDay, dailyBonuses, false);
        }

        var nextDayNumber = GetNextDayNumber(dayList, currentDay);
        // 次の報酬日がない場合は、現在の状態を返す。
        if (!nextDayNumber.HasValue)
        {
            return BuildClaimResponse(monthEntity, currentDay, dailyBonuses, false);
        }

        currentDay = PersistClaim(accountId, monthEntity, accountBonus, nextDayNumber.Value, today.Day, jstNow);

        return BuildClaimResponse(monthEntity, currentDay, dailyBonuses, true);
    }

    private static void Validate(LoginBonusRegistration request)
    {
        if (string.IsNullOrWhiteSpace(request.Month))
        {
            throw new ArgumentException("month is required", nameof(request));
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
            if (day.DayNumber <= 0)
            {
                throw new ArgumentException("dayNumber must be >= 1", nameof(request));
            }

            foreach (var reward in day.Rewards)
            {
                if (reward.RewardId <= 0)
                {
                    throw new ArgumentException("rewardId must be >= 1", nameof(request));
                }

                if (reward.Quantity <= 0)
                {
                    throw new ArgumentException("quantity must be >= 1", nameof(request));
                }
            }
        }
    }

    private static List<LoginBonusDailyBonus> BuildDailyBonuses(List<EntityLoginBonusDay> days, int maxDayNumber)
    {
        var result = new List<LoginBonusDailyBonus>(Math.Max(0, maxDayNumber));
        var dayMap = days.ToDictionary(day => day.DayNumber, day => day);

        for (var dayNumber = 1; dayNumber <= maxDayNumber; dayNumber += 1)
        {
            if (!dayMap.TryGetValue(dayNumber, out var day))
            {
                result.Add(new LoginBonusDailyBonus());
                continue;
            }

            var bonuses = day.Rewards
                .OrderBy(reward => reward.RewardId)
                .Select(reward => new LoginBonusItemBonus
                {
                    Id = reward.RewardId,
                    Quantity = reward.Quantity
                })
                .ToList();

            result.Add(new LoginBonusDailyBonus
            {
                Bonuses = bonuses
            });
        }

        return result;
    }

    private static int? GetNextDayNumber(List<EntityLoginBonusDay> days, int currentDay)
    {
        foreach (var day in days)
        {
            if (day.DayNumber > currentDay)
            {
                return day.DayNumber;
            }
        }

        return null;
    }

    private static bool IsWithinPeriod(DateOnly today, string startDateText, string endDateText)
    {
        if (!DateOnly.TryParseExact(startDateText, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var startDate))
        {
            return false;
        }

        if (!DateOnly.TryParseExact(endDateText, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var endDate))
        {
            return false;
        }

        return today >= startDate && today <= endDate;
    }

    private static LoginBonusClaimResponse BuildClaimResponse(
        EntityLoginBonusMonth monthEntity,
        int currentDay,
        List<LoginBonusDailyBonus> dailyBonuses,
        bool claimedThisRequest)
    {
        return new LoginBonusClaimResponse
        {
            Period = new LoginBonusPeriod
            {
                Start = monthEntity.StartDate,
                End = monthEntity.EndDate
            },
            CurrentDay = currentDay,
            IsClaimedThisRequest = claimedThisRequest,
            DailyBonuses = dailyBonuses
        };
    }

    private int PersistClaim(
        long accountId,
        EntityLoginBonusMonth monthEntity,
        Data.Entities.AccountLoginBonus? accountBonus,
        int dayNumber,
        int todayDay,
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
                    MonthId = monthEntity.Id
                };
                _db.AccountLoginBonuses.Add(accountBonus);
            }

            accountBonus.CurrentDay = dayNumber;
            accountBonus.LastClaimedDay = todayDay;

            var logExists = _db.AccountLoginBonusLogs.Any(log =>
                log.AccountId == accountId &&
                log.MonthId == monthEntity.Id &&
                log.DayNumber == dayNumber);

            if (!logExists)
            {
                _db.AccountLoginBonusLogs.Add(new Data.Entities.AccountLoginBonusLog
                {
                    AccountId = accountId,
                    MonthId = monthEntity.Id,
                    DayNumber = dayNumber,
                    ClaimedAt = jstNow.ToString("yyyy-MM-dd'T'HH:mm:ss", CultureInfo.InvariantCulture)
                });
            }

            _db.SaveChanges();
            transaction.Commit();
            return accountBonus.CurrentDay;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}
