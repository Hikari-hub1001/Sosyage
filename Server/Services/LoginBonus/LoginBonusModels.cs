namespace Server.Services.LoginBonus;

public sealed record LoginBonusRegistration
{
    public long Id { get; init; }
    public string Name { get; init; } = "";
    public string Type { get; init; } = "";
    public string StartDate { get; init; } = "";
    public string EndDate { get; init; } = "";
    public List<LoginBonusDay> Days { get; init; } = new();
}

public sealed record LoginBonusDay
{
    public string Date { get; init; } = "";
    public List<LoginBonusReward> Rewards { get; init; } = new();
}

public sealed record LoginBonusReward
{
    public long ItemId { get; init; }
    public int Quantity { get; init; }
}

public sealed record LoginBonusClaimRequest
{
    public long Id { get; init; }
}

public sealed record LoginBonusClaimResponse
{
    public LoginBonusPeriod Period { get; init; } = new();
    public int CurrentDay { get; init; }
    public bool IsClaimedThisRequest { get; init; }
    public List<LoginBonusDailyBonus> DailyBonuses { get; init; } = new();
}

public sealed record LoginBonusPeriod
{
    public string Start { get; init; } = "";
    public string End { get; init; } = "";
}

public sealed record LoginBonusDailyBonus
{
    public List<LoginBonusItemBonus> Bonuses { get; init; } = new();
}

public sealed record LoginBonusItemBonus
{
    public long Id { get; init; }
    public int Quantity { get; init; }
}

public sealed record LoginBonusSummary
{
    public long Id { get; init; }
    public string Name { get; init; } = "";
}
