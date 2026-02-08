namespace Server.Data.Entities;

public sealed class LoginBonusDayReward
{
    public long Id { get; set; }
    public long LoginBonusDayId { get; set; }
    public long RewardId { get; set; }
    public LoginBonusDay? LoginBonusDay { get; set; }
    public Reward? Reward { get; set; }
}
