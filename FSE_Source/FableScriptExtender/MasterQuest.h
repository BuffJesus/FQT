#pragma once
#include "FableAPI.h"
#include <vector>

enum EGameflowPosition : __int32
{
    EGP_INTRO = 0x0,
    EGP_TRAINING = 0x64,
    EGP_WAITING_FOR_WASP_BOSS = 0x96,
    EGP_WASP_BOSS = 0xC8,
    EGP_MAZE_MEETING = 0x12C,
    EGP_WAITING_FOR_ORCHARD_FARM = 0x190,
    EGP_ORCHARD_FARM = 0x1C2,
    EGP_WAITING_FOR_TRADER_ESCORT = 0x1F4,
    EGP_TRADER_ESCORT = 0x226,
    EGP_SECOND_MAZE_MEETING = 0x258,
    EGP_BANDIT_CAMP = 0x2BC,
    EGP_WAITING_FOR_BANDIT_CAMP_TWINBLADE = 0x2DD,
    EGP_BANDIT_CAMP_TWINBLADE = 0x2FE,
    EGP_MAZE_TELEPORT_TO_WW = 0x320,
    EGP_TROPHY_DEALER = 0x352,
    EGP_WITCHWOOD_POST_TROPHY_DEALER = 0x358,
    EGP_WHITE_BALVERINE_KHG = 0x361,
    EGP_WHITE_BALVERINE_WITCHWOOD = 0x366,
    EGP_WITCHWOOD_WAITING_FOR_ARENA = 0x36B,
    EGP_ARENA = 0x384,
    EGP_MEET_SISTER = 0x3E8,
    EGP_WAITING_FOR_MCC = 0x41A,
    EGP_MINION_CLIFFTOP_CHASE = 0x44C,
    EGP_GRAVEYARD = 0x4B0,
    EGP_WAITING_FOR_FINALGRAVEYARD = 0x4C0,
    EGP_FINALGRAVEYARD = 0x4D1,
    EGP_PRISON = 0x4E2,
    EGP_HOOK_COAST = 0x514,
    EGP_WAITING_FOR_WIZARD_BATTLE = 0x5AA,
    EGP_WIZARD_BATTLE = 0x5DC,
    EGP_WAITING_FOR_FOCAL_SITES = 0x60E,
    EGP_FOCAL_SITES = 0x640,
    EGP_JACK_BOSS_FIGHT = 0x6A4,
    EGP_FIRE_HEART = 0x76C,
    EGP_THE_SHIP = 0x834,
    EGP_THE_ORACLE = 0x8FC,
    EGP_SCYTHE_MEETING = 0x960,
    EGP_HERO_SOULS = 0x9C4,
    EGP_DRAGON_FIGHT = 0xA28,
    EGP_FINISHED_GAME = 0xAF0,
};
enum EPlaceInHeroSouls : __int32
{
    EPIHS_1ST_SOUL_TALK_TO_BRIAR = 0x0,
    EPIHS_1ST_SOUL_TALK_TO_THUNDER = 0x1,
    EPIHS_1ST_SOUL_VISIT_THE_ARENA = 0x2,
    EPIHS_2ND_SOUL_TALK_TO_BRIAR_KILLED_THUNDER = 0x3,
    EPIHS_2ND_SOUL_TALK_TO_BRIAR_GOT_ARENA_SOUL = 0x4,
    EPIHS_2ND_SOUL_VISIT_YOUR_MOTHER = 0x5,
    EPIHS_3RD_SOUL_TALK_TO_BRIAR = 0x6,
    EPIHS_3RD_SOUL_TALK_TO_SCYTHE = 0x7,
};
enum EHeroNames : __int32
{
    NAME_NAMELESS = 0x0,
    NAME_FEASTMASTER = 0x1,
    NAME_BASILISK = 0x2,
    NAME_DARKBLADE = 0x3,
    NAME_WOLFSEYE = 0x4,
};

struct CQ_SunnyvaleMasterData : CScriptDataBase
{
    EGameflowPosition PostSavePosition; // 0x04
    int HenchmanCommentaryState; // 0x08
    int HenchmanOverrideState; // 0x0C
    bool HenchmanSacrificedBodge; // 0x10
    bool HauntedBarrowFieldsCompleted; // 0x11
    char pad_12[2]; // 0x12
    int RandomEntityVillagerMax; // 0x14
    int RandomEntityGuardMax; // 0x18
    int RandomEntityBanditMax; // 0x1C
    int RandomEntityVillagerCount; // 0x20
    int RandomEntityGuardCount; // 0x24
    int RandomEntityBanditCount; // 0x28
    float RandomEntityVillagerPercentage; // 0x2C
    float RandomEntityGuardPercentage; // 0x30
    float RandomEntityBanditPercentage; // 0x34
    bool RandomEntitySheriffAlive; // 0x38
    bool RandomEntityLieutenantAlive; // 0x39
    bool BanditCampTwinbladeKilled; // 0x3A
    char pad_3B; // 0x3B
    int ArcheryStateCurrent; // 0x3C
    int ArcheryStateRequired; // 0x40
    int ArcheryHighScore; // 0x44
    bool FriendOfForeman; // 0x48
    bool BridgeOpened; // 0x49
    bool GrannyMemoryReturned; // 0x4A
    bool IsLunaHuman; // 0x4B
    bool CondemnedManDead; // 0x4C
    bool CondemnedManForgiven; // 0x4D
    bool CondemnedManMeetsBodyGuard; // 0x4E
    bool CondemnedManMeetsBodyGuardCutSceneStart; // 0x4F
    bool CondemnedManMeetsBodyGuardCutSceneFinished; // 0x50
    char pad_51[3]; // 0x51
    CCharString TeddySolution; // 0x54
    int OrchardFarmRaidLastCompleted; // 0x58
    int OrchardFarmTraderEscortCounter; // 0x5C
    bool SeenAbbeyMotherAtGuild; // 0x60
    bool DefeatedThunder; // 0x61
    bool LostToThunder; // 0x62
    bool KilledThunder; // 0x63
    bool CollectedSoulFromArena; // 0x64
    bool KilledBriar; // 0x65
    bool CollectedSoulFromMother; // 0x66
    bool KilledGM; // 0x67
    bool CollectedSoulFromNostro; // 0x68
    char pad_69[3]; // 0x69
    int DeliveredSoul; // 0x6C
    EPlaceInHeroSouls CurrentHeroSoulsPosition; // 0x70
    bool WhisperKilledByHero; // 0x74
    bool ArenaFinished; // 0x75
    bool GatesRequireClosing; // 0x76
    bool GatesRequireOpening; // 0x77
    int HeroDrunkness; // 0x78
    bool TrophyDealerHeroSpokenToDemonDoors; // 0x7C
    char pad_7D[3]; // 0x7D
    float VillagerAngryRating; // 0x80
    bool MotherDodgingKraken; // 0x84
    char pad_85[3]; // 0x85
    int JackBossBattleResult; // 0x88
    bool JackBossBattleHeroGoodAtEnd; // 0x8C
    bool WifeLeavingYou; // 0x8D
    bool WifeLeftYou; // 0x8E
    char pad_8F; // 0x8F
    int CurrentScene; // 0x90
    bool StartNextScene; // 0x94
    char pad_95[3]; // 0x95
    int CurrentChapter; // 0x98
    bool ScorpionsDestroyed; // 0x9C
    bool ScorpionsDestroyedCutscenePlayed; // 0x9D
    bool SkillTrainingStarted; // 0x9E
    bool WillTrainingStarted; // 0x9F
    bool MovingDummiesNeeded; // 0xA0
    char pad_A1[3]; // 0xA1
    int SkillScore; // 0xA4
    int HighestSkillScore; // 0xA8
    int WillScore; // 0xAC
    bool MeleeApprenticeNeededForCutscene; // 0xB0
    char pad_B1[3]; // 0xB1
    int GlobalMeleeGrade; // 0xB4
    int GlobalSkillGrade; // 0xB8
    int GlobalWillGrade; // 0xBC
    bool GuildWarningOccuring; // 0xC0
    bool SkillTestOccuring; // 0xC1
    bool WillTestOccuring; // 0xC2
    bool SkillRepeating; // 0xC3
    bool SkillRepeatKnown; // 0xC4
    bool SkillDummyReset; // 0xC5
    bool HeroTakingGuildTest; // 0xC6
    bool SwordInTheStoneComplete; // 0xC7
    bool AmbushTradersAllGuardsDead; // 0xC8
    bool AmbushTradersAllTradersDead; // 0xC9
    char pad_CA[2]; // 0xCA
    int AmbushTradersKillCount; // 0xCC
    int AmbushTradersBanditHireCount; // 0xD0
    bool AmbushTradersSpyDead; // 0xD4
    bool StruckDealWithLadyGrey; // 0xD5
    bool HeroExposedLadyGrey; // 0xD6
    bool HeroMarriedLadyGrey; // 0xD7
    bool BountyHuntWithinTimeLimit; // 0xD8
    bool BountyHuntDecapitation; // 0xD9
    bool BountyHuntTimeLimitExceeded; // 0xDA
    char pad_DB; // 0xDB
    unsigned int AchievementsWorthyOfSong; // 0xDC
    bool BreakSiegeFinished; // 0xE0
    bool WhiteBalverineFinished; // 0xE1
    bool MadBomberFinished; // 0xE2
    bool BodyGuardsMustStandAndWait; // 0xE3
    bool BodyGuardsInLimbo; // 0xE4
    bool WifeHeroFlirting; // 0xE5
    char pad_E6[2]; // 0xE6
    int PrisonRaceNumber; // 0xE8
    bool PrisonRaceWonByHero; // 0xEC
    bool WasPrisonRaceEverWonByHero; // 0xED
    bool PrisonKeyStolenByHero; // 0xEE
    bool BooksPreviouslyOpened[3]; // 0xEF
    bool PrisonTorturedBefore; // 0xF2
    bool PrisonGameDoneBefore; // 0xF3
    bool HeroDollsScriptUsingTeacher; // 0xF4
    char pad_F5[3]; // 0xF5
    int MaxChickenKickingScore; // 0xF8
    unsigned int StoryTellerSpecialStories; // 0xFC
    unsigned int StoryTellerToldSpecialStories; // 0x100
    EHeroNames GHeroName; // 0x104
    bool TimeAdvancePointTriggered; // 0x108
    bool OFBRCratesStolen; // 0x109
    bool OFBR_NoCratesWereStolen; // 0x10A
    bool DarkwoodAllTradersAlive; // 0x10B
    bool MadBomberNoBombsExplode; // 0x10C
    bool HobbeCavePerpertratorKilled; // 0x10D
    char pad_10E[2]; // 0x10E
    int HobbeCaveInnocentSacrificed; // 0x110
    bool HobbeCaveBoyUnharmed; // 0x114
    bool BanditCampKillNoBandits; // 0x115
    bool BreakSiegeNoAssistance; // 0x116
    bool BreakSiegeKilledLeader; // 0x117
    bool OrchardFarmBanditKilled; // 0x118
    bool OrchardFarmGuardKilled; // 0x119
    bool BanditCampKillManyBandits; // 0x11A
    bool HobbeContestKillMoreHobbes; // 0x11B
    bool GraveyardKillAllInnocents; // 0x11C
    char pad_11D[3]; // 0x11D
    int DarkwoodPickpocketedAllTraders; // 0x120
    bool HangingTreeBanditKilled; // 0x124
    bool HangingTreeGuardKilled; // 0x125
    bool TCGKillNoBandits; // 0x126
    bool TCGMadeTimeLimit; // 0x127
    bool TCEKeepBanditFollowerAlive; // 0x128
    bool TCEMadeTimeLimit; // 0x129
    bool RansomVictimVictimNoDamage; // 0x12A
    bool RansomVictimKidnappersKilled; // 0x12B
    bool RansomVictimSaveVictim; // 0x12C
    bool RansomVictimHaveVictimKilled; // 0x12D
    bool MinionCampBriarNoDamage; // 0x12E
    bool FireHeartFreeAllPrisoners; // 0x12F
    bool FireHeartKillAllPrisoners; // 0x130
    bool TCETimeLimitBoastTaken; // 0x131
    bool TCGTimeLimitBoastTaken; // 0x132
    bool HeroWoreMask; // 0x133

    char BoastRegions[16]; // 0x134

    bool SingingStonesInSync; // 0x144

    char pad_145[3]; // 0x145;
};