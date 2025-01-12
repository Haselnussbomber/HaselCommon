using System.Linq;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI;
using Microsoft.Extensions.Logging;

namespace HaselCommon.Services;

[RegisterSingleton]
public unsafe class UnlocksObserver : IDisposable
{
    private readonly ILogger<UnlocksObserver> _logger;
    private readonly Hook<RaptureAtkModule.Delegates.Update> _updateHook;

    public event Action? Update;

    public UnlocksObserver(ILogger<UnlocksObserver> logger, IGameInteropProvider gameInteropProvider)
    {
        _logger = logger;

        _updateHook = gameInteropProvider.HookFromAddress<RaptureAtkModule.Delegates.Update>(
            RaptureAtkModule.StaticVirtualTablePointer->Update,
            RaptureAtkModuleUpdateDetour);

        _updateHook.Enable();
    }

    public void Dispose()
    {
        _updateHook.Dispose();
    }

    private void RaptureAtkModuleUpdateDetour(RaptureAtkModule* module, float delta)
    {
        if (Update != null && module->AgentUpdateFlag.HasFlag(RaptureAtkModule.AgentUpdateFlags.UnlocksUpdate))
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

        _updateHook.OriginalDisposeSafe(module, delta);
    }
}
