using System.Collections.Generic;
using Dalamud.Memory;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using static HaselCommon.Utils.Globals.UnsafeGlobals;

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

    private HashSet<nint> _addedUnits { get; } = new();
    private HashSet<nint> _removedUnits { get; } = new();
    private HashSet<nint> _loadedUnits { get; } = new();
    private Dictionary<nint, string> _nameCache { get; } = new();

    private void Update()
    {
        _addedUnits.Clear();
        _removedUnits.Clear();

        var raptureAtkModule = RaptureAtkModule.Instance();
        if (raptureAtkModule == null)
            return;

        // check added units
        var allLoadedList = &raptureAtkModule->RaptureAtkUnitManager.AtkUnitManager.AllLoadedUnitsList;
        for (var i = 0; i < allLoadedList->Count; i++)
        {
            var address = *(nint*)AsPointer(ref allLoadedList->EntriesSpan[i]);
            if (address == 0 || _loadedUnits.Contains(address) || !raptureAtkModule->AtkModule.IsAddonReady(((AtkUnitBase*)address)->ID))
                continue;

            _addedUnits.Add(address);
        }

        foreach (var address in _addedUnits)
        {
            var unitBase = (AtkUnitBase*)address;
            var name = MemoryHelper.ReadStringNullTerminated((nint)unitBase->Name);

            if (!_nameCache.ContainsKey(address))
                _nameCache.Add(address, name);

            AddonOpen?.Invoke(name);
        }

        // check removed units
        foreach (var loadedUnitAddress in _loadedUnits)
        {
            var isLoaded = false;

            for (var i = 0; i < allLoadedList->Count; i++)
            {
                var address = *(nint*)AsPointer(ref allLoadedList->EntriesSpan[i]);
                if (address == loadedUnitAddress)
                {
                    isLoaded = true;
                    break;
                }
            }

            if (!isLoaded)
                _removedUnits.Add(loadedUnitAddress);
        }

        foreach (var address in _removedUnits)
        {
            if (_nameCache.TryGetValue(address, out var name))
            {
                _nameCache.Remove(address);

                AddonClose?.Invoke(name);
            }
        }

        // update LoadedUnits
        foreach (var address in _addedUnits)
        {
            _loadedUnits.Add(address);
        }

        foreach (var address in _removedUnits)
        {
            _loadedUnits.Remove(address);
        }
    }
}
