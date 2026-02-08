namespace Server.Data.Entities;

public sealed class Reward
{
    public long Id { get; set; }
    public string Type { get; set; } = "";
    public long? ItemId { get; set; }
    public int Quantity { get; set; }
    public ItemMaster? Item { get; set; }
}
