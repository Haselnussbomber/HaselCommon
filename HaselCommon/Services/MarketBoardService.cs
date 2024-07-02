using System.Collections.Generic;
using Dalamud.Game.Network.Structures;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI.Info;

namespace HaselCommon.Services;

public unsafe class MarketBoardService : IDisposable
{
    private readonly IMarketBoard MarketBoard;
    private readonly Hook<InfoProxyItemSearch.Delegates.ProcessRequestResult>? ProcessRequestResultHook;
    private readonly Hook<InfoProxyItemSearch.Delegates.EndRequest>? EndRequestHook;
    private readonly List<IMarketBoardItemListing> Listings = [];

    public delegate void ListingsStartDelegate();
    public event ListingsStartDelegate? ListingsStart;

    public delegate void ListingsPageDelegate(IReadOnlyList<IMarketBoardItemListing> listings);
    public event ListingsPageDelegate? ListingsPage;

    public delegate void ListingsEndDelegate(IReadOnlyList<IMarketBoardItemListing> listings);
    public event ListingsEndDelegate? ListingsEnd;

    public MarketBoardService(IGameInteropProvider gameInteropProvider, IMarketBoard marketBoard)
    {
        MarketBoard = marketBoard;

        ProcessRequestResultHook = gameInteropProvider.HookFromAddress<InfoProxyItemSearch.Delegates.ProcessRequestResult>(
            InfoProxyItemSearch.MemberFunctionPointers.ProcessRequestResult,
            ProcessRequestResultDetour);

        EndRequestHook = gameInteropProvider.HookFromAddress<InfoProxyItemSearch.Delegates.EndRequest>(
            InfoProxyItemSearch.StaticVirtualTablePointer->EndRequest,
            EndRequestDetour);

        ProcessRequestResultHook?.Enable();
        EndRequestHook?.Enable();

        MarketBoard.OfferingsReceived += OnOfferingsReceived;
    }

    public void Dispose()
    {
        MarketBoard.OfferingsReceived -= OnOfferingsReceived;
        ProcessRequestResultHook?.Dispose();
        EndRequestHook?.Dispose();

        GC.SuppressFinalize(this);
    }

    private nint ProcessRequestResultDetour(InfoProxyItemSearch* infoProxy, nint a2, nint a3, nint a4, int a5, byte a6, int a7)
    {
        Listings.Clear();
        ListingsStart?.Invoke();
        return ProcessRequestResultHook!.Original(infoProxy, a2, a3, a4, a5, a6, a7);
    }

    private void EndRequestDetour(InfoProxyItemSearch* infoProxy)
    {
        EndRequestHook!.Original(infoProxy);
        ListingsEnd?.Invoke(Listings);
        Listings.Clear();
    }

    private void OnOfferingsReceived(IMarketBoardCurrentOfferings currentOfferings)
    {
        Listings.AddRange(currentOfferings.ItemListings);
        ListingsPage?.Invoke(currentOfferings.ItemListings);
    }
}
