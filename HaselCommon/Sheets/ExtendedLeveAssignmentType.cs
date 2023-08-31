using Dalamud.Utility;
using Lumina.Excel.GeneratedSheets;

namespace HaselCommon.Sheets;

public class ExtendedLeveAssignmentType : LeveAssignmentType
{
    private string? _name { get; set; } = null;

    public string StringName
        => _name ??= Name.ToDalamudString().ToString() ?? "";
}
