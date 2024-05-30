using System.Collections.Generic;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace HaselCommon.Services;

public unsafe class AddonObserver : IDisposable
{
    public delegate void CallbackDelegate(string addonName);
    public event CallbackDelegate? AddonOpen;
    public event CallbackDelegate? AddonClose;

    private readonly HashSet<Pointer<AtkUnitBase>> VisibleUnits = new(256);
    private readonly HashSet<Pointer<AtkUnitBase>> RemovedUnits = new(16);
    private readonly Dictionary<Pointer<AtkUnitBase>, string> NameCache = new(256);

    public AddonObserver()
    {
        Service.Framework.Update += OnFrameworkUpdate;
    }

    public void Dispose()
    {
        Service.Framework.Update -= OnFrameworkUpdate;
    }

    public bool IsAddonVisible(string name)
        => NameCache.ContainsValue(name);

    private void OnFrameworkUpdate(IFramework framework)
    {
        foreach (var atkUnitBase in RaptureAtkModule.Instance()->RaptureAtkUnitManager.AtkUnitManager.AllLoadedUnitsList.Entries)
        {
            if (atkUnitBase.Value != null && atkUnitBase.Value->IsReady && atkUnitBase.Value->IsVisible)
                VisibleUnits.Add(atkUnitBase);
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

            var name = address.Value->NameString;
            NameCache.Add(address, name);
            AddonOpen?.Invoke(name);
        }
    }
}
