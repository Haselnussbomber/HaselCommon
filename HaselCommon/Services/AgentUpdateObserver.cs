using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace HaselCommon.Services;

public unsafe class AgentUpdateObserver : IDisposable
{
    private delegate void RaptureAtkModule_OnUpdateDelegate(RaptureAtkModule* module);
    private Hook<RaptureAtkModule_OnUpdateDelegate>? _onUpdateHook;

    public event Action? OnInventoryUpdate;
    public event Action? OnActionBarUpdate;
    public event Action? OnRetainerUpdate;
    public event Action? OnNameplateUpdate;
    public event Action? OnUnlocksUpdate;
    public event Action? OnMainCommandEnabledStateUpdate;
    public event Action? OnHousingInventoryUpdate;

    public AgentUpdateObserver()
    {
        var vtablePtr = Service.SigScanner.ScanText("33 C9 48 8D 05 ?? ?? ?? ?? 48 89 8F") + 5;
        var vtableAddress = vtablePtr + *(int*)vtablePtr + 4;
        var onUpdateAddress = vtableAddress + 0x8 * 58;
        _onUpdateHook = Service.GameInteropProvider.HookFromFunctionPointerVariable<RaptureAtkModule_OnUpdateDelegate>(onUpdateAddress, RaptureAtkModule_OnUpdateDetour);
        _onUpdateHook?.Enable();
    }

    private void RaptureAtkModule_OnUpdateDetour(RaptureAtkModule* module)
    {
        if (module->AgentUpdateFlag.HasFlag(RaptureAtkModule.AgentUpdateFlags.InventoryUpdate))
            OnInventoryUpdate?.Invoke();

        if (module->AgentUpdateFlag.HasFlag(RaptureAtkModule.AgentUpdateFlags.ActionBarUpdate))
            OnActionBarUpdate?.Invoke();

        if (module->AgentUpdateFlag.HasFlag(RaptureAtkModule.AgentUpdateFlags.RetainerUpdate))
            OnRetainerUpdate?.Invoke();

        if (module->AgentUpdateFlag.HasFlag(RaptureAtkModule.AgentUpdateFlags.NameplateUpdate))
            OnNameplateUpdate?.Invoke();

        if (module->AgentUpdateFlag.HasFlag(RaptureAtkModule.AgentUpdateFlags.UnlocksUpdate))
            OnUnlocksUpdate?.Invoke();

        if (module->AgentUpdateFlag.HasFlag(RaptureAtkModule.AgentUpdateFlags.MainCommandEnabledStateUpdate))
            OnMainCommandEnabledStateUpdate?.Invoke();

        if (module->AgentUpdateFlag.HasFlag(RaptureAtkModule.AgentUpdateFlags.HousingInventoryUpdate))
            OnHousingInventoryUpdate?.Invoke();

        _onUpdateHook!.OriginalDisposeSafe(module);
    }

    public void Dispose()
    {
        _onUpdateHook?.Dispose();
        _onUpdateHook = null;
    }
}
