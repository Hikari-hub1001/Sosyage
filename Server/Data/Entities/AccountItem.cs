namespace Server.Data.Entities;

public sealed class AccountItem
{
    public long AccountId { get; set; }
    public long ItemId { get; set; }
    public int Quantity { get; set; }

    public Account Account { get; set; } = null!;
    public ItemMaster Item { get; set; } = null!;
}
