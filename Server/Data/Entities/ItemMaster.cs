namespace Server.Data.Entities;

public sealed class ItemMaster
{
    public long Id { get; set; }
    public string Name { get; set; } = "";
    public string ItemType { get; set; } = "";
}
