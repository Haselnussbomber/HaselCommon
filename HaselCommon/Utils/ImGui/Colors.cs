using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI;
using Lumina.Excel.GeneratedSheets;

namespace HaselCommon.Utils;

public static class Colors
{
    public static HaselColor Transparent { get; } = new(Vector4.Zero);
    public static HaselColor White { get; } = new(Vector4.One);
    public static HaselColor Black { get; } = new(0, 0, 0);
    public static HaselColor Orange { get; } = new(1, 0.6f, 0);
    public static HaselColor Gold { get; } = new(0.847f, 0.733f, 0.49f);
    public static HaselColor Green { get; } = new(0, 1, 0);
    public static HaselColor Yellow { get; } = new(1, 1, 0);
    public static HaselColor Red { get; } = new(1, 0, 0);
    public static HaselColor Grey { get; } = new(0.73f, 0.73f, 0.73f);
    public static HaselColor Grey2 { get; } = new(0.87f, 0.87f, 0.87f);
    public static HaselColor Grey3 { get; } = new(0.6f, 0.6f, 0.6f);
    public static HaselColor Grey4 { get; } = new(0.3f, 0.3f, 0.3f);

    public static unsafe bool IsLightTheme
        => RaptureAtkModule.Instance()->AtkUIColorHolder.ActiveColorThemeType == 1;

    private static readonly Lazy<Dictionary<byte, HaselColor>> ItemRarityColors = new(()
        => GetSheet<Item>()
            .Where(item => !string.IsNullOrEmpty(item.Name.ToDalamudString().ToString()))
            .Select(item => item.Rarity)
            .Distinct()
            .Select(rarity => (Rarity: rarity, Color: HaselColor.FromUiForeground(547u + rarity * 2u)))
            .ToDictionary(tuple => tuple.Rarity, tuple => tuple.Color));

    public static HaselColor GetItemRarityColor(byte rarity)
        => ItemRarityColors.Value[rarity];

    public static unsafe HaselColor GetItemLevelColor(byte classJob, Item item, params Vector4[] colors)
    {
        if (colors.Length < 2)
            throw new ArgumentException("At least two colors are required for interpolation.");

        var expArrayIndex = GetRow<ClassJob>(classJob)?.ExpArrayIndex;
        if (expArrayIndex is null or -1)
            return White;

        var level = PlayerState.Instance()->ClassJobLevels[(short)expArrayIndex];
        if (level < 1 || !ItemUtils.MaxLevelRanges.TryGetValue(level, out var range))
            return White;

        var itemLevel = item.LevelItem.Row;

        // special case for Fisher's Secondary Tool
        // which has only one item, Spearfishing Gig
        if (item.ItemUICategory.Row == 99)
            return itemLevel == 180 ? Green : Red;

        if (itemLevel < range.Min)
            return Red;

        var value = (itemLevel - range.Min) / (float)(range.Max - range.Min);

        var startIndex = (int)(value * (colors.Length - 1));
        var endIndex = Math.Min(startIndex + 1, colors.Length - 1);

        if (startIndex < 0 || startIndex >= colors.Length || endIndex < 0 || endIndex >= colors.Length)
            return White;

        var t = value * (colors.Length - 1) - startIndex;
        return new(Vector4.Lerp(colors[startIndex], colors[endIndex], t));
    }
}
