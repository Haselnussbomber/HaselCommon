using Dalamud.Plugin.Services;
using HaselCommon.Services.Events;
using Lumina.Excel.GeneratedSheets;

namespace HaselCommon.Events;

internal class ClientStateEventEmitter : IDisposable
{
    private readonly IEventEmitter _eventEmitter;
    private readonly IClientState _clientState;

    public ClientStateEventEmitter(IEventEmitter eventEmitter, IClientState clientState)
    {
        _eventEmitter = eventEmitter;
        _clientState = clientState;

        _clientState.CfPop += OnCfPop;
        _clientState.ClassJobChanged += OnClassJobChanged;
        _clientState.EnterPvP += OnEnterPvP;
        _clientState.LeavePvP += OnLeavePvP;
        _clientState.LevelChanged += OnLevelChanged;
        _clientState.Login += OnLogin;
        _clientState.Logout += OnLogout;
        _clientState.TerritoryChanged += OnTerritoryChanged;
    }

    public void Dispose()
    {
        _clientState.CfPop -= OnCfPop;
        _clientState.ClassJobChanged -= OnClassJobChanged;
        _clientState.EnterPvP -= OnEnterPvP;
        _clientState.LeavePvP -= OnLeavePvP;
        _clientState.LevelChanged -= OnLevelChanged;
        _clientState.Login -= OnLogin;
        _clientState.Logout -= OnLogout;
        _clientState.TerritoryChanged -= OnTerritoryChanged;
        GC.SuppressFinalize(this);
    }

    private void OnCfPop(ContentFinderCondition cfc)
    {
        _eventEmitter.TriggerEvent(ClientStateEvents.CfPop, new ClientStateEvents.CfPopEventArgs
        {
            ContentFinderCondition = cfc
        });
    }

    private void OnClassJobChanged(uint classJobId)
    {
        _eventEmitter.TriggerEvent(ClientStateEvents.ClassJobChange, new ClientStateEvents.ClassJobChangeEventArgs
        {
            ClassJobId = classJobId
        });
    }

    private void OnEnterPvP()
    {
        _eventEmitter.TriggerEvent(ClientStateEvents.EnterPvP);
    }

    private void OnLeavePvP()
    {
        _eventEmitter.TriggerEvent(ClientStateEvents.LeavePvP);
    }

    private void OnLevelChanged(uint classJobId, uint level)
    {
        _eventEmitter.TriggerEvent(ClientStateEvents.LevelChange, new ClientStateEvents.LevelChangeEventArgs
        {
            ClassJobId = classJobId,
            Level = level
        });
    }

    private void OnLogin()
    {
        _eventEmitter.TriggerEvent(ClientStateEvents.Login);
    }

    private void OnLogout()
    {
        _eventEmitter.TriggerEvent(ClientStateEvents.Logout);
    }

    private void OnTerritoryChanged(ushort territoryTypeId)
    {
        _eventEmitter.TriggerEvent(ClientStateEvents.TerritoryChange, new ClientStateEvents.TerritoryChangeEventArgs
        {
            TerritoryTypeId =
            territoryTypeId
        });
    }
}
