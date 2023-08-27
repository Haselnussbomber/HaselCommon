using Dalamud.Utility;

namespace HaselCommon.Sheets;

public class LeveAssignmentType : Lumina.Excel.GeneratedSheets.LeveAssignmentType
{
    private string? _name { get; set; } = null;

    public string StringName
        => _name ??= Name.ToDalamudString().ToString() ?? "";
}
