using Dalamud.Game.Gui.Dtr;
using Dalamud.Game.Text.SeStringHandling;

namespace HaselCommon.Extensions;

public static partial class IDtrBarExtensions
{
    extension(IDtrBar dtrBar)
    {
        public DtrBarEntry GetDisposable(string title)
        {
            return new DtrBarEntry(dtrBar.Get(title));
        }
    }
}

public class DtrBarEntry : IDtrBarEntry, IDisposable
{
    public IDtrBarEntry Entry { get; }

    public string Title => Entry.Title;
    public bool HasClickAction => Entry.HasClickAction;
    public bool UserHidden => Entry.UserHidden;
    public (Vector2 Min, Vector2 Max) ScreenBounds => Entry.ScreenBounds;

    public SeString? Text
    {
        get => Entry.Text;
        set => Entry.Text = value;
    }

    public SeString? Tooltip
    {
        get => Entry.Tooltip;
        set => Entry.Tooltip = value;
    }

    public bool Shown
    {
        get => Entry.Shown;
        set => Entry.Shown = value;
    }

    public ushort MinimumWidth
    {
        get => Entry.MinimumWidth;
        set => Entry.MinimumWidth = value;
    }

    public Action<DtrInteractionEvent>? OnClick
    {
        get => Entry.OnClick;
        set => Entry.OnClick = value;
    }

    public DtrBarEntry(IDtrBarEntry dtrBarEntry)
    {
        Entry = dtrBarEntry;

        if (ServiceLocator.TryGetService<IDalamudPluginInterface>(out var pluginInterface))
            Entry.Tooltip = pluginInterface.Manifest.Name;

        Entry.Shown = false;
    }

    public void Dispose()
    {
        Entry.Remove();
    }

    public void Remove()
    {
        Entry.Remove();
    }
}
