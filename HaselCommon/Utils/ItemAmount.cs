using Lumina.Excel.Sheets;

namespace HaselCommon.Utils;

public record struct ItemAmount
{
    public ItemAmount(Item item, uint amount)
    {
        Item = item;
        Amount = amount;
    }

    public Item Item { get; }
    public uint Amount { get; set; }
}
