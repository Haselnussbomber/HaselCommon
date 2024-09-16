using System.Collections.Generic;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.STD;
using HaselCommon.Services.Events;

namespace HaselCommon.Events;

internal class PlayerStateEventEmitter : IDisposable
{
    private readonly IEventEmitter _eventEmitter;
    private readonly IFramework _framework;

    // TODO: add FixedSizeArrays

    // fields
    private bool _isLoaded;
    private string _characterName = string.Empty;
    private uint _entityId;
    private ulong _contentId;
    private int _dutyPenalty;
    private byte _maxLevel;
    private byte _maxExpansion;
    private byte _sex;
    private byte _race;
    private byte _tribe;
    private byte _currentClassJobId;
    // private nint _currentClassJobRow;
    private short _currentLevel;
    private short[] _classJobLevels = [];
    private int[] _classJobExperience = [];
    private short _syncedLevel;
    private byte _isLevelSynced;
    private bool _hasPremiumSaddlebag;
    private byte _guardianDeity;
    private byte _birthMonth;
    private byte _birthDay;
    private byte _firstClass;
    private byte _startTown;
    private ushort[] _activeFestivalIds = [];
    private ushort[] _activeFestivalPhases = [];
    private int _baseStrength;
    private int _baseDexterity;
    private int _baseVitality;
    private int _baseIntelligence;
    private int _baseMind;
    private int _basePiety;
    private int[] _attributes = [];
    private byte _grandCompany;
    private byte _gcRankMaelstrom;
    private byte _gcRankTwinAdders;
    private byte _gcRankImmortalFlames;
    private ushort _homeAetheryteId;
    private byte _favouriteAetheryteCount;
    private ushort[] _favouriteAetherytes = [];
    private ushort _freeAetheryteId;
    private ushort _freeAetherytePlayStationPlus;
    private uint _baseRestedExperience;
    private byte[] _unlockedMountsBitmask = [];
    private byte[] _unlockedOrnamentsBitmask = [];
    private byte[] _unlockedGlassesStylesBitmask = [];
    private ushort _numOwnedMounts;
    private byte[] _caughtFishBitmask = [];
    private uint _numFishCaught;
    private uint _fishingBait;
    private byte[] _unlockedSpearfishingNotebookBitmask = [];
    private byte[] _caughtSpearfishBitmask = [];
    private uint _numSpearfishCaught;
    private byte[] _contentRouletteCompletion = [];
    private short _playerCommendations;
    private byte[] _selectedPoses = [];
    private byte _sightseeingLogUnlockState;
    private byte _sightseeingLogUnlockStateEx;
    private byte[] _unlockedAdventureBitmask = [];
    private byte[] _completedAdventureBitmask = [];
    private byte[] _unlockFlags = [];
    private byte _deliveryLevel;
    // private byte _unkWeddingPlanFlag;
    private byte _meisterFlag;
    private int _squadronMissionCompletionTimestamp;
    private int _squadronTrainingCompletionTimestamp;
    private ushort _activeGcArmyExpedition;
    private ushort _activeGcArmyTraining;
    private bool _hasNewGcArmyCandidate;
    private byte[] _unlockedMinerFolkloreTomeBitmask = [];
    private byte[] _unlockedBotanistFolkloreTomeBitmask = [];
    private byte[] _unlockedFishingFolkloreTomeBitmask = [];
    // private bool _unkGcPvpMountActionCheck;
    private byte[] _weeklyBingoOrderData = [];
    private byte[] _weeklyBingoRewardData = [];
    private byte _weeklyBingoExpMultiplier;
    // private bool _weeklyBingoUnk63;
    private StdPair<uint, uint>[] _contentKeyValueData = [];
    private byte _mentorVersion;
    private uint[] _desynthesisLevels = [];

    // props
    private bool _isLegacy; // QuestSpecialFlags & 1
    private bool _isWarriorOfLight; // QuestSpecialFlags & 2
    private DateTime _weeklyBingoExpireDateTime = DateTime.MinValue;
    private uint _weeklyBingoNumSecondChancePoints;
    private bool _hasWeeklyBingoJournal;
    private int _weeklyBingoNumPlacedStickers;

    // funcs
    private bool _isMentor;
    private bool _isBattleMentor;
    private bool _isTradeMentor;
    private bool _isNovice;
    private bool _isReturner;
    private bool _isLoginSecurityToken;
    private bool _isBuddyInStable;
    private bool _isNoviceNetworkAutoJoinEnabled;
    private bool _isWeeklyBingoExpired;

    public PlayerStateEventEmitter(IEventEmitter eventEmitter, IFramework framework)
    {
        _eventEmitter = eventEmitter;
        _framework = framework;
        _framework.Update += Update;
    }

    public void Dispose()
    {
        _framework.Update -= Update;
        GC.SuppressFinalize(this);
    }

    private unsafe void Update(IFramework framework)
    {
        var playerState = PlayerState.Instance();

        // fields
        Update(ref _isLoaded, playerState->IsLoaded == 1, PlayerStateEvents.IsLoadedChanged);
        Update(ref _characterName, playerState->CharacterNameString, PlayerStateEvents.CharacterNameChanged);
        Update(ref _entityId, playerState->EntityId, PlayerStateEvents.EntityIdChanged);
        Update(ref _contentId, playerState->ContentId, PlayerStateEvents.ContentIdChanged);
        Update(ref _dutyPenalty, playerState->PenaltyTimestamps[0], PlayerStateEvents.DutyPenaltyChanged);
        Update(ref _maxLevel, playerState->MaxLevel, PlayerStateEvents.MaxLevelChanged);
        Update(ref _maxExpansion, playerState->MaxExpansion, PlayerStateEvents.MaxExpansionChanged);
        Update(ref _sex, playerState->Sex, PlayerStateEvents.SexChanged);
        Update(ref _race, playerState->Race, PlayerStateEvents.RaceChanged);
        Update(ref _tribe, playerState->Tribe, PlayerStateEvents.TribeChanged);
        Update(ref _currentClassJobId, playerState->CurrentClassJobId, PlayerStateEvents.CurrentClassJobIdChanged);
        Update(ref _currentLevel, playerState->CurrentLevel, PlayerStateEvents.CurrentLevelChanged);
        Update(ref _classJobLevels, playerState->ClassJobLevels, PlayerStateEvents.ClassJobLevelsChanged);
        Update(ref _classJobExperience, playerState->ClassJobExperience, PlayerStateEvents.ClassJobExperienceChanged);
        Update(ref _syncedLevel, playerState->SyncedLevel, PlayerStateEvents.SyncedLevelChanged);
        Update(ref _isLevelSynced, playerState->IsLevelSynced, PlayerStateEvents.IsLevelSyncedChanged);
        Update(ref _hasPremiumSaddlebag, playerState->HasPremiumSaddlebag, PlayerStateEvents.HasPremiumSaddlebagChanged);
        Update(ref _guardianDeity, playerState->GuardianDeity, PlayerStateEvents.GuardianDeityChanged);
        Update(ref _birthMonth, playerState->BirthMonth, PlayerStateEvents.BirthMonthChanged);
        Update(ref _birthDay, playerState->BirthDay, PlayerStateEvents.BirthDayChanged);
        Update(ref _firstClass, playerState->FirstClass, PlayerStateEvents.FirstClassChanged);
        Update(ref _startTown, playerState->StartTown, PlayerStateEvents.StartTownChanged);
        Update(ref _activeFestivalIds, playerState->ActiveFestivalIds, PlayerStateEvents.ActiveFestivalIdsChanged);
        Update(ref _activeFestivalPhases, playerState->ActiveFestivalPhases, PlayerStateEvents.ActiveFestivalPhasesChanged);
        Update(ref _baseStrength, playerState->BaseStrength, PlayerStateEvents.BaseStrengthChanged);
        Update(ref _baseDexterity, playerState->BaseDexterity, PlayerStateEvents.BaseDexterityChanged);
        Update(ref _baseVitality, playerState->BaseVitality, PlayerStateEvents.BaseVitalityChanged);
        Update(ref _baseIntelligence, playerState->BaseIntelligence, PlayerStateEvents.BaseIntelligenceChanged);
        Update(ref _baseMind, playerState->BaseMind, PlayerStateEvents.BaseMindChanged);
        Update(ref _basePiety, playerState->BasePiety, PlayerStateEvents.BasePietyChanged);
        Update(ref _attributes, playerState->Attributes, PlayerStateEvents.AttributesChanged);
        Update(ref _grandCompany, playerState->GrandCompany, PlayerStateEvents.GrandCompanyChanged);
        Update(ref _gcRankMaelstrom, playerState->GCRankMaelstrom, PlayerStateEvents.GCRankMaelstromChanged);
        Update(ref _gcRankTwinAdders, playerState->GCRankTwinAdders, PlayerStateEvents.GCRankTwinAddersChanged);
        Update(ref _gcRankImmortalFlames, playerState->GCRankImmortalFlames, PlayerStateEvents.GCRankImmortalFlamesChanged);
        Update(ref _homeAetheryteId, playerState->HomeAetheryteId, PlayerStateEvents.HomeAetheryteIdChanged);
        Update(ref _favouriteAetheryteCount, playerState->FavouriteAetheryteCount, PlayerStateEvents.FavouriteAetheryteCountChanged);
        Update(ref _favouriteAetherytes, playerState->FavouriteAetherytes, PlayerStateEvents.FavouriteAetherytesChanged);
        Update(ref _freeAetheryteId, playerState->FreeAetheryteId, PlayerStateEvents.FreeAetheryteIdChanged);
        Update(ref _freeAetherytePlayStationPlus, playerState->FreeAetherytePlayStationPlus, PlayerStateEvents.FreeAetherytePlayStationPlusChanged);
        Update(ref _baseRestedExperience, playerState->BaseRestedExperience, PlayerStateEvents.BaseRestedExperienceChanged);
        Update(ref _unlockedMountsBitmask, playerState->UnlockedMountsBitmask, PlayerStateEvents.UnlockedMountsChanged); // TODO: send lumina rows instead of bitmask
        Update(ref _unlockedOrnamentsBitmask, playerState->UnlockedOrnamentsBitmask, PlayerStateEvents.UnlockedOrnamentsChanged); // TODO: send lumina rows instead of bitmask
        Update(ref _unlockedGlassesStylesBitmask, playerState->UnlockedGlassesStylesBitmask, PlayerStateEvents.UnlockedGlassesStylesChanged); // TODO: send lumina rows instead of bitmask
        Update(ref _numOwnedMounts, playerState->NumOwnedMounts, PlayerStateEvents.NumOwnedMountsChanged);
        Update(ref _caughtFishBitmask, playerState->CaughtFishBitmask, PlayerStateEvents.CaughtFishChanged);
        Update(ref _numFishCaught, playerState->NumFishCaught, PlayerStateEvents.NumFishCaughtChanged);
        Update(ref _fishingBait, playerState->FishingBait, PlayerStateEvents.FishingBaitChanged);
        Update(ref _unlockedSpearfishingNotebookBitmask, playerState->UnlockedSpearfishingNotebookBitmask, PlayerStateEvents.UnlockedSpearfishingNotebookChanged); // TODO: send lumina rows instead of bitmask
        Update(ref _caughtSpearfishBitmask, playerState->CaughtSpearfishBitmask, PlayerStateEvents.CaughtSpearfishChanged); // TODO: send lumina rows instead of bitmask
        Update(ref _numSpearfishCaught, playerState->NumSpearfishCaught, PlayerStateEvents.NumSpearfishCaughtChanged);
        Update(ref _contentRouletteCompletion, playerState->ContentRouletteCompletion, PlayerStateEvents.ContentRouletteCompletionChanged);
        Update(ref _playerCommendations, playerState->PlayerCommendations, PlayerStateEvents.PlayerCommendationsChanged);
        Update(ref _selectedPoses, playerState->SelectedPoses, PlayerStateEvents.SelectedPosesChanged);
        Update(ref _sightseeingLogUnlockState, playerState->SightseeingLogUnlockState, PlayerStateEvents.SightseeingLogUnlockStateChanged);
        Update(ref _sightseeingLogUnlockStateEx, playerState->SightseeingLogUnlockStateEx, PlayerStateEvents.SightseeingLogUnlockStateExChanged);
        Update(ref _unlockedAdventureBitmask, playerState->UnlockedAdventureBitmask, PlayerStateEvents.UnlockedAdventureChanged); // TODO: send lumina rows instead of bitmask
        Update(ref _completedAdventureBitmask, playerState->CompletedAdventureBitmask, PlayerStateEvents.CompletedAdventureChanged); // TODO: send lumina rows instead of bitmask
        Update(ref _unlockFlags, playerState->UnlockFlags, PlayerStateEvents.UnlockFlagsChanged);
        Update(ref _deliveryLevel, playerState->DeliveryLevel, PlayerStateEvents.DeliveryLevelChanged);
        Update(ref _meisterFlag, playerState->MeisterFlag, PlayerStateEvents.MeisterFlagChanged);
        Update(ref _squadronMissionCompletionTimestamp, playerState->SquadronMissionCompletionTimestamp, PlayerStateEvents.SquadronMissionCompletionTimestampChanged);
        Update(ref _squadronTrainingCompletionTimestamp, playerState->SquadronTrainingCompletionTimestamp, PlayerStateEvents.SquadronTrainingCompletionTimestampChanged);
        Update(ref _activeGcArmyExpedition, playerState->ActiveGcArmyExpedition, PlayerStateEvents.ActiveGcArmyExpeditionChanged);
        Update(ref _activeGcArmyTraining, playerState->ActiveGcArmyTraining, PlayerStateEvents.ActiveGcArmyTrainingChanged);
        Update(ref _hasNewGcArmyCandidate, playerState->HasNewGcArmyCandidate, PlayerStateEvents.HasNewGcArmyCandidateChanged);
        Update(ref _unlockedMinerFolkloreTomeBitmask, playerState->UnlockedMinerFolkloreTomeBitmask, PlayerStateEvents.UnlockedMinerFolkloreTomeChanged); // TODO: send lumina rows instead of bitmask
        Update(ref _unlockedBotanistFolkloreTomeBitmask, playerState->UnlockedBotanistFolkloreTomeBitmask, PlayerStateEvents.UnlockedBotanistFolkloreTomeChanged); // TODO: send lumina rows instead of bitmask
        Update(ref _unlockedFishingFolkloreTomeBitmask, playerState->UnlockedFishingFolkloreTomeBitmask, PlayerStateEvents.UnlockedFishingFolkloreTomeChanged); // TODO: send lumina rows instead of bitmask
        Update(ref _weeklyBingoOrderData, playerState->WeeklyBingoOrderData, PlayerStateEvents.WeeklyBingoOrderDataChanged); // TODO: send lumina rows instead of bitmask
        Update(ref _weeklyBingoRewardData, playerState->WeeklyBingoRewardData, PlayerStateEvents.WeeklyBingoRewardDataChanged); // TODO: send lumina rows instead of bitmask
        Update(ref _weeklyBingoExpMultiplier, playerState->WeeklyBingoExpMultiplier, PlayerStateEvents.WeeklyBingoExpMultiplierChanged);
        Update(ref _contentKeyValueData, playerState->ContentKeyValueData, PlayerStateEvents.ContentKeyValueDataChanged);
        Update(ref _mentorVersion, playerState->MentorVersion, PlayerStateEvents.MentorVersionChanged);
        Update(ref _desynthesisLevels, playerState->DesynthesisLevels, PlayerStateEvents.DesynthesisLevelsChanged);

        // props
        Update(ref _isLegacy, playerState->IsLegacy, PlayerStateEvents.IsLegacyChanged);
        Update(ref _isWarriorOfLight, playerState->IsWarriorOfLight, PlayerStateEvents.IsWarriorOfLightChanged);
        Update(ref _weeklyBingoExpireDateTime, playerState->WeeklyBingoExpireDateTime, PlayerStateEvents.WeeklyBingoExpireDateTimeChanged);
        Update(ref _weeklyBingoNumSecondChancePoints, playerState->WeeklyBingoNumSecondChancePoints, PlayerStateEvents.WeeklyBingoNumSecondChancePointsChanged);
        Update(ref _hasWeeklyBingoJournal, playerState->HasWeeklyBingoJournal, PlayerStateEvents.HasWeeklyBingoJournalChanged);
        Update(ref _weeklyBingoNumPlacedStickers, playerState->WeeklyBingoNumPlacedStickers, PlayerStateEvents.WeeklyBingoNumPlacedStickersChanged);

        // funcs
        Update(ref _isMentor, playerState->IsMentor(), PlayerStateEvents.IsMentorChanged);
        Update(ref _isBattleMentor, playerState->IsBattleMentor(), PlayerStateEvents.IsBattleMentorChanged);
        Update(ref _isTradeMentor, playerState->IsTradeMentor(), PlayerStateEvents.IsTradeMentorChanged);
        Update(ref _isNovice, playerState->IsNovice(), PlayerStateEvents.IsNoviceChanged);
        Update(ref _isReturner, playerState->IsReturner(), PlayerStateEvents.IsReturnerChanged);
        Update(ref _isLoginSecurityToken, playerState->IsPlayerStateFlagSet(PlayerStateFlag.IsLoginSecurityToken), PlayerStateEvents.IsLoginSecurityTokenChanged);
        Update(ref _isBuddyInStable, playerState->IsPlayerStateFlagSet(PlayerStateFlag.IsBuddyInStable), PlayerStateEvents.IsBuddyInStableChanged);
        Update(ref _isNoviceNetworkAutoJoinEnabled, playerState->IsPlayerStateFlagSet(PlayerStateFlag.IsNoviceNetworkAutoJoinEnabled), PlayerStateEvents.IsNoviceNetworkAutoJoinEnabledChanged);
        Update(ref _isWeeklyBingoExpired, playerState->IsWeeklyBingoExpired(), PlayerStateEvents.IsWeeklyBingoExpiredChanged);

        // TODO: GetWeeklyBingoTaskStatus, GetBeastTribeRank, GetBeastTribeCurrentReputation
    }

    private void Update<T>(ref T oldValue, T newValue, string eventName) where T : notnull
    {
        if (!EqualityComparer<T>.Default.Equals(oldValue, newValue))
        {
            _eventEmitter.TriggerEvent(eventName, new ValueChangedEventArgs<T>() { OldValue = oldValue, NewValue = newValue });
            oldValue = newValue;
        }
    }

    private void Update<T>(ref T[] oldValue, Span<T> newValue, string eventName) where T : notnull
    {
        if (!oldValue.AsSpan().SequenceEqual(newValue))
        {
            var newArray = newValue.ToArray();
            _eventEmitter.TriggerEvent(eventName, new ValueChangedEventArgs<T[]>() { OldValue = oldValue, NewValue = newArray });
            oldValue = newArray;
        }
    }
}
