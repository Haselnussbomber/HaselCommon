using System.Collections.Generic;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;

namespace HaselCommon.Utils;

public class ColumnSelect<T, TItem> : Column<TItem> where T : struct, Enum, IEquatable<T>
{
    public ColumnSelect(T initialValue)
    {
        FilterValue = initialValue;
    }

    protected virtual IReadOnlyList<T> Values
        => Enum.GetValues<T>();

    protected virtual string[] Names
        => Enum.GetNames<T>();

    protected virtual void SetValue(T value)
        => FilterValue = value;

    public    T   FilterValue;
    protected int idx = -1;

    public override bool DrawFilter()
    {
        using var id    = ImRaii.PushId(FilterLabel);
        using var style = ImRaii.PushStyle(ImGuiStyleVar.FrameRounding, 0);
        ImGui.SetNextItemWidth(-Table.ArrowWidth * ImGuiHelpers.GlobalScale);
        using var combo = ImRaii.Combo(string.Empty, idx < 0 ? t(LabelKey) : Names[idx]);
        if (!combo)
            return false;

        var       ret = false;
        for (var i = 0; i < Names.Length; ++i)
        {
            if (FilterValue.Equals(Values[i]))
                idx = i;
            if (!ImGui.Selectable(Names[i], idx == i) || idx == i)
                continue;

            idx = i;
            SetValue(Values[i]);
            ret = true;
        }

        return ret;
    }
}
