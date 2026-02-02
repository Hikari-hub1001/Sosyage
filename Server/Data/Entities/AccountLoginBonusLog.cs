namespace Server.Data.Entities;

public sealed class AccountLoginBonusLog
{
    public long Id { get; set; }
    public long AccountId { get; set; }
    public long MonthId { get; set; }
    public int DayNumber { get; set; }
    public string ClaimedAt { get; set; } = "";
    public Account? Account { get; set; }
    public LoginBonusMonth? Month { get; set; }
}
