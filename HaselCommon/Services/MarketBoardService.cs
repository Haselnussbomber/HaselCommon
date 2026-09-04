using System.Threading.Tasks;
using Dalamud.Game.Network.Structures;
using FFXIVClientStructs.FFXIV.Client.UI.Info;

namespace HaselCommon.Services;

[RegisterSingleton, AutoConstruct]
public unsafe partial class MarketBoardService : IAsyncDisposable
{
    private readonly IMarketBoard _marketBoard;
    private readonly IFramework _framework;

    private readonly List<IMarketBoardItemListing> _listings = [];

    private Hook<InfoProxyItemSearch.Delegates.ProcessRequestResult>? _processRequestResultHook;
    private Hook<InfoProxyItemSearch.Delegates.EndRequest>? _endRequestHook;

    public delegate void ListingsStartDelegate();
    public event ListingsStartDelegate? ListingsStart;

    public delegate void ListingsPageDelegate(IReadOnlyList<IMarketBoardItemListing> listings);
    public event ListingsPageDelegate? ListingsPage;

    public delegate void ListingsEndDelegate(IReadOnlyList<IMarketBoardItemListing> listings);
    public event ListingsEndDelegate? ListingsEnd;

    [AutoPostConstruct]
    private void Initialize(IGameInteropProvider gameInteropProvider)
    {
        _processRequestResultHook = gameInteropProvider.EnabledHookFromAddress<InfoProxyItemSearch.Delegates.ProcessRequestResult>(
            InfoProxyItemSearch.MemberFunctionPointers.ProcessRequestResult,
            ProcessRequestResultDetour);

        _endRequestHook = gameInteropProvider.EnabledHookFromAddress<InfoProxyItemSearch.Delegates.EndRequest>(
            InfoProxyItemSearch.StaticVirtualTablePointer->EndRequest,
            EndRequestDetour);

        _marketBoard.OfferingsReceived += OnOfferingsReceived;
    }

    public ValueTask DisposeAsync()
    {
        _marketBoard.OfferingsReceived -= OnOfferingsReceived;

        return new ValueTask(_framework.Run(() =>
        {
            DisposeAndNull(ref _processRequestResultHook);
            DisposeAndNull(ref _endRequestHook);
        }));
    }

    private void ProcessRequestResultDetour(InfoProxyItemSearch* infoProxy, byte a2, int a3)
    {
        _listings.Clear();
        ListingsStart?.Invoke();
        _processRequestResultHook!.Original(infoProxy, a2, a3);
    }

    private void EndRequestDetour(InfoProxyItemSearch* infoProxy)
    {
        _endRequestHook!.Original(infoProxy);
        ListingsEnd?.Invoke(_listings);
        _listings.Clear();
    }

    private void OnOfferingsReceived(IMarketBoardCurrentOfferings currentOfferings)
    {
        _listings.AddRange(currentOfferings.ItemListings);
        ListingsPage?.Invoke(currentOfferings.ItemListings);
    }
}
