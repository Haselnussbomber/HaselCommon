using HaselCommon.Services;
using ImGuiNET;

namespace HaselCommon.Gui.ImGuiTable;

public class SortableTable<T> : Table<T>, IDisposable
{
    private readonly LanguageProvider _languageProvider;

    public bool IsSortDirty { get; set; } = true;

    public SortableTable(string id, LanguageProvider languageProvider) : base(id)
    {
        _languageProvider = languageProvider;
        
        Flags |= ImGuiTableFlags.Sortable;

        _languageProvider.LanguageChanged += OnLanguageChanged;
    }

    public virtual void Dispose()
    {
        _languageProvider.LanguageChanged -= OnLanguageChanged;
        GC.SuppressFinalize(this);
    }

    private void OnLanguageChanged(string langCode)
    {
        IsSortDirty = true;
    }

    protected override void PreDrawRows()
    {
        if (!Flags.HasFlag(ImGuiTableFlags.Sortable))
            return;

        var sortSpecs = ImGui.TableGetSortSpecs();
        IsSortDirty |= sortSpecs.SpecsDirty;

        if (!IsSortDirty)
            return;

        if (sortSpecs.SpecsCount == 0)
        {
            if (Flags.HasFlag(ImGuiTableFlags.SortTristate))
                SortTristate();
        }
        else
        {
            var column = Columns[sortSpecs.Specs.ColumnIndex];
            Rows.Sort((a, b) => (sortSpecs.Specs.SortDirection == ImGuiSortDirection.Descending ? -1 : 1) * column.Compare(a, b));
            FilteredRows = null;
        }

        sortSpecs.SpecsDirty = IsSortDirty = false;
    }

    protected virtual void SortTristate() { }
}
