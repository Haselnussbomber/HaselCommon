namespace HaselCommon.Utils;

public record ItemAmount(ItemHandle item, uint amount = 0)
{
    public ItemHandle Item { get; } = item;
    public uint Amount { get; set; } = amount;
}
