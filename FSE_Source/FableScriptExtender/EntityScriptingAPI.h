#pragma once
#include "FableAPI.h" 
#include <vector> 

struct CScriptThing;
struct CTCScriptedControl;
struct CGameScriptThing;
struct CThing;
struct C3DVector
{
    float x, y, z;
};
struct CScriptThing;
struct C3DVector;
struct CScriptGameResourceObjectScriptedThingBase;
class CWorld;
class CGameScriptInterface;
enum EAIStateGroupType : __int32
{
    AI_STATE_NONE = 0x0,
    AI_STATE_FIGHTING = 0x1,
    AI_STATE_FOLLOWING = 0x2,
    AI_STATE_MISC = 0x3,
};
enum EHeroAbility : __int32
{
    HERO_ABILITY_NULL = 0x0,
    HERO_ABILITY_FORCE_PUSH = 0x1,
    HERO_ABILITY_TIME_SPELL = 0x2,
    HERO_ABILITY_ENFLAME_SPELL = 0x3,
    HERO_ABILITY_PHYSICAL_SHIELD_SPELL = 0x4,
    HERO_ABILITY_TURNCOAT_SPELL = 0x5,
    HERO_ABILITY_DRAIN_LIFE_SPELL = 0x6,
    HERO_ABILITY_RAISE_DEAD_SPELL = 0x7,
    HERO_ABILITY_BERSERK = 0x8,
    HERO_ABILITY_DOUBLE_STRIKE = 0x9,
    HERO_ABILITY_SUMMON_SPELL = 0xA,
    HERO_ABILITY_LIGHTNING_SPELL = 0xB,
    HERO_ABILITY_BATTLE_CHARGE = 0xC,
    HERO_ABILITY_ASSASSIN_RUSH = 0xD,
    HERO_ABILITY_HEAL_LIFE_SPELL = 0xE,
    HERO_ABILITY_GHOST_SWORD_SPELL = 0xF,
    HERO_ABILITY_FIREBALL_SPELL = 0x10,
    HERO_ABILITY_MULTI_ARROW = 0x11,
    HERO_ABILITY_DIVINE_WRATH_SPELL = 0x12,
    HERO_ABILITY_UNHOLY_POWER_SPELL = 0x13,
    MAX_NUMBER_OF_HERO_ABILITIES = 0x14 // Keep this for range checking
};
enum EXboxControllerButton : __int32
{
    XBOX_PAD_UNDEFINED_BUTTON = 0x0,
    XBOX_PAD_X_BUTTON = 0x1,
    XBOX_PAD_Y_BUTTON = 0x2,
    XBOX_PAD_BLACK_BUTTON = 0x3,
    XBOX_PAD_A_BUTTON = 0x4,
    XBOX_PAD_B_BUTTON = 0x5,
    XBOX_PAD_WHITE_BUTTON = 0x6,
    XBOX_PAD_LEFT_TRIGGER = 0x7,
    XBOX_PAD_RIGHT_TRIGGER = 0x8,
    XBOX_PAD_LEFT_STICK_BUTTON = 0x9,
    XBOX_PAD_RIGHT_STICK_BUTTON = 0xA,
    XBOX_PAD_START_BUTTON = 0xB,
    XBOX_PAD_BACK_BUTTON = 0xC,
    XBOX_PAD_DPAD_UP_BUTTON = 0xD,
    XBOX_PAD_DPAD_DOWN_BUTTON = 0xE,
    XBOX_PAD_DPAD_LEFT_BUTTON = 0xF,
    XBOX_PAD_DPAD_RIGHT_BUTTON = 0x10,
    XBOX_PAD_LEFT_ANALOGUE_STICK = 0x11,
    XBOX_PAD_RIGHT_ANALOGUE_STICK = 0x12,
};
enum ETextGroupSelectionMethod {
    GROUP_SELECT_FIRST = 0,
    GROUP_SELECT_RANDOM = 1,
    GROUP_SELECT_RANDOM_NO_REPEAT = 2,
    GROUP_SELECT_SEQUENTIAL = 3,
    GROUP_SELECT_NONE = 4
};
enum EScriptEntityMoveType : __int32
{
    ENTITY_MOVE_WALK = 0x0,
    ENTITY_MOVE_RUN = 0x1,
    ENTITY_MOVE_SNEAK = 0x2, 
};

typedef void* (__thiscall* tCScriptThing_Destructor)(CScriptThing* This, unsigned int flags);
typedef const CCharString* (__thiscall* tCScriptThing_GetName)(CScriptThing* This);
typedef CCharString* (__thiscall* tCScriptThing_GetDefName)(CScriptThing* This, CCharString* pResult);
typedef CCharString* (__thiscall* tCScriptThing_GetDataString)(CScriptThing* This, CCharString* pResult);
typedef void(__thiscall* tCScriptThing_SetDataString)(CScriptThing* This, const CCharString* pDataString);
typedef C3DVector* (__thiscall* tCScriptThing_GetFocalPos)(CScriptThing* This, C3DVector* pResult);
typedef const C3DVector* (__thiscall* tCScriptThing_GetPos)(CScriptThing* This);
typedef C3DVector* (__thiscall* tCScriptThing_GetHomePos)(CScriptThing* This, C3DVector* pResult);
typedef CCharString* (__thiscall* tCScriptThing_GetCurrentMapName)(CScriptThing* This, CCharString* pResult);
typedef CCharString* (__thiscall* tCScriptThing_GetHomeMapName)(CScriptThing* This, CCharString* pResult);
typedef float(__thiscall* tCScriptThing_GetAngleXY)(CScriptThing* This);
typedef CThing* (__thiscall* tCScriptThing_GetPThing)(CScriptThing* This);
typedef unsigned __int64(__thiscall* tCScriptThing_GetPThingUniqueID)(CScriptThing* This);
typedef EAIStateGroupType(__thiscall* tCScriptThing_GetCurrentStateGroupType)(CScriptThing* This);
typedef EScriptAIPriority(__thiscall* tCScriptThing_GetCurrentScriptPriority)(CScriptThing* This);
typedef bool(__thiscall* tCScriptThing_IsSneaking)(CScriptThing* This);
typedef bool(__thiscall* tCScriptThing_IsAwareOfHero)(CScriptThing* This);
typedef bool(__thiscall* tCScriptThing_MsgIsTriggeredBy)(CScriptThing* This, const CCharString* pTriggerName);
typedef bool(__thiscall* tCScriptThing_MsgIsKilledBy)(CScriptThing* This, const CCharString* pKillerName);
typedef CScriptThing* (__thiscall* tCScriptThing_MsgWhoKilledMe)(CScriptThing* This, CScriptThing* pResult);
typedef bool(__thiscall* tCScriptThing_MsgIsUsedBy)(CScriptThing* This, const CCharString* pUserName);
typedef bool(__thiscall* tCScriptThing_MsgIsHitBy)(CScriptThing* This, const CCharString* pHitterName);
typedef bool(__thiscall* tCScriptThing_MsgIsHitByWithFlourish)(CScriptThing* This, const CCharString* pHitterName);
typedef bool(__thiscall* tCScriptThing_MsgIsHitByWithDecapitate)(CScriptThing* This, const CCharString* pHitterName);
typedef bool(__thiscall* tCScriptThing_MsgIsHitByWithWeapon)(CScriptThing* This, const CCharString* pHitterName, const CCharString* pWeaponName);
typedef bool(__thiscall* tCScriptThing_MsgIsHitByWithProjectileWeapon)(CScriptThing* This, const CCharString* pHitterName, float* pOutDamage);
typedef CScriptThing* (__thiscall* tCScriptThing_MsgWhoHitMe)(CScriptThing* This, CScriptThing* pResult);
typedef bool(__thiscall* tCScriptThing_MsgIsTalkedToBy)(CScriptThing* This, const CCharString* pTalkerName);
typedef CScriptThing* (__thiscall* tCScriptThing_MsgWhoTalkedToMe)(CScriptThing* This, CScriptThing* pResult);
typedef bool(__thiscall* tCScriptThing_MsgExpressionPerformedTo)(CScriptThing* This, CCharString* pOutExpressionName);
typedef CScriptThing* (__thiscall* tCScriptThing_MsgWhoExpressedToMe)(CScriptThing* This, CScriptThing* pResult);
typedef int(__thiscall* tCScriptThing_MsgHowLongWasExpressionPerformed)(CScriptThing* This);
typedef bool(__thiscall* tCScriptThing_MsgIsKnockedOutBy)(CScriptThing* This, const CCharString* pName);
typedef CScriptThing* (__thiscall* tCScriptThing_MsgWhoKnockedMeOut)(CScriptThing* This, CScriptThing* pResult);
typedef bool(__thiscall* tCScriptThing_MsgReceivedMoney)(CScriptThing* This, int* pOutAmount);
typedef bool(__thiscall* tCScriptThing_MsgIsPresentedWithItem)(CScriptThing* This, CCharString* pOutItemName);
typedef bool(__thiscall* tCScriptThing_MsgReceivedInventoryItem)(CScriptThing* This);
typedef bool(__thiscall* tCScriptThing_MsgPerformedSpecialAbility)(CScriptThing* This, EHeroAbility ability);
typedef bool(__thiscall* tCScriptThing_MsgPerformedAnySpecialAbility)(CScriptThing* This);
typedef bool(__thiscall* tCScriptThing_MsgPerformedAnyAggressiveSpecialAbility)(CScriptThing* This);
typedef bool(__thiscall* tCScriptThing_MsgPerformedAnyNonAggressiveSpecialAbility)(CScriptThing* This);
typedef bool(__thiscall* tCScriptThing_MsgIsHitBySpecialAbilityFrom)(CScriptThing* This, EHeroAbility ability, const CCharString* pSourceName);
typedef bool(__thiscall* tCScriptThing_MsgIsHitByAnySpecialAbilityFrom)(CScriptThing* This, const CCharString* pSourceName);
typedef bool(__thiscall* tCScriptThing_MsgIsHitByAnyAggressiveSpecialAbilityFrom)(CScriptThing* This, const CCharString* pSourceName);
typedef bool(__thiscall* tCScriptThing_MsgIsHitByAnyNonAggressiveSpecialAbilityFrom)(CScriptThing* This, const CCharString* pSourceName);
typedef bool(__thiscall* tCScriptThing_MsgOpenedChest)(CScriptThing* This);
typedef bool(__thiscall* tCScriptThing_MsgHitEnemyWithMeleeWeapon)(CScriptThing* This);
typedef bool(__thiscall* tCScriptThing_MsgHitFriendWithMeleeWeapon)(CScriptThing* This);
typedef bool(__thiscall* tCScriptThing_MsgHitEnemyWithRangedWeapon)(CScriptThing* This);
typedef bool(__thiscall* tCScriptThing_MsgHitFriendWithRangedWeapon)(CScriptThing* This);
typedef bool(__thiscall* tCScriptThing_MsgHitEnemyWithBareHands)(CScriptThing* This);
typedef bool(__thiscall* tCScriptThing_MsgHitFriendWithBareHands)(CScriptThing* This);
typedef bool(__thiscall* tCScriptThing_MsgAttackedWithMeleeWeaponWithoutHittingAnything)(CScriptThing* This);
typedef bool(__thiscall* tCScriptThing_MsgAttackedWithBareHandsWithoutHittingAnything)(CScriptThing* This);
typedef bool(__thiscall* tCScriptThing_MsgTalkedToAnyone)(CScriptThing* This);
typedef bool(__thiscall* tCScriptThing_MsgGetThingsKilled)(CScriptThing* This, std::vector<unsigned long>* pOutUIDs);
typedef bool(__thiscall* tCScriptThing_MsgPerformedFlourish)(CScriptThing* This);
typedef bool(__thiscall* tCScriptThing_MsgPerformedSuccessfulFlourish)(CScriptThing* This);
typedef bool(__thiscall* tCScriptThing_MsgOnMoralityChanged)(CScriptThing* This, int* pOutNewMorality);
typedef bool(__thiscall* tCScriptThing_MsgOnCutSceneAnimEvent)(CScriptThing* This, CCharString* pOutEventName);
typedef bool(__thiscall* tCScriptThing_MsgIsKicked)(CScriptThing* This);
typedef bool(__thiscall* tCScriptThing_IsUnconscious)(CScriptThing* This);
typedef bool(__thiscall* tCScriptThing_IsUsable)(CScriptThing* This);
typedef bool(__thiscall* tCScriptThing_IsBeingCarriedBy)(CScriptThing* This, const CCharString* pCarrierName);
typedef bool(__thiscall* tCScriptThing_IsOpenDoor)(CScriptThing* This);
typedef bool(__thiscall* tCScriptThing_IsSummonedCreature)(CScriptThing* This);
typedef void(__thiscall* tCScriptThing_SetAsUsable)(CScriptThing* This, bool bIsUsable);
typedef void(__thiscall* tCScriptThing_SetFriendsWithEverythingFlag)(CScriptThing* This, bool bIsFriends);
typedef bool(__thiscall* tCScriptThing_GetActivationTriggerStatus)(CScriptThing* This);
typedef void(__thiscall* tCScriptThing_SetActivationTriggerStatus)(CScriptThing* This, bool bIsActive);
typedef void(__thiscall* tCScriptThing_SetToKillOnLevelUnload)(CScriptThing* This, bool bShouldKill);
typedef void(__thiscall* tCScriptThing_UpdateThingAttachment)(CScriptThing* This);
typedef void(__thiscall* tCScriptThing_IncrementScriptCounter)(CScriptThing* This);
typedef void(__thiscall* tCScriptThing_DecrementScriptCounter)(CScriptThing* This);
typedef int(__thiscall* tCScriptThing_GetScriptCounter)(CScriptThing* This);
typedef bool(__thiscall* tCScriptThing_IsAlive)(CScriptThing* This);
typedef bool(__thiscall* tCScriptThing_IsDead)(CScriptThing* This);
typedef bool(__thiscall* tCScriptThing_IsNull)(CScriptThing* This);
typedef bool(__thiscall* tCScriptThing_IsInputTypeWithButton)(CScriptThing* This, EXboxControllerButton button);

struct CScriptThingVTable
{
    tCScriptThing_Destructor                                    Destructor;                                             // 0x00 (Index 0)
    tCScriptThing_GetName                                       GetName;                                                // 0x04 (Index 1)
    tCScriptThing_GetDefName                                    GetDefName;                                             // 0x08 (Index 2)
    tCScriptThing_GetDataString                                 GetDataString;                                          // 0x0C (Index 3)
    tCScriptThing_SetDataString                                 SetDataString;                                          // 0x10 (Index 4)
    tCScriptThing_GetFocalPos                                   GetFocalPos;                                            // 0x14 (Index 5)
    tCScriptThing_GetPos                                        GetPos;                                                 // 0x18 (Index 6)
    tCScriptThing_GetHomePos                                    GetHomePos;                                             // 0x1C (Index 7)
    tCScriptThing_GetCurrentMapName                             GetCurrentMapName;                                      // 0x20 (Index 8)
    tCScriptThing_GetHomeMapName                                GetHomeMapName;                                         // 0x24 (Index 9)
    tCScriptThing_GetAngleXY                                    GetAngleXY;                                             // 0x28 (Index 10)
    tCScriptThing_GetPThing                                     GetPThing;                                              // 0x2C (Index 11)
    tCScriptThing_GetPThingUniqueID                             GetPThingUniqueID;                                      // 0x30 (Index 12)
    tCScriptThing_GetCurrentStateGroupType                      GetCurrentStateGroupType;                               // 0x34 (Index 13)
    tCScriptThing_GetCurrentScriptPriority                      GetCurrentScriptPriority;                               // 0x38 (Index 14)
    tCScriptThing_IsSneaking                                    IsSneaking;                                             // 0x3C (Index 15)
    tCScriptThing_IsAwareOfHero                                 IsAwareOfHero;                                          // 0x40 (Index 16)
    tCScriptThing_MsgIsTriggeredBy                              MsgIsTriggeredBy;                                       // 0x44 (Index 17)
    tCScriptThing_MsgIsKilledBy                                 MsgIsKilledBy;                                          // 0x48 (Index 18)
    tCScriptThing_MsgWhoKilledMe                                MsgWhoKilledMe;                                         // 0x4C (Index 19)
    tCScriptThing_MsgIsUsedBy                                   MsgIsUsedBy;                                            // 0x50 (Index 20)
    tCScriptThing_MsgIsHitBy                                    MsgIsHitBy;                                             // 0x54 (Index 21)
    tCScriptThing_MsgIsHitByWithFlourish                        MsgIsHitByWithFlourish;                                 // 0x58 (Index 22)
    tCScriptThing_MsgIsHitByWithDecapitate                      MsgIsHitByWithDecapitate;                               // 0x5C (Index 23)
    tCScriptThing_MsgIsHitByWithWeapon                          MsgIsHitByWithWeapon;                                   // 0x60 (Index 24)
    tCScriptThing_MsgIsHitByWithProjectileWeapon                MsgIsHitByWithProjectileWeapon;                         // 0x64 (Index 25)
    tCScriptThing_MsgWhoHitMe                                   MsgWhoHitMe;                                            // 0x68 (Index 26)
    tCScriptThing_MsgIsTalkedToBy                               MsgIsTalkedToBy;                                        // 0x6C (Index 27)
    tCScriptThing_MsgWhoTalkedToMe                              MsgWhoTalkedToMe;                                       // 0x70 (Index 28)
    tCScriptThing_MsgExpressionPerformedTo                      MsgExpressionPerformedTo;                               // 0x74 (Index 29)
    tCScriptThing_MsgWhoExpressedToMe                           MsgWhoExpressedToMe;                                    // 0x78 (Index 30)
    tCScriptThing_MsgHowLongWasExpressionPerformed              MsgHowLongWasExpressionPerformed;                       // 0x7C (Index 31)
    tCScriptThing_MsgIsKnockedOutBy                             MsgIsKnockedOutBy;                                      // 0x80 (Index 32)
    tCScriptThing_MsgWhoKnockedMeOut                            MsgWhoKnockedMeOut;                                     // 0x84 (Index 33)
    tCScriptThing_MsgReceivedMoney                              MsgReceivedMoney;                                       // 0x88 (Index 34)
    tCScriptThing_MsgIsPresentedWithItem                        MsgIsPresentedWithItem;                                 // 0x8C (Index 35)
    tCScriptThing_MsgReceivedInventoryItem                      MsgReceivedInventoryItem;                               // 0x90 (Index 36)
    tCScriptThing_MsgPerformedSpecialAbility                    MsgPerformedSpecialAbility;                             // 0x94 (Index 37)
    tCScriptThing_MsgPerformedAnySpecialAbility                 MsgPerformedAnySpecialAbility;                          // 0x98 (Index 38)
    tCScriptThing_MsgPerformedAnyAggressiveSpecialAbility       MsgPerformedAnyAggressiveSpecialAbility;                // 0x9C (Index 39)
    tCScriptThing_MsgPerformedAnyNonAggressiveSpecialAbility    MsgPerformedAnyNonAggressiveSpecialAbility;             // 0xA0 (Index 40)
    tCScriptThing_MsgIsHitBySpecialAbilityFrom                  MsgIsHitBySpecialAbilityFrom;                           // 0xA4 (Index 41)
    tCScriptThing_MsgIsHitByAnySpecialAbilityFrom               MsgIsHitByAnySpecialAbilityFrom;                        // 0xA8 (Index 42)
    tCScriptThing_MsgIsHitByAnyAggressiveSpecialAbilityFrom     MsgIsHitByAnyAggressiveSpecialAbilityFrom;              // 0xAC (Index 43)
    tCScriptThing_MsgIsHitByAnyNonAggressiveSpecialAbilityFrom  MsgIsHitByAnyNonAggressiveSpecialAbilityFrom;           // 0xB0 (Index 44)
    tCScriptThing_MsgOpenedChest                                MsgOpenedChest;                                         // 0xB4 (Index 45)
    tCScriptThing_MsgHitEnemyWithMeleeWeapon                    MsgHitEnemyWithMeleeWeapon;                             // 0xB8 (Index 46)
    tCScriptThing_MsgHitFriendWithMeleeWeapon                   MsgHitFriendWithMeleeWeapon;                            // 0xBC (Index 47)
    tCScriptThing_MsgHitEnemyWithRangedWeapon                   MsgHitEnemyWithRangedWeapon;                            // 0xC0 (Index 48)
    tCScriptThing_MsgHitFriendWithRangedWeapon                  MsgHitFriendWithRangedWeapon;                           // 0xC4 (Index 49)
    tCScriptThing_MsgHitEnemyWithBareHands                      MsgHitEnemyWithBareHands;                               // 0xC8 (Index 50)
    tCScriptThing_MsgHitFriendWithBareHands                     MsgHitFriendWithBareHands;                              // 0xCC (Index 51)
    tCScriptThing_MsgAttackedWithMeleeWeaponWithoutHittingAnything MsgAttackedWithMeleeWeaponWithoutHittingAnything;    // 0xD0 (Index 52)
    tCScriptThing_MsgAttackedWithBareHandsWithoutHittingAnything MsgAttackedWithBareHandsWithoutHittingAnything;        // 0xD4 (Index 53)
    tCScriptThing_MsgTalkedToAnyone                             MsgTalkedToAnyone;                                      // 0xD8 (Index 54)
    tCScriptThing_MsgGetThingsKilled                            MsgGetThingsKilled;                                     // 0xDC (Index 55)
    tCScriptThing_MsgPerformedFlourish                          MsgPerformedFlourish;                                   // 0xE0 (Index 56)
    tCScriptThing_MsgPerformedSuccessfulFlourish                MsgPerformedSuccessfulFlourish;                         // 0xE4 (Index 57)
    tCScriptThing_MsgOnMoralityChanged                          MsgOnMoralityChanged;                                   // 0xE8 (Index 58)
    tCScriptThing_MsgOnCutSceneAnimEvent                        MsgOnCutSceneAnimEvent;                                 // 0xEC (Index 59)
    tCScriptThing_MsgIsKicked                                   MsgIsKicked;                                            // 0xF0 (Index 60)
    tCScriptThing_IsUnconscious                                 IsUnconscious;                                          // 0xF4 (Index 61)
    tCScriptThing_IsUsable                                      IsUsable;                                               // 0xF8 (Index 62)
    tCScriptThing_IsBeingCarriedBy                              IsBeingCarriedBy;                                       // 0xFC (Index 63)
    tCScriptThing_IsOpenDoor                                    IsOpenDoor;                                             // 0x100 (Index 64)
    tCScriptThing_IsSummonedCreature                            IsSummonedCreature;                                     // 0x104 (Index 65)
    tCScriptThing_SetAsUsable                                   SetAsUsable;                                            // 0x108 (Index 66)
    tCScriptThing_SetFriendsWithEverythingFlag                  SetFriendsWithEverythingFlag;                           // 0x10C (Index 67)
    tCScriptThing_GetActivationTriggerStatus                    GetActivationTriggerStatus;                             // 0x110 (Index 68)
    tCScriptThing_SetActivationTriggerStatus                    SetActivationTriggerStatus;                             // 0x114 (Index 69)
    tCScriptThing_SetToKillOnLevelUnload                        SetToKillOnLevelUnload;                                 // 0x118 (Index 70)
    tCScriptThing_UpdateThingAttachment                         UpdateThingAttachment;                                  // 0x11C (Index 71)
    tCScriptThing_IncrementScriptCounter                        IncrementScriptCounter;                                 // 0x120 (Index 72)
    tCScriptThing_DecrementScriptCounter                        DecrementScriptCounter;                                 // 0x124 (Index 73)
    tCScriptThing_GetScriptCounter                              GetScriptCounter;                                       // 0x128 (Index 74)
    tCScriptThing_IsAlive                                       IsAlive;                                                // 0x12C (Index 75)
    tCScriptThing_IsDead                                        IsDead;                                                 // 0x130 (Index 76)
    tCScriptThing_IsNull                                        IsNull;                                                 // 0x134 (Index 77)
    tCScriptThing_IsInputTypeWithButton                         IsInputTypeWithButton;                                  // 0x138 (Index 78)
};

struct CScriptGameResourceObjectScriptedThingBaseVTable {
    void(__thiscall* Destructor)(CScriptGameResourceObjectScriptedThingBase* This);                                                                                           // 0x00 (Index 0)
    void(__thiscall* Validate)(CScriptGameResourceObjectScriptedThingBase* This);                                                                                             // 0x04 (Index 1)
    void(__thiscall* SummonerLightningOrbAttackTarget)(CScriptGameResourceObjectScriptedThingBase* This, const CScriptThing* pTarget);                                       // 0x08 (Index 2)
    void(__thiscall* FireProjectileWeaponAtTarget)(CScriptGameResourceObjectScriptedThingBase* This, const CScriptThing* pTarget);                                           // 0x0C (Index 3)
    void(__thiscall* MoveToPosition)(CScriptGameResourceObjectScriptedThingBase* This, const C3DVector* pPos, float radius, EScriptEntityMoveType moveType, bool b1, bool b2); // 0x10 (Index 4)
    void(__thiscall* MoveToThing)(CScriptGameResourceObjectScriptedThingBase* This, const CScriptThing* pThing, float radius, EScriptEntityMoveType moveType, bool b1, bool b2, bool b3, bool b4); // 0x14 (Index 5)
    void(__thiscall* FollowPreCalculatedRoute)(CScriptGameResourceObjectScriptedThingBase* This, const CScriptThing* pRoute, EScriptEntityMoveType moveType, bool b1, bool b2); // 0x18 (Index 6)
    void(__thiscall* FollowThing)(CScriptGameResourceObjectScriptedThingBase* This, const CScriptThing* pThing, float distance, bool b1);                                    // 0x1C (Index 7)
    void(__thiscall* StopFollowingThing)(CScriptGameResourceObjectScriptedThingBase* This, const CScriptThing* pThing);                                                      // 0x20 (Index 8)
    bool(__thiscall* IsFollowActionRunning)(CScriptGameResourceObjectScriptedThingBase* This, const CScriptThing* pThing);                                                   // 0x24 (Index 9)
    void(__thiscall* ClearCommands)(CScriptGameResourceObjectScriptedThingBase* This);                                                                                       // 0x28 (Index 10)
    void(__thiscall* WaitWhilePerformingTasks)(CScriptGameResourceObjectScriptedThingBase* This);                                                                            // 0x2C (Index 11)
    CScriptThing* (__thiscall* GetScriptThing)(CScriptGameResourceObjectScriptedThingBase* This, CScriptThing* pResult);                                                      // 0x30 (Index 12)
    void(__thiscall* Speak)(CScriptGameResourceObjectScriptedThingBase* This, const CScriptThing* pTarget, const char* dialogueKey, ETextGroupSelectionMethod method, bool b1, bool b2, bool b3); // 0x34 (Index 13)
    void(__thiscall* SpeakWithID)(CScriptGameResourceObjectScriptedThingBase* This, const CScriptThing* pTarget, unsigned int dialogueID, ETextGroupSelectionMethod method, bool b1, bool b2, bool b3); // 0x38 (Index 14)
    void(__thiscall* AskHeroQuestion)(CScriptGameResourceObjectScriptedThingBase* This, const CScriptThing* pHero, const char* questionKey, const CCharString* pOption1, const CCharString* pOption2, const CCharString* pOption3, bool b1, bool b2); // 0x3C (Index 15)
    void(__thiscall* Converse)(CScriptGameResourceObjectScriptedThingBase* This, const CScriptGameResourceObjectScriptedThingBase* pOther, const char* dialogueKey);        // 0x40 (Index 16)
    void(__thiscall* PerformExpression)(CScriptGameResourceObjectScriptedThingBase* This, const CScriptThing* pTarget, const CCharString* pExpressionName);                 // 0x44 (Index 17)
    void(__thiscall* PlayAnimation)(CScriptGameResourceObjectScriptedThingBase* This, const CCharString* pAnimName, bool b1, bool b2, bool b3, bool b4, bool b5, bool b6, bool b7); // 0x48 (Index 18)
    void(__thiscall* PlayCombatAnimation)(CScriptGameResourceObjectScriptedThingBase* This, const CCharString* pAnimName, bool b1, bool b2, bool b3, bool b4, bool b5, bool b6); // 0x4C (Index 19)
    void(__thiscall* PlayLoopingAnimation)(CScriptGameResourceObjectScriptedThingBase* This, const CCharString* pAnimName, int loopCount, bool b1, bool b2, bool b3, bool b4, bool b5, bool b6, bool b7); // 0x50 (Index 20)
    void(__thiscall* ClearAllActions)(CScriptGameResourceObjectScriptedThingBase* This);                                                                                     // 0x54 (Index 21)
    void(__thiscall* ClearAllActionsIncludingLoopingAnimations)(CScriptGameResourceObjectScriptedThingBase* This);                                                          // 0x58 (Index 22)
    void(__thiscall* MoveToAndPickUpGenericBox)(CScriptGameResourceObjectScriptedThingBase* This, CScriptThing* pBox, EScriptEntityMoveType moveType, bool b1);            // 0x5C (Index 23)
    void(__thiscall* DropGenericBox)(CScriptGameResourceObjectScriptedThingBase* This);                                                                                      // 0x60 (Index 24)
    void(__thiscall* UnsheatheWeapons)(CScriptGameResourceObjectScriptedThingBase* This);                                                                                    // 0x64 (Index 25)
    bool(__thiscall* IsPerformingScriptTask)(CScriptGameResourceObjectScriptedThingBase* This);                                                                             // 0x68 (Index 26)
    bool(__thiscall* IsFollowingThing)(CScriptGameResourceObjectScriptedThingBase* This);                                                                                    // 0x6C (Index 27)
    void(__thiscall* WaitForEntityToFinishPerformingTasks)(CScriptGameResourceObjectScriptedThingBase* This, const CScriptGameResourceObjectScriptedThingBase* pOther);    // 0x70 (Index 28)
    void(__thiscall* Wait)(CScriptGameResourceObjectScriptedThingBase* This, float seconds);                                                                                 // 0x74 (Index 29)
};

struct CGameScriptThing : CScriptThing
{

    CCountedPointer<CThing> PThing;         // 0x0C
    int ScriptCounter;                      // 0x14
    unsigned __int64 ThingUID;              // 0x18
    CCharString Name;                       // 0x20
    CCharString DataString;                 // 0x24
    C3DVector Pos;                          // 0x28
    C3DVector HomePos;                      // 0x34
    float AngleXY;                          // 0x40
    CWorld* World;                          // 0x44
};

typedef void(__thiscall* tCGameScriptThing_Destructor)(CGameScriptThing* This);
typedef const CCharString* (__thiscall* tCGameScriptThing_GetName)(CGameScriptThing* This);
typedef CCharString* (__thiscall* tCGameScriptThing_GetDefName)(CGameScriptThing* This, CCharString* pResult);
typedef CCharString* (__thiscall* tCGameScriptThing_GetDataString)(CGameScriptThing* This, CCharString* pResult);
typedef void(__thiscall* tCGameScriptThing_SetDataString)(CGameScriptThing* This, const CCharString* pDataString);
typedef C3DVector* (__thiscall* tCGameScriptThing_GetFocalPos)(CGameScriptThing* This, C3DVector* pResult);
typedef const C3DVector* (__thiscall* tCGameScriptThing_GetPos)(CGameScriptThing* This);
typedef C3DVector* (__thiscall* tCGameScriptThing_GetHomePos)(CGameScriptThing* This, C3DVector* pResult);
typedef CCharString* (__thiscall* tCGameScriptThing_GetCurrentMapName)(CGameScriptThing* This, CCharString* pResult);
typedef CCharString* (__thiscall* tCGameScriptThing_GetHomeMapName)(CGameScriptThing* This, CCharString* pResult);
typedef float(__thiscall* tCGameScriptThing_GetAngleXY)(CGameScriptThing* This);
typedef CThing* (__thiscall* tCGameScriptThing_GetPThing)(CGameScriptThing* This);
typedef unsigned __int64(__thiscall* tCGameScriptThing_GetPThingUniqueID)(CGameScriptThing* This);
typedef EAIStateGroupType(__thiscall* tCGameScriptThing_GetCurrentStateGroupType)(CGameScriptThing* This);
typedef EScriptAIPriority(__thiscall* tCGameScriptThing_GetCurrentScriptPriority)(CGameScriptThing* This);
typedef bool(__thiscall* tCGameScriptThing_IsSneaking)(CGameScriptThing* This);
typedef bool(__thiscall* tCGameScriptThing_IsAwareOfHero)(CGameScriptThing* This);
typedef bool(__thiscall* tCGameScriptThing_MsgIsTriggeredBy)(CGameScriptThing* This, const CCharString* pTriggerName);
typedef bool(__thiscall* tCGameScriptThing_MsgIsKilledBy)(CGameScriptThing* This, const CCharString* pKillerName);
typedef CScriptThing* (__thiscall* tCGameScriptThing_MsgWhoKilledMe)(CGameScriptThing* This, CScriptThing* pResult);
typedef bool(__thiscall* tCGameScriptThing_MsgIsUsedBy)(CGameScriptThing* This, const CCharString* pUserName);
typedef bool(__thiscall* tCGameScriptThing_MsgIsHitBy)(CGameScriptThing* This, const CCharString* pHitterName);
typedef bool(__thiscall* tCGameScriptThing_MsgIsHitByWithFlourish)(CGameScriptThing* This, const CCharString* pHitterName);
typedef bool(__thiscall* tCGameScriptThing_MsgIsHitByWithDecapitate)(CGameScriptThing* This, const CCharString* pHitterName);
typedef bool(__thiscall* tCGameScriptThing_MsgIsHitByWithWeapon)(CGameScriptThing* This, const CCharString* pHitterName, const CCharString* pWeaponName);
typedef bool(__thiscall* tCGameScriptThing_MsgIsHitByWithProjectileWeapon)(CGameScriptThing* This, const CCharString* pHitterName, float* pOutDamage);
typedef CScriptThing* (__thiscall* tCGameScriptThing_MsgWhoHitMe)(CGameScriptThing* This, CScriptThing* pResult);
typedef bool(__thiscall* tCGameScriptThing_MsgIsTalkedToBy)(CGameScriptThing* This, const CCharString* pTalkerName);
typedef CScriptThing* (__thiscall* tCGameScriptThing_MsgWhoTalkedToMe)(CGameScriptThing* This, CScriptThing* pResult);
typedef bool(__thiscall* tCGameScriptThing_MsgExpressionPerformedTo)(CGameScriptThing* This, CCharString* pOutExpressionName);
typedef CScriptThing* (__thiscall* tCGameScriptThing_MsgWhoExpressedToMe)(CGameScriptThing* This, CScriptThing* pResult);
typedef int(__thiscall* tCGameScriptThing_MsgHowLongWasExpressionPerformed)(CGameScriptThing* This);
typedef bool(__thiscall* tCGameScriptThing_MsgIsKnockedOutBy)(CGameScriptThing* This, const CCharString* pName);
typedef CScriptThing* (__thiscall* tCGameScriptThing_MsgWhoKnockedMeOut)(CGameScriptThing* This, CScriptThing* pResult);
typedef bool(__thiscall* tCGameScriptThing_MsgReceivedMoney)(CGameScriptThing* This, int* pOutAmount);
typedef bool(__thiscall* tCGameScriptThing_MsgIsPresentedWithItem)(CGameScriptThing* This, CCharString* pOutItemName);
typedef bool(__thiscall* tCGameScriptThing_MsgReceivedInventoryItem)(CGameScriptThing* This);
typedef bool(__thiscall* tCGameScriptThing_MsgPerformedSpecialAbility)(CGameScriptThing* This, EHeroAbility ability);
typedef bool(__thiscall* tCGameScriptThing_MsgPerformedAnySpecialAbility)(CGameScriptThing* This);
typedef bool(__thiscall* tCGameScriptThing_MsgPerformedAnyAggressiveSpecialAbility)(CGameScriptThing* This);
typedef bool(__thiscall* tCGameScriptThing_MsgPerformedAnyNonAggressiveSpecialAbility)(CGameScriptThing* This);
typedef bool(__thiscall* tCGameScriptThing_MsgIsHitBySpecialAbilityFrom)(CGameScriptThing* This, EHeroAbility ability, const CCharString* pSourceName);
typedef bool(__thiscall* tCGameScriptThing_MsgIsHitByAnySpecialAbilityFrom)(CGameScriptThing* This, const CCharString* pSourceName);
typedef bool(__thiscall* tCGameScriptThing_MsgIsHitByAnyAggressiveSpecialAbilityFrom)(CGameScriptThing* This, const CCharString* pSourceName);
typedef bool(__thiscall* tCGameScriptThing_MsgIsHitByAnyNonAggressiveSpecialAbilityFrom)(CGameScriptThing* This, const CCharString* pSourceName);
typedef bool(__thiscall* tCGameScriptThing_MsgOpenedChest)(CGameScriptThing* This);
typedef bool(__thiscall* tCGameScriptThing_MsgHitEnemyWithMeleeWeapon)(CGameScriptThing* This);
typedef bool(__thiscall* tCGameScriptThing_MsgHitFriendWithMeleeWeapon)(CGameScriptThing* This);
typedef bool(__thiscall* tCGameScriptThing_MsgHitEnemyWithRangedWeapon)(CGameScriptThing* This);
typedef bool(__thiscall* tCGameScriptThing_MsgHitFriendWithRangedWeapon)(CGameScriptThing* This);
typedef bool(__thiscall* tCGameScriptThing_MsgHitEnemyWithBareHands)(CGameScriptThing* This);
typedef bool(__thiscall* tCGameScriptThing_MsgHitFriendWithBareHands)(CGameScriptThing* This);
typedef bool(__thiscall* tCGameScriptThing_MsgAttackedWithMeleeWeaponWithoutHittingAnything)(CGameScriptThing* This);
typedef bool(__thiscall* tCGameScriptThing_MsgAttackedWithBareHandsWithoutHittingAnything)(CGameScriptThing* This);
typedef bool(__thiscall* tCGameScriptThing_MsgTalkedToAnyone)(CGameScriptThing* This);
typedef bool(__thiscall* tCGameScriptThing_MsgGetThingsKilled)(CGameScriptThing* This, std::vector<unsigned long>* pOutUIDs);
typedef bool(__thiscall* tCGameScriptThing_MsgPerformedFlourish)(CGameScriptThing* This);
typedef bool(__thiscall* tCGameScriptThing_MsgPerformedSuccessfulFlourish)(CGameScriptThing* This);
typedef bool(__thiscall* tCGameScriptThing_MsgOnMoralityChanged)(CGameScriptThing* This, int* pOutNewMorality);
typedef bool(__thiscall* tCGameScriptThing_MsgOnCutSceneAnimEvent)(CGameScriptThing* This, CCharString* pOutEventName);
typedef bool(__thiscall* tCGameScriptThing_MsgIsKicked)(CGameScriptThing* This);
typedef bool(__thiscall* tCGameScriptThing_IsUnconscious)(CGameScriptThing* This);
typedef bool(__thiscall* tCGameScriptThing_IsUsable)(CGameScriptThing* This);
typedef bool(__thiscall* tCGameScriptThing_IsBeingCarriedBy)(CGameScriptThing* This, const CCharString* pCarrierName);
typedef bool(__thiscall* tCGameScriptThing_IsOpenDoor)(CGameScriptThing* This);
typedef bool(__thiscall* tCGameScriptThing_IsSummonedCreature)(CGameScriptThing* This);
typedef void(__thiscall* tCGameScriptThing_SetAsUsable)(CGameScriptThing* This, bool bIsUsable);
typedef void(__thiscall* tCGameScriptThing_SetFriendsWithEverythingFlag)(CGameScriptThing* This, bool bIsFriends);
typedef bool(__thiscall* tCGameScriptThing_GetActivationTriggerStatus)(CGameScriptThing* This);
typedef void(__thiscall* tCGameScriptThing_SetActivationTriggerStatus)(CGameScriptThing* This, bool bIsActive);
typedef void(__thiscall* tCGameScriptThing_SetToKillOnLevelUnload)(CGameScriptThing* This, bool bShouldKill);
typedef void(__thiscall* tCGameScriptThing_UpdateThingAttachment)(CGameScriptThing* This);
typedef void(__thiscall* tCGameScriptThing_IncrementScriptCounter)(CGameScriptThing* This);
typedef void(__thiscall* tCGameScriptThing_DecrementScriptCounter)(CGameScriptThing* This);
typedef int(__thiscall* tCGameScriptThing_GetScriptCounter)(CGameScriptThing* This);
typedef bool(__thiscall* tCGameScriptThing_IsAlive)(CGameScriptThing* This);
typedef bool(__thiscall* tCGameScriptThing_IsDead)(CGameScriptThing* This);
typedef bool(__thiscall* tCGameScriptThing_IsNull)(CGameScriptThing* This);
typedef bool(__thiscall* tCGameScriptThing_IsEqualTo)(CGameScriptThing* This, const CScriptThing* pOther);

struct CGameScriptThingVTable
{
    tCGameScriptThing_Destructor                                    Destructor;                                             // 0x00
    tCGameScriptThing_GetName                                       GetName;                                                // 0x04
    tCGameScriptThing_GetDefName                                    GetDefName;                                             // 0x08
    tCGameScriptThing_GetDataString                                 GetDataString;                                          // 0x0C
    tCGameScriptThing_SetDataString                                 SetDataString;                                          // 0x10
    tCGameScriptThing_GetFocalPos                                   GetFocalPos;                                            // 0x14
    tCGameScriptThing_GetPos                                        GetPos;                                                 // 0x18
    tCGameScriptThing_GetHomePos                                    GetHomePos;                                             // 0x1C
    tCGameScriptThing_GetCurrentMapName                             GetCurrentMapName;                                      // 0x20
    tCGameScriptThing_GetHomeMapName                                GetHomeMapName;                                         // 0x24
    tCGameScriptThing_GetAngleXY                                    GetAngleXY;                                             // 0x28
    tCGameScriptThing_GetPThing                                     GetPThing;                                              // 0x2C
    tCGameScriptThing_GetPThingUniqueID                             GetPThingUniqueID;                                      // 0x30
    tCGameScriptThing_GetCurrentStateGroupType                      GetCurrentStateGroupType;                               // 0x34
    tCGameScriptThing_GetCurrentScriptPriority                      GetCurrentScriptPriority;                               // 0x38
    tCGameScriptThing_IsSneaking                                    IsSneaking;                                             // 0x3C
    tCGameScriptThing_IsAwareOfHero                                 IsAwareOfHero;                                          // 0x40
    tCGameScriptThing_MsgIsTriggeredBy                              MsgIsTriggeredBy;                                       // 0x44
    tCGameScriptThing_MsgIsKilledBy                                 MsgIsKilledBy;                                          // 0x48
    tCGameScriptThing_MsgWhoKilledMe                                MsgWhoKilledMe;                                         // 0x4C
    tCGameScriptThing_MsgIsUsedBy                                   MsgIsUsedBy;                                            // 0x50
    tCGameScriptThing_MsgIsHitBy                                    MsgIsHitBy;                                             // 0x54
    tCGameScriptThing_MsgIsHitByWithFlourish                        MsgIsHitByWithFlourish;                                 // 0x58
    tCGameScriptThing_MsgIsHitByWithDecapitate                      MsgIsHitByWithDecapitate;                               // 0x5C
    tCGameScriptThing_MsgIsHitByWithWeapon                          MsgIsHitByWithWeapon;                                   // 0x60
    tCGameScriptThing_MsgIsHitByWithProjectileWeapon                MsgIsHitByWithProjectileWeapon;                         // 0x64
    tCGameScriptThing_MsgWhoHitMe                                   MsgWhoHitMe;                                            // 0x68
    tCGameScriptThing_MsgIsTalkedToBy                               MsgIsTalkedToBy;                                        // 0x6C
    tCGameScriptThing_MsgWhoTalkedToMe                              MsgWhoTalkedToMe;                                       // 0x70
    tCGameScriptThing_MsgExpressionPerformedTo                      MsgExpressionPerformedTo;                               // 0x74
    tCGameScriptThing_MsgWhoExpressedToMe                           MsgWhoExpressedToMe;                                    // 0x78
    tCGameScriptThing_MsgHowLongWasExpressionPerformed              MsgHowLongWasExpressionPerformed;                       // 0x7C
    tCGameScriptThing_MsgIsKnockedOutBy                             MsgIsKnockedOutBy;                                      // 0x80
    tCGameScriptThing_MsgWhoKnockedMeOut                            MsgWhoKnockedMeOut;                                     // 0x84
    tCGameScriptThing_MsgReceivedMoney                              MsgReceivedMoney;                                       // 0x88
    tCGameScriptThing_MsgIsPresentedWithItem                        MsgIsPresentedWithItem;                                 // 0x8C
    tCGameScriptThing_MsgReceivedInventoryItem                      MsgReceivedInventoryItem;                               // 0x90
    tCGameScriptThing_MsgPerformedSpecialAbility                    MsgPerformedSpecialAbility;                             // 0x94
    tCGameScriptThing_MsgPerformedAnySpecialAbility                 MsgPerformedAnySpecialAbility;                          // 0x98
    tCGameScriptThing_MsgPerformedAnyAggressiveSpecialAbility       MsgPerformedAnyAggressiveSpecialAbility;                // 0x9C
    tCGameScriptThing_MsgPerformedAnyNonAggressiveSpecialAbility    MsgPerformedAnyNonAggressiveSpecialAbility;             // 0xA0
    tCGameScriptThing_MsgIsHitBySpecialAbilityFrom                  MsgIsHitBySpecialAbilityFrom;                           // 0xA4
    tCGameScriptThing_MsgIsHitByAnySpecialAbilityFrom               MsgIsHitByAnySpecialAbilityFrom;                        // 0xA8
    tCGameScriptThing_MsgIsHitByAnyAggressiveSpecialAbilityFrom     MsgIsHitByAnyAggressiveSpecialAbilityFrom;              // 0xAC
    tCGameScriptThing_MsgIsHitByAnyNonAggressiveSpecialAbilityFrom  MsgIsHitByAnyNonAggressiveSpecialAbilityFrom;           // 0xB0
    tCGameScriptThing_MsgOpenedChest                                MsgOpenedChest;                                         // 0xB4
    tCGameScriptThing_MsgHitEnemyWithMeleeWeapon                    MsgHitEnemyWithMeleeWeapon;                             // 0xB8
    tCGameScriptThing_MsgHitFriendWithMeleeWeapon                   MsgHitFriendWithMeleeWeapon;                            // 0xBC
    tCGameScriptThing_MsgHitEnemyWithRangedWeapon                   MsgHitEnemyWithRangedWeapon;                            // 0xC0
    tCGameScriptThing_MsgHitFriendWithRangedWeapon                  MsgHitFriendWithRangedWeapon;                           // 0xC4
    tCGameScriptThing_MsgHitEnemyWithBareHands                      MsgHitEnemyWithBareHands;                               // 0xC8
    tCGameScriptThing_MsgHitFriendWithBareHands                     MsgHitFriendWithBareHands;                              // 0xCC
    tCGameScriptThing_MsgAttackedWithMeleeWeaponWithoutHittingAnything MsgAttackedWithMeleeWeaponWithoutHittingAnything;    // 0xD0
    tCGameScriptThing_MsgAttackedWithBareHandsWithoutHittingAnything MsgAttackedWithBareHandsWithoutHittingAnything;        // 0xD4
    tCGameScriptThing_MsgTalkedToAnyone                             MsgTalkedToAnyone;                                      // 0xD8
    tCGameScriptThing_MsgGetThingsKilled                            MsgGetThingsKilled;                                     // 0xDC
    tCGameScriptThing_MsgPerformedFlourish                          MsgPerformedFlourish;                                   // 0xE0
    tCGameScriptThing_MsgPerformedSuccessfulFlourish                MsgPerformedSuccessfulFlourish;                         // 0xE4
    tCGameScriptThing_MsgOnMoralityChanged                          MsgOnMoralityChanged;                                   // 0xE8
    tCGameScriptThing_MsgOnCutSceneAnimEvent                        MsgOnCutSceneAnimEvent;                                 // 0xEC
    tCGameScriptThing_MsgIsKicked                                   MsgIsKicked;                                            // 0xF0
    tCGameScriptThing_IsUnconscious                                 IsUnconscious;                                          // 0xF4
    tCGameScriptThing_IsUsable                                      IsUsable;                                               // 0xF8
    tCGameScriptThing_IsBeingCarriedBy                              IsBeingCarriedBy;                                       // 0xFC
    tCGameScriptThing_IsOpenDoor                                    IsOpenDoor;                                             // 0x100
    tCGameScriptThing_IsSummonedCreature                            IsSummonedCreature;                                     // 0x104
    tCGameScriptThing_SetAsUsable                                   SetAsUsable;                                            // 0x108
    tCGameScriptThing_SetFriendsWithEverythingFlag                  SetFriendsWithEverythingFlag;                           // 0x10C
    tCGameScriptThing_GetActivationTriggerStatus                    GetActivationTriggerStatus;                             // 0x110
    tCGameScriptThing_SetActivationTriggerStatus                    SetActivationTriggerStatus;                             // 0x114
    tCGameScriptThing_SetToKillOnLevelUnload                        SetToKillOnLevelUnload;                                 // 0x118
    tCGameScriptThing_UpdateThingAttachment                         UpdateThingAttachment;                                  // 0x11C
    tCGameScriptThing_IncrementScriptCounter                        IncrementScriptCounter;                                 // 0x120
    tCGameScriptThing_DecrementScriptCounter                        DecrementScriptCounter;                                 // 0x124
    tCGameScriptThing_GetScriptCounter                              GetScriptCounter;                                       // 0x128
    tCGameScriptThing_IsAlive                                       IsAlive;                                                // 0x12C
    tCGameScriptThing_IsDead                                        IsDead;                                                 // 0x130
    tCGameScriptThing_IsNull                                        IsNull;                                                 // 0x134
    tCGameScriptThing_IsEqualTo                                     IsEqualTo;                                              // 0x138
};

struct CScriptGameResourceObjectScriptedThing : CScriptGameResourceObjectScriptedThingBase
{

    CGameScriptInterface* ScriptInterface;  // 0x10
    CCountedPointer<CThing> PThing;             // 0x14
    int                     ScriptID;           // 0x1C
    bool                    NullResource;       // 0x20
    char                    padding[3];         // 0x21
    int                     ActionPriority;     // 0x24
};

typedef void(__thiscall* tCSGrost_Destructor)(void* This);
typedef void(__thiscall* tCSGrost_Validate)(void* This);
typedef void(__thiscall* tCSGrost_SummonerLightningOrbAttackTarget)(void* This, const CScriptThing* pTarget);
typedef void(__thiscall* tCSGrost_FireProjectileWeaponAtTarget)(void* This, const CScriptThing* pTarget);
typedef void(__thiscall* tCSGrost_MoveToPosition)(void* This, const C3DVector* pPos, float radius, EScriptEntityMoveType moveType, bool b1, bool b2);
typedef void(__thiscall* tCSGrost_MoveToThing)(void* This,const CScriptThing* target,float proximity,EScriptEntityMoveType move_type,CTCScriptedControl* wait,bool avoid_dynamic_obstacles,bool ignore_path_preferability,bool face_movement); 
typedef void(__thiscall* tCSGrost_FollowPreCalculatedRoute)(void* This, const CScriptThing* pRoute, EScriptEntityMoveType moveType, bool b1, bool b2);
typedef void(__thiscall* tCSGrost_FollowThing)(void* This, const CScriptThing* pThing, float distance, bool b1);
typedef void(__thiscall* tCSGrost_StopFollowingThing)(void* This, const CScriptThing* pThing);
typedef bool(__thiscall* tCSGrost_IsFollowActionRunning)(void* This, const CScriptThing* pThing);
typedef void(__thiscall* tCSGrost_ClearCommands)(void* This);
typedef void(__thiscall* tCSGrost_WaitWhilePerformingTasks)(void* This);
typedef CScriptThing* (__thiscall* tCSGrost_GetScriptThing)(void* This, CScriptThing* pResult);
typedef void(__thiscall* tCSGrost_Speak)(void* This, const CScriptThing* pTarget, const char* dialogueKey, ETextGroupSelectionMethod method, bool b1, bool b2, bool b3);
typedef void(__thiscall* tCSGrost_SpeakWithID)(void* This, const CScriptThing* pTarget, unsigned int dialogueID, ETextGroupSelectionMethod method, bool b1, bool b2, bool b3);
typedef void(__thiscall* tCSGrost_AskHeroQuestion)(void* This, const CScriptThing* pHero, const char* questionKey, const CCharString* pOption1, const CCharString* pOption2, const CCharString* pOption3, bool b1, bool b2);
typedef void(__thiscall* tCSGrost_Converse)(void* This, const CScriptGameResourceObjectScriptedThingBase* pOther, const char* dialogueKey);
typedef void(__thiscall* tCSGrost_PerformExpression)(void* This, const CScriptThing* pTarget, const CCharString* pExpressionName);
typedef void(__thiscall* tCSGrost_PlayAnimation)(void* This,const CCharString* anim,bool stay_on_last_frame,CTCScriptedControl* overwrite_existing_actions,bool add_as_queued_action,bool wait_for_anim_to_finish,bool use_physics,bool anim_may_need_camera_position_updated,bool allow_looking);
typedef void(__thiscall* tCSGrost_PlayCombatAnimation)(void* This,const CCharString* pAnimName,bool use_physics,void* overwrite_existing_actions,bool add_as_queued_action,bool wait_for_anim_to_finish,bool anim_may_need_camera_position_updated,bool allow_looking);
typedef void(__thiscall* tCSGrost_PlayLoopingAnimation)(void* This,const CCharString* pAnimName,int num_loops,bool use_movement,void* overwrite_existing_actions,bool add_as_queued_action,bool wait_for_anim_to_finish,bool use_physics,bool anim_may_need_camera_position_updated,bool allow_looking);
typedef void(__thiscall* tCSGrost_ClearAllActions)(void* This);
typedef void(__thiscall* tCSGrost_ClearAllActionsIncludingLoopingAnimations)(void* This);
typedef void(__thiscall* tCSGrost_MoveToAndPickUpGenericBox)(void* This,CScriptThing* pBox,EScriptEntityMoveType moveType,BOOL avoid_dynamic_obstacles);
typedef void(__thiscall* tCSGrost_DropGenericBox)(void* This);
typedef void(__thiscall* tCSGrost_UnsheatheWeapons)(void* This);
typedef bool(__thiscall* tCSGrost_IsPerformingScriptTask)(void* This);
typedef bool(__thiscall* tCSGrost_IsFollowingThing)(void* This);
typedef void(__thiscall* tCSGrost_WaitForEntityToFinishPerformingTasks)(void* This, const CScriptGameResourceObjectScriptedThingBase* pOther);
typedef void(__thiscall* tCSGrost_Wait)(void* This, float seconds);
typedef bool(__thiscall* tCSGrost_IsNull)(void* This);

struct CScriptGameResourceObjectScriptedThingVTable {
    tCSGrost_Destructor                                   Destructor;                                       // 0x00
    tCSGrost_Validate                                     Validate;                                         // 0x04
    tCSGrost_SummonerLightningOrbAttackTarget             SummonerLightningOrbAttackTarget;                 // 0x08
    tCSGrost_FireProjectileWeaponAtTarget                 FireProjectileWeaponAtTarget;                     // 0x0C
    tCSGrost_MoveToPosition                               MoveToPosition;                                   // 0x10
    tCSGrost_MoveToThing                                  MoveToThing;                                      // 0x14
    tCSGrost_FollowPreCalculatedRoute                     FollowPreCalculatedRoute;                         // 0x18
    tCSGrost_FollowThing                                  FollowThing;                                      // 0x1C
    tCSGrost_StopFollowingThing                           StopFollowingThing;                               // 0x20
    tCSGrost_IsFollowActionRunning                        IsFollowActionRunning;                            // 0x24
    tCSGrost_ClearCommands                                ClearCommands;                                    // 0x28
    tCSGrost_WaitWhilePerformingTasks                     WaitWhilePerformingTasks;                         // 0x2C
    tCSGrost_GetScriptThing                               GetScriptThing;                                   // 0x30
    tCSGrost_Speak                                        Speak;                                            // 0x34
    tCSGrost_SpeakWithID                                  SpeakWithID;                                      // 0x38
    tCSGrost_AskHeroQuestion                              AskHeroQuestion;                                  // 0x3C
    tCSGrost_Converse                                     Converse;                                         // 0x40
    tCSGrost_PerformExpression                            PerformExpression;                                // 0x44
    tCSGrost_PlayAnimation                                PlayAnimation;                                    // 0x48
    tCSGrost_PlayCombatAnimation                          PlayCombatAnimation;                              // 0x4C
    tCSGrost_PlayLoopingAnimation                         PlayLoopingAnimation;                             // 0x50
    tCSGrost_ClearAllActions                              ClearAllActions;                                  // 0x54
    tCSGrost_ClearAllActionsIncludingLoopingAnimations    ClearAllActionsIncludingLoopingAnimations;        // 0x58
    tCSGrost_MoveToAndPickUpGenericBox                    MoveToAndPickUpGenericBox;                        // 0x5C
    tCSGrost_DropGenericBox                               DropGenericBox;                                   // 0x60
    tCSGrost_UnsheatheWeapons                             UnsheatheWeapons;                                 // 0x64
    tCSGrost_IsPerformingScriptTask                       IsPerformingScriptTask;                           // 0x68
    tCSGrost_IsFollowingThing                             IsFollowingThing;                                 // 0x6C
    tCSGrost_WaitForEntityToFinishPerformingTasks         WaitForEntityToFinishPerformingTasks;             // 0x70
    tCSGrost_Wait                                         Wait;                                             // 0x74
    tCSGrost_IsNull                                       IsNull;                                           // 0x78
};