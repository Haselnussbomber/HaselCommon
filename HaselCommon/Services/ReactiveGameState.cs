using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using R3;

namespace HaselCommon.Services;

[RegisterSingleton]
public unsafe class ReactiveGameState : IDisposable
{
    private readonly IDisposable _disposable;

    public ReadOnlyReactiveProperty<bool> IsLoggedIn { get; }
    public ReadOnlyReactiveProperty<uint> TerritoryTypeId { get; }
    public ReadOnlyReactiveProperty<ushort> ContentFinderConditionId { get; }
    public ReadOnlyReactiveProperty<uint> MapId { get; }
    public ReadOnlyReactiveProperty<uint> Instance { get; }
    public ReadOnlyReactiveProperty<bool> InSanctuary { get; }
    public ReadOnlyReactiveProperty<GameObjectId> Target { get; }

    public ReactiveGameState()
    {
        var eachTick = Observable.EveryUpdate().Share();

        _disposable = Disposable.Combine(
            IsLoggedIn = eachTick.TrackProperty(GetIsLoggedIn),
            TerritoryTypeId = eachTick.TrackProperty(GetTerritoryTypeId),
            ContentFinderConditionId = eachTick.TrackProperty(GetContentFinderConditionId),
            MapId = eachTick.TrackProperty(GetMapId),
            Instance = eachTick.TrackProperty(GetInstance),
            InSanctuary = eachTick.TrackProperty(GetInSanctuary),
            Target = eachTick.TrackProperty(GetTarget));
    }

    public void Dispose()
    {
        _disposable.Dispose();
    }

    private bool GetIsLoggedIn()
    {
        var agentLobby = AgentLobby.Instance();
        return agentLobby != null && agentLobby->IsLoggedIn;
    }

    private uint GetTerritoryTypeId()
    {
        var gameMain = GameMain.Instance();
        return gameMain != null ? gameMain->CurrentTerritoryTypeId : default;
    }

    private ushort GetContentFinderConditionId()
    {
        var gameMain = GameMain.Instance();
        return gameMain != null ? gameMain->CurrentContentFinderConditionId : default;
    }

    private uint GetMapId()
    {
        var agentMap = AgentMap.Instance();
        return agentMap != null ? agentMap->CurrentMapId : default;
    }

    private uint GetInstance()
    {
        var uiState = UIState.Instance();
        return uiState != null ? uiState->PublicInstance.InstanceId : default;
    }

    private bool GetInSanctuary()
    {
        var territoryInfo = TerritoryInfo.Instance();
        return territoryInfo != null && territoryInfo->InSanctuary;
    }

    private GameObjectId GetTarget()
    {
        var targetSystem = TargetSystem.Instance();
        return targetSystem != null ? targetSystem->TargetObjectId : default;
    }
}
