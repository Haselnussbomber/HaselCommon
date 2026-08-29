using FFXIVClientStructs.FFXIV.Component.GUI;

namespace HaselCommon.Services;

[RegisterSingleton, AutoConstruct]
public unsafe partial class AddonObserver : IDisposable
{
    public delegate void AddonShowDelegate(AtkUnitBase* addon);
    public delegate void AddonHideDelegate(AtkUnitBase* addon);

    private readonly ILogger<AddonObserver> _logger;
    private readonly IGameInteropProvider _gameInteropProvider;

    private readonly HashSet<Pointer<AtkUnitBase>> _visibleUnits = new(256);

    private Hook<UpdateAppliedVisibilityStateDelegate>? _hook;

    public event AddonShowDelegate? Show;
    public event AddonHideDelegate? Hide;

    [return: MarshalAs(UnmanagedType.U1)]
    public delegate bool UpdateAppliedVisibilityStateDelegate(AtkUnitBase* thisPtr);

    [AutoPostConstruct]
    private void Initialize()
    {
        _hook = _gameInteropProvider.EnabledHookFromSignature<UpdateAppliedVisibilityStateDelegate>(
            "E8 ?? ?? ?? ?? 84 C0 0F 84 ?? ?? ?? ?? 44 0F B6 97",
            UpdateAppliedVisibilityStateDetour);
    }

    public void Dispose()
    {
        DisposeAndNull(ref _hook);
    }

    private bool UpdateAppliedVisibilityStateDetour(AtkUnitBase* thisPtr)
    {
        var ret = _hook!.OriginalDisposeSafe(thisPtr);

        if (thisPtr->VisibilityState == thisPtr->AppliedVisibilityState)
        {
            if (thisPtr->AppliedVisibilityState.HasFlag(AtkUnitBaseVisibilityState.Show) && (thisPtr->VisibilityFlags & 1) == 0 && _visibleUnits.Add(thisPtr))
            {
                _logger.LogTrace("Show: {name}", thisPtr->NameString);

                foreach (var action in Delegate.EnumerateInvocationList(Show))
                {
                    try
                    {
                        action(thisPtr);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Exception during raise of {handler}", action.Method);
                    }
                }
            }
            else if ((thisPtr->AppliedVisibilityState.HasFlag(AtkUnitBaseVisibilityState.Hide) || (thisPtr->VisibilityFlags & 1) == 1) && _visibleUnits.Remove(thisPtr))
            {
                _logger.LogTrace("Hide: {name}", thisPtr->NameString);

                foreach (var action in Delegate.EnumerateInvocationList(Hide))
                {
                    try
                    {
                        action(thisPtr);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Exception during raise of {handler}", action.Method);
                    }
                }
            }
        }

        return ret;
    }
}
