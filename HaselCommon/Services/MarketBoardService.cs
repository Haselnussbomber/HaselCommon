using System.Collections.Generic;
using Dalamud.Game.Network.Structures;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI.Info;

namespace HaselCommon.Services;

public unsafe class MarketBoardService : IDisposable
{
    private readonly IMarketBoard _marketBoard;
    private readonly Hook<InfoProxyItemSearch.Delegates.ProcessRequestResult> _processRequestResultHook;
    private readonly Hook<InfoProxyItemSearch.Delegates.EndRequest> _endRequestHook;
    private readonly List<IMarketBoardItemListing> _listings = [];

    public delegate void ListingsStartDelegate();
    public event ListingsStartDelegate? ListingsStart;

    public delegate void ListingsPageDelegate(IReadOnlyList<IMarketBoardItemListing> listings);
    public event ListingsPageDelegate? ListingsPage;

    public delegate void ListingsEndDelegate(IReadOnlyList<IMarketBoardItemListing> listings);
    public event ListingsEndDelegate? ListingsEnd;

    public MarketBoardService(IGameInteropProvider gameInteropProvider, IMarketBoard marketBoard)
    {
        _marketBoard = marketBoard;

        _processRequestResultHook = gameInteropProvider.HookFromAddress<InfoProxyItemSearch.Delegates.ProcessRequestResult>(
            InfoProxyItemSearch.MemberFunctionPointers.ProcessRequestResult,
            ProcessRequestResultDetour);

        _endRequestHook = gameInteropProvider.HookFromAddress<InfoProxyItemSearch.Delegates.EndRequest>(
            InfoProxyItemSearch.StaticVirtualTablePointer->EndRequest,
            EndRequestDetour);

        _processRequestResultHook.Enable();
        _endRequestHook.Enable();

        _marketBoard.OfferingsReceived += OnOfferingsReceived;
    }

    public void Dispose()
    {
        _marketBoard.OfferingsReceived -= OnOfferingsReceived;
        _processRequestResultHook.Dispose();
        _endRequestHook.Dispose();

        GC.SuppressFinalize(this);
    }

    private nint ProcessRequestResultDetour(InfoProxyItemSearch* infoProxy, nint a2, nint a3, nint a4, int a5, byte a6, int a7)
    {
        _listings.Clear();
        ListingsStart?.Invoke();
        return _processRequestResultHook.Original(infoProxy, a2, a3, a4, a5, a6, a7);
    }

    private void EndRequestDetour(InfoProxyItemSearch* infoProxy)
    {
        _endRequestHook.Original(infoProxy);
        ListingsEnd?.Invoke(_listings);
        _listings.Clear();
    }

    private void OnOfferingsReceived(IMarketBoardCurrentOfferings currentOfferings)
    {
        _listings.AddRange(currentOfferings.ItemListings);
        ListingsPage?.Invoke(currentOfferings.ItemListings);
    }
}
