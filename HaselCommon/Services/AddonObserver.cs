using System.Collections.Generic;
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

    public AddonObserver()
    {
        Service.Framework.Update += OnFrameworkUpdate;
    }

    public void Dispose()
    {
        Service.Framework.Update -= OnFrameworkUpdate;
        GC.SuppressFinalize(this);
    }

    private void OnFrameworkUpdate(IFramework framework)
    {
        Update();
    }

    private HashSet<nint> LoadedUnits { get; } = [];
    private Dictionary<nint, string> NameCache { get; } = [];

    private void Update()
    {
        var raptureAtkModule = RaptureAtkModule.Instance();
        if (raptureAtkModule == null)
            return;

        var allLoadedList = raptureAtkModule->RaptureAtkUnitManager.AtkUnitManager.AllLoadedUnitsList;

        static bool IsAddonReady(nint address)
            => (*(byte*)(address + 0x189) & 0x1) == 0x1; // see AtkModule.IsAddonReady()

        static bool IsAddonVisible(nint address)
            => (*(ushort*)(address + 0x182) & 0x20) == 0x20; // see AtkUnitBase.IsVisible

        foreach (var loadedUnitAddress in LoadedUnits)
        {
            var isLoaded = false;

            foreach (AtkUnitBase* unitBase in allLoadedList.EntriesSpan)
            {
                var address = (nint)unitBase;
                if (address == loadedUnitAddress && IsAddonReady(address) && IsAddonVisible(address))
                {
                    isLoaded = true;
                    break;
                }
            }

            if (!isLoaded && NameCache.TryGetValue(loadedUnitAddress, out var name))
            {
                NameCache.Remove(loadedUnitAddress);
                LoadedUnits.Remove(loadedUnitAddress);
                AddonClose?.Invoke(name);
            }
        }

        foreach (AtkUnitBase* unitBase in allLoadedList.EntriesSpan)
        {
            var address = (nint)unitBase;
            if (address == 0 || !IsAddonReady(address) || !IsAddonVisible(address) || LoadedUnits.Contains(address))
                continue;

            var name = MemoryHelper.ReadString((nint)unitBase->Name, 32);

            NameCache.Add(address, name);
            LoadedUnits.Add(address);
            AddonOpen?.Invoke(name);
        }
    }
}
