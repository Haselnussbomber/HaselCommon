using Dalamud.Plugin.Services;
using HaselCommon.Services;

namespace HaselCommon.Events;

[RegisterSingleton<IEventProvider>(Duplicate = DuplicateStrategy.Append), AutoConstruct]
internal sealed unsafe partial class GameEventsProvider : IEventProvider
{
    private readonly EventDispatcher _eventDispatcher;
    private readonly IClientState _clientState;

    [AutoPostConstruct]
    private void Initialize()
    {
        _clientState.TerritoryChanged += OnTerritoryChanged;
    }

    public void Dispose()
    {
        _clientState.TerritoryChanged -= OnTerritoryChanged;
    }

    private void OnTerritoryChanged(ushort territoryTypeId)
    {
        _eventDispatcher.Trigger(GameEvents.TerritoryChanged, new GameEvents.TerritoryChangedArgs
        {
            TerritoryTypeId = territoryTypeId,
        });
    }
}
