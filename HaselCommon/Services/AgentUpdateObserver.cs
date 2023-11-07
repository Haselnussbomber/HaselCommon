using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace HaselCommon.Services;

public unsafe class AgentUpdateObserver : IDisposable
{
    private delegate void RaptureAtkModule_UpdateDelegate(RaptureAtkModule* module, float a2);
    private Hook<RaptureAtkModule_UpdateDelegate>? _updateHook;

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
        var updateAddress = vtableAddress + 0x8 * 58;
        _updateHook = Service.GameInteropProvider.HookFromFunctionPointerVariable<RaptureAtkModule_UpdateDelegate>(updateAddress, RaptureAtkModule_UpdateDetour);
        _updateHook?.Enable();
    }

    private void RaptureAtkModule_UpdateDetour(RaptureAtkModule* module, float delta)
    {
        try
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
        }
        catch (Exception ex)
        {
            Service.PluginLog.Error(ex, "Error during RaptureAtkModule_UpdateDetour");
        }

        _updateHook!.OriginalDisposeSafe(module, delta);
    }

    public void Dispose()
    {
        _updateHook?.Dispose();
        _updateHook = null;
    }
}
