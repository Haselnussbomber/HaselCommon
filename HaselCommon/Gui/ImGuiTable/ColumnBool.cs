using System.Linq;
using Dalamud.Interface.Utility.Raii;
using HaselCommon.Graphics;
using ImGuiNET;

namespace HaselCommon.Gui.ImGuiTable;

[Flags]
public enum BoolValues
{
    False = 1,
    True = 2,
}

public class ColumnBool<TRow> : ColumnFlags<BoolValues, TRow>
{
    private BoolValues _filterValue;
    public override BoolValues FilterValue => _filterValue;

    public ColumnBool()
    {
        AllFlags = Enum.GetValues<BoolValues>().Aggregate((a, b) => a | b);
        _filterValue = AllFlags;
    }

    public virtual bool ToBool(TRow row)
        => true;

    public override bool ShouldShow(TRow row)
    {
        var value = ToBool(row);
        return (FilterValue.HasFlag(BoolValues.True) && value) ||
               (FilterValue.HasFlag(BoolValues.False) && !value);
    }

    public override unsafe void DrawColumn(TRow row)
    {
        var value = ToBool(row);
        using (ImRaii.PushColor(ImGuiCol.Text, (uint)(value ? Color.Green : Color.Red)))
            ImGui.TextUnformatted(Names[value ? 1 : 0]);
    }

    public override unsafe int Compare(TRow a, TRow b)
        => ToBool(a).CompareTo(ToBool(b));

    public override void SetValue(BoolValues value, bool enable)
    {
        if (enable)
            _filterValue |= value;
        else
            _filterValue &= ~value;
    }
}
