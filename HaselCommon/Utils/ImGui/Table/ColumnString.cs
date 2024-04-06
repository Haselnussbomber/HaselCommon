using System.Text.RegularExpressions;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;

namespace HaselCommon.Utils;

public class ColumnString<TItem> : Column<TItem>
{
    public ColumnString()
    {
        Flags &= ~ImGuiTableColumnFlags.NoResize;
    }

    public string FilterValue = string.Empty;
    protected Regex? filterRegex;

    public virtual string ToName(TItem item)
        => item!.ToString() ?? string.Empty;

    public override int Compare(TItem lhs, TItem rhs)
        => string.Compare(ToName(lhs), ToName(rhs), StringComparison.InvariantCulture);

    public override bool DrawFilter()
    {
        using var style = ImRaii.PushStyle(ImGuiStyleVar.FrameRounding, 0);

        ImGui.SetNextItemWidth(-Table.ArrowWidth * ImGuiHelpers.GlobalScale);
        var tmp = FilterValue;
        if (!ImGui.InputTextWithHint(FilterLabel, t(LabelKey), ref tmp, 256) || tmp == FilterValue)
            return false;

        FilterValue = tmp;
        try
        {
            filterRegex = new Regex(FilterValue, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        }
        catch
        {
            filterRegex = null;
        }

        return true;
    }

    public override bool FilterFunc(TItem item)
    {
        var name = ToName(item);
        return FilterValue.Length == 0 || (filterRegex?.IsMatch(name) ?? name.Contains(FilterValue, StringComparison.OrdinalIgnoreCase));
    }

    public override void DrawColumn(TItem item, int idx)
    {
        ImGui.TextUnformatted(ToName(item));
    }
}
