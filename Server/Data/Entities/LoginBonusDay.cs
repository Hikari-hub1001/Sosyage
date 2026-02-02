namespace Server.Data.Entities;

public sealed class LoginBonusDay
{
    public long Id { get; set; }
    public long MonthId { get; set; }
    public int DayNumber { get; set; }
    public LoginBonusMonth? Month { get; set; }
    public List<LoginBonusDayReward> Rewards { get; set; } = new();
}
