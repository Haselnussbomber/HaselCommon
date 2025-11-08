using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace HaselCommon.Globals;

public static unsafe class Addon
{
    public static T* GetAddon<T>(ReadOnlySpan<byte> name, int index = 1) where T : unmanaged
    {
        var raptureAtkModule = RaptureAtkModule.Instance();
        var addon = raptureAtkModule->RaptureAtkUnitManager.GetAddonByName(name, index);
        var ready = addon != null && addon->IsReady;
        return ready ? (T*)addon : null;
    }

    public static T* GetAddon<T>(string name, int index = 1) where T : unmanaged
    {
        var raptureAtkModule = RaptureAtkModule.Instance();
        var addon = raptureAtkModule->RaptureAtkUnitManager.GetAddonByName(name, index);
        var ready = addon != null && addon->IsReady;
        return ready ? (T*)addon : null;
    }

    public static T* GetAddon<T>(ushort addonId) where T : unmanaged
    {
        var raptureAtkModule = RaptureAtkModule.Instance();
        var addon = raptureAtkModule->RaptureAtkUnitManager.GetAddonById(addonId);
        var ready = addon != null && addon->IsReady;
        return ready ? (T*)addon : null;
    }

    public static T* GetAddon<T>(AgentId agentId) where T : unmanaged
    {
        var agent = AgentModule.Instance()->GetAgentByInternalId(agentId);
        var active = agent != null && agent->IsAgentActive();
        return active ? GetAddon<T>((ushort)agent->GetAddonId()) : null;
    }

    // ---

    public static bool TryGetAddon<T>(ReadOnlySpan<byte> name, int index, out T* addon) where T : unmanaged
        => (addon = GetAddon<T>(name, index)) != null;

    public static bool TryGetAddon<T>(string name, int index, out T* addon) where T : unmanaged
        => (addon = GetAddon<T>(name, index)) != null;

    public static bool TryGetAddon<T>(ReadOnlySpan<byte> name, out T* addon) where T : unmanaged
        => (addon = GetAddon<T>(name, 1)) != null;

    public static bool TryGetAddon<T>(string name, out T* addon) where T : unmanaged
        => (addon = GetAddon<T>(name, 1)) != null;

    public static bool TryGetAddon<T>(ushort addonId, out T* addon) where T : unmanaged
        => (addon = GetAddon<T>(addonId)) != null;

    public static bool TryGetAddon<T>(AgentId agentId, out T* addon) where T : unmanaged
        => (addon = GetAddon<T>(agentId)) != null;

    // ---

    public static bool IsAddonOpen(ReadOnlySpan<byte> name)
        => TryGetAddon<AtkUnitBase>(name, out var addon) && addon->IsVisible;

    public static bool IsAddonOpen(string name)
        => TryGetAddon<AtkUnitBase>(name, out var addon) && addon->IsVisible;

    public static bool IsAddonOpen(ReadOnlySpan<byte> name, int index)
        => TryGetAddon<AtkUnitBase>(name, index, out var addon) && addon->IsVisible;

    public static bool IsAddonOpen(string name, int index)
        => TryGetAddon<AtkUnitBase>(name, index, out var addon) && addon->IsVisible;

    public static bool IsAddonOpen(ushort addonId)
        => TryGetAddon<AtkUnitBase>(addonId, out var addon) && addon->IsVisible;

    public static bool IsAddonOpen(AgentId agentId)
        => TryGetAddon<AtkUnitBase>(agentId, out var addon) && addon->IsVisible;
}
