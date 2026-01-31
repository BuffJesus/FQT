using System.Collections.Generic;
using FableQuestTool.Models;

namespace FableQuestTool.Data;

public static class NodeDefinitions
{
    public static List<NodeDefinition> GetAllNodes()
    {
        List<NodeDefinition> nodes = new();
        nodes.AddRange(GetTriggerNodes());
        nodes.AddRange(GetActionNodes());
        nodes.AddRange(GetConditionNodes());
        nodes.AddRange(GetFlowNodes());
        return nodes;
    }

    public static List<NodeDefinition> GetTriggerNodes()
    {
        return new List<NodeDefinition>
        {
            new() { Type = "onHeroTalks", Label = "When Hero Talks", Category = "trigger", Icon = "💬", IsAdvanced = false,
                Description = "Triggered when the hero initiates conversation with this entity",
                Properties = new(),
                CodeTemplate = "if Me:IsTalkedToByHero() then\n{CHILDREN}\nend" },
            
            new() { Type = "onHeroHits", Label = "When Hero Hits", Category = "trigger", Icon = "⚔️", IsAdvanced = false,
                Description = "Triggered when the hero hits this entity with any attack",
                Properties = new(),
                CodeTemplate = "if Me:MsgIsHitByHero() then\n{CHILDREN}\nend" },
            
            new() { Type = "onHeroHitsWithFlourish", Label = "When Flourish Hit", Category = "trigger", Icon = "✨", IsAdvanced = true,
                Description = "Triggered when the hero hits with a flourish attack",
                Properties = new(),
                CodeTemplate = "if Me:MsgIsHitByHeroWithFlourish() then\n{CHILDREN}\nend" },
            
            new() { Type = "onHeroHitsWithWeapon", Label = "When Hit With Weapon", Category = "trigger", Icon = "🗡️", IsAdvanced = true,
                Description = "Triggered when hit with a specific weapon type",
                Properties = new() { 
                    new NodeProperty { Name = "weaponName", Type = "string", Label = "Weapon Name", DefaultValue = "WEAPON_IRON_SWORD" }
                },
                CodeTemplate = "if Me:MsgIsHitByHeroWithWeapon(\"{weaponName}\") then\n{CHILDREN}\nend" },
            
            new() { Type = "onEntityKilled", Label = "When Killed", Category = "trigger", Icon = "💀", IsAdvanced = false,
                Description = "Triggered when this entity dies",
                Properties = new(),
                CodeTemplate = "if Me:IsDead() then\n{CHILDREN}\nend" },
            
            new() { Type = "onKilledByHero", Label = "When Killed By Hero", Category = "trigger", Icon = "⚰️", IsAdvanced = false,
                Description = "Triggered when killed specifically by the hero",
                Properties = new(),
                CodeTemplate = "if Me:IsKilledByHero() then\n{CHILDREN}\nend" },
            
            new() { Type = "onProximity", Label = "When Hero Nearby", Category = "trigger", Icon = "📍", IsAdvanced = false,
                Description = "Triggered when hero comes within specified distance",
                Properties = new() {
                    new NodeProperty { Name = "distance", Type = "float", Label = "Distance", DefaultValue = "5.0" }
                },
                CodeTemplate = "if Quest:IsDistanceBetweenThingsUnder(hero, Me, {distance}) then\n{CHILDREN}\nend" },
            
            new() { Type = "onItemPresented", Label = "When Item Given", Category = "trigger", Icon = "🎁", IsAdvanced = false,
                Description = "Triggered when hero presents an item to this entity",
                Properties = new() {
                    new NodeProperty { Name = "item", Type = "string", Label = "Item", DefaultValue = "OBJECT_APPLE", Options = new List<string>(GameData.Objects) }
                },
                CodeTemplate = "if Me:MsgIsPresentedWithItem() then\n{CHILDREN}\nend" },
            
            new() { Type = "onHeroUsed", Label = "When Used By Hero", Category = "trigger", Icon = "🔧", IsAdvanced = false,
                Description = "Triggered when hero uses/activates this entity",
                Properties = new(),
                CodeTemplate = "if Me:MsgIsUsedByHero() then\n{CHILDREN}\nend" },
            
            new() { Type = "onTriggered", Label = "When Triggered", Category = "trigger", Icon = "⚡", IsAdvanced = false,
                Description = "Triggered when hero enters a trigger region",
                Properties = new(),
                CodeTemplate = "if Me:MsgIsTriggeredByHero() then\n{CHILDREN}\nend" },
            
            new() { Type = "onKnockedOut", Label = "When Knocked Out", Category = "trigger", Icon = "😵", IsAdvanced = true,
                Description = "Triggered when this entity is knocked unconscious",
                Properties = new(),
                CodeTemplate = "if Me:MsgIsKnockedOutByHero() then\n{CHILDREN}\nend" },
            
            new() { Type = "onKicked", Label = "When Kicked", Category = "trigger", Icon = "🦶", IsAdvanced = true,
                Description = "Triggered when this entity is kicked",
                Properties = new(),
                CodeTemplate = "if Me:MsgIsKicked() then\n{CHILDREN}\nend" },
            
            new() { Type = "onAwareOfHero", Label = "When Aware of Hero", Category = "trigger", Icon = "👁️", IsAdvanced = true,
                Description = "Triggered when entity becomes aware of hero's presence",
                Properties = new(),
                CodeTemplate = "if Me:IsAwareOfHero() then\n{CHILDREN}\nend" },
            
            new() { Type = "onRegionLoaded", Label = "When Region Loads", Category = "trigger", Icon = "🗺️", IsAdvanced = true,
                Description = "Triggered when specified region loads",
                Properties = new() {
                    new NodeProperty { Name = "region", Type = "string", Label = "Region", DefaultValue = "Oakvale", Options = new List<string>(GameData.Regions) }
                },
                CodeTemplate = "if Quest:IsRegionLoaded(\"{region}\") then\n{CHILDREN}\nend" },
            
            new() { Type = "onStateChange", Label = "When State Changes", Category = "trigger", Icon = "🔄", IsAdvanced = true,
                Description = "Triggered when a state variable reaches a specific value",
                Properties = new() {
                    new NodeProperty { Name = "stateName", Type = "string", Label = "State Name", DefaultValue = "questStarted" },
                    new NodeProperty { Name = "value", Type = "string", Label = "Value", DefaultValue = "true" }
                },
                CodeTemplate = "if Quest:GetStateBool(\"{stateName}\") == {value} then\n{CHILDREN}\nend" }
        };
    }

    public static List<NodeDefinition> GetActionNodes()
    {
        return new List<NodeDefinition>
        {
            new() { Type = "showDialogue", Label = "Show Dialogue", Category = "action", Icon = "💬", IsAdvanced = false,
                Description = "Display dialogue text from this entity",
                Properties = new() {
                    new NodeProperty { Name = "text", Type = "text", Label = "Dialogue Text", DefaultValue = "Hello, hero!" }
                },
                CodeTemplate = "Me:SpeakAndWait(\"{text}\")" },
            
            new() { Type = "speakToHero", Label = "Speak To Hero", Category = "action", Icon = "🗣️", IsAdvanced = false,
                Description = "Entity speaks directly to the hero",
                Properties = new() {
                    new NodeProperty { Name = "text", Type = "text", Label = "Text", DefaultValue = "Greetings!" }
                },
                CodeTemplate = "Me:Speak(hero, \"{text}\")" },
            
            new() { Type = "giveReward", Label = "Give Reward", Category = "action", Icon = "💰", IsAdvanced = false,
                Description = "Give gold and/or items to hero",
                Properties = new() {
                    new NodeProperty { Name = "gold", Type = "int", Label = "Gold", DefaultValue = "100" },
                    new NodeProperty { Name = "item", Type = "string", Label = "Item (optional)", DefaultValue = "", Options = new List<string>(GameData.Objects) }
                },
                CodeTemplate = "Quest:GiveHeroGold({gold})\nif \"{item}\" ~= \"\" then Quest:GiveHeroObject(\"{item}\") end" },
            
            new() { Type = "giveItem", Label = "Give Item", Category = "action", Icon = "🎁", IsAdvanced = false,
                Description = "Give specific item(s) to hero",
                Properties = new() {
                    new NodeProperty { Name = "item", Type = "string", Label = "Item", DefaultValue = "OBJECT_HEALTH_POTION", Options = new List<string>(GameData.Objects) },
                    new NodeProperty { Name = "amount", Type = "int", Label = "Amount", DefaultValue = "1" }
                },
                CodeTemplate = "Quest:GiveHeroObject(\"{item}\", {amount})" },
            
            new() { Type = "takeItem", Label = "Take Item", Category = "action", Icon = "📤", IsAdvanced = false,
                Description = "Remove item from hero's inventory",
                Properties = new() {
                    new NodeProperty { Name = "item", Type = "string", Label = "Item", DefaultValue = "OBJECT_APPLE", Options = new List<string>(GameData.Objects) }
                },
                CodeTemplate = "Quest:TakeObjectFromHero(\"{item}\")" },
            
            new() { Type = "setState", Label = "Set State", Category = "action", Icon = "💾", IsAdvanced = false,
                Description = "Set a quest state variable",
                Properties = new() {
                    new NodeProperty { Name = "name", Type = "string", Label = "Variable Name", DefaultValue = "questStarted" },
                    new NodeProperty { Name = "value", Type = "string", Label = "Value", DefaultValue = "true" }
                },
                CodeTemplate = "Quest:SetStateBool(\"{name}\", {value})" },
            
            new() { Type = "setGlobal", Label = "Set Global", Category = "action", Icon = "🌍", IsAdvanced = true,
                Description = "Set a global game variable",
                Properties = new() {
                    new NodeProperty { Name = "name", Type = "string", Label = "Variable Name", DefaultValue = "globalEvent" },
                    new NodeProperty { Name = "value", Type = "string", Label = "Value", DefaultValue = "true" }
                },
                CodeTemplate = "Quest:SetGlobalBool(\"{name}\", {value})" },
            
            new() { Type = "becomeHostile", Label = "Become Hostile", Category = "action", Icon = "😡", IsAdvanced = false,
                Description = "Make this entity attack the hero",
                Properties = new(),
                CodeTemplate = "Quest:EntitySetThingAsEnemyOfThing(Me, hero)" },
            
            new() { Type = "becomeAlly", Label = "Become Ally", Category = "action", Icon = "🤝", IsAdvanced = false,
                Description = "Make this entity allied with the hero",
                Properties = new(),
                CodeTemplate = "Quest:EntitySetThingAsAllyOfThing(Me, hero)" },
            
            new() { Type = "followHero", Label = "Follow Hero", Category = "action", Icon = "👣", IsAdvanced = false,
                Description = "Entity follows the hero",
                Properties = new() {
                    new NodeProperty { Name = "distance", Type = "float", Label = "Follow Distance", DefaultValue = "2.0" }
                },
                CodeTemplate = "Me:FollowThing(hero, {distance}, true)" },
            
            new() { Type = "stopFollowing", Label = "Stop Following", Category = "action", Icon = "🛑", IsAdvanced = false,
                Description = "Stop following the hero",
                Properties = new(),
                CodeTemplate = "Me:StopFollowingThing(hero)" },
            
            new() { Type = "moveToMarker", Label = "Move To Marker", Category = "action", Icon = "📍", IsAdvanced = false,
                Description = "Move to a map marker",
                Properties = new() {
                    new NodeProperty { Name = "marker", Type = "string", Label = "Marker Name", DefaultValue = "MARKER_DESTINATION" }
                },
                CodeTemplate = "local marker = Quest:GetNamedThing(\"{marker}\")\nMe:MoveToThing(marker, 1.0, 1)" },
            
            new() { Type = "moveToPosition", Label = "Move To Position", Category = "action", Icon = "🎯", IsAdvanced = true,
                Description = "Move to specific coordinates",
                Properties = new() {
                    new NodeProperty { Name = "x", Type = "float", Label = "X", DefaultValue = "0" },
                    new NodeProperty { Name = "y", Type = "float", Label = "Y", DefaultValue = "0" },
                    new NodeProperty { Name = "z", Type = "float", Label = "Z", DefaultValue = "0" }
                },
                CodeTemplate = "Me:MoveToPosition({{x}={x}, {y}={y}, {z}={z}}, 1.0, 1)" },
            
            new() { Type = "teleportToMarker", Label = "Teleport To Marker", Category = "action", Icon = "⚡", IsAdvanced = true,
                Description = "Instantly teleport to a marker",
                Properties = new() {
                    new NodeProperty { Name = "marker", Type = "string", Label = "Marker Name", DefaultValue = "MARKER_SPAWN" }
                },
                CodeTemplate = "local marker = Quest:GetNamedThing(\"{marker}\")\nQuest:EntityTeleportToThing(Me, marker)" },
            
            new() { Type = "playAnimation", Label = "Play Animation", Category = "action", Icon = "🎬", IsAdvanced = true,
                Description = "Play an animation",
                Properties = new() {
                    new NodeProperty { Name = "anim", Type = "string", Label = "Animation", DefaultValue = "idle" },
                    new NodeProperty { Name = "wait", Type = "bool", Label = "Wait for Completion", DefaultValue = "true" }
                },
                CodeTemplate = "Me:PlayAnimation(\"{anim}\", {wait})" },
            
            new() { Type = "playLoopingAnim", Label = "Play Looping Anim", Category = "action", Icon = "🔁", IsAdvanced = true,
                Description = "Play animation multiple times",
                Properties = new() {
                    new NodeProperty { Name = "anim", Type = "string", Label = "Animation", DefaultValue = "idle" },
                    new NodeProperty { Name = "loops", Type = "int", Label = "Loop Count", DefaultValue = "1" }
                },
                CodeTemplate = "Me:PlayLoopingAnimation(\"{anim}\", {loops})" },
            
            new() { Type = "completeQuest", Label = "Complete Quest", Category = "action", Icon = "✅", IsAdvanced = false,
                Description = "Mark quest as completed",
                Properties = new() {
                    new NodeProperty { Name = "showScreen", Type = "bool", Label = "Show Completion Screen", DefaultValue = "true" }
                },
                CodeTemplate = "Quest:SetQuestAsCompleted(\"{QUEST_NAME}\", {showScreen}, false, false)" },
            
            new() { Type = "failQuest", Label = "Fail Quest", Category = "action", Icon = "❌", IsAdvanced = false,
                Description = "Mark quest as failed",
                Properties = new() {
                    new NodeProperty { Name = "message", Type = "text", Label = "Failure Message", DefaultValue = "Quest Failed" }
                },
                CodeTemplate = "Quest:SetQuestAsFailed(\"{QUEST_NAME}\", true, \"{message}\", true)" },
            
            new() { Type = "wait", Label = "Wait", Category = "action", Icon = "⏱️", IsAdvanced = false,
                Description = "Pause execution for specified seconds",
                Properties = new() {
                    new NodeProperty { Name = "seconds", Type = "float", Label = "Seconds", DefaultValue = "1.0" }
                },
                CodeTemplate = "Quest:Pause({seconds})" },
            
            new() { Type = "despawn", Label = "Remove Entity", Category = "action", Icon = "🗑️", IsAdvanced = false,
                Description = "Remove this entity from the world",
                Properties = new(),
                CodeTemplate = "Quest:RemoveThing(Me)" },
            
            new() { Type = "fadeOut", Label = "Fade Out", Category = "action", Icon = "🌑", IsAdvanced = true,
                Description = "Fade entity out over time",
                Properties = new() {
                    new NodeProperty { Name = "duration", Type = "float", Label = "Duration (seconds)", DefaultValue = "2.0" }
                },
                CodeTemplate = "Quest:EntityFadeOut(Me, {duration})" },
            
            new() { Type = "fadeIn", Label = "Fade In", Category = "action", Icon = "🌕", IsAdvanced = true,
                Description = "Fade entity in over time",
                Properties = new() {
                    new NodeProperty { Name = "duration", Type = "float", Label = "Duration (seconds)", DefaultValue = "2.0" }
                },
                CodeTemplate = "Quest:EntityFadeIn(Me, {duration})" },
            
            new() { Type = "setInvulnerable", Label = "Set Invulnerable", Category = "action", Icon = "🛡️", IsAdvanced = true,
                Description = "Make entity invulnerable or vulnerable",
                Properties = new() {
                    new NodeProperty { Name = "value", Type = "bool", Label = "Invulnerable", DefaultValue = "true" }
                },
                CodeTemplate = "Quest:EntitySetAsDamageable(Me, not {value})" },
            
            new() { Type = "setTargetable", Label = "Set Targetable", Category = "action", Icon = "🎯", IsAdvanced = true,
                Description = "Make entity targetable or not",
                Properties = new() {
                    new NodeProperty { Name = "value", Type = "bool", Label = "Targetable", DefaultValue = "true" }
                },
                CodeTemplate = "Quest:EntitySetTargetable(Me, {value})" },
            
            new() { Type = "sheatheWeapons", Label = "Sheathe Weapons", Category = "action", Icon = "⚔️", IsAdvanced = true,
                Description = "Put away weapons",
                Properties = new(),
                CodeTemplate = "Me:SheatheWeapons()" },
            
            new() { Type = "unsheatheWeapons", Label = "Unsheathe Weapons", Category = "action", Icon = "🗡️", IsAdvanced = true,
                Description = "Draw weapons",
                Properties = new(),
                CodeTemplate = "Me:UnsheatheWeapons()" },
            
            new() { Type = "showMessage", Label = "Show Message", Category = "action", Icon = "📨", IsAdvanced = false,
                Description = "Display on-screen message",
                Properties = new() {
                    new NodeProperty { Name = "text", Type = "text", Label = "Message", DefaultValue = "Objective Updated" },
                    new NodeProperty { Name = "duration", Type = "float", Label = "Duration", DefaultValue = "3.0" }
                },
                CodeTemplate = "Quest:ShowMessage(\"{text}\", {duration})" },
            
            new() { Type = "showTitleMessage", Label = "Show Title Message", Category = "action", Icon = "📢", IsAdvanced = false,
                Description = "Display large title message",
                Properties = new() {
                    new NodeProperty { Name = "text", Type = "text", Label = "Title", DefaultValue = "New Objective" },
                    new NodeProperty { Name = "duration", Type = "float", Label = "Duration", DefaultValue = "5.0" }
                },
                CodeTemplate = "Quest:AddScreenTitleMessage(\"{text}\", {duration}, true)" },
            
            new() { Type = "guildmasterMessage", Label = "Guildmaster Says", Category = "action", Icon = "🎓", IsAdvanced = true,
                Description = "Show message from guild master",
                Properties = new() {
                    new NodeProperty { Name = "key", Type = "string", Label = "Message Key", DefaultValue = "QUEST_MESSAGE" }
                },
                CodeTemplate = "Quest:HeroReceiveMessageFromGuildMaster(\"{key}\", \"Class\", true, true)" },
            
            new() { Type = "yesNoQuestion", Label = "Yes/No Question", Category = "action", Icon = "❓", IsAdvanced = false,
                Description = "Ask hero a yes/no question",
                Properties = new() {
                    new NodeProperty { Name = "question", Type = "text", Label = "Question", DefaultValue = "Do you accept?" },
                    new NodeProperty { Name = "yes", Type = "text", Label = "Yes Text", DefaultValue = "Yes" },
                    new NodeProperty { Name = "no", Type = "text", Label = "No Text", DefaultValue = "No" },
                    new NodeProperty { Name = "unsure", Type = "text", Label = "Unsure Text", DefaultValue = "I'm not sure" }
                },
                CodeTemplate = "local answer = Quest:GiveHeroYesNoQuestion(\"{question}\", \"{yes}\", \"{no}\", \"{unsure}\")" },
            
            new() { Type = "giveAbility", Label = "Give Ability", Category = "action", Icon = "✨", IsAdvanced = false,
                Description = "Grant hero a combat ability",
                Properties = new() {
                    new NodeProperty { Name = "abilityId", Type = "int", Label = "Ability ID", DefaultValue = "1", Options = new List<string>(GameData.Abilities) }
                },
                CodeTemplate = "Quest:GiveHeroAbility({abilityId}, true)" },
            
            new() { Type = "giveExpression", Label = "Give Expression", Category = "action", Icon = "😊", IsAdvanced = true,
                Description = "Unlock a hero expression",
                Properties = new() {
                    new NodeProperty { Name = "expression", Type = "string", Label = "Expression Name", DefaultValue = "LAUGH" },
                    new NodeProperty { Name = "level", Type = "int", Label = "Level", DefaultValue = "1" }
                },
                CodeTemplate = "Quest:GiveHeroExpression(\"{expression}\", {level})" },
            
            new() { Type = "giveMorality", Label = "Give Morality", Category = "action", Icon = "⚖️", IsAdvanced = false,
                Description = "Change hero's morality (+ good, - evil)",
                Properties = new() {
                    new NodeProperty { Name = "amount", Type = "float", Label = "Amount", DefaultValue = "10.0" }
                },
                CodeTemplate = "Quest:GiveHeroMorality({amount})" },
            
            new() { Type = "giveRenown", Label = "Give Renown", Category = "action", Icon = "⭐", IsAdvanced = false,
                Description = "Grant renown points",
                Properties = new() {
                    new NodeProperty { Name = "amount", Type = "int", Label = "Amount", DefaultValue = "50" }
                },
                CodeTemplate = "Quest:GiveHeroRenownPoints({amount})" },
            
            new() { Type = "giveExperience", Label = "Give Experience", Category = "action", Icon = "📈", IsAdvanced = false,
                Description = "Grant experience points",
                Properties = new() {
                    new NodeProperty { Name = "amount", Type = "int", Label = "Amount", DefaultValue = "100" }
                },
                CodeTemplate = "Quest:GiveHeroExperience({amount})" },
            
            new() { Type = "changeHealth", Label = "Change Health", Category = "action", Icon = "❤️", IsAdvanced = true,
                Description = "Modify hero's health",
                Properties = new() {
                    new NodeProperty { Name = "amount", Type = "float", Label = "Amount (+ heal, - damage)", DefaultValue = "50" },
                    new NodeProperty { Name = "canKill", Type = "bool", Label = "Can Kill", DefaultValue = "false" }
                },
                CodeTemplate = "Quest:ChangeHeroHealthBy({amount}, {canKill}, true)" },
            
            new() { Type = "cameraShake", Label = "Camera Shake", Category = "action", Icon = "📹", IsAdvanced = true,
                Description = "Shake the camera",
                Properties = new() {
                    new NodeProperty { Name = "intensity", Type = "float", Label = "Intensity", DefaultValue = "0.5" },
                    new NodeProperty { Name = "duration", Type = "float", Label = "Duration", DefaultValue = "1.0" }
                },
                CodeTemplate = "Quest:CameraShake({intensity}, {duration})" },
            
            new() { Type = "playSound", Label = "Play Sound", Category = "action", Icon = "🔊", IsAdvanced = true,
                Description = "Play a 2D sound effect",
                Properties = new() {
                    new NodeProperty { Name = "sound", Type = "string", Label = "Sound Name", DefaultValue = "SOUND_QUEST_COMPLETE" }
                },
                CodeTemplate = "Quest:Play2DSound(\"{sound}\")" },
            
            new() { Type = "createEffect", Label = "Create Effect", Category = "action", Icon = "💥", IsAdvanced = true,
                Description = "Create visual effect on entity",
                Properties = new() {
                    new NodeProperty { Name = "effect", Type = "string", Label = "Effect Name", DefaultValue = "SPARKLE" },
                    new NodeProperty { Name = "bone", Type = "string", Label = "Bone Name", DefaultValue = "" }
                },
                CodeTemplate = "Quest:CreateEffectOnThing(\"{effect}\", Me, \"{bone}\")" },
            
            new() { Type = "setReadableText", Label = "Set Readable Text", Category = "action", Icon = "📖", IsAdvanced = true,
                Description = "Set text for readable object",
                Properties = new() {
                    new NodeProperty { Name = "textKey", Type = "string", Label = "Text Key", DefaultValue = "TEXT_NOTE" }
                },
                CodeTemplate = "Me:SetReadableText(\"{textKey}\")" },
            
            new() { Type = "openDoor", Label = "Open Door", Category = "action", Icon = "🚪", IsAdvanced = true,
                Description = "Open a door object",
                Properties = new(),
                CodeTemplate = "Quest:OpenDoor(Me)" }
        };
    }

    public static List<NodeDefinition> GetConditionNodes()
    {
        return new List<NodeDefinition>
        {
            new() { Type = "checkState", Label = "Check State", Category = "condition", Icon = "?", IsAdvanced = false,
                Description = "Check if state variable equals value",
                Properties = new() {
                    new NodeProperty { Name = "name", Type = "string", Label = "State Name", DefaultValue = "questStarted" },
                    new NodeProperty { Name = "value", Type = "string", Label = "Expected Value", DefaultValue = "true" }
                },
                CodeTemplate = "if Quest:GetStateBool(\"{name}\") == {value} then\n{TRUE}\nelse\n{FALSE}\nend" },
            
            new() { Type = "checkGlobal", Label = "Check Global", Category = "condition", Icon = "?", IsAdvanced = true,
                Description = "Check if global variable equals value",
                Properties = new() {
                    new NodeProperty { Name = "name", Type = "string", Label = "Global Name", DefaultValue = "globalEvent" },
                    new NodeProperty { Name = "value", Type = "string", Label = "Expected Value", DefaultValue = "true" }
                },
                CodeTemplate = "if Quest:GetGlobalBool(\"{name}\") == {value} then\n{TRUE}\nelse\n{FALSE}\nend" },
            
            new() { Type = "checkHeroGold", Label = "Check Hero Gold", Category = "condition", Icon = "?", IsAdvanced = false,
                Description = "Check hero's gold amount",
                Properties = new() {
                    new NodeProperty { Name = "operator", Type = "select", Label = "Operator", DefaultValue = ">=", 
                        Options = new List<string> { "==", "!=", ">", ">=", "<", "<=" } },
                    new NodeProperty { Name = "amount", Type = "int", Label = "Gold Amount", DefaultValue = "100" }
                },
                CodeTemplate = "if Quest:GetHeroGold() {operator} {amount} then\n{TRUE}\nelse\n{FALSE}\nend" },
            
            new() { Type = "checkHeroMorality", Label = "Check Morality", Category = "condition", Icon = "?", IsAdvanced = true,
                Description = "Check hero's morality value",
                Properties = new() {
                    new NodeProperty { Name = "operator", Type = "select", Label = "Operator", DefaultValue = ">",
                        Options = new List<string> { "==", "!=", ">", ">=", "<", "<=" } },
                    new NodeProperty { Name = "threshold", Type = "float", Label = "Threshold", DefaultValue = "0" }
                },
                CodeTemplate = "if Quest:GetHeroMorality() {operator} {threshold} then\n{TRUE}\nelse\n{FALSE}\nend" },
            
            new() { Type = "checkHeroHealth", Label = "Check Health", Category = "condition", Icon = "?", IsAdvanced = true,
                Description = "Check hero's current health",
                Properties = new() {
                    new NodeProperty { Name = "operator", Type = "select", Label = "Operator", DefaultValue = "<",
                        Options = new List<string> { "==", "!=", ">", ">=", "<", "<=" } },
                    new NodeProperty { Name = "threshold", Type = "float", Label = "Threshold", DefaultValue = "50" }
                },
                CodeTemplate = "if Quest:GetHeroHealth() {operator} {threshold} then\n{TRUE}\nelse\n{FALSE}\nend" },
            
            new() { Type = "checkHasItem", Label = "Has Item", Category = "condition", Icon = "?", IsAdvanced = false,
                Description = "Check if hero has specific item",
                Properties = new() {
                    new NodeProperty { Name = "item", Type = "string", Label = "Item", DefaultValue = "OBJECT_APPLE", Options = new List<string>(GameData.Objects) }
                },
                CodeTemplate = "if Quest:DoesHeroHaveObject(\"{item}\") then\n{TRUE}\nelse\n{FALSE}\nend" },
            
            new() { Type = "checkIsAlive", Label = "Is Alive", Category = "condition", Icon = "?", IsAdvanced = false,
                Description = "Check if entity is alive",
                Properties = new(),
                CodeTemplate = "if Me:IsAlive() then\n{TRUE}\nelse\n{FALSE}\nend" },
            
            new() { Type = "checkYesNoAnswer", Label = "Check Answer", Category = "condition", Icon = "?", IsAdvanced = false,
                Description = "Check yes/no question answer (0=yes, 1=no, 2=unsure)",
                Properties = new() {
                    new NodeProperty { Name = "expectedAnswer", Type = "select", Label = "Expected", DefaultValue = "0",
                        Options = new List<string> { "0 (Yes)", "1 (No)", "2 (Unsure)" } }
                },
                CodeTemplate = "if answer == {expectedAnswer} then\n{TRUE}\nelse\n{FALSE}\nend" },
            
            new() { Type = "checkRegionLoaded", Label = "Region Loaded", Category = "condition", Icon = "?", IsAdvanced = true,
                Description = "Check if region is currently loaded",
                Properties = new() {
                    new NodeProperty { Name = "region", Type = "string", Label = "Region", DefaultValue = "Oakvale", Options = new List<string>(GameData.Regions) }
                },
                CodeTemplate = "if Quest:IsRegionLoaded(\"{region}\") then\n{TRUE}\nelse\n{FALSE}\nend" },
            
            new() { Type = "checkQuestComplete", Label = "Quest Complete", Category = "condition", Icon = "?", IsAdvanced = true,
                Description = "Check if another quest is completed",
                Properties = new() {
                    new NodeProperty { Name = "questName", Type = "string", Label = "Quest Name", DefaultValue = "QUEST_NAME" }
                },
                CodeTemplate = "if Quest:IsQuestComplete(\"{questName}\") then\n{TRUE}\nelse\n{FALSE}\nend" },
            
            new() { Type = "checkBoastTaken", Label = "Boast Taken", Category = "condition", Icon = "?", IsAdvanced = true,
                Description = "Check if boast was accepted",
                Properties = new() {
                    new NodeProperty { Name = "boastId", Type = "int", Label = "Boast ID", DefaultValue = "1" }
                },
                CodeTemplate = "if Quest:IsBoastTaken({boastId}, \"{QUEST_NAME}\") then\n{TRUE}\nelse\n{FALSE}\nend" }
        };
    }

    public static List<NodeDefinition> GetFlowNodes()
    {
        return new List<NodeDefinition>
        {
            new() { Type = "sequence", Label = "Sequence", Category = "flow", Icon = "→", IsAdvanced = false,
                Description = "Execute children in sequential order",
                Properties = new(),
                CodeTemplate = "{CHILDREN}" },
            
            new() { Type = "parallel", Label = "Parallel", Category = "flow", Icon = "⫼", IsAdvanced = true,
                Description = "Execute children simultaneously via threads",
                Properties = new(),
                CodeTemplate = "-- Parallel execution\n{CHILDREN}" },
            
            new() { Type = "loop", Label = "Loop", Category = "flow", Icon = "🔁", IsAdvanced = false,
                Description = "Repeat children N times",
                Properties = new() {
                    new NodeProperty { Name = "count", Type = "int", Label = "Loop Count", DefaultValue = "3" }
                },
                CodeTemplate = "for i = 1, {count} do\n{CHILDREN}\nend" },
            
            new() { Type = "whileLoop", Label = "While Loop", Category = "flow", Icon = "⟳", IsAdvanced = true,
                Description = "Repeat while condition is true",
                Properties = new() {
                    new NodeProperty { Name = "condition", Type = "text", Label = "Condition (Lua)", DefaultValue = "Me:IsAlive()" }
                },
                CodeTemplate = "while {condition} do\n{CHILDREN}\nif not Quest:Wait(0) then break end\nend" },
            
            new() { Type = "delay", Label = "Delay", Category = "flow", Icon = "⏱️", IsAdvanced = false,
                Description = "Wait before continuing",
                Properties = new() {
                    new NodeProperty { Name = "seconds", Type = "float", Label = "Seconds", DefaultValue = "1.0" }
                },
                CodeTemplate = "Quest:Pause({seconds})" },
            
            new() { Type = "randomChoice", Label = "Random Choice", Category = "flow", Icon = "🎲", IsAdvanced = true,
                Description = "Pick random branch based on weights",
                Properties = new() {
                    new NodeProperty { Name = "weights", Type = "text", Label = "Weights (comma separated)", DefaultValue = "1,1,1" }
                },
                CodeTemplate = "-- Random choice\nlocal choice = math.random(1, 3)\n{CHILDREN}" },
            
            new() { Type = "callFunction", Label = "Call Function", Category = "flow", Icon = "📞", IsAdvanced = true,
                Description = "Call a custom Lua function",
                Properties = new() {
                    new NodeProperty { Name = "functionName", Type = "string", Label = "Function Name", DefaultValue = "CustomFunction" }
                },
                CodeTemplate = "{functionName}()" }
        };
    }
}

public class NodeDefinition
{
    public string Type { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public bool IsAdvanced { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<NodeProperty> Properties { get; set; } = new();
    public string CodeTemplate { get; set; } = string.Empty;
}

public class NodeProperty
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public object? DefaultValue { get; set; }
    public List<string>? Options { get; set; }
}
