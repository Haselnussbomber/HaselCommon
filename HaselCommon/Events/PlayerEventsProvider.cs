using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using HaselCommon.Services;

namespace HaselCommon.Events;

[RegisterSingleton<IEventProvider>(Duplicate = DuplicateStrategy.Append), AutoConstruct]
internal sealed unsafe partial class PlayerEventsProvider : IEventProvider
{
    private readonly EventDispatcher _eventDispatcher;
    private readonly IClientState _clientState;
    private readonly IGameInteropProvider _gameInteropProvider;

    private delegate void RaptureHotbarModulePerformMateriaActionMigrationDelegate(RaptureHotbarModule* thisPtr);
    private Hook<RaptureHotbarModulePerformMateriaActionMigrationDelegate> _unlockHook;

    [AutoPostConstruct]
    private void Initialize()
    {
        _clientState.Login += OnLogin;
        _clientState.Logout += OnLogout;
        _clientState.ClassJobChanged += OnClassJobChanged;
        _clientState.LevelChanged += OnlevelChanged;

        // This function is called inside RaptureAtkModule.Update when RaptureAtkModule.AgentUpdateFlag has flag UnlocksUpdate.
        _unlockHook = _gameInteropProvider.HookFromSignature<RaptureHotbarModulePerformMateriaActionMigrationDelegate>(
            "E8 ?? ?? ?? ?? 48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 8B 8B ?? ?? ?? ?? 48 8B 01",
            RaptureHotbarModulePerformMateriaActionMigrationDetour);

        _unlockHook?.Enable();
    }

    public void Dispose()
    {
        _clientState.Login -= OnLogin;
        _clientState.Logout -= OnLogout;
        _clientState.ClassJobChanged -= OnClassJobChanged;
        _clientState.LevelChanged -= OnlevelChanged;

        _unlockHook?.Dispose();
    }

    private void OnLogin()
    {
        _eventDispatcher.Trigger(PlayerEvents.Login, new PlayerEvents.LoginArgs
        {
            ContentId = PlayerState.Instance()->ContentId,
        });
    }

    private void OnLogout(int type, int code)
    {
        _eventDispatcher.Trigger(PlayerEvents.Logout, new PlayerEvents.LogoutArgs
        {
            Type = type,
            Code = code,
        });
    }

    private void OnClassJobChanged(uint classJobId)
    {
        _eventDispatcher.Trigger(PlayerEvents.ClassJobChanged, new PlayerEvents.ClassJobChangedArgs
        {
            ClassJobId = classJobId,
        });
    }

    private void OnlevelChanged(uint classJobId, uint level)
    {
        _eventDispatcher.Trigger(PlayerEvents.LevelChanged, new PlayerEvents.LevelChangedArgs
        {
            ClassJobId = classJobId,
            Level = level,
        });
    }

    private void RaptureHotbarModulePerformMateriaActionMigrationDetour(RaptureHotbarModule* thisPtr)
    {
        _unlockHook!.OriginalDisposeSafe(thisPtr);

        _eventDispatcher.Trigger(PlayerEvents.UnlocksChanged);
    }
}
