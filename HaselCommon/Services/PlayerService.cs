using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI;
using Microsoft.Extensions.Logging;

namespace HaselCommon.Services;

public unsafe class PlayerService : IDisposable
{
    private readonly ILogger<PlayerService> _logger;
    private readonly Hook<UIModule.Delegates.HandlePacket>? _uIModuleHandlePacketHook;

    public delegate void OnClassJobChangeDelegate(uint classJobId);
    public delegate void OnLevelChangeDelegate(uint classJobId, uint level);

    public event OnClassJobChangeDelegate? ClassJobChange;
    public event OnLevelChangeDelegate? LevelChange;
    public event Action? LoggingOut;

    public PlayerService(ILogger<PlayerService> logger, IGameInteropProvider gameInteropProvider)
    {
        _logger = logger;

        _uIModuleHandlePacketHook = gameInteropProvider.HookFromAddress<UIModule.Delegates.HandlePacket>(
            UIModule.StaticVirtualTablePointer->HandlePacket,
            UIModuleHandlePacketDetour);

        _uIModuleHandlePacketHook?.Enable();
    }

    public void Dispose()
    {
        _uIModuleHandlePacketHook?.Dispose();
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
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in event handler for UIModulePacketType {type}", type);
        }
        finally
        {
            _uIModuleHandlePacketHook!.Original(uiModule, type, uintParam, packet);
        }
    }
}
