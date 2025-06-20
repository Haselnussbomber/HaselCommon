namespace HaselCommon.Game.Enums;

// Credits:
// https://github.com/MidoriKami/KamiLib/blob/master/Classes/TerritoryIntendedUseEnum.cs
// https://github.com/Xpahtalo/XpahtaLib/blob/main/XpahtaLib/DalamudUtilities/UsefulEnums/TerritoryIntendedUseEnum.cs
// and myself :)

public enum TerritoryIntendedUse : byte
{
    Town = 0,
    Overworld = 1,
    Inn = 2,
    Dungeon = 3, // Dungeons, Guildhests, Mahjong, Air Force One
    VariantDungeon = 4,
    MordionGaol = 5,
    OpeningArea = 6,
    BeforeTrialDung = 7,
    AllianceRaid = 8,
    PreEwOverworldQuestBattle = 9,
    Trial = 10,
    Unknown11 = 11, // unused
    WaitingRoom = 12,
    HousingOutdoor = 13,
    HousingIndoor = 14,
    SoloOverworldInstances = 15,
    Raid1 = 16,
    Raid2 = 17,
    Frontline = 18,
    ChocoboSquareOld = 19, // unused
    ChocoboRacing = 20,
    Firmament = 21,
    SanctumOfTheTwelve = 22, // Wedding
    GoldSaucer = 23,
    OriginalStepsOfFaith = 24, // unused
    LordOfVerminion = 25,
    ExploratoryMissions = 26,
    HallOfTheNovice = 27,
    CrystallineConflict = 28,
    SoloDuty = 29,
    GrandCompanyBarracks = 30,
    DeepDungeon = 31,
    Seasonal = 32, // During the Starlight Celebration, the music in Lower Jeuno will change to a Christmas version.
    TreasureMapInstance = 33,
    SeasonalInstancedArea = 34,
    TripleTriadBattlehall = 35,
    ChaoticRaid = 36,
    CrystallineConflictCustomMatch = 37,
    HuntingGrounds = 38, // Diadem
    RivalWings = 39,
    Unknown40 = 40,
    Eureka = 41,
    Unknown42 = 42, // unused, was Crystal Tower Training Grounds
    TheCalamityRetold = 43,
    LeapOfFaith = 44,
    MaskedCarnival = 45,
    OceanFishing = 46,
    Diadem = 47,
    Bozja = 48,
    IslandSanctuary = 49,
    TripleTriadOpenTournament = 50,
    TripleTriadInvitationalParlor = 51,
    DelubrumReginae = 52,
    DelubrumReginaeSavage = 53,
    EndwalkerMsqSoloOverworld = 54, // Propylaion and Ultima Thule
    Unknown55 = 55, // unused
    Elysion = 56,
    CriterionDungeon = 57,
    CriterionDungeonSavage = 58,
    Blunderville = 59,
    CosmicExploration = 60,
    OccultCrescent = 61,
    Unknown62 = 62, // unused, Forked Tower?
};
