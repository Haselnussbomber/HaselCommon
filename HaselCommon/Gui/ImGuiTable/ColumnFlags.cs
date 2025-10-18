namespace HaselCommon.Gui.ImGuiTable;

public class ColumnFlags<T, TItem> : Column<TItem> where T : struct, Enum
{
    public T AllFlags = default;

    public virtual IReadOnlyList<T> Values
        => Enum.GetValues<T>();

    public virtual string NameKeySpace => "";

    public virtual string[] Names
        => Enum.GetNames<T>();

    public virtual string GetTranslatedName(int index)
        => !string.IsNullOrEmpty(NameKeySpace) && ServiceLocator.GetService<TextService>()?.TryGetTranslation($"{NameKeySpace}.{Names[index]}", out var text) == true ? text : Names[index];

    public virtual T FilterValue
        => default;

    public virtual void SetValue(T value, bool enable)
        => throw new NotImplementedException();

    public override bool DrawFilter()
    {
        using var id = ImRaii.PushId("##Filter");
        using var style = ImRaii.PushStyle(ImGuiStyleVar.FrameRounding, 0);
        ImGui.SetNextItemWidth(-Table.ArrowWidth * ImGuiHelpers.GlobalScale);
        var all = FilterValue.HasFlag(AllFlags);
        using var color = ImRaii.PushColor(ImGuiCol.FrameBg, 0x803030A0, !all);
        using var combo = ImRaii.Combo(string.Empty, Label, ImGuiComboFlags.NoArrowButton);

        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
        {
            SetValue(AllFlags, true);
            return true;
        }

        var textService = ServiceLocator.GetService<TextService>();

        if (!all && ImGui.IsItemHovered())
            ImGui.SetTooltip(textService?.Translate("ImGuiTable.ColumnFlags.Filter.RightClickToClear") ?? "Right-click to clear filters.");

        if (!combo)
            return false;

        color.Pop();

        var ret = false;
        if (ImGui.Checkbox(textService?.Translate("ImGuiTable.ColumnFlags.Filter.EnableAll") ?? "Enable All", ref all))
        {
            SetValue(AllFlags, all);
            ret = true;
        }

        using var indent = ImRaii.PushIndent(10f);
        for (var i = 0; i < Names.Length; ++i)
        {
            var tmp = FilterValue.HasFlag(Values[i]);
            if (!ImGui.Checkbox(GetTranslatedName(i), ref tmp))
                continue;

            SetValue(Values[i], tmp);
            ret = true;
        }

        return ret;
    }
}
