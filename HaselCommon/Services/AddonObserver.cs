using System.Collections.Generic;
using System.Linq;
using Dalamud.Memory;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using NoAlloq;

namespace HaselCommon.Services;

public unsafe class AddonObserver : IDisposable
{
    public delegate void CallbackDelegate(string addonName);
    public event CallbackDelegate? AddonOpen;
    public event CallbackDelegate? AddonClose;

    private readonly HashSet<nint> LoadedUnits = new(256);
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

        var visibleUnits = RaptureAtkModule.Instance()->RaptureAtkUnitManager.AtkUnitManager.AllLoadedUnitsList.EntriesSpan
            .Select(entry => (nint)entry.Value)
            .Where(address => address != 0 && (*(byte*)(address + 0x189) & 0x1) == 0x1 && (*(ushort*)(address + 0x182) & 0x20) == 0x20);

        foreach (var address in LoadedUnits.Except(visibleUnits.Where(LoadedUnits.Contains).ToList()))
        {
            LoadedUnits.Remove(address);

            if (!NameCache.TryGetValue(address, out var name))
                continue;

            NameCache.Remove(address);
            AddonClose?.Invoke(name);
        }

        foreach (var address in visibleUnits.Where(address => !LoadedUnits.Contains(address)))
        {
            var name = MemoryHelper.ReadString((nint)((AtkUnitBase*)address)->Name, 32);

            NameCache.Add(address, name);
            LoadedUnits.Add(address);
            AddonOpen?.Invoke(name);
        }
    }
}
