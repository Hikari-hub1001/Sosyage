namespace Server.Data.Entities;

public sealed class AccountLoginBonus
{
    public long Id { get; set; }
    public long AccountId { get; set; }
    public long LoginBonusId { get; set; }
    public int ClaimCount { get; set; }
    public int? LastClaimedDay { get; set; }
    public Account? Account { get; set; }
    public LoginBonus? LoginBonus { get; set; }
}
