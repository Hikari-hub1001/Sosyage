namespace Server.Services.AdminAccount;

public sealed record AdminAccountItem(long Id, string Name, string LastLoginAt);

public sealed record AdminAccountListResult(
    int Offset,
    int Limit,
    long Total,
    IReadOnlyList<AdminAccountItem> Accounts);

public sealed record AdminAccountItemEntry(long ItemId, string Name, int Quantity);

public sealed record AdminAccountLoginHistoryEntry(
    string ClaimedAt,
    long LoginBonusId,
    string LoginBonusName,
    string LoginBonusDate,
    int ClaimCount);

public sealed record AdminAccountDetailResult(
    long Id,
    string Name,
    string LastLoginAt,
    IReadOnlyList<AdminAccountItemEntry> Items,
    IReadOnlyList<AdminAccountLoginHistoryEntry> LoginHistory);
