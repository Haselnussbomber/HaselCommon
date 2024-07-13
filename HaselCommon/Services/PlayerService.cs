using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI;
using Microsoft.Extensions.Logging;

namespace HaselCommon.Services;

public unsafe class PlayerService : IDisposable
{
    private readonly ILogger<PlayerService> Logger;
    private readonly Hook<UIModule.Delegates.HandlePacket>? UIModuleHandlePacketHook;

    public delegate void OnClassJobChangeDelegate(uint classJobId);
    public delegate void OnLevelChangeDelegate(uint classJobId, uint level);

    public event OnClassJobChangeDelegate? ClassJobChange;
    public event OnLevelChangeDelegate? LevelChange;
    public event Action? LoggingOut;

    public PlayerService(ILogger<PlayerService> logger, IGameInteropProvider gameInteropProvider)
    {
        Logger = logger;

        UIModuleHandlePacketHook = gameInteropProvider.HookFromAddress<UIModule.Delegates.HandlePacket>(
            UIModule.StaticVirtualTablePointer->HandlePacket,
            UIModuleHandlePacketDetour);

        UIModuleHandlePacketHook?.Enable();
    }

    public void Dispose()
    {
        UIModuleHandlePacketHook?.Dispose();
        GC.SuppressFinalize(this);
    }

    private void UIModuleHandlePacketDetour(UIModule* uiModule, UIModulePacketType type, uint uintParam, void* packet)
    {
        try
        {
            switch (type)
            {
                case UIModulePacketType.ClassJobChange:
                    ClassJobChange?.Invoke((byte)uintParam);
                    break;

                case UIModulePacketType.LevelChange:
                    LevelChange?.Invoke(*(uint*)packet, *(ushort*)((nint)packet + 4));
                    break;

                case UIModulePacketType.Logout:
                    LoggingOut?.Invoke();
                    break;

                default:
                    Logger.LogDebug("UIModulePacketType {type}", type);
                    break;
            }
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error in event handler for UIModulePacketType {type}", type);
        }
        finally
        {
            UIModuleHandlePacketHook!.Original(uiModule, type, uintParam, packet);
        }
    }
}
