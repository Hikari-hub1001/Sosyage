namespace Server.Data.Entities;

public sealed class Account
{
    public long Id { get; set; }
    public string Name { get; set; } = "";
    public string LastLoginAt { get; set; } = "";
    public List<AccountLoginBonus> LoginBonuses { get; set; } = new();
    public List<AccountLoginBonusLog> LoginBonusLogs { get; set; } = new();
}
