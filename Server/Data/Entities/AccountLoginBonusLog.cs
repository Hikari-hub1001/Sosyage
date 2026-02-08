namespace Server.Data.Entities;

public sealed class AccountLoginBonusLog
{
    public long Id { get; set; }
    public long AccountId { get; set; }
    public long LoginBonusId { get; set; }
    public long LoginBonusDayId { get; set; }
    public int ClaimCount { get; set; }
    public string ClaimedAt { get; set; } = "";
    public Account? Account { get; set; }
    public LoginBonus? LoginBonus { get; set; }
    public LoginBonusDay? LoginBonusDay { get; set; }
}
