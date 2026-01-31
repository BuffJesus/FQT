#include "LuaManager.h"
#include "LuaQuestState.h" 
#include "FableAPI.h" 
#include "LuaEntityHost.h"
#include "LuaEntityAPI.h"
#include "GameInterface.h"
#include <sstream>

void ReleaseScriptThing(CScriptThing* thing) {
    if (!thing) return;

    if (thing->pImp.Info) {
        if (thing->pImp.Info->RefCount > 0) {
            thing->pImp.Info->RefCount--;

        }
        else {
            LogToFile("!!! WARNING: Tried to decrement RefCount for CScriptThing* 0x"
                + std::to_string(reinterpret_cast<uintptr_t>(thing))
                + " but it was already 0 during GC!");
        }
    }
    else {
        LogToFile("!!! WARNING: Releasing CScriptThing* 0x"
            + std::to_string(reinterpret_cast<uintptr_t>(thing))
            + " with no pImp.Info block during GC. Cannot decrement ref count.");
    }
}

void LuaManager::RegisterBindingsInState(sol::state& lua) {
    LogToFile("    [LuaManager] Registering all C++ bindings in new state...");

    auto entityAPI = std::make_shared<LuaEntityAPI>();

    entityAPI->SetGameInterface(g_pDSTGame ? *g_pDSTGame : nullptr);

    auto cscriptThing_type = lua.new_usertype<CScriptThing>("CScriptThing", sol::no_constructor);

    cscriptThing_type["AcquireControl"] = [entityAPI](CScriptThing* pMe) {entityAPI->AcquireControl(pMe);};
    cscriptThing_type["ReleaseControl"] = [entityAPI]() {entityAPI->ReleaseControl();};
    cscriptThing_type["IsPerformingScriptTask"] = [entityAPI]() {return entityAPI->IsPerformingScriptTask();};

    cscriptThing_type["GetDefName"] = [entityAPI](CScriptThing* pMe) {return entityAPI->GetDefName(pMe);};
    cscriptThing_type["GetHomePos"] = [entityAPI](CScriptThing* pMe, sol::this_state s) {return entityAPI->GetHomePos(pMe, s);};
    cscriptThing_type["GetCurrentMapName"] = [entityAPI](CScriptThing* pMe) {return entityAPI->GetCurrentMapName(pMe);};
    cscriptThing_type["GetHomeMapName"] = [entityAPI](CScriptThing* pMe) {return entityAPI->GetHomeMapName(pMe);};
    cscriptThing_type["IsSneaking"] = [entityAPI](CScriptThing* pMe) {return entityAPI->IsSneaking(pMe);};
    cscriptThing_type["MsgWhoKilledMe"] = [entityAPI](CScriptThing* pMe) {return entityAPI->MsgWhoKilledMe(pMe);};
    cscriptThing_type["MsgWhoHitMe"] = [entityAPI](CScriptThing* pMe) {return entityAPI->MsgWhoHitMe(pMe);};
    cscriptThing_type["MsgIsHitByHeroWithFlourish"] = [entityAPI](CScriptThing* pMe) {return entityAPI->MsgIsHitByHeroWithFlourish(pMe);};
    cscriptThing_type["MsgIsHitByHeroWithDecapitate"] = [entityAPI](CScriptThing* pMe) {return entityAPI->MsgIsHitByHeroWithDecapitate(pMe);};
    cscriptThing_type["MsgIsHitByHeroWithWeapon"] = [entityAPI](CScriptThing* pMe, const std::string& weaponName) {return entityAPI->MsgIsHitByHeroWithWeapon(pMe, weaponName);};
    cscriptThing_type["MsgIsHitByHeroWithProjectileWeapon"] = [entityAPI](CScriptThing* pMe, sol::this_state s) {return entityAPI->MsgIsHitByHeroWithProjectileWeapon(pMe, s);};
    cscriptThing_type["MsgIsUsedByHero"] = [entityAPI](CScriptThing* pMe) {return entityAPI->MsgIsUsedByHero(pMe);};
    cscriptThing_type["MsgIsTriggeredByHero"] = [entityAPI](CScriptThing* pMe) {return entityAPI->MsgIsTriggeredByHero(pMe);};
    cscriptThing_type["MsgIsKnockedOutByHero"] = [entityAPI](CScriptThing* pMe) {return entityAPI->MsgIsKnockedOutByHero(pMe);};
    cscriptThing_type["MsgPerformedSpecialAbility"] = [entityAPI](CScriptThing* pMe, int abilityEnum) {return entityAPI->MsgPerformedSpecialAbility(pMe, abilityEnum);};
    cscriptThing_type["MsgPerformedAnySpecialAbility"] = [entityAPI](CScriptThing* pMe) {return entityAPI->MsgPerformedAnySpecialAbility(pMe);};
    cscriptThing_type["MsgPerformedAnyAggressiveSpecialAbility"] = [entityAPI](CScriptThing* pMe) {return entityAPI->MsgPerformedAnyAggressiveSpecialAbility(pMe);};
    cscriptThing_type["MsgPerformedAnyNonAggressiveSpecialAbility"] = [entityAPI](CScriptThing* pMe) {return entityAPI->MsgPerformedAnyNonAggressiveSpecialAbility(pMe);};
    cscriptThing_type["MsgReceivedInventoryItem"] = [entityAPI](CScriptThing* pMe) {return entityAPI->MsgReceivedInventoryItem(pMe);};
    cscriptThing_type["MsgIsHitByHeroSpecialAbility"] = [entityAPI](CScriptThing* pMe, int abilityEnum) {return entityAPI->MsgIsHitByHeroSpecialAbility(pMe, abilityEnum);};
    cscriptThing_type["MsgOpenedChest"] = [entityAPI](CScriptThing* pMe) {return entityAPI->MsgOpenedChest(pMe);};
    cscriptThing_type["MsgIsKicked"] = [entityAPI](CScriptThing* pMe) {return entityAPI->MsgIsKicked(pMe);};
    cscriptThing_type["IsUnconscious"] = [entityAPI](CScriptThing* pMe) {return entityAPI->IsUnconscious(pMe);};
    cscriptThing_type["IsOpenDoor"] = [entityAPI](CScriptThing* pMe) {return entityAPI->IsOpenDoor(pMe);};
    cscriptThing_type["SetAsUsable"] = [entityAPI](CScriptThing* pMe, bool isUsable) {entityAPI->SetAsUsable(pMe, isUsable);};
    cscriptThing_type["SetFriendsWithEverythingFlag"] = [entityAPI](CScriptThing* pMe, bool isFriends) {entityAPI->SetFriendsWithEverythingFlag(pMe, isFriends);};
    cscriptThing_type["SetActivationTriggerStatus"] = [entityAPI](CScriptThing* pMe, bool isActive) {entityAPI->SetActivationTriggerStatus(pMe, isActive);};
    cscriptThing_type["SetToKillOnLevelUnload"] = [entityAPI](CScriptThing* pMe, bool shouldKill) {entityAPI->SetToKillOnLevelUnload(pMe, shouldKill);};

    cscriptThing_type["SpeakAndWait"] = sol::overload([entityAPI](CScriptThing* pMe, const std::string& key) {entityAPI->SpeakAndWait(pMe, key, GROUP_SELECT_RANDOM_NO_REPEAT);},[entityAPI](CScriptThing* pMe, const std::string& key, int method) {entityAPI->SpeakAndWait(pMe, key, method);});
    cscriptThing_type["IsTalkedToByHero"] = [entityAPI](CScriptThing* pMe) {return entityAPI->IsTalkedToByHero(pMe);};
    cscriptThing_type["IsKilledByHero"] = [entityAPI](CScriptThing* pMe) {return entityAPI->IsKilledByHero(pMe);};
    cscriptThing_type["GetDataString"] = [entityAPI](CScriptThing* pMe) {return entityAPI->GetDataString(pMe);};
    cscriptThing_type["TakeExclusiveControl"] = [entityAPI](CScriptThing* pMe) {entityAPI->TakeExclusiveControl(pMe);};
    cscriptThing_type["SetReadableText"] = [entityAPI](CScriptThing* pMe, const std::string& textTag) {entityAPI->SetReadableText(pMe, textTag);};
    cscriptThing_type["MakeBehavioral"] = [entityAPI](CScriptThing* pMe) {entityAPI->MakeBehavioral(pMe);};
    cscriptThing_type["MsgIsPresentedWithItem"] = [entityAPI](CScriptThing* pMe, sol::this_state s) {return entityAPI->MsgIsPresentedWithItem(pMe, s);};
    cscriptThing_type["MsgIsHitByHero"] = [entityAPI](CScriptThing* pMe) {return entityAPI->MsgIsHitByHero(pMe);};
    cscriptThing_type["MsgIsHitByAnySpecialAbilityFromHero"] = [entityAPI](CScriptThing* pMe) {return entityAPI->MsgIsHitByAnySpecialAbilityFromHero(pMe);};
    cscriptThing_type["MsgIsHitByHealLifeFromHero"] = [entityAPI](CScriptThing* pMe) {return entityAPI->MsgIsHitByHealLifeFromHero(pMe);};
    cscriptThing_type["IsNull"] = [entityAPI](CScriptThing* pMe) {return entityAPI->IsNull(pMe);};
    cscriptThing_type["GetPos"] = [entityAPI](CScriptThing* pMe, sol::this_state s) {return entityAPI->GetPos(pMe, s);};
    cscriptThing_type["MsgIsKilledBy"] = [entityAPI](CScriptThing* pMe, const std::string& killerName) {return entityAPI->MsgIsKilledBy(pMe, killerName);};
    cscriptThing_type["IsAwareOfHero"] = [entityAPI](CScriptThing* pMe) {return entityAPI->IsAwareOfHero(pMe);};
    cscriptThing_type["MsgExpressionPerformedTo"] = [entityAPI](CScriptThing* pMe, sol::this_state s) {return entityAPI->MsgExpressionPerformedTo(pMe, s);};
    cscriptThing_type["MsgHowLongWasExpressionPerformed"] = [entityAPI](CScriptThing* pMe) {return entityAPI->MsgHowLongWasExpressionPerformed(pMe);};
    cscriptThing_type["MsgReceivedMoney"] = [entityAPI](CScriptThing* pMe, sol::this_state s) {return entityAPI->MsgReceivedMoney(pMe, s);};

    cscriptThing_type["GainControlAndSpeak"] = sol::overload([entityAPI](CScriptThing* pMe, const std::string& key) {entityAPI->GainControlAndSpeak(pMe, key, GROUP_SELECT_RANDOM_NO_REPEAT); }, [entityAPI](CScriptThing* pMe, const std::string& key, int method) {entityAPI->GainControlAndSpeak(pMe, key, method); });
    cscriptThing_type["GainControlAndMoveToPosition"] = [entityAPI](CScriptThing* pMe, sol::table pos, float radius, sol::optional<int> moveType) {entityAPI->MoveToPosition(pMe, pos, radius, moveType.value_or(ENTITY_MOVE_WALK));};
    cscriptThing_type["GainControlAndMoveToThing"] = [entityAPI](CScriptThing* pMe, const std::shared_ptr<CScriptThing>& target, float radius, sol::optional<int> moveType) {entityAPI->MoveToThing(pMe, target, radius, moveType.value_or(ENTITY_MOVE_WALK));};
    cscriptThing_type["GainControlAndClearCommands"] = [entityAPI](CScriptThing* pMe) {entityAPI->ClearCommands(pMe);};
    cscriptThing_type["GainControlAndPerformExpression"] = sol::overload([entityAPI](CScriptThing* pMe, const std::string& exprName) {entityAPI->PerformExpression(pMe, nullptr, exprName);},[entityAPI](CScriptThing* pMe, const std::shared_ptr<CScriptThing>& target, const std::string& exprName) {entityAPI->PerformExpression(pMe, target, exprName);});
    cscriptThing_type["GainControlAndPlayAnimation"] = [entityAPI](CScriptThing* pMe, const std::string& animName, sol::optional<bool> waitForFinish, sol::optional<bool> stayOnLastFrame, sol::optional<bool> allowLooking) {entityAPI->PlayAnimation(pMe, animName, waitForFinish, stayOnLastFrame, allowLooking);};
    cscriptThing_type["GainControlAndPlayCombatAnimation"] = [entityAPI](CScriptThing* pMe, const std::string& animName, sol::optional<bool> waitForFinish, sol::optional<bool> allowLooking) {entityAPI->PlayCombatAnimation(pMe, animName, waitForFinish, allowLooking);};
    cscriptThing_type["GainControlAndPlayLoopingAnimation"] = [entityAPI](CScriptThing* pMe, const std::string& animName, int loopCount, sol::optional<bool> waitForFinish, sol::optional<bool> useMovement, sol::optional<bool> allowLooking) {entityAPI->PlayLoopingAnimation(pMe, animName, loopCount, waitForFinish, useMovement, allowLooking);};
    cscriptThing_type["GainControlAndMoveToAndPickUpGenericBox"] = [entityAPI](CScriptThing* pMe, CScriptThing* pBox, sol::optional<int> moveType, sol::optional<bool> avoidObstacles) {entityAPI->MoveToAndPickUpGenericBox(pMe, pBox, moveType.value_or(ENTITY_MOVE_WALK), avoidObstacles);};
    cscriptThing_type["GainControlAndDropGenericBox"] = [entityAPI](CScriptThing* pMe) {entityAPI->DropGenericBox(pMe);};
    cscriptThing_type["GainControlAndUnsheatheWeapons"] = [entityAPI](CScriptThing* pMe) {entityAPI->UnsheatheWeapons(pMe);};
    cscriptThing_type["GainControlAndWait"] = [entityAPI](CScriptThing* pMe, float seconds) {entityAPI->Wait(pMe, seconds);};

    cscriptThing_type["IsAlive"] = [entityAPI](CScriptThing* pMe) {return entityAPI->IsAlive(pMe);};
    cscriptThing_type["IsDead"] = [entityAPI](CScriptThing* pMe) {return entityAPI->IsDead(pMe);};
    cscriptThing_type["FollowThing"] = [entityAPI](CScriptThing* pMe, const std::shared_ptr<CScriptThing>& target, float distance, bool avoidObstacles) {entityAPI->FollowThing(pMe, target, distance, avoidObstacles);};
    cscriptThing_type["IsFollowActionRunning"] =
        [entityAPI](CScriptThing* pMe, sol::object target_obj) -> bool {
        if (!pMe) { return false; }
        CScriptThing* rawTarget = nullptr;
        if (target_obj.is<std::shared_ptr<CScriptThing>>()) {rawTarget = target_obj.as<std::shared_ptr<CScriptThing>>().get();}
        else if (target_obj.is<CScriptThing*>()) {rawTarget = target_obj.as<CScriptThing*>();}
        if (!rawTarget) { return false; }
        try {return entityAPI->IsFollowActionRunning(pMe, rawTarget);}
        catch (...) { return false; }};

    cscriptThing_type["StopFollowingThing"] =
        [entityAPI](CScriptThing* pMe, sol::object target_obj) {
        if (!pMe) { return; }
        CScriptThing* rawTarget = nullptr;
        if (target_obj.is<std::shared_ptr<CScriptThing>>()) {
            rawTarget = target_obj.as<std::shared_ptr<CScriptThing>>().get();}
        else if (target_obj.is<CScriptThing*>()) {rawTarget = target_obj.as<CScriptThing*>();}
        if (!rawTarget) { return; }
        try {entityAPI->StopFollowingThing(pMe, rawTarget);}
        catch (...) { }};

    cscriptThing_type["IsFollowingThing"] = [entityAPI](CScriptThing* pMe) -> bool {
        if (!pMe) { return false; }
        try {return entityAPI->IsFollowingThing(pMe);}
        catch (...) { return false; }};

    cscriptThing_type["MoveToThing"] = [entityAPI](CScriptThing* pMe, const std::shared_ptr<CScriptThing>& target, float radius, sol::optional<int> moveType) {
        entityAPI->MoveToThing_NonBlocking(pMe, target, radius, moveType.value_or(ENTITY_MOVE_WALK));};
    cscriptThing_type["PlayAnimation"] = [entityAPI](CScriptThing* pMe, const std::string& animName, sol::optional<bool> stayOnLastFrame, sol::optional<bool> allowLooking) {
        entityAPI->PlayAnimation_NonBlocking(pMe, animName, stayOnLastFrame, allowLooking);};
    cscriptThing_type["PlayLoopingAnimation"] = [entityAPI](CScriptThing* pMe, const std::string& animName, int loopCount, sol::optional<bool> useMovement, sol::optional<bool> allowLooking) {
        entityAPI->PlayLoopingAnimation_NonBlocking(pMe, animName, loopCount, useMovement, allowLooking);};
    cscriptThing_type["PlayCombatAnimation"] = [entityAPI](CScriptThing* pMe, const std::string& animName, sol::optional<bool> allowLooking) {
        entityAPI->PlayCombatAnimation_NonBlocking(pMe, animName, allowLooking);};
    cscriptThing_type["MoveToPosition"] = [entityAPI](CScriptThing* pMe, sol::table pos, float radius, sol::optional<int> moveType) {
        entityAPI->MoveToPosition_NonBlocking(pMe, pos, radius, moveType.value_or(ENTITY_MOVE_WALK));};
    cscriptThing_type["FollowThing"] = [entityAPI](CScriptThing* pMe, const std::shared_ptr<CScriptThing>& target, float distance, bool avoidObstacles) {
        entityAPI->FollowThing_NonBlocking(pMe, target, distance, avoidObstacles);};
    cscriptThing_type["ClearCommands"] = [entityAPI](CScriptThing* pMe) {
        entityAPI->ClearCommands_NonBlocking(pMe);};
    cscriptThing_type["StopFollowingThing"] = [entityAPI](CScriptThing* pMe, const std::shared_ptr<CScriptThing>& target) {
        entityAPI->StopFollowingThing_NonBlocking(pMe, target);};
    cscriptThing_type["MoveToAndPickUpGenericBox"] = [entityAPI](CScriptThing* pMe, CScriptThing* pBox, sol::optional<int> moveType, sol::optional<bool> avoidObstacles) {
        entityAPI->MoveToAndPickUpGenericBox_NonBlocking(pMe, pBox, moveType.value_or(ENTITY_MOVE_WALK), avoidObstacles);};
    cscriptThing_type["DropGenericBox"] = [entityAPI](CScriptThing* pMe) {
        entityAPI->DropGenericBox_NonBlocking(pMe);};
    cscriptThing_type["FollowPreCalculatedRoute"] = [entityAPI](CScriptThing* pMe, const std::shared_ptr<CScriptThing>& route, sol::optional<int> moveType, sol::optional<bool> avoidObstacles, sol::optional<bool> ignorePathPref) {
        entityAPI->FollowPreCalculatedRoute_NonBlocking(pMe, route, moveType.value_or(ENTITY_MOVE_WALK), avoidObstacles, ignorePathPref);};
    cscriptThing_type["IsFollowActionRunning"] = [entityAPI](CScriptThing* pMe, sol::object target) -> bool {return 
        entityAPI->IsFollowActionRunning_NonBlocking(pMe, target);};
    cscriptThing_type["Wait"] = [entityAPI](CScriptThing* pMe, float seconds) {
        entityAPI->Wait_NonBlocking(pMe, seconds);};
    cscriptThing_type["WaitForEntityToFinishPerformingTasks"] = [entityAPI](CScriptThing* pMe, const std::shared_ptr<CScriptThing>& targetEntity) {
        entityAPI->WaitForEntityToFinishPerformingTasks_NonBlocking(pMe, targetEntity);};
    cscriptThing_type["ClearAllActions"] = [entityAPI](CScriptThing* pMe) {
        entityAPI->ClearAllActions_NonBlocking(pMe);};
    cscriptThing_type["ClearAllActionsIncludingLoopingAnimations"] = [entityAPI](CScriptThing* pMe) {
        entityAPI->ClearAllActionsIncludingLoopingAnimations_NonBlocking(pMe);};
    cscriptThing_type["UnsheatheWeapons"] = [entityAPI](CScriptThing* pMe) {
        entityAPI->UnsheatheWeapons_NonBlocking(pMe);};
    cscriptThing_type["SummonerLightningOrbAttackTarget"] = [entityAPI](CScriptThing* pMe, const std::shared_ptr<CScriptThing>& target) {
        entityAPI->SummonerLightningOrbAttackTarget_NonBlocking(pMe, target);};
    cscriptThing_type["FireProjectileWeaponAtTarget"] = [entityAPI](CScriptThing* pMe, const std::shared_ptr<CScriptThing>& target) {
        entityAPI->FireProjectileWeaponAtTarget_NonBlocking(pMe, target);};
    cscriptThing_type["Speak"] = sol::overload(
        [entityAPI](CScriptThing* pMe, const std::shared_ptr<CScriptThing>& target, const std::string& key) {
            entityAPI->Speak_Blocking(pMe, target, key, GROUP_SELECT_RANDOM_NO_REPEAT, false, true, false);},
        [entityAPI](CScriptThing* pMe, const std::shared_ptr<CScriptThing>& target, const std::string& key, int method) {
            entityAPI->Speak_Blocking(pMe, target, key, method, false, true, false);},
        [entityAPI](CScriptThing* pMe, const std::shared_ptr<CScriptThing>& target, const std::string& key, int method, bool listen, bool sound2D, bool overFade) {
            entityAPI->Speak_Blocking(pMe, target, key, method, listen, sound2D, overFade);});
    cscriptThing_type["Converse"] = [entityAPI](CScriptThing* pMe, const std::shared_ptr<CScriptThing>& target, const std::string& key) {
        entityAPI->Converse_NonBlocking(pMe, target, key);};

    auto questState_type = lua.new_usertype<LuaQuestState>("Quest", sol::no_constructor);

    questState_type["NewScriptFrame"] = sol::overload(sol::resolve<bool()>(&LuaQuestState::NewScriptFrame),sol::resolve<bool(CScriptThing*)>(&LuaQuestState::NewScriptFrame));
    questState_type["Log"] = &LuaQuestState::Log;
    questState_type["CreateThread"] = sol::overload([](LuaQuestState& self, const std::string& func) {self.CreateThread(func, sol::lua_nil); }, [](LuaQuestState& self, const std::string& func, sol::table args) {self.CreateThread(func, args); });
    questState_type["GetHero"] = &LuaQuestState::GetHero;
    questState_type["CreateEffectAtPos"] = [](LuaQuestState& self, const std::string& effectName, sol::table position, sol::optional<float> angle, sol::optional<bool> independent, sol::optional<bool> alwaysUpdate) {self.CreateEffectAtPos(effectName, position, angle, independent, alwaysUpdate); };
    questState_type["CreateEffectOnThing"] = [](LuaQuestState& self, const std::string& effectName, CScriptThing* pTarget, const std::string& boneName, sol::optional<bool> independent, sol::optional<bool> alwaysUpdate) {self.CreateEffectOnThing(effectName, pTarget, boneName, independent, alwaysUpdate); };
    questState_type["ShowMessage"] = &LuaQuestState::ShowMessage;
    questState_type["ShowMessageWithButtons"] = &LuaQuestState::ShowMessageWithButtons;
    questState_type["AddQuestRegion"] = &LuaQuestState::AddQuestRegion;
    questState_type["IsRegionLoaded"] = &LuaQuestState::IsRegionLoaded;
    questState_type["ActivateQuest"] = &LuaQuestState::ActivateQuest;
    questState_type["AddQuestCard"] = &LuaQuestState::AddQuestCard;
    questState_type["GiveQuestCardDirectly"] = &LuaQuestState::GiveQuestCardDirectly;
    questState_type["SetQuestGoldReward"] = &LuaQuestState::SetQuestGoldReward;
    questState_type["SetQuestRenownReward"] = &LuaQuestState::SetQuestRenownReward;
    questState_type["SetQuestCardObjective"] = &LuaQuestState::SetQuestCardObjective;
    questState_type["KickOffQuestStartScreen"] = &LuaQuestState::KickOffQuestStartScreen;
    questState_type["GetActiveQuestName"] = &LuaQuestState::GetActiveQuestName;
    questState_type["DeactivateQuest"] = &LuaQuestState::DeactivateQuest;
    questState_type["Pause"] = &LuaQuestState::Pause;
    questState_type["PlayCutscene"] = &LuaQuestState::PlayCutscene;
    questState_type["FixMovieSequenceCamera"] = &LuaQuestState::FixMovieSequenceCamera;
    questState_type["AddEntityBinding"] = &LuaQuestState::AddEntityBinding;
    questState_type["FinalizeEntityBindings"] = &LuaQuestState::FinalizeEntityBindings;
    questState_type["SetCreatureGeneratorsEnabled"] = &LuaQuestState::SetCreatureGeneratorsEnabled;
    questState_type["IsObjectInHeroPossession"] = &LuaQuestState::IsObjectInHeroPossession;
    questState_type["GetThingWithScriptName"] = &LuaQuestState::GetThingWithScriptName;
    questState_type["GetThingWithUID"] = &LuaQuestState::GetThingWithUID;
    questState_type["GetThingWithScriptNameAtRegion"] = &LuaQuestState::GetThingWithScriptNameAtRegion;
    questState_type["CreateCreature"] = &LuaQuestState::CreateCreature;
    questState_type["EntitySetTargetable"] = &LuaQuestState::EntitySetTargetable;
    questState_type["EntityTeleportToThing"] = &LuaQuestState::EntityTeleportToThing;
    questState_type["GetHealth"] = &LuaQuestState::GetHealth;
    questState_type["EntitySetAsDrawable"] = &LuaQuestState::EntitySetAsDrawable;
    questState_type["GiveHeroObject"] = &LuaQuestState::GiveHeroObject;
    questState_type["IsDistanceBetweenThingsUnder"] = &LuaQuestState::IsDistanceBetweenThingsUnder;
    questState_type["RemoveThing"] = &LuaQuestState::RemoveThing;
    questState_type["CreateCreatureNearby"] = &LuaQuestState::CreateCreatureNearby;
    questState_type["EntitySetAllStategroupsEnabled"] = &LuaQuestState::EntitySetAllStategroupsEnabled;
    questState_type["EntitySetAsKillable"] = &LuaQuestState::EntitySetAsKillable;
    questState_type["EntitySetOpinionReactionMask"] = &LuaQuestState::EntitySetOpinionReactionMask;
    questState_type["SetThingHasInformation"] = &LuaQuestState::SetThingHasInformation;
    questState_type["EntitySetFacingAngleTowardsThing"] = &LuaQuestState::EntitySetFacingAngleTowardsThing;
    questState_type["GiveHeroYesNoQuestion"] = &LuaQuestState::GiveHeroYesNoQuestion;
    questState_type["TakeObjectFromHero"] = &LuaQuestState::TakeObjectFromHero;
    questState_type["GiveHeroGold"] = &LuaQuestState::GiveHeroGold;
    questState_type["GiveHeroMorality"] = &LuaQuestState::GiveHeroMorality;
    questState_type["GiveHeroExperience"] = &LuaQuestState::GiveHeroExperience;
    questState_type["ClearThingHasInformation"] = &LuaQuestState::ClearThingHasInformation;
    questState_type["FadeScreenOut"] = &LuaQuestState::FadeScreenOut;
    questState_type["FadeScreenIn"] = &LuaQuestState::FadeScreenIn;
    questState_type["SetQuestWorldMapOffset"] = &LuaQuestState::SetQuestWorldMapOffset;
    questState_type["HeroReceiveMessageFromGuildMaster"] = &LuaQuestState::HeroReceiveMessageFromGuildMaster;
    questState_type["IsLevelLoaded"] = &LuaQuestState::IsLevelLoaded;
    questState_type["OverrideMusic"] = &LuaQuestState::OverrideMusic;
    questState_type["RegisterTimer"] = &LuaQuestState::RegisterTimer;
    questState_type["SetTimer"] = &LuaQuestState::SetTimer;
    questState_type["GetTimer"] = &LuaQuestState::GetTimer;
    questState_type["GiveHeroTutorial"] = &LuaQuestState::GiveHeroTutorial;
    questState_type["SetQuestAsFailed"] = &LuaQuestState::SetQuestAsFailed;
    questState_type["StopOverrideMusic"] = &LuaQuestState::StopOverrideMusic;
    questState_type["ResetPlayerCreatureCombatMultiplier"] = &LuaQuestState::ResetPlayerCreatureCombatMultiplier;
    questState_type["SetQuestAsCompleted"] = &LuaQuestState::SetQuestAsCompleted;
    questState_type["DeactivateQuestLater"] = &LuaQuestState::DeactivateQuestLater;
    questState_type["AddRumourCategory"] = &LuaQuestState::AddRumourCategory;
    questState_type["AddNewRumourToCategory"] = &LuaQuestState::AddNewRumourToCategory;
    questState_type["AddGossipFactionToCategory"] = &LuaQuestState::AddGossipFactionToCategory;
    questState_type["GetThingWithUID"] = &LuaQuestState::GetThingWithUID;
    questState_type["GetAllThingsWithScriptName"] = &LuaQuestState::GetAllThingsWithScriptName;
    questState_type["EntitySetCutsceneBehaviour"] = &LuaQuestState::EntitySetCutsceneBehaviour;
    questState_type["CameraDefault"] = &LuaQuestState::CameraDefault;
    questState_type["GetHeroTargetedThing"] = &LuaQuestState::GetHeroTargetedThing;
    questState_type["GetNearestWithScriptName"] = &LuaQuestState::GetNearestWithScriptName;
    questState_type["GetFurthestWithScriptName"] = &LuaQuestState::GetFurthestWithScriptName;
    questState_type["GetAllThingsWithDefName"] = &LuaQuestState::GetAllThingsWithDefName;
    questState_type["GetNearestWithDefName"] = &LuaQuestState::GetNearestWithDefName;
    questState_type["GetFurthestWithDefName"] = &LuaQuestState::GetFurthestWithDefName;
    questState_type["EntitySetInFaction"] = &LuaQuestState::EntitySetInFaction;
    questState_type["EntityGetStockItemPrice"] = &LuaQuestState::EntityGetStockItemPrice;
    questState_type["EntitySetStockItemPrice"] = &LuaQuestState::EntitySetStockItemPrice;
    questState_type["EntitySetAsForSale"] = &LuaQuestState::EntitySetAsForSale;
    questState_type["CreateObject"] = sol::overload(&LuaQuestState::CreateObject, &LuaQuestState::CreateObjectWithRotation);
    questState_type["GiveHeroItem"] = &LuaQuestState::GiveHeroItem;
    questState_type["PlaySoundOnThing"] = &LuaQuestState::PlaySoundOnThing;
    questState_type["GetHeroAge"] = &LuaQuestState::GetHeroAge;
    questState_type["EntitySetAppearanceSeed"] = &LuaQuestState::EntitySetAppearanceSeed;
    questState_type["GetWorldFrame"] = &LuaQuestState::GetWorldFrame;
    questState_type["ShowOnScreenMessageWithFont"] = &LuaQuestState::ShowOnScreenMessageWithFont;
    questState_type["Play2DSound"] = &LuaQuestState::Play2DSound;
    questState_type["PlaySoundAtPos"] = &LuaQuestState::PlaySoundAtPos;

    questState_type["SetStateBool"] = &LuaQuestState::SetStateBool;
    questState_type["SetStateInt"] = &LuaQuestState::SetStateInt;
    questState_type["SetStateString"] = &LuaQuestState::SetStateString;
    questState_type["GetStateInt"] = &LuaQuestState::GetStateInt;
    questState_type["GetStateBool"] = &LuaQuestState::GetStateBool;
    questState_type["GetStateString"] = &LuaQuestState::GetStateString;
    questState_type["SetHeroGuideShowsQuestCards"] = &LuaQuestState::SetHeroGuideShowsQuestCards;

    questState_type["SetGlobalBool"] = &LuaQuestState::SetGlobalBool;
    questState_type["SetGlobalInt"] = &LuaQuestState::SetGlobalInt;
    questState_type["SetGlobalString"] = &LuaQuestState::SetGlobalString;
    questState_type["GetGlobalInt"] = &LuaQuestState::GetGlobalInt;
    questState_type["GetGlobalBool"] = &LuaQuestState::GetGlobalBool;
    questState_type["GetGlobalString"] = &LuaQuestState::GetGlobalString;

    questState_type["GetMasterGameState"] = &LuaQuestState::GetMasterGameState;
    questState_type["PersistTransferBool"] = &LuaQuestState::PersistTransferBool;
    questState_type["PersistTransferInt"] = &LuaQuestState::PersistTransferInt;
    questState_type["PersistTransferString"] = &LuaQuestState::PersistTransferString;
    questState_type["PersistTransferFloat"] = &LuaQuestState::PersistTransferFloat;
    questState_type["PersistTransferUInt"] = &LuaQuestState::PersistTransferUInt;

    questState_type["GetThingDebugInfo"] = &LuaQuestState::GetThingDebugInfo;
    questState_type["ReleaseThing"] = &LuaQuestState::ReleaseThing;
    questState_type["LogReceivedPointer"] = &LuaQuestState::LogReceivedPointer;

    questState_type["SetThingPersistent"] = &LuaQuestState::SetThingPersistent;
    questState_type["EntitySetAsDamageable"] = &LuaQuestState::EntitySetAsDamageable;
    questState_type["EntitySetAsToAddToComboMultiplierWhenHit"] = &LuaQuestState::EntitySetAsToAddToComboMultiplierWhenHit;
    questState_type["SetRegionExitAsActive"] = &LuaQuestState::SetRegionExitAsActive;
    questState_type["GetHeroFatness"] = &LuaQuestState::GetHeroFatness;
    questState_type["EntityFadeOut"] = &LuaQuestState::EntityFadeOut;
    questState_type["OpenDoor"] = &LuaQuestState::OpenDoor;
    questState_type["GetHeroGold"] = &LuaQuestState::GetHeroGold;
    questState_type["EntitySetThingAsEnemyOfThing"] = &LuaQuestState::EntitySetThingAsEnemyOfThing;
    questState_type["ChangeHeroHealthBy"] = &LuaQuestState::ChangeHeroHealthBy;
    questState_type["CameraShake"] = &LuaQuestState::CameraShake;
    questState_type["GetHeroHasMarried"] = &LuaQuestState::GetHeroHasMarried;
    questState_type["GetHeroHasMurderedWife"] = &LuaQuestState::GetHeroHasMurderedWife;
    questState_type["GetHeroRenownLevel"] = &LuaQuestState::GetHeroRenownLevel;
    questState_type["GetHeroTitle"] = &LuaQuestState::GetHeroTitle;
    questState_type["MsgOnHeroCastSpell"] = &LuaQuestState::MsgOnHeroCastSpell;
    questState_type["GiveHeroAbility"] = &LuaQuestState::GiveHeroAbility;

    questState_type["PauseAllNonScriptedEntities"] = &LuaQuestState::PauseAllNonScriptedEntities;
    questState_type["StartMovieSequence"] = &LuaQuestState::StartMovieSequence;
    questState_type["EndMovieSequence"] = &LuaQuestState::EndMovieSequence;
    questState_type["StartAmbientConversation"] = sol::overload([&](LuaQuestState& self, const std::shared_ptr<CScriptThing>& speaker, const std::shared_ptr<CScriptThing>& listener) {return self.StartAmbientConversation(speaker, listener, sol::nullopt, sol::nullopt);},&LuaQuestState::StartAmbientConversation);
    questState_type["AddLineToConversation"] = sol::overload(
        [&](LuaQuestState& self, int convoID, const std::string& key, const std::shared_ptr<CScriptThing>& speaker, const std::shared_ptr<CScriptThing>& listener) {
            self.AddLineToConversation(convoID, key, speaker, listener, sol::nullopt);},
        [&](LuaQuestState& self, int convoID, const std::string& key, const std::shared_ptr<CScriptThing>& speaker, const std::shared_ptr<CScriptThing>& listener, bool showSubtitle) {
            self.AddLineToConversation(convoID, key, speaker, listener, showSubtitle);});
    questState_type["IsConversationActive"] = &LuaQuestState::IsConversationActive;
    questState_type["RemoveConversation"] = sol::overload([&](LuaQuestState& self, int convoID) {self.RemoveConversation(convoID, sol::nullopt);},&LuaQuestState::RemoveConversation);

    questState_type["GetRegionName"] = &LuaQuestState::GetRegionName;
    questState_type["MsgIsLevelLoaded"] = &LuaQuestState::MsgIsLevelLoaded;
    questState_type["MsgIsLevelUnloaded"] = &LuaQuestState::MsgIsLevelUnloaded;
    questState_type["MsgOnQuestCompleted"] = &LuaQuestState::MsgOnQuestCompleted;
    questState_type["MsgOnQuestFailed"] = &LuaQuestState::MsgOnQuestFailed;
    questState_type["MsgOnQuestAccepted"] = &LuaQuestState::MsgOnQuestAccepted;
    questState_type["MsgOnHeroPickedPocket"] = &LuaQuestState::MsgOnHeroPickedPocket;
    questState_type["MsgOnHeroPickedLock"] = &LuaQuestState::MsgOnHeroPickedLock;
    questState_type["MsgOnFishingGameFinished"] = &LuaQuestState::MsgOnFishingGameFinished;
    questState_type["MsgOnTavernGameFinished"] = &LuaQuestState::MsgOnTavernGameFinished;
    questState_type["MsgOnHeroRewardedWithItemsFrom"] = &LuaQuestState::MsgOnHeroRewardedWithItemsFrom;
    questState_type["IsHeroAllowedHenchmenInCurrentRegion"] = &LuaQuestState::IsHeroAllowedHenchmenInCurrentRegion;
    questState_type["IsHeroAllowedHenchmenInRegion"] = &LuaQuestState::IsHeroAllowedHenchmenInRegion;
    questState_type["DontPopulateNextLoadedRegion"] = &LuaQuestState::DontPopulateNextLoadedRegion;
    questState_type["DisplayQuestInfo"] = [&](LuaQuestState& self, bool bDisplay) {self.DisplayQuestInfo(bDisplay); };
    questState_type["SetQuestInfoName"] = [&](LuaQuestState& self, const std::string& name) {self.SetQuestInfoName(name); };
    questState_type["SetQuestInfoText"] = [&](LuaQuestState& self, const std::string& text) {self.SetQuestInfoText(text); };
    questState_type["AddQuestInfoBar"] = &LuaQuestState::AddQuestInfoBar;
    questState_type["AddQuestInfoBarHealth"] = [&](LuaQuestState& self, const std::shared_ptr<CScriptThing>& spThing, sol::table color, const std::string& texture, float scale) {return self.AddQuestInfoBarHealth(spThing, color, texture, scale); };
    questState_type["AddQuestInfoCounter"] = &LuaQuestState::AddQuestInfoCounter;
    questState_type["UpdateQuestInfoCounter"] = &LuaQuestState::UpdateQuestInfoCounter;

    questState_type["AddScreenMessage"] = sol::overload([](LuaQuestState& self, const std::string& message) {self.AddScreenMessage(message, sol::nullopt);},sol::resolve<void(const std::string&, sol::optional<int>)>(&LuaQuestState::AddScreenMessage));
    questState_type["MsgOnHeroHairTypeChanged"] = &LuaQuestState::MsgOnHeroHairTypeChanged;
    questState_type["MsgOnHeroUsedTeleporter"] = &LuaQuestState::MsgOnHeroUsedTeleporter;
    questState_type["MsgOnHeroUsedGuildSeal"] = &LuaQuestState::MsgOnHeroUsedGuildSeal;
    questState_type["MsgOnGameSavedManually"] = &LuaQuestState::MsgOnGameSavedManually;
    questState_type["MsgOnHeroSlept"] = &LuaQuestState::MsgOnHeroSlept;
    questState_type["MsgOnHeroFiredRangedWeapon"] = &LuaQuestState::MsgOnHeroFiredRangedWeapon;

    questState_type["TurnCreatureInto"] = &LuaQuestState::TurnCreatureInto;
    questState_type["DeregisterTimer"] = &LuaQuestState::DeregisterTimer;
    questState_type["CreateLight"] = sol::overload([](LuaQuestState& self, sol::table pos, sol::table col, const std::string& script, float inner, float outer) {return self.CreateLight(pos, col, script, inner, outer, sol::nullopt);},
        sol::resolve<std::shared_ptr<CScriptThing>(sol::table, sol::table, const std::string&, float, float, sol::optional<float>)>(&LuaQuestState::CreateLight));
    questState_type["CreateObjectOnEntity"] = &LuaQuestState::CreateObjectOnEntity;
    questState_type["CreateCreatureOnEntity"] = &LuaQuestState::CreateCreatureOnEntity;
    questState_type["CreateExperienceOrb"] = &LuaQuestState::CreateExperienceOrb;
    questState_type["CreateExplosion"] = &LuaQuestState::CreateExplosion;
    questState_type["CreatePhysicalBarrier"] = &LuaQuestState::CreatePhysicalBarrier;
    questState_type["CreateRumble"] = &LuaQuestState::CreateRumble;
    questState_type["ClearAllRumbles"] = &LuaQuestState::ClearAllRumbles;
    questState_type["AddScreenTitleMessage"] = sol::overload([](LuaQuestState& self, const std::string& message, float duration) {self.AddScreenTitleMessage(message, duration, sol::nullopt);},
        sol::resolve<void(const std::string&, float, sol::optional<bool>)>(&LuaQuestState::AddScreenTitleMessage));
    questState_type["DisplayGameInfo"] = &LuaQuestState::DisplayGameInfo;
    questState_type["DisplayGameInfoText"] = &LuaQuestState::DisplayGameInfoText;
    questState_type["IsSafeToDisplayGameInfo"] = &LuaQuestState::IsSafeToDisplayGameInfo;
    questState_type["DisplayTutorial"] = &LuaQuestState::DisplayTutorial;
    questState_type["IsTutorialSystemEnabled"] = &LuaQuestState::IsTutorialSystemEnabled;
    questState_type["GiveHeroWeapon"] = &LuaQuestState::GiveHeroWeapon;
    questState_type["SetWeaponAsHerosActiveWeapon"] = &LuaQuestState::SetWeaponAsHerosActiveWeapon;
    questState_type["GiveHeroItemsFromContainer"] = &LuaQuestState::GiveHeroItemsFromContainer;
    questState_type["SetHeroAbleToGainExperience"] = &LuaQuestState::SetHeroAbleToGainExperience;
    questState_type["SheatheHeroWeapons"] = &LuaQuestState::SheatheHeroWeapons;
    questState_type["SetHeroWillAsUsable"] = &LuaQuestState::SetHeroWillAsUsable;
    questState_type["SetHeroWeaponsAsUsable"] = &LuaQuestState::SetHeroWeaponsAsUsable;
    questState_type["SetWeaponOutCrimeEnabled"] = &LuaQuestState::SetWeaponOutCrimeEnabled;
    questState_type["SetGuardsIgnoreCrimes"] = &LuaQuestState::SetGuardsIgnoreCrimes;
    questState_type["RemoveAllHeroWeapons"] = &LuaQuestState::RemoveAllHeroWeapons;
    questState_type["IsReportedOrUnreportedCrimeKnown"] = &LuaQuestState::IsReportedOrUnreportedCrimeKnown;
    questState_type["ConfiscateAllHeroItems"] = &LuaQuestState::ConfiscateAllHeroItems;
    questState_type["ConfiscateAllHeroWeapons"] = &LuaQuestState::ConfiscateAllHeroWeapons;
    questState_type["ConfiscateItemsOfTypeFromHero"] = &LuaQuestState::ConfiscateItemsOfTypeFromHero;
    questState_type["ReturnAllConfiscatedItemsToHero"] = &LuaQuestState::ReturnAllConfiscatedItemsToHero;
    questState_type["MakeHeroCarryItemInHand"] = sol::overload(sol::resolve<void(const std::shared_ptr<CScriptThing>&, bool, bool)>(&LuaQuestState::MakeHeroCarryItemInHand),sol::resolve<void(const std::string&)>(&LuaQuestState::MakeHeroCarryItemInHand));
    questState_type["AddTattooToHero"] = &LuaQuestState::AddTattooToHero;
    questState_type["IsPlayerZTargetingThing"] = &LuaQuestState::IsPlayerZTargetingThing;
    questState_type["IsPlayerCreatureBlocking"] = &LuaQuestState::IsPlayerCreatureBlocking;
    questState_type["IsPlayerCreatureReadyToFireProjectileWeapon"] = &LuaQuestState::IsPlayerCreatureReadyToFireProjectileWeapon;
    questState_type["GetPlayerCreatureCombatMultiplier"] = &LuaQuestState::GetPlayerCreatureCombatMultiplier;
    questState_type["GetPlayerCreatureCombatMultiplierRunningNumHits"] = &LuaQuestState::GetPlayerCreatureCombatMultiplierRunningNumHits;
    questState_type["IsPlayerCreatureFlourishEnabled"] = &LuaQuestState::IsPlayerCreatureFlourishEnabled;
    questState_type["SetPlayerCreatureOnlyTarget"] = &LuaQuestState::SetPlayerCreatureOnlyTarget;
    questState_type["ResetPlayerCreatureOnlyTarget"] = &LuaQuestState::ResetPlayerCreatureOnlyTarget;
    questState_type["GetHeroMorality"] = &LuaQuestState::GetHeroMorality;
    questState_type["GetHeroMoralityCategory"] = &LuaQuestState::GetHeroMoralityCategory;
    questState_type["ChangeHeroMoralityDueToTheft"] = &LuaQuestState::ChangeHeroMoralityDueToTheft;
    questState_type["ChangeHeroMoralityDueToPicklock"] = &LuaQuestState::ChangeHeroMoralityDueToPicklock;
    questState_type["GiveHeroRenownPoints"] = &LuaQuestState::GiveHeroRenownPoints;
    questState_type["IsHeroRenownLevelFull"] = &LuaQuestState::IsHeroRenownLevelFull;
    questState_type["IncreaseHeroRenownLevel"] = &LuaQuestState::IncreaseHeroRenownLevel;
    questState_type["GetHeroStrengthLevel"] = &LuaQuestState::GetHeroStrengthLevel;
    questState_type["GetHeroSkillLevel"] = &LuaQuestState::GetHeroSkillLevel;
    questState_type["GetHeroWillLevel"] = &LuaQuestState::GetHeroWillLevel;
    questState_type["GetHeroStatLevel"] = &LuaQuestState::GetHeroStatLevel;
    questState_type["GetHeroStatMax"] = &LuaQuestState::GetHeroStatMax;
    questState_type["SetHeroAge"] = &LuaQuestState::SetHeroAge;
    questState_type["SetHeroAsTeenager"] = &LuaQuestState::SetHeroAsTeenager;
    questState_type["SetHeroAsApprentice"] = &LuaQuestState::SetHeroAsApprentice;
    questState_type["GetDistanceHeroCanBeHeardFrom"] = &LuaQuestState::GetDistanceHeroCanBeHeardFrom;
    questState_type["GetHeroRoughExperienceLevel"] = &LuaQuestState::GetHeroRoughExperienceLevel;
    questState_type["GetHeroExperienceAvailableToSpend"] = &LuaQuestState::GetHeroExperienceAvailableToSpend;
    questState_type["GetHeroScariness"] = &LuaQuestState::GetHeroScariness;
    questState_type["GetHeroAttractiveness"] = &LuaQuestState::GetHeroAttractiveness;
    questState_type["GetHeroWillEnergyLevel"] = &LuaQuestState::GetHeroWillEnergyLevel;
    questState_type["SetHeroWillEnergyLevel"] = &LuaQuestState::SetHeroWillEnergyLevel;
    questState_type["SetHeroWillEnergyAsAbleToRefill"] = &LuaQuestState::SetHeroWillEnergyAsAbleToRefill;
    questState_type["GetNumberOfItemsOfTypeInInventory"] = &LuaQuestState::GetNumberOfItemsOfTypeInInventory;
    questState_type["IsHeroHandLampLit"] = &LuaQuestState::IsHeroHandLampLit;
    questState_type["SetHeroHandLampAsLit"] = &LuaQuestState::SetHeroHandLampAsLit;
    questState_type["IsWearingClothingItem"] = &LuaQuestState::IsWearingClothingItem;
    questState_type["IsHeroNaked"] = &LuaQuestState::IsHeroNaked;
    questState_type["RemoveHeroClothing"] = &LuaQuestState::RemoveHeroClothing;
    questState_type["SetHeroAsWearing"] = &LuaQuestState::SetHeroAsWearing;
    questState_type["ChangeHeroHairstyle"] = &LuaQuestState::ChangeHeroHairstyle;
    questState_type["RemoveHeroHairstyle"] = &LuaQuestState::RemoveHeroHairstyle;
    questState_type["IsWearingHairstyle"] = &LuaQuestState::IsWearingHairstyle;
    questState_type["IsPlayerCarryingItemOfType"] = &LuaQuestState::IsPlayerCarryingItemOfType;
    questState_type["IsPlayerWieldingWeapon"] = &LuaQuestState::IsPlayerWieldingWeapon;
    questState_type["IsEntityWieldingWeapon"] = &LuaQuestState::IsEntityWieldingWeapon;
    questState_type["IsEntityWieldingMeleeWeapon"] = &LuaQuestState::IsEntityWieldingMeleeWeapon;
    questState_type["IsEntityWieldingRangedWeapon"] = &LuaQuestState::IsEntityWieldingRangedWeapon;
    questState_type["GetPreviouslyWieldedMeleeWeaponName"] = &LuaQuestState::GetPreviouslyWieldedMeleeWeaponName;
    questState_type["GetPreviouslyWieldedRangedWeaponName"] = &LuaQuestState::GetPreviouslyWieldedRangedWeaponName;
    questState_type["ApplyHeroPenaltyForDeath"] = &LuaQuestState::ApplyHeroPenaltyForDeath;
    questState_type["GiveHeroTitle"] = &LuaQuestState::GiveHeroTitle;
    questState_type["EntitySetAsMarryable"] = &LuaQuestState::EntitySetAsMarryable;
    questState_type["EntitySetAsAbleToRegionFollowWhenMarried"] = &LuaQuestState::EntitySetAsAbleToRegionFollowWhenMarried;
    questState_type["EntityForceMarriageToHero"] = &LuaQuestState::EntityForceMarriageToHero;
    questState_type["IsEntityMarriedToHero"] = &LuaQuestState::IsEntityMarriedToHero;
    questState_type["IsEntityMarriable"] = &LuaQuestState::IsEntityMarriable;
    questState_type["GetNumberOfTimesHeroHasHadSex"] = &LuaQuestState::GetNumberOfTimesHeroHasHadSex;
    questState_type["SetNumberOfTimesHeroHasHadSex"] = &LuaQuestState::SetNumberOfTimesHeroHasHadSex;
    questState_type["SetHeroAsHavingHadSex"] = &LuaQuestState::SetHeroAsHavingHadSex;
    questState_type["SetHeroAsHavingHadGaySex"] = &LuaQuestState::SetHeroAsHavingHadGaySex;
    questState_type["GiveThingHeroRewardItem"] = &LuaQuestState::GiveThingHeroRewardItem;
    questState_type["GiveThingItemInHand"] = &LuaQuestState::GiveThingItemInHand;
    questState_type["GiveThingItemInSlot"] = &LuaQuestState::GiveThingItemInSlot;
    questState_type["GiveHeroExpression"] = sol::overload([](LuaQuestState& self, const std::string& name, int level) {self.GiveHeroExpression(name, level, sol::nullopt);},sol::resolve<void(const std::string&, int, sol::optional<bool>)>(&LuaQuestState::GiveHeroExpression));
    questState_type["HeroHasExpression"] = &LuaQuestState::HeroHasExpression;
    questState_type["IsHeroPerformingExpression"] = &LuaQuestState::IsHeroPerformingExpression;
    questState_type["EntitySetAsAllowedToFollowHero"] = &LuaQuestState::EntitySetAsAllowedToFollowHero;
    questState_type["EntitySetAsAllowedToChangeRegionFollowingState"] = &LuaQuestState::EntitySetAsAllowedToChangeRegionFollowingState;
    questState_type["EntitySetAsRespondingToFollowAndWaitExpressions"] = &LuaQuestState::EntitySetAsRespondingToFollowAndWaitExpressions;
    questState_type["EntitySetAsMirroringHeroEnemyRelationsWhileFollowing"] = &LuaQuestState::EntitySetAsMirroringHeroEnemyRelationsWhileFollowing;
    questState_type["TeleportAllFollowersToHeroPosition"] = &LuaQuestState::TeleportAllFollowersToHeroPosition;
    questState_type["EntityTeleportToHeroPosition"] = &LuaQuestState::EntityTeleportToHeroPosition;
    questState_type["SendEntityEvent"] = sol::overload(sol::resolve<void(int, const std::shared_ptr<CScriptThing>&, const std::shared_ptr<CScriptThing>&)>(&LuaQuestState::SendEntityEvent),
        [](LuaQuestState& self, int eventType, const std::shared_ptr<CScriptThing>& spSender) {self.SendEntityEvent(eventType, spSender, nullptr);},
        [](LuaQuestState& self, int eventType) {self.SendEntityEvent(eventType, nullptr, nullptr);});
    questState_type["GetWaterHeightAtPosition"] = &LuaQuestState::GetWaterHeightAtPosition;
    questState_type["GetHeroHealthMax"] = &LuaQuestState::GetHeroHealthMax;
    questState_type["GetHeroHealthPercentage"] = &LuaQuestState::GetHeroHealthPercentage;
    questState_type["GetHeroWillEnergy"] = &LuaQuestState::GetHeroWillEnergy;
    questState_type["GetHeroWillEnergyMax"] = &LuaQuestState::GetHeroWillEnergyMax;
    questState_type["GetHeroWillEnergyPercentage"] = &LuaQuestState::GetHeroWillEnergyPercentage;
    questState_type["ModifyThingHealth"] = sol::overload([](LuaQuestState& self, const std::shared_ptr<CScriptThing>& spThing, float amount) {self.ModifyThingHealth(spThing, amount, sol::nullopt);},
        sol::resolve<void(const std::shared_ptr<CScriptThing>&, float, sol::optional<bool>)>(&LuaQuestState::ModifyThingHealth));
    questState_type["EntitySetMaxHealth"] = &LuaQuestState::EntitySetMaxHealth;
    questState_type["SetThingAsKilled"] = &LuaQuestState::SetThingAsKilled;
    questState_type["GiveHeroNewQuestObjective"] = &LuaQuestState::GiveHeroNewQuestObjective;
    questState_type["TellHeroQuestObjectiveCompleted"] = &LuaQuestState::TellHeroQuestObjectiveCompleted;
    questState_type["TellHeroQuestObjectiveFailed"] = &LuaQuestState::TellHeroQuestObjectiveFailed;
    questState_type["IsHeroOnQuest"] = &LuaQuestState::IsHeroOnQuest;
    questState_type["SetGuildMasterMessages"] = &LuaQuestState::SetGuildMasterMessages;
    questState_type["IsQuestActive"] = &LuaQuestState::IsQuestActive;
    questState_type["IsQuestRegistered"] = &LuaQuestState::IsQuestRegistered;
    questState_type["IsQuestCompleted"] = &LuaQuestState::IsQuestCompleted;
    questState_type["IsQuestFailed"] = &LuaQuestState::IsQuestFailed;
    questState_type["SetQuestAsPersistent"] = &LuaQuestState::SetQuestAsPersistent;
    questState_type["GetExclusiveQuestScriptName"] = &LuaQuestState::GetExclusiveQuestScriptName;
    questState_type["RemoveQuestCardFromGuild"] = &LuaQuestState::RemoveQuestCardFromGuild;
    questState_type["RemoveQuestCardFromHero"] = &LuaQuestState::RemoveQuestCardFromHero;
    questState_type["RemoveAllAvailableQuestCardsFromGuild"] = &LuaQuestState::RemoveAllAvailableQuestCardsFromGuild;
    questState_type["FailAllActiveQuests"] = &LuaQuestState::FailAllActiveQuests;
    questState_type["GetAllActiveQuestInfo"] = &LuaQuestState::GetAllActiveQuestInfo;
    questState_type["AddFeatCard"] = &LuaQuestState::AddFeatCard;
    questState_type["AddBoast"] = &LuaQuestState::AddBoast;
    questState_type["RemoveBoast"] = &LuaQuestState::RemoveBoast;
    questState_type["SetBoastAsFailed"] = &LuaQuestState::SetBoastAsFailed;
    questState_type["SetBoastAsCompleted"] = &LuaQuestState::SetBoastAsCompleted;
    questState_type["IsBoastTaken"] = &LuaQuestState::IsBoastTaken;
    questState_type["KickOffDeathScreen"] = &LuaQuestState::KickOffDeathScreen;
    questState_type["KickOffCreditsScreen"] = &LuaQuestState::KickOffCreditsScreen;
    questState_type["SetPreferredQuickAccessItem"] = &LuaQuestState::SetPreferredQuickAccessItem;
    questState_type["GetDeathRecoveryMarkerName"] = &LuaQuestState::GetDeathRecoveryMarkerName;
    questState_type["SetDeathRecoveryMarkerName"] = &LuaQuestState::SetDeathRecoveryMarkerName;
    questState_type["ResetDeathRecoveryMarkerNameToDefault"] = &LuaQuestState::ResetDeathRecoveryMarkerNameToDefault;
    questState_type["IsToFailQuestOnDeath"] = &LuaQuestState::IsToFailQuestOnDeath;
    questState_type["SetWhetherToFailQuestOnDeath"] = &LuaQuestState::SetWhetherToFailQuestOnDeath;
    questState_type["ResetWhetherToFailQuestOnDeathToDefault"] = &LuaQuestState::ResetWhetherToFailQuestOnDeathToDefault;
    questState_type["GetMostRecentValidUsedTarget"] = &LuaQuestState::GetMostRecentValidUsedTarget;
    questState_type["GetMostRecentValidUsedTargetName"] = &LuaQuestState::GetMostRecentValidUsedTargetName;
    questState_type["AddQuestInfoTimer"] = &LuaQuestState::AddQuestInfoTimer;
    questState_type["AddQuestInfoCounterList"] = &LuaQuestState::AddQuestInfoCounterList;
    questState_type["AddQuestInfoTickByAction"] = &LuaQuestState::AddQuestInfoTickByAction;
    questState_type["AddQuestInfoTickByText"] = &LuaQuestState::AddQuestInfoTickByText;
    questState_type["UpdateQuestInfoBar"] = &LuaQuestState::UpdateQuestInfoBar;
    questState_type["ChangeQuestInfoBarColour"] = &LuaQuestState::ChangeQuestInfoBarColour;
    questState_type["UpdateQuestInfoTimer"] = &LuaQuestState::UpdateQuestInfoTimer;
    questState_type["UpdateQuestInfoCounterList"] = &LuaQuestState::UpdateQuestInfoCounterList;
    questState_type["UpdateQuestInfoTick"] = &LuaQuestState::UpdateQuestInfoTick;
    questState_type["RemoveQuestInfoElement"] = &LuaQuestState::RemoveQuestInfoElement;
    questState_type["RemoveAllQuestInfoElements"] = &LuaQuestState::RemoveAllQuestInfoElements;
    questState_type["DisplayTime"] = &LuaQuestState::DisplayTime;
    questState_type["DisplayMoneyBag"] = &LuaQuestState::DisplayMoneyBag;
    questState_type["DisplayMiniGameInfo"] = &LuaQuestState::DisplayMiniGameInfo;
    questState_type["UpdateMiniGameInfoBar"] = &LuaQuestState::UpdateMiniGameInfoBar;
    questState_type["IsEntityPickPocketable"] = &LuaQuestState::IsEntityPickPocketable;
    questState_type["IsEntityPickLockable"] = &LuaQuestState::IsEntityPickLockable;
    questState_type["IsEntityStealable"] = &LuaQuestState::IsEntityStealable;
    questState_type["EntitySetAsPickPocketed"] = &LuaQuestState::EntitySetAsPickPocketed;
    questState_type["EntitySetAsPickLocked"] = &LuaQuestState::EntitySetAsPickLocked;
    questState_type["EntitySetAsStolen"] = &LuaQuestState::EntitySetAsStolen;
    questState_type["MiniMapAddMarker"] = &LuaQuestState::MiniMapAddMarker;
    questState_type["MiniMapSetMarkerGraphic"] = &LuaQuestState::MiniMapSetMarkerGraphic;
    questState_type["MiniMapRemoveMarker"] = &LuaQuestState::MiniMapRemoveMarker;
    questState_type["MiniMapRemoveAllMarkers"] = &LuaQuestState::MiniMapRemoveAllMarkers;
    questState_type["MiniMapAllowRouteBetweenRegions"] = &LuaQuestState::MiniMapAllowRouteBetweenRegions;
    questState_type["MiniMapSetAsEnabled"] = &LuaQuestState::MiniMapSetAsEnabled;
    questState_type["EntitySetAsHiddenOnMiniMap"] = &LuaQuestState::EntitySetAsHiddenOnMiniMap;
    questState_type["SetHUDEnabled"] = &LuaQuestState::SetHUDEnabled;
    questState_type["EntitySetWillBeUsingNarrator"] = &LuaQuestState::EntitySetWillBeUsingNarrator;
    questState_type["EntityResetAsPureAINarrator"] = &LuaQuestState::EntityResetAsPureAINarrator;
    questState_type["PlayAVIMovie"] = &LuaQuestState::PlayAVIMovie;
    questState_type["FadeScreenOutUntilNextCallToFadeScreenIn"] = &LuaQuestState::FadeScreenOutUntilNextCallToFadeScreenIn;
    questState_type["IsScreenFadingOut"] = &LuaQuestState::IsScreenFadingOut;
    questState_type["EndCutFade"] = &LuaQuestState::EndCutFade;
    questState_type["EndLetterBox"] = &LuaQuestState::EndLetterBox;
    questState_type["PauseAllEntities"] = &LuaQuestState::PauseAllEntities;
    questState_type["SetAllowScreenFadingOnNextRegionChange"] = &LuaQuestState::SetAllowScreenFadingOnNextRegionChange;
    questState_type["SetAllowScreenFadingIfAlreadyFaded"] = &LuaQuestState::SetAllowScreenFadingIfAlreadyFaded;
    questState_type["SetAbilityAvailability"] = &LuaQuestState::SetAbilityAvailability;
    questState_type["SetEnvironmentalEffectsAlwaysUpdate"] = &LuaQuestState::SetEnvironmentalEffectsAlwaysUpdate;
    questState_type["SetDeadCreaturesAndExperienceOrbsAndDropBagsAsHidden"] = &LuaQuestState::SetDeadCreaturesAndExperienceOrbsAndDropBagsAsHidden;
    questState_type["RemoveDeadCreature"] = &LuaQuestState::RemoveDeadCreature;
    questState_type["CameraSetCameraPreloadFlag"] = &LuaQuestState::CameraSetCameraPreloadFlag;
    questState_type["CameraCircleAroundThing"] = &LuaQuestState::CameraCircleAroundThing;
    questState_type["CameraCircleAroundPos"] = &LuaQuestState::CameraCircleAroundPos;
    questState_type["CameraMoveToPosAndLookAtPos"] = &LuaQuestState::CameraMoveToPosAndLookAtPos;
    questState_type["CameraMoveToPosAndLookAtThing"] = &LuaQuestState::CameraMoveToPosAndLookAtThing;
    questState_type["CameraMoveBetweenLookingAt"] = sol::overload(
        sol::resolve<void(const std::shared_ptr<CScriptThing>&, sol::table, sol::table, float, float)>(&LuaQuestState::CameraMoveBetweenLookingAt),
        sol::resolve<void(sol::table, sol::table, sol::table, float, float)>(&LuaQuestState::CameraMoveBetweenLookingAt));
    questState_type["CameraMoveBetweenLookFromAndLookTo"] = &LuaQuestState::CameraMoveBetweenLookFromAndLookTo;
    questState_type["CameraUseCameraPoint"] = sol::overload(
        sol::resolve<void(const std::shared_ptr<CScriptThing>&, const std::shared_ptr<CScriptThing>&, float, int, int)>(&LuaQuestState::CameraUseCameraPoint),
        sol::resolve<void(const std::string&, const std::shared_ptr<CScriptThing>&, float, int, int)>(&LuaQuestState::CameraUseCameraPoint),
        sol::resolve<void(const std::shared_ptr<CScriptThing>&, sol::table, sol::table, float, int, int)>(&LuaQuestState::CameraUseCameraPoint),
        sol::resolve<void(const std::string&, sol::table, sol::table, float, int, int)>(&LuaQuestState::CameraUseCameraPoint));    
    questState_type["CameraResetToViewBehindHero"] = &LuaQuestState::CameraResetToViewBehindHero;
    questState_type["IsCameraInScriptedMode"] = &LuaQuestState::IsCameraInScriptedMode;
    questState_type["CameraUseScreenEffect"] = &LuaQuestState::CameraUseScreenEffect;
    questState_type["CameraCancelScreenEffect"] = &LuaQuestState::CameraCancelScreenEffect;
    questState_type["IsCameraPosOnScreen"] = &LuaQuestState::IsCameraPosOnScreen;
    questState_type["GetGameAngleXY"] = &LuaQuestState::GetGameAngleXY;
    questState_type["CameraEarthquakeIntensityAtPos"] = &LuaQuestState::CameraEarthquakeIntensityAtPos;
    questState_type["CameraDoConversation"] = sol::overload([](LuaQuestState& self, const std::shared_ptr<CScriptThing>& spSpeaker, const std::shared_ptr<CScriptThing>& spListener, int eOp) {
        self.CameraDoConversation(spSpeaker, spListener, eOp, sol::nullopt);},
        sol::resolve<void(const std::shared_ptr<CScriptThing>&, const std::shared_ptr<CScriptThing>&, int, sol::optional<bool>)>(&LuaQuestState::CameraDoConversation));
    questState_type["IsChestOpen"] = &LuaQuestState::IsChestOpen;
    questState_type["OpenChest"] = sol::overload([](LuaQuestState& self, const std::shared_ptr<CScriptThing>& spChest) {return self.OpenChest(spChest, sol::nullopt);},sol::resolve<bool(const std::shared_ptr<CScriptThing>&, sol::optional<bool>)>(&LuaQuestState::OpenChest));
    questState_type["CloseChest"] = &LuaQuestState::CloseChest;
    questState_type["GetNumberOfKeysNeededToUnlockChest"] = &LuaQuestState::GetNumberOfKeysNeededToUnlockChest;
    questState_type["DisplayLockedChestMessage"] = &LuaQuestState::DisplayLockedChestMessage;
    questState_type["SetTrophyAsMountable"] = &LuaQuestState::SetTrophyAsMountable;
    questState_type["SetVillageLimbo"] = &LuaQuestState::SetVillageLimbo;
    questState_type["SetCreatureNotReload"] = &LuaQuestState::SetCreatureNotReload;
    questState_type["IsSleepingTime"] = &LuaQuestState::IsSleepingTime;
    questState_type["EnableGuards"] = &LuaQuestState::EnableGuards;
    questState_type["EnableVillagerDefTypes"] = &LuaQuestState::EnableVillagerDefTypes;
    questState_type["TryToRespawnDefNamed"] = &LuaQuestState::TryToRespawnDefNamed;
    questState_type["ClearHeroEnemyOfGuards"] = &LuaQuestState::ClearHeroEnemyOfGuards;
    questState_type["SetThingAsUsable"] = &LuaQuestState::SetThingAsUsable;
    questState_type["SetThingHomeBuilding"] = &LuaQuestState::SetThingHomeBuilding;
    questState_type["GiveThingBestEnemyTarget"] = &LuaQuestState::GiveThingBestEnemyTarget;
    questState_type["ClearThingBestEnemyTarget"] = &LuaQuestState::ClearThingBestEnemyTarget;
    questState_type["EntitySetInLimbo"] = sol::overload(
        [](LuaQuestState& self, const std::shared_ptr<CScriptThing>& spThing, bool bIsInLimbo) {self.EntitySetInLimbo(spThing, bIsInLimbo, sol::nullopt);},
        sol::resolve<void(const std::shared_ptr<CScriptThing>&, bool, sol::optional<bool>)>(&LuaQuestState::EntitySetInLimbo));
    questState_type["IsEntityInLimbo"] = &LuaQuestState::IsEntityInLimbo;
    questState_type["AddCrimeCommitted"] = sol::overload(
        sol::resolve<void(const std::shared_ptr<CScriptThing>&, int, sol::optional<bool>, const std::shared_ptr<CScriptThing>&, const std::shared_ptr<CScriptThing>&, int)>(&LuaQuestState::AddCrimeCommitted),
        [](LuaQuestState& self, const std::shared_ptr<CScriptThing>& spVillage, int crimeType, const std::shared_ptr<CScriptThing>& spCriminal, const std::shared_ptr<CScriptThing>& spVictim, int deedType) {
            self.AddCrimeCommitted(spVillage, crimeType, sol::nullopt, spCriminal, spVictim, deedType);},
        [](LuaQuestState& self, const std::shared_ptr<CScriptThing>& spVillage, int crimeType, const std::shared_ptr<CScriptThing>& spCriminal, int deedType) {
            self.AddCrimeCommitted(spVillage, crimeType, sol::nullopt, spCriminal, nullptr, deedType);},
        [](LuaQuestState& self, const std::shared_ptr<CScriptThing>& spVillage, int crimeType, int deedType) {
            self.AddCrimeCommitted(spVillage, crimeType, sol::nullopt, nullptr, nullptr, deedType);});
    questState_type["SetVillageAttitude"] = &LuaQuestState::SetVillageAttitude;
    questState_type["EntityGetShotStrikePos"] = &LuaQuestState::EntityGetShotStrikePos;
    questState_type["EntitySetNegateAllHits"] = &LuaQuestState::EntitySetNegateAllHits;
    questState_type["EntitySetEvadeAllHits"] = &LuaQuestState::EntitySetEvadeAllHits;
    questState_type["EntitySetAbleToBeEngagedInCombat"] = &LuaQuestState::EntitySetAbleToBeEngagedInCombat;
    questState_type["EntitySetAlwaysBlockAttacksFromThing"] = &LuaQuestState::EntitySetAlwaysBlockAttacksFromThing;
    questState_type["EntitySetAttackThingImmediately"] = sol::overload(
        [](LuaQuestState& self, const std::shared_ptr<CScriptThing>& spAttacker, const std::shared_ptr<CScriptThing>& spTarget) {self.EntitySetAttackThingImmediately(spAttacker, spTarget, sol::nullopt, sol::nullopt);},
        [](LuaQuestState& self, const std::shared_ptr<CScriptThing>& spAttacker, const std::shared_ptr<CScriptThing>& spTarget, bool b1) {self.EntitySetAttackThingImmediately(spAttacker, spTarget, b1, sol::nullopt);},
        sol::resolve<void(const std::shared_ptr<CScriptThing>&, const std::shared_ptr<CScriptThing>&, sol::optional<bool>, sol::optional<bool>)>(&LuaQuestState::EntitySetAttackThingImmediately));
    questState_type["EntitySetCombatType"] = &LuaQuestState::EntitySetCombatType;
    questState_type["EntityResetCombatTypeToDefault"] = &LuaQuestState::EntityResetCombatTypeToDefault;
    questState_type["EntitySetMaxNumberOfAttackers"] = &LuaQuestState::EntitySetMaxNumberOfAttackers;
    questState_type["EntityClearMaxNumberOfAttackers"] = &LuaQuestState::EntityClearMaxNumberOfAttackers;
    questState_type["EntityAttachToScript"] = &LuaQuestState::EntityAttachToScript;
    questState_type["EntitySetCombatAbility"] = &LuaQuestState::EntitySetCombatAbility;
    questState_type["EntitySetRangedTarget"] = &LuaQuestState::EntitySetRangedTarget;
    questState_type["EntityClearRangedTarget"] = &LuaQuestState::EntityClearRangedTarget;
    questState_type["EntitySetTargetingValidTargetWithoutLOS"] = &LuaQuestState::EntitySetTargetingValidTargetWithoutLOS;
    questState_type["EntitySetTargetingType"] = &LuaQuestState::EntitySetTargetingType;
    questState_type["EntityTeleportToPosition"] = sol::overload([](LuaQuestState& self, const std::shared_ptr<CScriptThing>& spThing, sol::table pos, float f) {self.EntityTeleportToPosition(spThing, pos, f, sol::nullopt, sol::nullopt);},
        [](LuaQuestState& self, const std::shared_ptr<CScriptThing>& spThing, sol::table pos, float f, bool b1) {
            self.EntityTeleportToPosition(spThing, pos, f, b1, sol::nullopt);},sol::resolve<void(const std::shared_ptr<CScriptThing>&, sol::table, float, sol::optional<bool>, sol::optional<bool>)>(&LuaQuestState::EntityTeleportToPosition));
    questState_type["EntitySetFacingAngle"] = sol::overload([](LuaQuestState& self, const std::shared_ptr<CScriptThing>& spThing, float angle) {
            self.EntitySetFacingAngle(spThing, angle, sol::nullopt);},sol::resolve<void(const std::shared_ptr<CScriptThing>&, float, sol::optional<bool>)>(&LuaQuestState::EntitySetFacingAngle));
    questState_type["EntitySetPerceptionVariables"] = &LuaQuestState::EntitySetPerceptionVariables;
    questState_type["EntitySetThingAsWantingMoney"] = &LuaQuestState::EntitySetThingAsWantingMoney;
    questState_type["EntitySetAppearanceMorphSeed"] = &LuaQuestState::EntitySetAppearanceMorphSeed;
    questState_type["SetEntityAsRegionFollowing"] = &LuaQuestState::SetEntityAsRegionFollowing;
    questState_type["SetEntityAsFollowingHeroThroughTeleporters"] = &LuaQuestState::SetEntityAsFollowingHeroThroughTeleporters;
    questState_type["EntityGetAppearanceSeed"] = &LuaQuestState::EntityGetAppearanceSeed;
    questState_type["EntityPlayObjectAnimation"] = sol::overload([](LuaQuestState& self, const std::shared_ptr<CScriptThing>& spThing, const std::string& animName) {
            self.EntityPlayObjectAnimation(spThing, animName, sol::nullopt);},
        sol::resolve<void(const std::shared_ptr<CScriptThing>&, const std::string&, sol::optional<bool>)>(&LuaQuestState::EntityPlayObjectAnimation));
    questState_type["EntitySetMaxRunningSpeed"] = &LuaQuestState::EntitySetMaxRunningSpeed;
    questState_type["EntitySetMaxWalkingSpeed"] = &LuaQuestState::EntitySetMaxWalkingSpeed;
    questState_type["EntityResetMaxRunningSpeed"] = &LuaQuestState::EntityResetMaxRunningSpeed;
    questState_type["EntityResetMaxWalkingSpeed"] = &LuaQuestState::EntityResetMaxWalkingSpeed;
    questState_type["EntityAttachToVillage"] = &LuaQuestState::EntityAttachToVillage;
    questState_type["EntitySetAsSittingOnFloor"] = &LuaQuestState::EntitySetAsSittingOnFloor;
    questState_type["EntitySetAsHavingBoundHands"] = &LuaQuestState::EntitySetAsHavingBoundHands;
    questState_type["EntitySetAsRemoveAllMovementBlockingModes"] = &LuaQuestState::EntitySetAsRemoveAllMovementBlockingModes;
    questState_type["EntitySetAsScared"] = &LuaQuestState::EntitySetAsScared;
    questState_type["EntitySetAsDrunk"] = &LuaQuestState::EntitySetAsDrunk;
    questState_type["EntityForceToLookAtThing"] = &LuaQuestState::EntityForceToLookAtThing;
    questState_type["EntityForceToLookAtCamera"] = &LuaQuestState::EntityForceToLookAtCamera;
    questState_type["EntityForceToLookAtNothing"] = &LuaQuestState::EntityForceToLookAtNothing;
    questState_type["EntityResetForceToLookAt"] = &LuaQuestState::EntityResetForceToLookAt;
    questState_type["EntitySetShotAccuracyPercentage"] = &LuaQuestState::EntitySetShotAccuracyPercentage;
    questState_type["EntityGetStandingOnThing"] = &LuaQuestState::EntityGetStandingOnThing;
    questState_type["EntityGetStandingInsideBuilding"] = &LuaQuestState::EntityGetStandingInsideBuilding;
    questState_type["EntityDropGenericBox"] = &LuaQuestState::EntityDropGenericBox;
    questState_type["EntitySheatheWeapons"] = sol::overload(
        [](LuaQuestState& self, const std::shared_ptr<CScriptThing>& spThing) { self.EntitySheatheWeapons(spThing, sol::nullopt); },
        sol::resolve<void(const std::shared_ptr<CScriptThing>&, sol::optional<bool>)>(&LuaQuestState::EntitySheatheWeapons));
    questState_type["EntityUnsheatheWeapons"] = sol::overload(
        [](LuaQuestState& self, const std::shared_ptr<CScriptThing>& spThing) { self.EntityUnsheatheWeapons(spThing, sol::nullopt); },
        sol::resolve<void(const std::shared_ptr<CScriptThing>&, sol::optional<bool>)>(&LuaQuestState::EntityUnsheatheWeapons));
    questState_type["EntityUnsheatheMeleeWeapon"] = sol::overload(
        [](LuaQuestState& self, const std::shared_ptr<CScriptThing>& spThing) { self.EntityUnsheatheMeleeWeapon(spThing, sol::nullopt); },
        sol::resolve<void(const std::shared_ptr<CScriptThing>&, sol::optional<bool>)>(&LuaQuestState::EntityUnsheatheMeleeWeapon));
    questState_type["EntityUnsheatheRangedWeapon"] = sol::overload(
        [](LuaQuestState& self, const std::shared_ptr<CScriptThing>& spThing) { self.EntityUnsheatheRangedWeapon(spThing, sol::nullopt); },
        sol::resolve<void(const std::shared_ptr<CScriptThing>&, sol::optional<bool>)>(&LuaQuestState::EntityUnsheatheRangedWeapon));
    questState_type["EntitySetAlpha"] = sol::overload(
        [](LuaQuestState& self, const std::shared_ptr<CScriptThing>& spThing, float alpha) { self.EntitySetAlpha(spThing, alpha, sol::nullopt); },
        sol::resolve<void(const std::shared_ptr<CScriptThing>&, float, sol::optional<bool>)>(&LuaQuestState::EntitySetAlpha));
    questState_type["EntitySetAsAbleToWalkThroughSolidObjects"] = &LuaQuestState::EntitySetAsAbleToWalkThroughSolidObjects;
    questState_type["EntitySetAsRespondToHit"] = &LuaQuestState::EntitySetAsRespondToHit;
    questState_type["EntitySetAsLocked"] = &LuaQuestState::EntitySetAsLocked;
    questState_type["EntityDecapitate"] = &LuaQuestState::EntityDecapitate;
    questState_type["EntityGiveGold"] = &LuaQuestState::EntityGiveGold;
    questState_type["EntityGetSex"] = &LuaQuestState::EntityGetSex;
    questState_type["EntitySetAllowBossPhaseChanges"] = &LuaQuestState::EntitySetAllowBossPhaseChanges;
    questState_type["EntityGetBossPhase"] = &LuaQuestState::EntityGetBossPhase;
    questState_type["EntitySetBossPhase"] = &LuaQuestState::EntitySetBossPhase;
    questState_type["EntityResetCreatureMode"] = &LuaQuestState::EntityResetCreatureMode;
    questState_type["EntitySetAsReceivingEvents"] = &LuaQuestState::EntitySetAsReceivingEvents;
    questState_type["EntitySetAsToAddToStatChangesWhenHit"] = &LuaQuestState::EntitySetAsToAddToStatChangesWhenHit;
    questState_type["EntityLeaveCombatStance"] = &LuaQuestState::EntityLeaveCombatStance;
    questState_type["EntitySetAsUseMovementInActions"] = &LuaQuestState::EntitySetAsUseMovementInActions;
    questState_type["EntitySetAsDisplayingEmoteIcon"] = &LuaQuestState::EntitySetAsDisplayingEmoteIcon;
    questState_type["EntitySetAsCollidableToThings"] = &LuaQuestState::EntitySetAsCollidableToThings;
    questState_type["EntityEnableGravity"] = &LuaQuestState::EntityEnableGravity;
    questState_type["EntitySetLightAsOn"] = &LuaQuestState::EntitySetLightAsOn;
    questState_type["EntityFadeIn"] = &LuaQuestState::EntityFadeIn;
    questState_type["EntityBeginLoadingAnimation"] = &LuaQuestState::EntityBeginLoadingAnimation;
    questState_type["EntityBeginLoadingBasicAnimations"] = &LuaQuestState::EntityBeginLoadingBasicAnimations;
    questState_type["EntityCastForcePush"] = sol::overload(
        [](LuaQuestState& self, const std::shared_ptr<CScriptThing>& spCaster) { return self.EntityCastForcePush(spCaster, sol::nullopt); },
        sol::resolve<bool(const std::shared_ptr<CScriptThing>&, sol::optional<bool>)>(&LuaQuestState::EntityCastForcePush));
    questState_type["EntityCastLightningAtTarget"] = &LuaQuestState::EntityCastLightningAtTarget;
    questState_type["BeginLoadingMesh"] = &LuaQuestState::BeginLoadingMesh;
    questState_type["EntityWillTeleportToArea"] = &LuaQuestState::EntityWillTeleportToArea;
    questState_type["EntityStartScreamerSuperAttackThing"] = &LuaQuestState::EntityStartScreamerSuperAttackThing;
    questState_type["EntityEndScreamerSuperAttackThing"] = &LuaQuestState::EntityEndScreamerSuperAttackThing;
    questState_type["SetLightColour"] = &LuaQuestState::SetLightColour;
    questState_type["CreatureGeneratorSetFamily"] = &LuaQuestState::CreatureGeneratorSetFamily;
    questState_type["CreatureGeneratorTrigger"] = &LuaQuestState::CreatureGeneratorTrigger;
    questState_type["CreatureGeneratorSetAlwaysCreateCreaturesOnTrigger"] = &LuaQuestState::CreatureGeneratorSetAlwaysCreateCreaturesOnTrigger;
    questState_type["CreatureGeneratorIsDepleted"] = &LuaQuestState::CreatureGeneratorIsDepleted;
    questState_type["CreatureGeneratorIsDestroyed"] = &LuaQuestState::CreatureGeneratorIsDestroyed;
    questState_type["CreatureGeneratorSetGeneratedCreatureScriptName"] = &LuaQuestState::CreatureGeneratorSetGeneratedCreatureScriptName;
    questState_type["CreatureGeneratorSetNumTriggers"] = &LuaQuestState::CreatureGeneratorSetNumTriggers;
    questState_type["CreatureGeneratorGetNumGeneratedCreatures"] = &LuaQuestState::CreatureGeneratorGetNumGeneratedCreatures;
    questState_type["CreatureGeneratorAreAllCreaturesAlive"] = &LuaQuestState::CreatureGeneratorAreAllCreaturesAlive;
    questState_type["CreatureGeneratorAddTriggerer"] = &LuaQuestState::CreatureGeneratorAddTriggerer;
    questState_type["CreatureGeneratorRemoveTriggerer"] = &LuaQuestState::CreatureGeneratorRemoveTriggerer;
    questState_type["SetCreatureGeneratorEnabled"] = &LuaQuestState::SetCreatureGeneratorEnabled;
    questState_type["SetCreatureGeneratorsEnabledDuringScript"] = &LuaQuestState::SetCreatureGeneratorsEnabledDuringScript;
    questState_type["SetCreatureGeneratorsCreatureGroupAsEnabled"] = &LuaQuestState::SetCreatureGeneratorsCreatureGroupAsEnabled;
    questState_type["IsCreatureGenerationEnabledForRegion"] = &LuaQuestState::IsCreatureGenerationEnabledForRegion;
    questState_type["IsCreatureFlying"] = &LuaQuestState::IsCreatureFlying;
    questState_type["SetTeleporterAsActive"] = &LuaQuestState::SetTeleporterAsActive;
    questState_type["IsTeleporterActive"] = &LuaQuestState::IsTeleporterActive;
    questState_type["SetTeleportingAsActive"] = &LuaQuestState::SetTeleportingAsActive;
    questState_type["IsTeleportingActive"] = &LuaQuestState::IsTeleportingActive;
    questState_type["SetRegionEntranceAsActive"] = &LuaQuestState::SetRegionEntranceAsActive;
    questState_type["SetRegionTextDisplayAsActive"] = &LuaQuestState::SetRegionTextDisplayAsActive;
    questState_type["SetHeroSleepingAsEnabled"] = &LuaQuestState::SetHeroSleepingAsEnabled;
    questState_type["IsHeroSleepingEnabled"] = &LuaQuestState::IsHeroSleepingEnabled;
    questState_type["SetExperienceSpendingAsEnabled"] = &LuaQuestState::SetExperienceSpendingAsEnabled;
    questState_type["SetMoralityChangingAsEnabled"] = &LuaQuestState::SetMoralityChangingAsEnabled;
    questState_type["SetSummonerDeathExplosionAffectsHero"] = &LuaQuestState::SetSummonerDeathExplosionAffectsHero;
    questState_type["GetNearestEnabledDiggingSpot"] = &LuaQuestState::GetNearestEnabledDiggingSpot;
    questState_type["IsDiggingSpotEnabled"] = &LuaQuestState::IsDiggingSpotEnabled;
    questState_type["IsDiggingSpotHidden"] = &LuaQuestState::IsDiggingSpotHidden;
    questState_type["SetDiggingSpotAsHidden"] = &LuaQuestState::SetDiggingSpotAsHidden;
    questState_type["CheckForCameraMessage"] = &LuaQuestState::CheckForCameraMessage;
    questState_type["WaitForCameraMessage"] = &LuaQuestState::WaitForCameraMessage;
    questState_type["SetThingAsConscious"] = sol::overload(
        [](LuaQuestState& self, const std::shared_ptr<CScriptThing>& sp, bool b) { self.SetThingAsConscious(sp, b, sol::nullopt); },
        sol::resolve<void(const std::shared_ptr<CScriptThing>&, bool, sol::optional<std::string>)>(&LuaQuestState::SetThingAsConscious));
    questState_type["SetFireToThing"] = &LuaQuestState::SetFireToThing;
    questState_type["ExtinguishFiresOnThing"] = &LuaQuestState::ExtinguishFiresOnThing;
    questState_type["IsThingOnFire"] = &LuaQuestState::IsThingOnFire;
    questState_type["AddItemToContainer"] = &LuaQuestState::AddItemToContainer;
    questState_type["RemoveItemFromContainer"] = &LuaQuestState::RemoveItemFromContainer;
    questState_type["EntitySetDeathContainerAsEnabled"] = &LuaQuestState::EntitySetDeathContainerAsEnabled;
    questState_type["GetItemDefNamesFromContainer"] = &LuaQuestState::GetItemDefNamesFromContainer;
    questState_type["EntitySetStategroupEnabled"] = &LuaQuestState::EntitySetStategroupEnabled;
    questState_type["EntitySetCombatEnabled"] = &LuaQuestState::EntitySetCombatEnabled;
    questState_type["EntitySetSleepEnabled"] = &LuaQuestState::EntitySetSleepEnabled;
    questState_type["EntitySetOpinionReactionsEnabled"] = &LuaQuestState::EntitySetOpinionReactionsEnabled;
    questState_type["EntitySetDeedReactionsEnabled"] = &LuaQuestState::EntitySetDeedReactionsEnabled;
    questState_type["DebugGetAllTextEntriesForTargetedThing"] = &LuaQuestState::DebugGetAllTextEntriesForTargetedThing;
    questState_type["EntityUnsetThingAsEnemyOfThing"] = &LuaQuestState::EntityUnsetThingAsEnemyOfThing;
    questState_type["EntitySetThingAsAllyOfThing"] = &LuaQuestState::EntitySetThingAsAllyOfThing;
    questState_type["EntityUnsetThingAsAllyOfThing"] = &LuaQuestState::EntityUnsetThingAsAllyOfThing;
    questState_type["SetFactionAsAlliedToFaction"] = &LuaQuestState::SetFactionAsAlliedToFaction;
    questState_type["SetFactionAsNeutralToFaction"] = &LuaQuestState::SetFactionAsNeutralToFaction;
    questState_type["SetFactionAsEnemyToFaction"] = &LuaQuestState::SetFactionAsEnemyToFaction;
    questState_type["AreEntitiesEnemies"] = &LuaQuestState::AreEntitiesEnemies;
    questState_type["GetNextInOpinionAttitudeGraph"] = &LuaQuestState::GetNextInOpinionAttitudeGraph;
    questState_type["GetOpinionAttitudeAsString"] = &LuaQuestState::GetOpinionAttitudeAsString;
    questState_type["EntityGetOpinionAttitudeToPlayer"] = &LuaQuestState::EntityGetOpinionAttitudeToPlayer;
    questState_type["EntityGetOpinionAttitudeToPlayerAsString"] = &LuaQuestState::EntityGetOpinionAttitudeToPlayerAsString;
    questState_type["EntityGetOpinionOfPlayer"] = &LuaQuestState::EntityGetOpinionOfPlayer;
    questState_type["EntitySetOpinionReactionMaskByInt"] = &LuaQuestState::EntitySetOpinionReactionMaskByInt;
    questState_type["EntitySetOpinionDeedMaskByString"] = &LuaQuestState::EntitySetOpinionDeedMaskByString;
    questState_type["EntitySetOpinionDeedMaskByInt"] = &LuaQuestState::EntitySetOpinionDeedMaskByInt;
    questState_type["EntitySetOpinionDeedTypeEnabled"] = &LuaQuestState::EntitySetOpinionDeedTypeEnabled;
    questState_type["EntitySetOpinionAttitudeEnabled"] = &LuaQuestState::EntitySetOpinionAttitudeEnabled;
    questState_type["EntitySetOpinionReactionEnabled"] = &LuaQuestState::EntitySetOpinionReactionEnabled;
    questState_type["EntitySetPersonalityOverrideByInt"] = &LuaQuestState::EntitySetPersonalityOverrideByInt;
    questState_type["EntitySetPersonalityOverrideByString"] = &LuaQuestState::EntitySetPersonalityOverrideByString;
    questState_type["EntityClearPersonalityOverride"] = &LuaQuestState::EntityClearPersonalityOverride;
    questState_type["EntitySetAsOpinionSourceByInt"] = &LuaQuestState::EntitySetAsOpinionSourceByInt;
    questState_type["EntitySetAsOpinionSourceByString"] = &LuaQuestState::EntitySetAsOpinionSourceByString;
    questState_type["EntityUnsetAsOpinionSource"] = sol::overload([](LuaQuestState& self, const std::shared_ptr<CScriptThing>& spThing) { self.EntityUnsetAsOpinionSource(spThing, sol::nullopt); },
        sol::resolve<void(const std::shared_ptr<CScriptThing>&, sol::optional<bool>)>(&LuaQuestState::EntityUnsetAsOpinionSource));
    questState_type["OpinionSourceSetAsExclusive"] = &LuaQuestState::OpinionSourceSetAsExclusive;
    questState_type["OpinionSourceSetAsAttentionGrabbing"] = &LuaQuestState::OpinionSourceSetAsAttentionGrabbing;
    questState_type["EntityPostOpinionDeedToAll"] = &LuaQuestState::EntityPostOpinionDeedToAll;
    questState_type["EntityPostOpinionDeedToRecipient"] = &LuaQuestState::EntityPostOpinionDeedToRecipient;
    questState_type["EntityPostOpinionDeedToRecipientVillage"] = &LuaQuestState::EntityPostOpinionDeedToRecipientVillage;
    questState_type["EntityPostOpinionDeedKeepSearchingForWitnesses"] = &LuaQuestState::EntityPostOpinionDeedKeepSearchingForWitnesses;
    questState_type["RemoveOpinionDeedStillSearchingForWitnesses"] = &LuaQuestState::RemoveOpinionDeedStillSearchingForWitnesses;
    questState_type["IsDeedWitnessed"] = &LuaQuestState::IsDeedWitnessed;
    questState_type["CanThingBe_Seen_ByOtherThing"] = &LuaQuestState::CanThingBe_Seen_ByOtherThing;
    questState_type["CanThingBe_NearlySeen_ByOtherThing"] = &LuaQuestState::CanThingBe_NearlySeen_ByOtherThing;
    questState_type["CanThingBe_Smelled_ByOtherThing"] = &LuaQuestState::CanThingBe_Smelled_ByOtherThing;
    questState_type["CanThingBe_Heard_ByOtherThing"] = &LuaQuestState::CanThingBe_Heard_ByOtherThing;
    questState_type["IsThingAwareOfOtherThingInAnyWay"] = &LuaQuestState::IsThingAwareOfOtherThingInAnyWay;
    questState_type["EntitySetAsAwareOfThing"] = &LuaQuestState::EntitySetAsAwareOfThing;
    questState_type["EntitySetSoundRadius"] = &LuaQuestState::EntitySetSoundRadius;
    questState_type["EntitySetSmellRadius"] = &LuaQuestState::EntitySetSmellRadius;
    questState_type["EntitySetSightRadius"] = &LuaQuestState::EntitySetSightRadius;
    questState_type["EntitySetExtendedSightRadius"] = &LuaQuestState::EntitySetExtendedSightRadius;
    questState_type["EntitySetGiveUpChaseRadius"] = &LuaQuestState::EntitySetGiveUpChaseRadius;
    questState_type["EntityGetHearingRadius"] = &LuaQuestState::EntityGetHearingRadius;
    questState_type["ManuallyTriggerTrap"] = &LuaQuestState::ManuallyTriggerTrap;
    questState_type["ManuallyResetTrap"] = &LuaQuestState::ManuallyResetTrap;
    questState_type["SetTimeOfDay"] = &LuaQuestState::SetTimeOfDay;
    questState_type["GetTimeOfDay"] = &LuaQuestState::GetTimeOfDay;
    questState_type["SetTimeAsStopped"] = &LuaQuestState::SetTimeAsStopped;
    questState_type["FastForwardTimeTo"] = &LuaQuestState::FastForwardTimeTo;
    questState_type["IsTimeOfDayBetween"] = &LuaQuestState::IsTimeOfDayBetween;
    questState_type["GetDayOfWeek"] = &LuaQuestState::GetDayOfWeek;
    questState_type["GetDayCount"] = &LuaQuestState::GetDayCount;
    questState_type["GetConstantFPS"] = &LuaQuestState::GetConstantFPS;
    questState_type["TransitionToTheme"] = &LuaQuestState::TransitionToTheme;
    questState_type["ResetToDefaultTheme"] = &LuaQuestState::ResetToDefaultTheme;
    questState_type["TransitionToThemeAllInternals"] = &LuaQuestState::TransitionToThemeAllInternals;
    questState_type["ResetToDefaultThemeAllInternals"] = &LuaQuestState::ResetToDefaultThemeAllInternals;
    questState_type["TransitionToThemeExternals"] = &LuaQuestState::TransitionToThemeExternals;
    questState_type["ResetToDefaultThemeExternals"] = &LuaQuestState::ResetToDefaultThemeExternals;
    questState_type["SetEnvironmentThemeWeightAllChannels"] = &LuaQuestState::SetEnvironmentThemeWeightAllChannels;
    questState_type["SetEnvironmentThemeWeightAllInternals"] = &LuaQuestState::SetEnvironmentThemeWeightAllInternals;
    questState_type["SetEnvironmentThemeWeightExternals"] = &LuaQuestState::SetEnvironmentThemeWeightExternals;
    questState_type["SetSoundThemesAsEnabledForRegion"] = &LuaQuestState::SetSoundThemesAsEnabledForRegion;
    questState_type["SetAllSoundsAsMuted"] = &LuaQuestState::SetAllSoundsAsMuted;
    questState_type["RadialBlurFadeTo"] = sol::overload(&LuaQuestState::RadialBlurFadeTo_NoPos,&LuaQuestState::RadialBlurFadeTo_WithPos);
    questState_type["RadialBlurFadeOut"] = &LuaQuestState::RadialBlurFadeOut;
    questState_type["IsRadialBlurFadeActive"] = &LuaQuestState::IsRadialBlurFadeActive;
    questState_type["CancelRadialBlurFade"] = &LuaQuestState::CancelRadialBlurFade;
    questState_type["RadialBlurSetCenterWorldPos"] = &LuaQuestState::RadialBlurSetCenterWorldPos;
    questState_type["DisplacementMonochromeEffectColourFadeTo"] = &LuaQuestState::DisplacementMonochromeEffectColourFadeTo;
    questState_type["DisplacementMonochromeEffectColourFadeOut"] = &LuaQuestState::DisplacementMonochromeEffectColourFadeOut;
    questState_type["ScreenFilterFadeTo"] = &LuaQuestState::ScreenFilterFadeTo;
    questState_type["ScreenFilterFadeOut"] = &LuaQuestState::ScreenFilterFadeOut;
    questState_type["SetThingAndCarriedItemsNotAffectedByScreenFilter"] = &LuaQuestState::SetThingAndCarriedItemsNotAffectedByScreenFilter;
    questState_type["UnSetThingAndCarriedItemsNotAffectedByScreenFilter"] = &LuaQuestState::UnSetThingAndCarriedItemsNotAffectedByScreenFilter;
    questState_type["IsGiftRomantic"] = &LuaQuestState::IsGiftRomantic;
    questState_type["IsGiftFriendly"] = &LuaQuestState::IsGiftFriendly;
    questState_type["IsGiftOffensive"] = &LuaQuestState::IsGiftOffensive;
    questState_type["IsThingABed"] = &LuaQuestState::IsThingABed;
    questState_type["IsThingAChest"] = &LuaQuestState::IsThingAChest;
    questState_type["IsThingADoor"] = &LuaQuestState::IsThingADoor;
    questState_type["IsThingSmashable"] = &LuaQuestState::IsThingSmashable;
    questState_type["IsThingSearchable"] = &LuaQuestState::IsThingSearchable;
    questState_type["ApplyScriptBrush"] = &LuaQuestState::ApplyScriptBrush;
    questState_type["EnableDecals"] = &LuaQuestState::EnableDecals;
    questState_type["PlayCriteriaSoundOnThing"] = &LuaQuestState::PlayCriteriaSoundOnThing;
    questState_type["IsSoundPlaying"] = &LuaQuestState::IsSoundPlaying;
    questState_type["EnableSounds"] = &LuaQuestState::EnableSounds;
    questState_type["StopSound"] = &LuaQuestState::StopSound;
    questState_type["CacheMusicSet"] = &LuaQuestState::CacheMusicSet;
    questState_type["EnableDangerMusic"] = &LuaQuestState::EnableDangerMusic;
    questState_type["IsDangerMusicEnabled"] = &LuaQuestState::IsDangerMusicEnabled;
    questState_type["StartCountdownTimer"] = &LuaQuestState::StartCountdownTimer;
    questState_type["GetCountdownTimer"] = &LuaQuestState::GetCountdownTimer;
    questState_type["AutoSaveCheckPoint"] = &LuaQuestState::AutoSaveCheckPoint;
    questState_type["AutoSaveQuestStart"] = &LuaQuestState::AutoSaveQuestStart;
    questState_type["AutoSave"] = &LuaQuestState::AutoSave;
    questState_type["SetSavingAsEnabled"] = &LuaQuestState::SetSavingAsEnabled;
    questState_type["IsSavingEnabled"] = &LuaQuestState::IsSavingEnabled;
    questState_type["SetSaveGameMarkerPos"] = &LuaQuestState::SetSaveGameMarkerPos;
    questState_type["ResetToFrontEnd"] = &LuaQuestState::ResetToFrontEnd;
    questState_type["SetGuildSealRecallLocation"] = &LuaQuestState::SetGuildSealRecallLocation;
    questState_type["GetGuildSealRecallPos"] = &LuaQuestState::GetGuildSealRecallPos;
    questState_type["GetGuildSealRecallAngleXY"] = &LuaQuestState::GetGuildSealRecallAngleXY;
    questState_type["SetReadableObjectText"] = &LuaQuestState::SetReadableObjectText;
    questState_type["SetReadableObjectTextTag"] = &LuaQuestState::SetReadableObjectTextTag;
    questState_type["RemoveRumourCategory"] = &LuaQuestState::RemoveRumourCategory;
    questState_type["SetCategoryActivity"] = &LuaQuestState::SetCategoryActivity;
    questState_type["AddGossipVillage"] = &LuaQuestState::AddGossipVillage;
    questState_type["SetIsGossipForPlayer_ByObject"] = &LuaQuestState::SetIsGossipForPlayer_ByObject;
    questState_type["SetIsGossipForPlayer_ByName"] = &LuaQuestState::SetIsGossipForPlayer_ByName;
    questState_type["SetIsGossipForPlayer"] = &LuaQuestState::SetIsGossipForPlayer_ByName;
    questState_type["GetBestTimePairs"] = &LuaQuestState::GetBestTimePairs;
    questState_type["GetBestTimeSorting"] = &LuaQuestState::GetBestTimeSorting;
    questState_type["GetBestScoreBlackjack"] = &LuaQuestState::GetBestScoreBlackjack;
    questState_type["GetBestScoreCoinGolfOakVale"] = &LuaQuestState::GetBestScoreCoinGolfOakVale;
    questState_type["GetBestScoreCoinGolfSnowSpire"] = &LuaQuestState::GetBestScoreCoinGolfSnowSpire;
    questState_type["GetBestScoreShoveHaPenny"] = &LuaQuestState::GetBestScoreShoveHaPenny;
    questState_type["GetBestTimeGuessTheAddition"] = &LuaQuestState::GetBestTimeGuessTheAddition;
    questState_type["IsHeroInTavernGame"] = &LuaQuestState::IsHeroInTavernGame;
    questState_type["GetNumHousesOwned"] = &LuaQuestState::GetNumHousesOwned;
    questState_type["StartSneaking"] = &LuaQuestState::StartSneaking;
    questState_type["GetStealDuration"] = &LuaQuestState::GetStealDuration;
    questState_type["SetUseableByHero"] = &LuaQuestState::SetUseableByHero;
    questState_type["SetOwnedByHero"] = &LuaQuestState::SetOwnedByHero;
    questState_type["SetTavernTableAvailableForUse"] = &LuaQuestState::SetTavernTableAvailableForUse;
    questState_type["SetIsThingTurncoatable"] = &LuaQuestState::SetIsThingTurncoatable;
    questState_type["SetIsThingForcePushable"] = &LuaQuestState::SetIsThingForcePushable;
    questState_type["SetIsThingLightningable"] = &LuaQuestState::SetIsThingLightningable;
    questState_type["SetIsThingEpicSpellable"] = &LuaQuestState::SetIsThingEpicSpellable;
    questState_type["IsThingTurncoated"] = &LuaQuestState::IsThingTurncoated;
    questState_type["AddCreatureScriptedMode"] = &LuaQuestState::AddCreatureScriptedMode;
    questState_type["RemoveCreatureScriptedMode"] = &LuaQuestState::RemoveCreatureScriptedMode;
    questState_type["ForceShipsVisible"] = &LuaQuestState::ForceShipsVisible;
    questState_type["GetSleepingPositionAndOrientationFromBed"] = &LuaQuestState::GetSleepingPositionAndOrientationFromBed;
    questState_type["SetBedAvailability"] = &LuaQuestState::SetBedAvailability;
    questState_type["RepopulateVillage"] = &LuaQuestState::RepopulateVillage;
    questState_type["SmashAllWindowsWithinRadiusOfPoint"] = &LuaQuestState::SmashAllWindowsWithinRadiusOfPoint;
    questState_type["SetResidency"] = &LuaQuestState::SetResidency;
    questState_type["SetThankingPhrase"] = &LuaQuestState::SetThankingPhrase;
    questState_type["GetThankingPhrase"] = &LuaQuestState::GetThankingPhrase;
    questState_type["ResetThankingPhrase"] = &LuaQuestState::ResetThankingPhrase;
    questState_type["SetIgnoringPhrase"] = &LuaQuestState::SetIgnoringPhrase;
    questState_type["GetIgnoringPhrase"] = &LuaQuestState::GetIgnoringPhrase;
    questState_type["ResetIgnoringPhrase"] = &LuaQuestState::ResetIgnoringPhrase;
    questState_type["SetWanderCentrePoint"] = &LuaQuestState::SetWanderCentrePoint;
    questState_type["GetWanderCentrePoint"] = &LuaQuestState::GetWanderCentrePoint;
    questState_type["ResetWanderCentrePoint"] = &LuaQuestState::ResetWanderCentrePoint;
    questState_type["SetWanderMinDistance"] = &LuaQuestState::SetWanderMinDistance;
    questState_type["GetWanderMinDistance"] = &LuaQuestState::GetWanderMinDistance;
    questState_type["ResetWanderMinDistance"] = &LuaQuestState::ResetWanderMinDistance;
    questState_type["SetWanderMaxDistance"] = &LuaQuestState::SetWanderMaxDistance;
    questState_type["GetWanderMaxDistance"] = &LuaQuestState::GetWanderMaxDistance;
    questState_type["ResetWanderMaxDistance"] = &LuaQuestState::ResetWanderMaxDistance;
    questState_type["SetGossipCounter"] = &LuaQuestState::SetGossipCounter;
    questState_type["GetGossipCounter"] = &LuaQuestState::GetGossipCounter;
    questState_type["ResetGossipCounter"] = &LuaQuestState::ResetGossipCounter;
    questState_type["SetMaxGossipPhrase"] = &LuaQuestState::SetMaxGossipPhrase;
    questState_type["GetMaxGossipPhrase"] = &LuaQuestState::GetMaxGossipPhrase;
    questState_type["ResetMaxGossipPhrase"] = &LuaQuestState::ResetMaxGossipPhrase;
    questState_type["SetWarningPhrase"] = &LuaQuestState::SetWarningPhrase;
    questState_type["GetWarningPhrase"] = &LuaQuestState::GetWarningPhrase;
    questState_type["ResetWarningPhrase"] = &LuaQuestState::ResetWarningPhrase;
    questState_type["SetBeerRequestPhrase"] = &LuaQuestState::SetBeerRequestPhrase;
    questState_type["GetBeerRequestPhrase"] = &LuaQuestState::GetBeerRequestPhrase;
    questState_type["ResetBeerRequestPhrase"] = &LuaQuestState::ResetBeerRequestPhrase;
    questState_type["SetScriptingStateGroup"] = &LuaQuestState::SetScriptingStateGroup;
    questState_type["GetScriptingStateGroup"] = &LuaQuestState::GetScriptingStateGroup;
    questState_type["ResetScriptingStateGroup"] = &LuaQuestState::ResetScriptingStateGroup;
    questState_type["SetMaxHeroReactionDistance"] = &LuaQuestState::SetMaxHeroReactionDistance;
    questState_type["GetMaxHeroReactionDistance"] = &LuaQuestState::GetMaxHeroReactionDistance;
    questState_type["ResetMaxHeroReactionDistance"] = &LuaQuestState::ResetMaxHeroReactionDistance;
    questState_type["SetActionFrequency"] = &LuaQuestState::SetActionFrequency;
    questState_type["GetActionFrequency"] = &LuaQuestState::GetActionFrequency;
    questState_type["ResetActionFrequency"] = &LuaQuestState::ResetActionFrequency;
    questState_type["SetActionFrequencyVariation"] = &LuaQuestState::SetActionFrequencyVariation;
    questState_type["GetActionFrequencyVariation"] = &LuaQuestState::GetActionFrequencyVariation;
    questState_type["ResetActionFrequencyVariation"] = &LuaQuestState::ResetActionFrequencyVariation;
    questState_type["SetAction"] = &LuaQuestState::SetAction;
    questState_type["GetAction"] = &LuaQuestState::GetAction;
    questState_type["ResetAction"] = &LuaQuestState::ResetAction;
    questState_type["SetFaceHeroForAction"] = &LuaQuestState::SetFaceHeroForAction;
    questState_type["GetFaceHeroForAction"] = &LuaQuestState::GetFaceHeroForAction;
    questState_type["ResetFaceHeroForAction"] = &LuaQuestState::ResetFaceHeroForAction;
    questState_type["SetTargetName"] = &LuaQuestState::SetTargetName;
    questState_type["GetTargetName"] = &LuaQuestState::GetTargetName;
    questState_type["ResetTargetName"] = &LuaQuestState::ResetTargetName;
    questState_type["SetFollowDistance"] = &LuaQuestState::SetFollowDistance;
    questState_type["GetFollowDistance"] = &LuaQuestState::GetFollowDistance;
    questState_type["ResetFollowDistance"] = &LuaQuestState::ResetFollowDistance;
    questState_type["SetAttackHeroOnSight"] = &LuaQuestState::SetAttackHeroOnSight;
    questState_type["GetAttackHeroOnSight"] = &LuaQuestState::GetAttackHeroOnSight;
    questState_type["ResetAttackHeroOnSight"] = &LuaQuestState::ResetAttackHeroOnSight;
    questState_type["SetTimeToSpendHarassingHero"] = &LuaQuestState::SetTimeToSpendHarassingHero;
    questState_type["GetTimeToSpendHarassingHero"] = &LuaQuestState::GetTimeToSpendHarassingHero;
    questState_type["ResetTimeToSpendHarassingHero"] = &LuaQuestState::ResetTimeToSpendHarassingHero;
    questState_type["SetCombatNearbyEnemyFleeingBreakOffRange"] = &LuaQuestState::SetCombatNearbyEnemyFleeingBreakOffRange;
    questState_type["GetCombatNearbyEnemyFleeingBreakOffRange"] = &LuaQuestState::GetCombatNearbyEnemyFleeingBreakOffRange;
    questState_type["ResetCombatNearbyEnemyFleeingBreakOffRange"] = &LuaQuestState::ResetCombatNearbyEnemyFleeingBreakOffRange;
    questState_type["SetCombatNearbyBreakOffRange"] = &LuaQuestState::SetCombatNearbyBreakOffRange;
    questState_type["GetCombatNearbyBreakOffRange"] = &LuaQuestState::GetCombatNearbyBreakOffRange;
    questState_type["ResetCombatNearbyBreakOffRange"] = &LuaQuestState::ResetCombatNearbyBreakOffRange;
    questState_type["SetStealStealableItems"] = &LuaQuestState::SetStealStealableItems;
    questState_type["GetStealStealableItems"] = &LuaQuestState::GetStealStealableItems;
    questState_type["ResetStealStealableItems"] = &LuaQuestState::ResetStealStealableItems;
    questState_type["SetRecoverStealableItems"] = &LuaQuestState::SetRecoverStealableItems;
    questState_type["GetRecoverStealableItems"] = &LuaQuestState::GetRecoverStealableItems;
    questState_type["ResetRecoverStealableItems"] = &LuaQuestState::ResetRecoverStealableItems;
    questState_type["SetTakeStealableItemToRandomDestination"] = &LuaQuestState::SetTakeStealableItemToRandomDestination;
    questState_type["GetTakeStealableItemToRandomDestination"] = &LuaQuestState::GetTakeStealableItemToRandomDestination;
    questState_type["ResetTakeStealableItemToRandomDestination"] = &LuaQuestState::ResetTakeStealableItemToRandomDestination;
    questState_type["SetKillSelfAndStealableItemAfterReachingDestination"] = &LuaQuestState::SetKillSelfAndStealableItemAfterReachingDestination;
    questState_type["GetKillSelfAndStealableItemAfterReachingDestination"] = &LuaQuestState::GetKillSelfAndStealableItemAfterReachingDestination;
    questState_type["ResetKillSelfAndStealableItemAfterReachingDestination"] = &LuaQuestState::ResetKillSelfAndStealableItemAfterReachingDestination;
    questState_type["SetAllowedToFollow"] = &LuaQuestState::SetAllowedToFollow;
    questState_type["GetAllowedToFollow"] = &LuaQuestState::GetAllowedToFollow;
    questState_type["ResetAllowedToFollow"] = &LuaQuestState::ResetAllowedToFollow;
    questState_type["SetTableName"] = &LuaQuestState::SetTableName;
    questState_type["GetTableName"] = &LuaQuestState::GetTableName;
    questState_type["ResetTableName"] = &LuaQuestState::ResetTableName;
    questState_type["SetSeatName"] = &LuaQuestState::SetSeatName;
    questState_type["GetSeatName"] = &LuaQuestState::GetSeatName;
    questState_type["ResetSeatName"] = &LuaQuestState::ResetSeatName;
    questState_type["SetDisableHeadLooking"] = &LuaQuestState::SetDisableHeadLooking;
    questState_type["GetDisableHeadLooking"] = &LuaQuestState::GetDisableHeadLooking;
    questState_type["ResetDisableHeadLooking"] = &LuaQuestState::ResetDisableHeadLooking;
    questState_type["SetIsPushableByHero"] = &LuaQuestState::SetIsPushableByHero;
    questState_type["GetIsPushableByHero"] = &LuaQuestState::GetIsPushableByHero;
    questState_type["ResetIsPushableByHero"] = &LuaQuestState::ResetIsPushableByHero;
    questState_type["SetLookForFiniteTime"] = &LuaQuestState::SetLookForFiniteTime;
    questState_type["GetLookForFiniteTime"] = &LuaQuestState::GetLookForFiniteTime;
    questState_type["ResetLookForFiniteTime"] = &LuaQuestState::ResetLookForFiniteTime;
    questState_type["SetAvoidRegionExits"] = &LuaQuestState::SetAvoidRegionExits;
    questState_type["GetAvoidRegionExits"] = &LuaQuestState::GetAvoidRegionExits;
    questState_type["ResetAvoidRegionExits"] = &LuaQuestState::ResetAvoidRegionExits;
    questState_type["SetTargetingDistanceOffset"] = &LuaQuestState::SetTargetingDistanceOffset;
    questState_type["GetTargetingDistanceOffset"] = &LuaQuestState::GetTargetingDistanceOffset;
    questState_type["ResetTargetingDistanceOffset"] = &LuaQuestState::ResetTargetingDistanceOffset;
    questState_type["SetPlayerUsingMeleeDummies"] = &LuaQuestState::SetPlayerUsingMeleeDummies;
    questState_type["GetPlayerUsingMeleeDummies"] = &LuaQuestState::GetPlayerUsingMeleeDummies;
    questState_type["SetPlayerUsingRangedDummies"] = &LuaQuestState::SetPlayerUsingRangedDummies;
    questState_type["GetPlayerUsingRangedDummies"] = &LuaQuestState::GetPlayerUsingRangedDummies;
    questState_type["SetPlayerUsingWillDummies"] = &LuaQuestState::SetPlayerUsingWillDummies;
    questState_type["GetPlayerUsingWillDummies"] = &LuaQuestState::GetPlayerUsingWillDummies;
    questState_type["SetCheapHeadLooking"] = &LuaQuestState::SetCheapHeadLooking;
    questState_type["GetCheapHeadLooking"] = &LuaQuestState::GetCheapHeadLooking;
    questState_type["SetQuitTavernGame"] = &LuaQuestState::SetQuitTavernGame;
    questState_type["GetQuitTavernGame"] = &LuaQuestState::GetQuitTavernGame;
    questState_type["SetPrizeTavernTable"] = &LuaQuestState::SetPrizeTavernTable;
    questState_type["GetPrizeTavernTable"] = &LuaQuestState::GetPrizeTavernTable;
    questState_type["SetBettingActive"] = &LuaQuestState::SetBettingActive;
    questState_type["GetBettingActive"] = &LuaQuestState::GetBettingActive;
    questState_type["SetBettingAccept"] = &LuaQuestState::SetBettingAccept;
    questState_type["GetBettingAccept"] = &LuaQuestState::GetBettingAccept;
    questState_type["SetBettingAmount"] = &LuaQuestState::SetBettingAmount;
    questState_type["GetBettingAmount"] = &LuaQuestState::GetBettingAmount;
    questState_type["SetCountBetMoneyDown"] = &LuaQuestState::SetCountBetMoneyDown;
    questState_type["GetCountBetMoneyDown"] = &LuaQuestState::GetCountBetMoneyDown;
    questState_type["SetSpotTheAdditionBeaten"] = &LuaQuestState::SetSpotTheAdditionBeaten;
    questState_type["GetSpotTheAdditionBeaten"] = &LuaQuestState::GetSpotTheAdditionBeaten;
    questState_type["SetGlobalTargetingDistanceOffset"] = &LuaQuestState::SetGlobalTargetingDistanceOffset;
    questState_type["GetGlobalTargetingDistanceOffset"] = &LuaQuestState::GetGlobalTargetingDistanceOffset;
    questState_type["SetTradingPriceMult"] = &LuaQuestState::SetTradingPriceMult;
    questState_type["GetTradingPriceMult"] = &LuaQuestState::GetTradingPriceMult;
    questState_type["SetBoastingEnabled"] = &LuaQuestState::SetBoastingEnabled;
    questState_type["GetBoastingEnabled"] = &LuaQuestState::GetBoastingEnabled;
    questState_type["SetActiveGossipCategories"] = &LuaQuestState::SetActiveGossipCategories;
    questState_type["GetActiveGossipCategories"] = sol::overload(
        sol::resolve<sol::object(sol::this_state)>(&LuaQuestState::GetActiveGossipCategories),
        sol::resolve<sol::object(const std::string&, sol::this_state)>(&LuaQuestState::GetActiveGossipCategories));
    questState_type["GetActiveGossipCategoriesSize"] = &LuaQuestState::GetActiveGossipCategoriesSize;
    questState_type["ClearActiveGossipCategories"] = &LuaQuestState::ClearActiveGossipCategories;
    questState_type["GetIsGossipForPlayer"] = sol::overload(
        sol::resolve<sol::object(sol::this_state)>(&LuaQuestState::GetIsGossipForPlayer),
        sol::resolve<sol::object(const std::string&, sol::this_state)>(&LuaQuestState::GetIsGossipForPlayer));
    questState_type["GetIsGossipForPlayerSize"] = &LuaQuestState::GetIsGossipForPlayerSize;
    questState_type["ClearIsGossipForPlayer"] = &LuaQuestState::ClearIsGossipForPlayer;
    questState_type["SetGossip"] = &LuaQuestState::SetGossip;
    questState_type["GetGossip"] = sol::overload(
        sol::resolve<sol::object(const std::string&, sol::this_state)>(&LuaQuestState::GetGossip),
        sol::resolve<std::string(const std::string&, int)>(&LuaQuestState::GetGossip));
    questState_type["GetGossipSize"] = &LuaQuestState::GetGossipSize;
    questState_type["ClearGossip"] = &LuaQuestState::ClearGossip;
    questState_type["RemoveGossip"] = &LuaQuestState::RemoveGossip;
    questState_type["AddGossip"] = sol::overload(
        sol::resolve<void(const std::string&)>(&LuaQuestState::AddGossip),
        sol::resolve<void(const std::string&, const std::string&)>(&LuaQuestState::AddGossip));
    questState_type["SetGossipVillages"] = &LuaQuestState::SetGossipVillages;
    questState_type["GetGossipVillages"] = sol::overload(
        sol::resolve<sol::object(const std::string&, sol::this_state)>(&LuaQuestState::GetGossipVillages),
        sol::resolve<std::string(const std::string&, int)>(&LuaQuestState::GetGossipVillages));
    questState_type["GetGossipVillagesSize"] = &LuaQuestState::GetGossipVillagesSize;
    questState_type["ClearGossipVillages"] = &LuaQuestState::ClearGossipVillages;
    questState_type["RemoveGossipVillages"] = &LuaQuestState::RemoveGossipVillages;
    questState_type["AddGossipVillages"] = sol::overload(
        sol::resolve<void(const std::string&)>(&LuaQuestState::AddGossipVillages),
        sol::resolve<void(const std::string&, const std::string&)>(&LuaQuestState::AddGossipVillages));
    questState_type["SetGossipFactions"] = &LuaQuestState::SetGossipFactions;
    questState_type["GetGossipFactions"] = sol::overload(
        sol::resolve<sol::object(const std::string&, sol::this_state)>(&LuaQuestState::GetGossipFactions),
        sol::resolve<std::string(const std::string&, int)>(&LuaQuestState::GetGossipFactions));
    questState_type["GetGossipFactionsSize"] = &LuaQuestState::GetGossipFactionsSize;
    questState_type["ClearGossipFactions"] = &LuaQuestState::ClearGossipFactions;
    questState_type["RemoveGossipFactions"] = &LuaQuestState::RemoveGossipFactions;
    questState_type["AddGossipFactions"] = sol::overload(
        sol::resolve<void(const std::string&)>(&LuaQuestState::AddGossipFactions),
        sol::resolve<void(const std::string&, const std::string&)>(&LuaQuestState::AddGossipFactions));
    questState_type["SetTrapAsActive"] = &LuaQuestState::SetTrapAsActive;
    questState_type["GetRandomThingWithScriptName"] = &LuaQuestState::GetRandomThingWithScriptName;
    questState_type["GetAllCreaturesExcludingHero"] = &LuaQuestState::GetAllCreaturesExcludingHero;
    questState_type["GetAllThingsWithDefNameByDistanceFrom"] = &LuaQuestState::GetAllThingsWithDefNameByDistanceFrom;
    questState_type["GetAllCreaturesInAreaWithScriptName"] = &LuaQuestState::GetAllCreaturesInAreaWithScriptName;
    questState_type["SetCreatureCreationDelayFrames"] = &LuaQuestState::SetCreatureCreationDelayFrames;
    questState_type["ResetCreatureCreationDelayFrames"] = &LuaQuestState::ResetCreatureCreationDelayFrames;
    questState_type["MsgOnFeatAccepted"] = &LuaQuestState::MsgOnFeatAccepted;
    questState_type["MsgIsBoastMade"] = &LuaQuestState::MsgIsBoastMade;
    questState_type["MsgOnBoastsMade"] = &LuaQuestState::MsgOnBoastsMade;
    questState_type["RemoveBoastMessage"] = &LuaQuestState::RemoveBoastMessage;
    questState_type["IsQuestStartScreenActive"] = &LuaQuestState::IsQuestStartScreenActive;
    questState_type["MsgOnLeavingQuestStartScreen"] = &LuaQuestState::MsgOnLeavingQuestStartScreen;
    questState_type["MsgOnLeavingExperienceSpendingScreen"] = &LuaQuestState::MsgOnLeavingExperienceSpendingScreen;
    questState_type["MsgIsQuestionAnsweredYesOrNo"] = &LuaQuestState::MsgIsQuestionAnsweredYesOrNo;
    questState_type["MsgIsGameInfoClickedPast"] = &LuaQuestState::MsgIsGameInfoClickedPast;
    questState_type["MsgIsTutorialClickedPast"] = &LuaQuestState::MsgIsTutorialClickedPast;
    questState_type["MsgIsActionModeButtonPressed"] = &LuaQuestState::MsgIsActionModeButtonPressed;
    questState_type["MsgOnExpressionPerformed"] = &LuaQuestState::MsgOnExpressionPerformed;
    questState_type["MsgIsCutSceneSkipped"] = &LuaQuestState::MsgIsCutSceneSkipped;
    questState_type["RemoveAllCutSceneSkippedMessages"] = &LuaQuestState::RemoveAllCutSceneSkippedMessages;
    questState_type["MsgOnChestOpeningCancelled"] = &LuaQuestState::MsgOnChestOpeningCancelled;
    questState_type["GetHeroHealth"] = &LuaQuestState::GetHeroHealth;
    questState_type["RespawnHero"] = &LuaQuestState::RespawnHero;
    questState_type["GetHeroHasCurrentMarriage"] = &LuaQuestState::GetHeroHasCurrentMarriage;
    questState_type["GetHeroHasDivorcedMarriage"] = &LuaQuestState::GetHeroHasDivorcedMarriage;
    questState_type["GetHeroHasChildren"] = &LuaQuestState::GetHeroHasChildren;
    questState_type["IsHeroChild"] = &LuaQuestState::IsHeroChild;
    questState_type["CancelHeroTeleportEffects"] = &LuaQuestState::CancelHeroTeleportEffects;
    questState_type["EntityFollowThing"] = &LuaQuestState::EntityFollowThing;
    questState_type["EntityStopFollowing"] = &LuaQuestState::EntityStopFollowing;
    questState_type["GetFollowingEntityList"] = &LuaQuestState::GetFollowingEntityList;
    questState_type["GetHeroSummonedCreaturesList"] = &LuaQuestState::GetHeroSummonedCreaturesList;
    questState_type["GetPerceivingHeroEntityList"] = &LuaQuestState::GetPerceivingHeroEntityList;
    questState_type["IsEntityFollowingHero"] = &LuaQuestState::IsEntityFollowingHero;
    questState_type["IsEntityAbleToAttack"] = &LuaQuestState::IsEntityAbleToAttack;
    questState_type["EntityGetThingInPrimarySlot"] = &LuaQuestState::EntityGetThingInPrimarySlot;
    questState_type["IsInCutscene"] = &LuaQuestState::IsInCutscene;
    questState_type["SetCreatureBrain"] = &LuaQuestState::SetCreatureBrain;

    lua.new_usertype<CPersistContext>("CPersistContext", sol::no_constructor);

    LogToFile("    'Quest','CScriptThing' and 'CPersistContext' usertypes created and bound in Lua.");
}

void LuaManager::Initialize() {
        if (m_initialized) {
            LogToFile("--- LuaManager already initialized, skipping ---");
            return;
        }

        LogToFile("--- Initializing LuaManager (in Isolated State mode) ---");

        m_initialized = true;

        LogToFile("    'LuaManager' initialized. States will be created per-script.");
    }

void LuaManager::Shutdown() {
    LogToFile("--- Shutting down LuaManager ---");

    ClearAllEntityData();

    {
        m_globalState.clear();
    }

    //m_entityAPI.SetGameInterface(nullptr);
    m_initialized = false;

    LogToFile("--- LuaManager shutdown complete ---");
}

void LuaManager::Reinitialize() {
    LogToFile("--- Reinitializing LuaManager (full reset) ---");

    Shutdown();

    // m_lua = sol::state(); // <-- REMOVE

    m_initialized = false;
    Initialize();

    LogToFile("--- LuaManager reinitialization complete ---");
}

void LuaManager::ClearAllEntityData() {
    LogToFile("--- Clearing all entity script data ---");
    size_t count = m_entityScriptDataMap.size();
    m_entityScriptDataMap.clear();
    LogToFile("--- Entity data cleared (" + std::to_string(count) + " entries removed) ---");
}

LuaManager& LuaManager::GetInstance() {
    static LuaManager instance;
    return instance;
}

void LuaManager::RegisterEntityScriptData(LuaEntityHost* pHost, const std::string& scriptName, LuaQuestState* pQuestState) {
    std::stringstream ss;
    ss << "    [LuaManager] Registered entity script data for host at 0x" << std::hex << (DWORD)pHost;
    LogToFile(ss.str());

    // Create a NEW, ISOLATED Lua state for this entity
    auto pNewState = std::make_unique<sol::state>();
    pNewState->open_libraries(sol::lib::base, sol::lib::string, sol::lib::math, sol::lib::table);

    // Register all C++ bindings in this new state
    RegisterBindingsInState(*pNewState);

    // Create an environment in THIS state
    sol::environment newEnv(*pNewState, sol::create, pNewState->globals());

    m_entityScriptDataMap[pHost] = EntityScriptData{
        scriptName,
        pQuestState,
        newEnv,
        sol::protected_function(),
        false,
        std::move(pNewState) // <-- Store the new state
    };
}

void LuaManager::UnregisterEntityScriptData(LuaEntityHost* pHost) {
    std::stringstream ss;
    ss << "    [LuaManager] Unregistered entity script data for host at 0x" << std::hex << (DWORD)pHost;
    LogToFile(ss.str());
    m_entityScriptDataMap.erase(pHost);
}

EntityScriptData* LuaManager::GetEntityScriptData(LuaEntityHost* pHost) {
    auto it = m_entityScriptDataMap.find(pHost);
    if (it != m_entityScriptDataMap.end()) {
        return &it->second;
    }
    return nullptr;
}

void LuaManager::SetGlobalStateBool(const std::string& key, bool value) {
    // std::lock_guard<std::mutex> lock(m_globalStateMutex); // <-- REMOVE
    m_globalState[key] = value;
}

void LuaManager::SetGlobalStateInt(const std::string& key, int value) {
    // std::lock_guard<std::mutex> lock(m_globalStateMutex); // <-- REMOVE
    m_globalState[key] = value;
}

void LuaManager::SetGlobalStateString(const std::string& key, const std::string& value) {
    // std::lock_guard<std::mutex> lock(m_globalStateMutex); // <-- REMOVE
    m_globalState[key] = value;
}

int LuaManager::GetGlobalStateInt(const std::string& key) {
    // std::lock_guard<std::mutex> lock(m_globalStateMutex); // <-- REMOVE
    auto it = m_globalState.find(key);
    if (it == m_globalState.end()) {
        return 0;
    }
    if (std::holds_alternative<int>(it->second)) {
        return std::get<int>(it->second);
    }
    return 0;
}

bool LuaManager::GetGlobalStateBool(const std::string& key) {
    // std::lock_guard<std::mutex> lock(m_globalStateMutex); // <-- REMOVE
    auto it = m_globalState.find(key);
    if (it == m_globalState.end()) {
        return false;
    }
    if (std::holds_alternative<bool>(it->second)) {
        return std::get<bool>(it->second);
    }
    return false;
}

std::string LuaManager::GetGlobalStateString(const std::string& key) {
    // std::lock_guard<std::mutex> lock(m_globalStateMutex); // <-- REMOVE
    auto it = m_globalState.find(key);
    if (it == m_globalState.end()) {
        return "";
    }
    if (std::holds_alternative<std::string>(it->second)) {
        return std::get<std::string>(it->second);
    }
    return "";
}
