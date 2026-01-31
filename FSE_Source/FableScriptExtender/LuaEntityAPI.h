#pragma once
#include "FableAPI.h"
#include "EntityScriptingAPI.h"
#include <string>
#include "sol/sol.hpp"


class CGameScriptInterfaceBase;

class LuaEntityAPI {
public:
    void SetGameInterface(CGameScriptInterfaceBase* pInterface);
    void AcquireControl(CScriptThing* pMe);
    void ReleaseControl();
    void MoveToThing_NonBlocking(CScriptThing* pMe, const std::shared_ptr<CScriptThing>& spTargetThing, float radius, int moveType);
    void PlayAnimation_NonBlocking(CScriptThing* pMe, const std::string& animName,sol::optional<bool> stayOnLastFrame, sol::optional<bool> allowLooking);
    void PlayLoopingAnimation_NonBlocking(CScriptThing* pMe, const std::string& animName, int loopCount, sol::optional<bool> useMovement, sol::optional<bool> allowLooking);
    void PlayCombatAnimation_NonBlocking(CScriptThing* pMe, const std::string& animName, sol::optional<bool> allowLooking);
    void MoveToPosition_NonBlocking(CScriptThing* pMe, sol::table position, float radius, int moveType);
    void FollowThing_NonBlocking(CScriptThing* pMe, const std::shared_ptr<CScriptThing>& spTargetThing, float distance, bool avoidObstacles);
    void ClearCommands_NonBlocking(CScriptThing* pMe);
    void StopFollowingThing_NonBlocking(CScriptThing* pMe, const std::shared_ptr<CScriptThing>& spTargetThing);
    void FollowPreCalculatedRoute_NonBlocking(CScriptThing* pMe, const std::shared_ptr<CScriptThing>& spRoute,int moveType, sol::optional<bool> avoidObstacles,sol::optional<bool> ignorePathPreferability);
    void MoveToAndPickUpGenericBox_NonBlocking(CScriptThing* pMe, CScriptThing* pBox, int moveType, sol::optional<bool> avoidObstacles);
    void DropGenericBox_NonBlocking(CScriptThing* pMe);
    void Wait_NonBlocking(CScriptThing* pMe, float seconds);
    void WaitForEntityToFinishPerformingTasks_NonBlocking(CScriptThing* pMe, const std::shared_ptr<CScriptThing>& spTargetEntity);
    void ClearAllActions_NonBlocking(CScriptThing* pMe);
    void ClearAllActionsIncludingLoopingAnimations_NonBlocking(CScriptThing* pMe);
    void UnsheatheWeapons_NonBlocking(CScriptThing* pMe);
    void SummonerLightningOrbAttackTarget_NonBlocking(CScriptThing* pMe, const std::shared_ptr<CScriptThing>& spTargetThing);
    void FireProjectileWeaponAtTarget_NonBlocking(CScriptThing* pMe, const std::shared_ptr<CScriptThing>& spTargetThing);
    void Speak_Blocking(CScriptThing* pMe, const std::shared_ptr<CScriptThing>& spTargetThing, const std::string& dialogueKey,int selectionMethod, bool makeTargetListen, bool soundIn2D, bool overScreenFade);
    void Converse_NonBlocking(CScriptThing* pMe, const std::shared_ptr<CScriptThing>& spTargetThing, const std::string& dialogueKey);
    void SpeakAndWait(CScriptThing* pMe, const std::string& dialogueKey, int selectionMethod = GROUP_SELECT_RANDOM_NO_REPEAT);
    void GainControlAndSpeak(CScriptThing* pMe, const std::string& dialogueKey, int selectionMethod = GROUP_SELECT_RANDOM_NO_REPEAT);
    void MakeBehavioral(CScriptThing* pMe);
    void SetReadableText(CScriptThing* pMe, const std::string& textTag);
    void TakeExclusiveControl(CScriptThing* pMe);
    void MoveToPosition(CScriptThing* pMe, sol::table position, float radius, int moveType);
    void MoveToThing(CScriptThing* pMe, const std::shared_ptr<CScriptThing>& spTargetThing, float radius, int moveType);
    void ClearCommands(CScriptThing* pMe);
    void PerformExpression(CScriptThing* pMe, const std::shared_ptr<CScriptThing>& spTargetThing, const std::string& expressionName);
    void PlayAnimation(CScriptThing* pMe, const std::string& animName, sol::optional<bool> waitForFinish, sol::optional<bool> stayOnLastFrame, sol::optional<bool> allowLooking);
    void PlayCombatAnimation(CScriptThing* pMe, const std::string& animName, sol::optional<bool> waitForFinish, sol::optional<bool> allowLooking);
    void PlayLoopingAnimation(CScriptThing* pMe, const std::string& animName, int loopCount, sol::optional<bool> waitForFinish, sol::optional<bool> useMovement, sol::optional<bool> allowLooking);
    void MoveToAndPickUpGenericBox(CScriptThing* pMe, CScriptThing* pBox, int moveType, sol::optional<bool> avoidObstacles);
    void DropGenericBox(CScriptThing* pMe);
    void UnsheatheWeapons(CScriptThing* pMe);
    void Wait(CScriptThing* pMe, float seconds);
    void FollowThing(CScriptThing* pMe, const std::shared_ptr<CScriptThing>& spTargetThing, float distance, bool avoidObstacles);
    void StopFollowingThing(CScriptThing* pMe, CScriptThing* pTargetThing);
    void SetAsUsable(CScriptThing* pMe, bool isUsable);
    void SetFriendsWithEverythingFlag(CScriptThing* pMe, bool isFriends);
    void SetActivationTriggerStatus(CScriptThing* pMe, bool isActive);
    void SetToKillOnLevelUnload(CScriptThing* pMe, bool shouldKill);

    bool IsFollowActionRunning_NonBlocking(CScriptThing* pMe, sol::object target_obj);
    bool IsPerformingScriptTask();
    bool IsTalkedToByHero(CScriptThing* pMe);
    bool IsNull(CScriptThing* pMe);
    bool IsKilledByHero(CScriptThing* pMe);
    bool IsAwareOfHero(CScriptThing* pMe);
    bool IsAlive(CScriptThing* pMe);
    bool IsDead(CScriptThing* pMe);
    bool IsFollowActionRunning(CScriptThing* pMe, CScriptThing* pTargetThing);
    bool IsFollowingThing(CScriptThing* pMe);
    bool IsUnconscious(CScriptThing* pMe);
    bool IsOpenDoor(CScriptThing* pMe);
    bool IsSneaking(CScriptThing* pMe);
    bool MsgIsHitByHeroWithFlourish(CScriptThing* pMe);
    bool MsgIsHitByHeroWithDecapitate(CScriptThing* pMe);
    bool MsgIsHitByHeroWithWeapon(CScriptThing* pMe, const std::string& weaponName);
    bool MsgIsUsedByHero(CScriptThing* pMe);
    bool MsgIsTriggeredByHero(CScriptThing* pMe);
    bool MsgIsKnockedOutByHero(CScriptThing* pMe);
    bool MsgPerformedSpecialAbility(CScriptThing* pMe, int abilityEnum);
    bool MsgPerformedAnySpecialAbility(CScriptThing* pMe);
    bool MsgPerformedAnyAggressiveSpecialAbility(CScriptThing* pMe);
    bool MsgPerformedAnyNonAggressiveSpecialAbility(CScriptThing* pMe);
    bool MsgReceivedInventoryItem(CScriptThing* pMe);
    bool MsgIsHitByHeroSpecialAbility(CScriptThing* pMe, int abilityEnum);
    bool MsgOpenedChest(CScriptThing* pMe);
    bool MsgIsKicked(CScriptThing* pMe);
    bool MsgIsPresentedWithItem(CScriptThing* pMe, sol::this_state s);
    bool MsgIsHitByHero(CScriptThing* pMe);
    bool MsgIsHitByAnySpecialAbilityFromHero(CScriptThing* pMe);
    bool MsgIsHitByHealLifeFromHero(CScriptThing* pMe);
    bool MsgIsKilledBy(CScriptThing* pMe, const std::string& killerName);

    std::string GetDataString(CScriptThing* pMe);
    std::string GetCurrentMapName(CScriptThing* pMe);
    std::string GetHomeMapName(CScriptThing* pMe);
    std::string GetDefName(CScriptThing* pMe);

    sol::table GetPos(CScriptThing* pMe, sol::this_state s);
    sol::table GetHomePos(CScriptThing* pMe, sol::this_state s);

    std::shared_ptr<CScriptThing> MsgWhoKilledMe(CScriptThing* pMe);
    std::shared_ptr<CScriptThing> MsgWhoHitMe(CScriptThing* pMe);

    sol::object MsgIsHitByHeroWithProjectileWeapon(CScriptThing* pMe, sol::this_state s);
    sol::object MsgExpressionPerformedTo(CScriptThing* pMe, sol::this_state s);
    sol::object MsgReceivedMoney(CScriptThing* pMe, sol::this_state s);

    int MsgHowLongWasExpressionPerformed(CScriptThing* pMe);

private:
    CScriptGameResourceObjectScriptedThingVTable* GetExpertVTable(CScriptThing* pMe);
    CGameScriptInterfaceBase* m_pGameInterface = nullptr;
    CScriptGameResourceObjectScriptedThingBase* m_pControlHandle = nullptr;
};