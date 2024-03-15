using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Dalamud.Memory;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace HaselCommon.Services;

public unsafe class AddonObserver : IDisposable
{
    public delegate void CallbackDelegate(string addonName);
    public event CallbackDelegate? AddonOpen;
    public event CallbackDelegate? AddonClose;

    private readonly HashSet<nint> VisibleUnits = new(256);
    private readonly HashSet<nint> RemovedUnits = new(16);
    private readonly Dictionary<nint, string> NameCache = new(256);

    public AddonObserver()
    {
        Service.Framework.Update += OnFrameworkUpdate;
    }

    public void Dispose()
    {
        Service.Framework.Update -= OnFrameworkUpdate;
    }

    private void OnFrameworkUpdate(IFramework framework)
    {
        /*
        static bool IsAddonReady(nint address)
            => (*(byte*)(address + 0x189) & 0x1) == 0x1; // see AtkModule.IsAddonReady()

        static bool IsAddonVisible(nint address)
            => (*(ushort*)(address + 0x182) & 0x20) == 0x20; // see AtkUnitBase.IsVisible
        */

        VisibleUnits.Clear();

        foreach (ref var address in new Span<nint>(Unsafe.AsPointer(ref RaptureAtkModule.Instance()->RaptureAtkUnitManager.AtkUnitManager.AllLoadedUnitsList.Entries[0]), 256))
        {
            if (address == 0)
                break;
            if ((*(byte*)(address + 0x189) & 0x1) == 0x1 && (*(ushort*)(address + 0x182) & 0x20) == 0x20)
                VisibleUnits.Add(address);
        }

        RemovedUnits.Clear();

        foreach (var (address, name) in NameCache)
        {
            if (!VisibleUnits.Contains(address) && RemovedUnits.Add(address))
            {
                NameCache.Remove(address);
                AddonClose?.Invoke(name);
            }
        }

        foreach (var address in VisibleUnits)
        {
            if (NameCache.ContainsKey(address))
                continue;

            var name = MemoryHelper.ReadString((nint)((AtkUnitBase*)address)->Name, 32);

            NameCache.Add(address, name);
            AddonOpen?.Invoke(name);
        }
    }
}
