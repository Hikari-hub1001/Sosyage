namespace Server.Services.AdminAccount;

public sealed record AdminAccountItem(long Id, string Name, string LastLoginAt);

public sealed record AdminAccountListResult(
    int Offset,
    int Limit,
    long Total,
    IReadOnlyList<AdminAccountItem> Accounts);
