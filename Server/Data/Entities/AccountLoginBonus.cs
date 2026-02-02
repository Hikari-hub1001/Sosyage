namespace Server.Data.Entities;

public sealed class AccountLoginBonus
{
    public long Id { get; set; }
    public long AccountId { get; set; }
    public long MonthId { get; set; }
    public int CurrentDay { get; set; }
    public int? LastClaimedDay { get; set; }
    public Account? Account { get; set; }
    public LoginBonusMonth? Month { get; set; }
}
