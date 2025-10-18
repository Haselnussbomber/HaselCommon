namespace HaselCommon.Gui.ImGuiTable;

public class ColumnYesNo<TRow> : ColumnBool<TRow>
{
    public override string NameKeySpace => "ImGuiTable.ColumnYesNo";
    public override string[] Names => ["No", "Yes"];
}
