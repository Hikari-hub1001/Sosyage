namespace Server.Data.Entities;

public sealed class LoginBonusDay
{
    public long Id { get; set; }
    public long LoginBonusId { get; set; }
    public string Date { get; set; } = "";
    public LoginBonus? LoginBonus { get; set; }
    public List<LoginBonusDayReward> Rewards { get; set; } = new();
}
