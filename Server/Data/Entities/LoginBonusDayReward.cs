namespace Server.Data.Entities;

public sealed class LoginBonusDayReward
{
    public long Id { get; set; }
    public long DayId { get; set; }
    public int RewardId { get; set; }
    public int Quantity { get; set; }
    public LoginBonusDay? Day { get; set; }
}
