namespace Server.Data.Entities;

public sealed class LoginBonus
{
    public long Id { get; set; }
    public string Name { get; set; } = "";
    public string StartDate { get; set; } = "";
    public string EndDate { get; set; } = "";
    public string Type { get; set; } = "";
    public List<LoginBonusDay> Days { get; set; } = new();
}
