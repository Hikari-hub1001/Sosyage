namespace Server.Data.Entities;

public sealed class LoginBonusMonth
{
    public long Id { get; set; }
    public string Month { get; set; } = "";
    public string StartDate { get; set; } = "";
    public string EndDate { get; set; } = "";
    public List<LoginBonusDay> Days { get; set; } = new();
}
