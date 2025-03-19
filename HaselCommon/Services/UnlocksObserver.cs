using System.Linq;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Microsoft.Extensions.Logging;

namespace HaselCommon.Services;

[RegisterSingleton, AutoConstruct]
public unsafe partial class UnlocksObserver : IDisposable
{
    private readonly ILogger<UnlocksObserver> _logger;

    private delegate void RaptureHotbarModulePerformMateriaActionMigrationDelegate(RaptureHotbarModule* thisPtr);
    private Hook<RaptureHotbarModulePerformMateriaActionMigrationDelegate> _hook;

    public event Action? Update;

    [AutoPostConstruct]
    private void Initialize(IGameInteropProvider gameInteropProvider)
    {
        // This might look weird at first, but this function is called inside RaptureAtkModule.Update
        // when RaptureAtkModule.AgentUpdateFlag has flag UnlocksUpdate, which is exactly what I need.
        // Previously, this class hooked RaptureAtkModule.Update directly, but this caused a deadlock
        // when enabling the hook for some reason. See #dalamud-dev:
        // https://discord.com/channels/581875019861328007/860813266468732938/1322031324390756373

        _hook = gameInteropProvider.HookFromSignature<RaptureHotbarModulePerformMateriaActionMigrationDelegate>(
            "E8 ?? ?? ?? ?? 48 8D 0D ?? ?? ?? ?? E8 ?? ?? ?? ?? 48 8B 8B ?? ?? ?? ?? 48 8B 01",
            RaptureHotbarModulePerformMateriaActionMigrationDetour);

        _hook.Enable();
    }

    public void Dispose()
    {
        _hook.Dispose();
    }

    private void RaptureHotbarModulePerformMateriaActionMigrationDetour(RaptureHotbarModule* thisPtr)
    {
        if (Update != null)
        {
            foreach (var action in Update.GetInvocationList().Cast<Action>())
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception during raise of {handler}", action.Method);
                }
            }
        }

        _hook.OriginalDisposeSafe(thisPtr);
    }
}
