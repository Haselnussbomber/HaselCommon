using System.Collections.Generic;
using System.Linq;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace HaselCommon.Services;

[RegisterSingleton]
public class PrismBoxProvider : IDisposable
{
    private readonly IFramework _framework;
    private readonly IClientState _clientState;
    private readonly List<uint> _itemIds = [];
    private DateTime _lastCheck = DateTime.MinValue;

    public ReadOnlySpan<uint> ItemIds => CollectionsMarshal.AsSpan(_itemIds);

    public PrismBoxProvider(IFramework framework, IClientState clientState)
    {
        _framework = framework;
        _clientState = clientState;

        _framework.Update += OnFrameworkUpdate;
        _clientState.Logout += OnLogout;
    }

    public void Dispose()
    {
        _framework.Update -= OnFrameworkUpdate;
        _clientState.Logout -= OnLogout;
        GC.SuppressFinalize(this);
    }

    private unsafe void OnFrameworkUpdate(IFramework framework)
    {
        if (!_clientState.IsLoggedIn)
            return;

        var mirageManager = MirageManager.Instance();
        if (!mirageManager->PrismBoxLoaded)
            return;

        var hasChanges = false;

        if (DateTime.Now - _lastCheck > TimeSpan.FromSeconds(2))
        {
            hasChanges = !ItemIds.SequenceEqual(mirageManager->PrismBoxItemIds);
            _lastCheck = DateTime.Now;
        }

        if (hasChanges)
        {
            _itemIds.Clear();
            _itemIds.AddRange(mirageManager->PrismBoxItemIds);
        }
    }

    private void OnLogout(int type, int code)
    {
        _itemIds.Clear();
        _lastCheck = DateTime.MinValue;
    }
}
