namespace HaselCommon.Events;

public static class PlayerStateEvents
{
    // fields
    public const string IsLoadedChanged = "PlayerState.IsLoadedChanged";
    public const string CharacterNameChanged = "PlayerState.CharacterNameChanged";
    public const string EntityIdChanged = "PlayerState.EntityIdChanged";
    public const string ContentIdChanged = "PlayerState.ContentIdChanged";
    public const string DutyPenaltyChanged = "PlayerState.DutyPenaltyChanged";
    public const string MaxLevelChanged = "PlayerState.MaxLevelChanged";
    public const string MaxExpansionChanged = "PlayerState.MaxExpansionChanged";
    public const string SexChanged = "PlayerState.SexChanged";
    public const string RaceChanged = "PlayerState.RaceChanged";
    public const string TribeChanged = "PlayerState.TribeChanged";
    public const string CurrentClassJobIdChanged = "PlayerState.CurrentClassJobIdChanged";
    public const string CurrentLevelChanged = "PlayerState.CurrentLevelChanged";
    public const string ClassJobLevelsChanged = "PlayerState.ClassJobLevelsChanged";
    public const string ClassJobExperienceChanged = "PlayerState.ClassJobExperienceChanged";
    public const string SyncedLevelChanged = "PlayerState.SyncedLevelChanged";
    public const string IsLevelSyncedChanged = "PlayerState.IsLevelSyncedChanged";
    public const string HasPremiumSaddlebagChanged = "PlayerState.HasPremiumSaddlebagChanged";
    public const string GuardianDeityChanged = "PlayerState.GuardianDeityChanged";
    public const string BirthMonthChanged = "PlayerState.BirthMonthChanged";
    public const string BirthDayChanged = "PlayerState.BirthDayChanged";
    public const string FirstClassChanged = "PlayerState.FirstClassChanged";
    public const string StartTownChanged = "PlayerState.StartTownChanged";
    public const string ActiveFestivalIdsChanged = "PlayerState.ActiveFestivalIdsChanged";
    public const string ActiveFestivalPhasesChanged = "PlayerState.ActiveFestivalPhasesChanged";
    public const string BaseStrengthChanged = "PlayerState.BaseStrengthChanged";
    public const string BaseDexterityChanged = "PlayerState.BaseDexterityChanged";
    public const string BaseVitalityChanged = "PlayerState.BaseVitalityChanged";
    public const string BaseIntelligenceChanged = "PlayerState.BaseIntelligenceChanged";
    public const string BaseMindChanged = "PlayerState.BaseMindChanged";
    public const string BasePietyChanged = "PlayerState.BasePietyChanged";
    public const string AttributesChanged = "PlayerState.AttributesChanged";
    public const string GrandCompanyChanged = "PlayerState.GrandCompanyChanged";
    public const string GCRankMaelstromChanged = "PlayerState.GCRankMaelstromChanged";
    public const string GCRankTwinAddersChanged = "PlayerState.GCRankTwinAddersChanged";
    public const string GCRankImmortalFlamesChanged = "PlayerState.GCRankImmortalFlamesChanged";
    public const string HomeAetheryteIdChanged = "PlayerState.HomeAetheryteIdChanged";
    public const string FavouriteAetheryteCountChanged = "PlayerState.FavouriteAetheryteCountChanged";
    public const string FavouriteAetherytesChanged = "PlayerState.FavouriteAetherytesChanged";
    public const string FreeAetheryteIdChanged = "PlayerState.FreeAetheryteIdChanged";
    public const string FreeAetherytePlayStationPlusChanged = "PlayerState.FreeAetherytePlayStationPlusChanged";
    public const string BaseRestedExperienceChanged = "PlayerState.BaseRestedExperienceChanged";
    public const string UnlockedMountsChanged = "PlayerState.UnlockedMountsChanged";
    public const string UnlockedOrnamentsChanged = "PlayerState.UnlockedOrnamentsChanged";
    public const string UnlockedGlassesStylesChanged = "PlayerState.UnlockedGlassesStylesChanged";
    public const string NumOwnedMountsChanged = "PlayerState.NumOwnedMountsChanged";
    public const string CaughtFishChanged = "PlayerState.CaughtFishChanged";
    public const string NumFishCaughtChanged = "PlayerState.NumFishCaughtChanged";
    public const string FishingBaitChanged = "PlayerState.FishingBaitChanged";
    public const string UnlockedSpearfishingNotebookChanged = "PlayerState.UnlockedSpearfishingNotebookChanged";
    public const string CaughtSpearfishChanged = "PlayerState.CaughtSpearfishChanged";
    public const string NumSpearfishCaughtChanged = "PlayerState.NumSpearfishCaughtChanged";
    public const string ContentRouletteCompletionChanged = "PlayerState.ContentRouletteCompletionChanged";
    public const string PlayerCommendationsChanged = "PlayerState.PlayerCommendationsChanged";
    public const string SelectedPosesChanged = "PlayerState.SelectedPosesChanged";
    public const string SightseeingLogUnlockStateChanged = "PlayerState.SightseeingLogUnlockStateChanged";
    public const string SightseeingLogUnlockStateExChanged = "PlayerState.SightseeingLogUnlockStateExChanged";
    public const string UnlockedAdventureChanged = "PlayerState.UnlockedAdventureChanged";
    public const string CompletedAdventureChanged = "PlayerState.CompletedAdventureChanged";
    public const string UnlockFlagsChanged = "PlayerState.UnlockFlagsChanged";
    public const string DeliveryLevelChanged = "PlayerState.DeliveryLevelChanged";
    public const string MeisterFlagChanged = "PlayerState.MeisterFlagChanged";
    public const string SquadronMissionCompletionTimestampChanged = "PlayerState.SquadronMissionCompletionTimestampChanged";
    public const string SquadronTrainingCompletionTimestampChanged = "PlayerState.SquadronTrainingCompletionTimestampChanged";
    public const string ActiveGcArmyExpeditionChanged = "PlayerState.ActiveGcArmyExpeditionChanged";
    public const string ActiveGcArmyTrainingChanged = "PlayerState.ActiveGcArmyTrainingChanged";
    public const string HasNewGcArmyCandidateChanged = "PlayerState.HasNewGcArmyCandidateChanged";
    public const string UnlockedMinerFolkloreTomeChanged = "PlayerState.UnlockedMinerFolkloreTomeChanged";
    public const string UnlockedBotanistFolkloreTomeChanged = "PlayerState.UnlockedBotanistFolkloreTomeChanged";
    public const string UnlockedFishingFolkloreTomeChanged = "PlayerState.UnlockedFishingFolkloreTomehanged";
    public const string WeeklyBingoOrderDataChanged = "PlayerState.WeeklyBingoOrderDataChanged";
    public const string WeeklyBingoRewardDataChanged = "PlayerState.WeeklyBingoRewardDataChanged";
    public const string WeeklyBingoExpMultiplierChanged = "PlayerState.WeeklyBingoExpMultiplierChanged";
    public const string ContentKeyValueDataChanged = "PlayerState.ContentKeyValueDataChanged";
    public const string MentorVersionChanged = "PlayerState.MentorVersionChanged";
    public const string DesynthesisLevelsChanged = "PlayerState.DesynthesisLevelsChanged";

    // props
    public const string IsLegacyChanged = "PlayerState.IsLegacyChanged";
    public const string IsWarriorOfLightChanged = "PlayerState.IsWarriorOfLightChanged";
    public const string WeeklyBingoExpireDateTimeChanged = "PlayerState.WeeklyBingoExpireDateTimeChanged";
    public const string WeeklyBingoNumSecondChancePointsChanged = "PlayerState.WeeklyBingoNumSecondChancePointsChanged";
    public const string HasWeeklyBingoJournalChanged = "PlayerState.HasWeeklyBingoJournalChanged";
    public const string WeeklyBingoNumPlacedStickersChanged = "PlayerState.WeeklyBingoNumPlacedStickersChanged";

    // funcs
    public const string IsMentorChanged = "PlayerState.IsMentorChanged";
    public const string IsBattleMentorChanged = "PlayerState.IsBattleMentorChanged";
    public const string IsTradeMentorChanged = "PlayerState.IsTradeMentorChanged";
    public const string IsNoviceChanged = "PlayerState.IsNoviceChanged";
    public const string IsReturnerChanged = "PlayerState.IsReturnerChanged";
    public const string IsLoginSecurityTokenChanged = "PlayerState.IsLoginSecurityTokenChanged";
    public const string IsBuddyInStableChanged = "PlayerState.IsBuddyInStableChanged";
    public const string IsNoviceNetworkAutoJoinEnabledChanged = "PlayerState.IsNoviceNetworkAutoJoinEnabledChanged";
    public const string IsWeeklyBingoExpiredChanged = "PlayerState.IsWeeklyBingoExpiredChanged";
}
