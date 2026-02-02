namespace Server.Services.LoginBonus;

public sealed record LoginBonusRegistration
{
    public string Month { get; init; } = "";
    public string StartDate { get; init; } = "";
    public string EndDate { get; init; } = "";
    public List<LoginBonusDay> Days { get; init; } = new();
}

public sealed record LoginBonusDay
{
    public int DayNumber { get; init; }
    public List<LoginBonusReward> Rewards { get; init; } = new();
}

public sealed record LoginBonusReward
{
    public int RewardId { get; init; }
    public int Quantity { get; init; }
}

public sealed record LoginBonusClaimRequest
{
    public long AccountId { get; init; }
}

public sealed record LoginBonusClaimResponse
{
    public LoginBonusPeriod Period { get; init; } = new();
    public int CurrentDay { get; init; }
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
    public int Id { get; init; }
    public int Quantity { get; init; }
}
