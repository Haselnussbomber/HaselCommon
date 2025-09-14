using FFXIVClientStructs.FFXIV.Client.Game.UI;
using R3;

namespace HaselCommon.Services;

[RegisterSingleton]
public unsafe class ReactivePlayerState : IDisposable
{
    private readonly IDisposable _disposable;

    public ReadOnlyReactiveProperty<string> CharacterName { get; }
    public ReadOnlyReactiveProperty<uint> EntityId { get; }
    public ReadOnlyReactiveProperty<ulong> ContentId { get; }
    public ReadOnlyReactiveProperty<byte> ClassJobId { get; }
    public ReadOnlyReactiveProperty<short> CurrentLevel { get; }
    public ReadOnlyReactiveProperty<short> SyncedLevel { get; }
    public ReadOnlyReactiveProperty<bool> IsLevelSynced { get; }
    public ReadOnlyReactiveProperty<short> EffectiveLevel { get; }
    public ReadOnlyReactiveProperty<byte> GrandCompany { get; }

    public ReactivePlayerState()
    {
        var eachTick = Observable.EveryUpdate().Share();

        _disposable = Disposable.Combine(
            CharacterName = eachTick.TrackProperty(GetCharacterName),
            EntityId = eachTick.TrackProperty(GetEntityId),
            ContentId = eachTick.TrackProperty(GetContentId),
            ClassJobId = eachTick.TrackProperty(GetClassJobId),
            CurrentLevel = eachTick.TrackProperty(GetCurrentLevel),
            SyncedLevel = eachTick.TrackProperty(GetSyncedLevel),
            IsLevelSynced = eachTick.TrackProperty(GetIsLevelSynced),
            EffectiveLevel = eachTick.TrackProperty(GetEffectiveLevel),
            GrandCompany = eachTick.TrackProperty(GetGrandCompany));
    }

    public void Dispose()
    {
        _disposable.Dispose();
    }

    private string GetCharacterName()
    {
        var playerState = PlayerState.Instance();
        return playerState != null ? playerState->CharacterNameString : string.Empty;
    }

    private uint GetEntityId()
    {
        var playerState = PlayerState.Instance();
        return playerState != null ? playerState->EntityId : default;
    }

    private ulong GetContentId()
    {
        var playerState = PlayerState.Instance();
        return playerState != null ? playerState->ContentId : default;
    }

    private byte GetClassJobId()
    {
        var playerState = PlayerState.Instance();
        return playerState != null ? playerState->CurrentClassJobId : default;
    }

    private short GetCurrentLevel()
    {
        var playerState = PlayerState.Instance();
        return playerState != null ? playerState->CurrentLevel : default;
    }

    private short GetSyncedLevel()
    {
        var playerState = PlayerState.Instance();
        return playerState != null ? playerState->SyncedLevel : default;
    }

    private bool GetIsLevelSynced()
    {
        var playerState = PlayerState.Instance();
        return playerState != null && playerState->IsLevelSynced;
    }

    private short GetEffectiveLevel()
    {
        var playerState = PlayerState.Instance();
        return playerState != null ? (playerState->IsLevelSynced ? playerState->SyncedLevel : playerState->CurrentLevel) : default;
    }

    private byte GetGrandCompany()
    {
        var playerState = PlayerState.Instance();
        return playerState != null ? playerState->GrandCompany : default;
    }
}
