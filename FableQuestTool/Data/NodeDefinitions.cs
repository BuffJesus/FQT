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
                CodeTemplate = "Me:SpeakAndWait(\"{text}\")\n{CHILDREN}" },
            
            new() { Type = "speakToHero", Label = "Speak To Hero", Category = "action", Icon = "🗣️", IsAdvanced = false,
                Description = "Entity speaks directly to the hero",
                Properties = new() {
                    new NodeProperty { Name = "text", Type = "text", Label = "Text", DefaultValue = "Greetings!" }
                },
                CodeTemplate = "Me:Speak(hero, \"{text}\")\n{CHILDREN}" },

            new() { Type = "giveReward", Label = "Give Reward", Category = "action", Icon = "💰", IsAdvanced = false,
                Description = "Give gold and/or items to hero",
                Properties = new() {
                    new NodeProperty { Name = "gold", Type = "int", Label = "Gold", DefaultValue = "100" },
                    new NodeProperty { Name = "item", Type = "string", Label = "Item (optional)", DefaultValue = "", Options = new List<string>(GameData.Objects) }
                },
                CodeTemplate = "Quest:GiveHeroGold({gold})\nif \"{item}\" ~= \"\" then Quest:GiveHeroObject(\"{item}\") end\n{CHILDREN}" },

            new() { Type = "giveItem", Label = "Give Item", Category = "action", Icon = "🎁", IsAdvanced = false,
                Description = "Give specific item(s) to hero",
                Properties = new() {
                    new NodeProperty { Name = "item", Type = "string", Label = "Item", DefaultValue = "OBJECT_HEALTH_POTION", Options = new List<string>(GameData.Objects) },
                    new NodeProperty { Name = "amount", Type = "int", Label = "Amount", DefaultValue = "1" }
                },
                CodeTemplate = "Quest:GiveHeroObject(\"{item}\", {amount})\n{CHILDREN}" },

            new() { Type = "takeItem", Label = "Take Item", Category = "action", Icon = "📤", IsAdvanced = false,
                Description = "Remove item from hero's inventory",
                Properties = new() {
                    new NodeProperty { Name = "item", Type = "string", Label = "Item", DefaultValue = "OBJECT_APPLE", Options = new List<string>(GameData.Objects) }
                },
                CodeTemplate = "Quest:TakeObjectFromHero(\"{item}\")\n{CHILDREN}" },

            new() { Type = "setState", Label = "Set State", Category = "action", Icon = "💾", IsAdvanced = false,
                Description = "Set a quest state variable",
                Properties = new() {
                    new NodeProperty { Name = "name", Type = "string", Label = "Variable Name", DefaultValue = "questStarted" },
                    new NodeProperty { Name = "value", Type = "string", Label = "Value", DefaultValue = "true" }
                },
                CodeTemplate = "Quest:SetStateBool(\"{name}\", {value})\n{CHILDREN}" },

            new() { Type = "setGlobal", Label = "Set Global", Category = "action", Icon = "🌍", IsAdvanced = true,
                Description = "Set a global game variable",
                Properties = new() {
                    new NodeProperty { Name = "name", Type = "string", Label = "Variable Name", DefaultValue = "globalEvent" },
                    new NodeProperty { Name = "value", Type = "string", Label = "Value", DefaultValue = "true" }
                },
                CodeTemplate = "Quest:SetGlobalBool(\"{name}\", {value})\n{CHILDREN}" },

            new() { Type = "becomeHostile", Label = "Become Hostile", Category = "action", Icon = "😡", IsAdvanced = false,
                Description = "Make this entity attack the hero",
                Properties = new(),
                CodeTemplate = "Quest:EntitySetThingAsEnemyOfThing(Me, hero)\n{CHILDREN}" },

            new() { Type = "becomeAlly", Label = "Become Ally", Category = "action", Icon = "🤝", IsAdvanced = false,
                Description = "Make this entity allied with the hero",
                Properties = new(),
                CodeTemplate = "Quest:EntitySetThingAsAllyOfThing(Me, hero)\n{CHILDREN}" },

            new() { Type = "followHero", Label = "Follow Hero", Category = "action", Icon = "👣", IsAdvanced = false,
                Description = "Entity follows the hero",
                Properties = new() {
                    new NodeProperty { Name = "distance", Type = "float", Label = "Follow Distance", DefaultValue = "2.0" }
                },
                CodeTemplate = "Me:FollowThing(hero, {distance}, true)\n{CHILDREN}" },

            new() { Type = "stopFollowing", Label = "Stop Following", Category = "action", Icon = "🛑", IsAdvanced = false,
                Description = "Stop following the hero",
                Properties = new(),
                CodeTemplate = "Me:StopFollowingThing(hero)\n{CHILDREN}" },

            new() { Type = "moveToMarker", Label = "Move To Marker", Category = "action", Icon = "📍", IsAdvanced = false,
                Description = "Move to a map marker",
                Properties = new() {
                    new NodeProperty { Name = "marker", Type = "string", Label = "Marker Name", DefaultValue = "MARKER_DESTINATION" }
                },
                CodeTemplate = "local marker = Quest:GetNamedThing(\"{marker}\")\nMe:MoveToThing(marker, 1.0, 1)\n{CHILDREN}" },

            new() { Type = "moveToPosition", Label = "Move To Position", Category = "action", Icon = "🎯", IsAdvanced = true,
                Description = "Move to specific coordinates",
                Properties = new() {
                    new NodeProperty { Name = "x", Type = "float", Label = "X", DefaultValue = "0" },
                    new NodeProperty { Name = "y", Type = "float", Label = "Y", DefaultValue = "0" },
                    new NodeProperty { Name = "z", Type = "float", Label = "Z", DefaultValue = "0" }
                },
                CodeTemplate = "Me:MoveToPosition({x={x}, y={y}, z={z}}, 1.0, 1)\n{CHILDREN}" },

            new() { Type = "teleportToMarker", Label = "Teleport To Marker", Category = "action", Icon = "⚡", IsAdvanced = true,
                Description = "Instantly teleport to a marker",
                Properties = new() {
                    new NodeProperty { Name = "marker", Type = "string", Label = "Marker Name", DefaultValue = "MARKER_SPAWN" }
                },
                CodeTemplate = "local marker = Quest:GetNamedThing(\"{marker}\")\nQuest:EntityTeleportToThing(Me, marker)\n{CHILDREN}" },

            new() { Type = "playAnimation", Label = "Play Animation", Category = "action", Icon = "🎬", IsAdvanced = true,
                Description = "Play an animation",
                Properties = new() {
                    new NodeProperty { Name = "anim", Type = "string", Label = "Animation", DefaultValue = "idle" },
                    new NodeProperty { Name = "wait", Type = "bool", Label = "Wait for Completion", DefaultValue = "true" }
                },
                CodeTemplate = "Me:PlayAnimation(\"{anim}\", {wait})\n{CHILDREN}" },

            new() { Type = "playLoopingAnim", Label = "Play Looping Anim", Category = "action", Icon = "🔁", IsAdvanced = true,
                Description = "Play animation multiple times",
                Properties = new() {
                    new NodeProperty { Name = "anim", Type = "string", Label = "Animation", DefaultValue = "idle" },
                    new NodeProperty { Name = "loops", Type = "int", Label = "Loop Count", DefaultValue = "1" }
                },
                CodeTemplate = "Me:PlayLoopingAnimation(\"{anim}\", {loops})\n{CHILDREN}" },
            
            new() { Type = "completeQuest", Label = "Complete Quest", Category = "action", Icon = "✅", IsAdvanced = false,
                Description = "Mark quest as completed and give rewards (Note: Rewards must be configured in quest settings)",
                Properties = new() {
                    new NodeProperty { Name = "showScreen", Type = "bool", Label = "Show Completion Screen", DefaultValue = "true" }
                },
                CodeTemplate = "Quest:SetStateBool(\"QuestCompleted\", true)\n{CHILDREN}" },
            
            new() { Type = "failQuest", Label = "Fail Quest", Category = "action", Icon = "❌", IsAdvanced = false,
                Description = "Mark quest as failed",
                Properties = new() {
                    new NodeProperty { Name = "message", Type = "text", Label = "Failure Message", DefaultValue = "Quest Failed" }
                },
                CodeTemplate = "Quest:SetQuestAsFailed(\"{QUEST_NAME}\", true, \"{message}\", true)\n{CHILDREN}" },
            
            new() { Type = "wait", Label = "Wait", Category = "action", Icon = "⏱️", IsAdvanced = false,
                Description = "Pause execution for specified seconds",
                Properties = new() {
                    new NodeProperty { Name = "seconds", Type = "float", Label = "Seconds", DefaultValue = "1.0" }
                },
                CodeTemplate = "Quest:Pause({seconds})\n{CHILDREN}" },
            
            new() { Type = "despawn", Label = "Remove Entity", Category = "action", Icon = "🗑️", IsAdvanced = false,
                Description = "Remove this entity from the world",
                Properties = new(),
                CodeTemplate = "Quest:RemoveThing(Me)\n{CHILDREN}" },
            
            new() { Type = "fadeOut", Label = "Fade Out", Category = "action", Icon = "🌑", IsAdvanced = true,
                Description = "Fade entity out over time",
                Properties = new() {
                    new NodeProperty { Name = "duration", Type = "float", Label = "Duration (seconds)", DefaultValue = "2.0" }
                },
                CodeTemplate = "Quest:EntityFadeOut(Me, {duration})\n{CHILDREN}" },
            
            new() { Type = "fadeIn", Label = "Fade In", Category = "action", Icon = "🌕", IsAdvanced = true,
                Description = "Fade entity in over time",
                Properties = new() {
                    new NodeProperty { Name = "duration", Type = "float", Label = "Duration (seconds)", DefaultValue = "2.0" }
                },
                CodeTemplate = "Quest:EntityFadeIn(Me, {duration})\n{CHILDREN}" },
            
            new() { Type = "setInvulnerable", Label = "Set Invulnerable", Category = "action", Icon = "🛡️", IsAdvanced = true,
                Description = "Make entity invulnerable or vulnerable",
                Properties = new() {
                    new NodeProperty { Name = "value", Type = "bool", Label = "Invulnerable", DefaultValue = "true" }
                },
                CodeTemplate = "Quest:EntitySetAsDamageable(Me, not {value})\n{CHILDREN}" },
            
            new() { Type = "setTargetable", Label = "Set Targetable", Category = "action", Icon = "🎯", IsAdvanced = true,
                Description = "Make entity targetable or not",
                Properties = new() {
                    new NodeProperty { Name = "value", Type = "bool", Label = "Targetable", DefaultValue = "true" }
                },
                CodeTemplate = "Quest:EntitySetTargetable(Me, {value})\n{CHILDREN}" },
            
            new() { Type = "sheatheWeapons", Label = "Sheathe Weapons", Category = "action", Icon = "⚔️", IsAdvanced = true,
                Description = "Put away weapons",
                Properties = new(),
                CodeTemplate = "Me:SheatheWeapons()\n{CHILDREN}" },
            
            new() { Type = "unsheatheWeapons", Label = "Unsheathe Weapons", Category = "action", Icon = "🗡️", IsAdvanced = true,
                Description = "Draw weapons",
                Properties = new(),
                CodeTemplate = "Me:UnsheatheWeapons()\n{CHILDREN}" },
            
            new() { Type = "showMessage", Label = "Show Message", Category = "action", Icon = "📨", IsAdvanced = false,
                Description = "Display on-screen message",
                Properties = new() {
                    new NodeProperty { Name = "text", Type = "text", Label = "Message", DefaultValue = "Objective Updated" },
                    new NodeProperty { Name = "duration", Type = "float", Label = "Duration", DefaultValue = "3.0" }
                },
                CodeTemplate = "Quest:ShowMessage(\"{text}\", {duration})\n{CHILDREN}" },
            
            new() { Type = "showTitleMessage", Label = "Show Title Message", Category = "action", Icon = "📢", IsAdvanced = false,
                Description = "Display large title message",
                Properties = new() {
                    new NodeProperty { Name = "text", Type = "text", Label = "Title", DefaultValue = "New Objective" },
                    new NodeProperty { Name = "duration", Type = "float", Label = "Duration", DefaultValue = "5.0" }
                },
                CodeTemplate = "Quest:AddScreenTitleMessage(\"{text}\", {duration}, true)\n{CHILDREN}" },
            
            new() { Type = "guildmasterMessage", Label = "Guildmaster Says", Category = "action", Icon = "🎓", IsAdvanced = true,
                Description = "Show message from guild master",
                Properties = new() {
                    new NodeProperty { Name = "key", Type = "string", Label = "Message Key", DefaultValue = "QUEST_MESSAGE" }
                },
                CodeTemplate = "Quest:HeroReceiveMessageFromGuildMaster(\"{key}\", \"Class\", true, true)\n{CHILDREN}" },
            
            new() { Type = "yesNoQuestion", Label = "Yes/No Question", Category = "action", Icon = "❓", IsAdvanced = false,
                Description = "Ask hero a yes/no question",
                Properties = new() {
                    new NodeProperty { Name = "question", Type = "text", Label = "Question", DefaultValue = "Do you accept?" },
                    new NodeProperty { Name = "yes", Type = "text", Label = "Yes Text", DefaultValue = "Yes" },
                    new NodeProperty { Name = "no", Type = "text", Label = "No Text", DefaultValue = "No" },
                    new NodeProperty { Name = "unsure", Type = "text", Label = "Unsure Text", DefaultValue = "I'm not sure" }
                },
                CodeTemplate = "local answer = Quest:GiveHeroYesNoQuestion(\"{question}\", \"{yes}\", \"{no}\", \"{unsure}\")\n{CHILDREN}" },
            
            new() { Type = "giveAbility", Label = "Give Ability", Category = "action", Icon = "✨", IsAdvanced = false,
                Description = "Grant hero a combat ability",
                Properties = new() {
                    new NodeProperty { Name = "abilityId", Type = "int", Label = "Ability ID", DefaultValue = "1", Options = new List<string>(GameData.Abilities) }
                },
                CodeTemplate = "Quest:GiveHeroAbility({abilityId}, true)\n{CHILDREN}" },
            
            new() { Type = "giveExpression", Label = "Give Expression", Category = "action", Icon = "😊", IsAdvanced = true,
                Description = "Unlock a hero expression",
                Properties = new() {
                    new NodeProperty { Name = "expression", Type = "string", Label = "Expression Name", DefaultValue = "LAUGH" },
                    new NodeProperty { Name = "level", Type = "int", Label = "Level", DefaultValue = "1" }
                },
                CodeTemplate = "Quest:GiveHeroExpression(\"{expression}\", {level})\n{CHILDREN}" },
            
            new() { Type = "giveMorality", Label = "Give Morality", Category = "action", Icon = "⚖️", IsAdvanced = false,
                Description = "Change hero's morality (+ good, - evil)",
                Properties = new() {
                    new NodeProperty { Name = "amount", Type = "float", Label = "Amount", DefaultValue = "10.0" }
                },
                CodeTemplate = "Quest:GiveHeroMorality({amount})\n{CHILDREN}" },
            
            new() { Type = "giveRenown", Label = "Give Renown", Category = "action", Icon = "⭐", IsAdvanced = false,
                Description = "Grant renown points",
                Properties = new() {
                    new NodeProperty { Name = "amount", Type = "int", Label = "Amount", DefaultValue = "50" }
                },
                CodeTemplate = "Quest:GiveHeroRenownPoints({amount})\n{CHILDREN}" },
            
            new() { Type = "giveExperience", Label = "Give Experience", Category = "action", Icon = "📈", IsAdvanced = false,
                Description = "Grant experience points",
                Properties = new() {
                    new NodeProperty { Name = "amount", Type = "int", Label = "Amount", DefaultValue = "100" }
                },
                CodeTemplate = "Quest:GiveHeroExperience({amount})\n{CHILDREN}" },
            
            new() { Type = "changeHealth", Label = "Change Health", Category = "action", Icon = "❤️", IsAdvanced = true,
                Description = "Modify hero's health",
                Properties = new() {
                    new NodeProperty { Name = "amount", Type = "float", Label = "Amount (+ heal, - damage)", DefaultValue = "50" },
                    new NodeProperty { Name = "canKill", Type = "bool", Label = "Can Kill", DefaultValue = "false" }
                },
                CodeTemplate = "Quest:ChangeHeroHealthBy({amount}, {canKill}, true)\n{CHILDREN}" },
            
            new() { Type = "cameraShake", Label = "Camera Shake", Category = "action", Icon = "📹", IsAdvanced = true,
                Description = "Shake the camera",
                Properties = new() {
                    new NodeProperty { Name = "intensity", Type = "float", Label = "Intensity", DefaultValue = "0.5" },
                    new NodeProperty { Name = "duration", Type = "float", Label = "Duration", DefaultValue = "1.0" }
                },
                CodeTemplate = "Quest:CameraShake({intensity}, {duration})\n{CHILDREN}" },
            
            new() { Type = "playSound", Label = "Play Sound", Category = "action", Icon = "🔊", IsAdvanced = true,
                Description = "Play a 2D sound effect",
                Properties = new() {
                    new NodeProperty { Name = "sound", Type = "string", Label = "Sound Name", DefaultValue = "SOUND_QUEST_COMPLETE" }
                },
                CodeTemplate = "Quest:Play2DSound(\"{sound}\")\n{CHILDREN}" },
            
            new() { Type = "createEffect", Label = "Create Effect", Category = "action", Icon = "💥", IsAdvanced = true,
                Description = "Create visual effect on entity",
                Properties = new() {
                    new NodeProperty { Name = "effect", Type = "string", Label = "Effect Name", DefaultValue = "SPARKLE" },
                    new NodeProperty { Name = "bone", Type = "string", Label = "Bone Name", DefaultValue = "" }
                },
                CodeTemplate = "Quest:CreateEffectOnThing(\"{effect}\", Me, \"{bone}\")\n{CHILDREN}" },
            
            new() { Type = "setReadableText", Label = "Set Readable Text", Category = "action", Icon = "📖", IsAdvanced = true,
                Description = "Set text for readable object",
                Properties = new() {
                    new NodeProperty { Name = "textKey", Type = "string", Label = "Text Key", DefaultValue = "TEXT_NOTE" }
                },
                CodeTemplate = "Me:SetReadableText(\"{textKey}\")\n{CHILDREN}" },
            
            new() { Type = "openDoor", Label = "Open Door", Category = "action", Icon = "🚪", IsAdvanced = true,
                Description = "Open a door object",
                Properties = new(),
                CodeTemplate = "Quest:OpenDoor(Me)\n{CHILDREN}" },

            // ===== CONVERSATION SYSTEM =====
            new() { Type = "startConversation", Label = "Start Conversation", Category = "action", Icon = "🎭", IsAdvanced = false,
                Description = "Begin a multi-line conversation (stores conversation ID for adding lines)",
                Properties = new() {
                    new NodeProperty { Name = "use2DSound", Type = "bool", Label = "Use 2D Sound (no spatial)", DefaultValue = "true" },
                    new NodeProperty { Name = "playInCutscene", Type = "bool", Label = "Play During Cutscene", DefaultValue = "false" }
                },
                CodeTemplate = "local convoId = Quest:StartAmbientConversation(Me, hero, {use2DSound}, {playInCutscene})\n{CHILDREN}" },

            new() { Type = "addConversationLine", Label = "Add Conversation Line", Category = "action", Icon = "💭", IsAdvanced = false,
                Description = "Add a dialogue line to current conversation (use after Start Conversation)",
                Properties = new() {
                    new NodeProperty { Name = "textKey", Type = "string", Label = "Text/Voice Key", DefaultValue = "DIALOGUE_LINE_001" },
                    new NodeProperty { Name = "showSubtitle", Type = "bool", Label = "Show Subtitle", DefaultValue = "true" }
                },
                CodeTemplate = "Quest:AddLineToConversation(convoId, \"{textKey}\", Me, hero, {showSubtitle})\n{CHILDREN}" },

            new() { Type = "endConversation", Label = "End Conversation", Category = "action", Icon = "🔚", IsAdvanced = false,
                Description = "End the current conversation",
                Properties = new() {
                    new NodeProperty { Name = "immediate", Type = "bool", Label = "End Immediately", DefaultValue = "false" }
                },
                CodeTemplate = "Quest:RemoveConversation(convoId, {immediate})\n{CHILDREN}" },

            new() { Type = "speakWithOptions", Label = "Speak (Advanced)", Category = "action", Icon = "🎤", IsAdvanced = true,
                Description = "Entity speaks with full audio and display options",
                Properties = new() {
                    new NodeProperty { Name = "text", Type = "text", Label = "Text/Voice Key", DefaultValue = "Hello, hero!" },
                    new NodeProperty { Name = "use2DSound", Type = "bool", Label = "Use 2D Sound", DefaultValue = "false" },
                    new NodeProperty { Name = "fadeScreen", Type = "bool", Label = "Fade Screen", DefaultValue = "false" },
                    new NodeProperty { Name = "blocking", Type = "bool", Label = "Wait for Completion", DefaultValue = "true" }
                },
                CodeTemplate = "Me:Speak(hero, \"{text}\", {use2DSound}, {fadeScreen}, {blocking})\n{CHILDREN}" },

            new() { Type = "narratorSpeak", Label = "Narrator Speaks", Category = "action", Icon = "📢", IsAdvanced = true,
                Description = "Play narration voice (entity marked as narrator)",
                Properties = new() {
                    new NodeProperty { Name = "textKey", Type = "string", Label = "Narration Key", DefaultValue = "NARRATION_001" }
                },
                CodeTemplate = "Quest:EntitySetWillBeUsingNarrator(Me, true)\nMe:SpeakAndWait(\"{textKey}\")\n{CHILDREN}" },

            // ===== AUDIO SYSTEM =====
            new() { Type = "play2DSound", Label = "Play 2D Sound", Category = "action", Icon = "🔈", IsAdvanced = false,
                Description = "Play non-spatial (2D) sound effect",
                Properties = new() {
                    new NodeProperty { Name = "sound", Type = "string", Label = "Sound Name", DefaultValue = "SOUND_UI_CLICK" }
                },
                CodeTemplate = "local soundId = Quest:Play2DSound(\"{sound}\")\n{CHILDREN}" },

            new() { Type = "playSoundAtPosition", Label = "Play Sound At Position", Category = "action", Icon = "📍🔊", IsAdvanced = true,
                Description = "Play sound at specific world coordinates",
                Properties = new() {
                    new NodeProperty { Name = "sound", Type = "string", Label = "Sound Name", DefaultValue = "SOUND_EXPLOSION" },
                    new NodeProperty { Name = "x", Type = "float", Label = "X", DefaultValue = "0" },
                    new NodeProperty { Name = "y", Type = "float", Label = "Y", DefaultValue = "0" },
                    new NodeProperty { Name = "z", Type = "float", Label = "Z", DefaultValue = "0" }
                },
                CodeTemplate = "local soundId = Quest:PlaySoundAtPos(\"{sound}\", {x={x}, y={y}, z={z}})\n{CHILDREN}" },

            new() { Type = "playSoundOnEntity", Label = "Play Sound On Entity", Category = "action", Icon = "🔊", IsAdvanced = false,
                Description = "Play spatial sound attached to this entity",
                Properties = new() {
                    new NodeProperty { Name = "sound", Type = "string", Label = "Sound Name", DefaultValue = "SOUND_FOOTSTEP" }
                },
                CodeTemplate = "local soundId = Quest:PlaySoundOnThing(\"{sound}\", Me)\n{CHILDREN}" },

            new() { Type = "stopSound", Label = "Stop Sound", Category = "action", Icon = "🔇", IsAdvanced = true,
                Description = "Stop a playing sound by ID (use after Play Sound nodes)",
                Properties = new(),
                CodeTemplate = "if soundId then Quest:StopSound(soundId) end\n{CHILDREN}" },

            new() { Type = "muteAllSounds", Label = "Mute All Sounds", Category = "action", Icon = "🔕", IsAdvanced = true,
                Description = "Mute or unmute all game sounds",
                Properties = new() {
                    new NodeProperty { Name = "muted", Type = "bool", Label = "Muted", DefaultValue = "true" }
                },
                CodeTemplate = "Quest:SetAllSoundsAsMuted({muted})\n{CHILDREN}" },

            // ===== MUSIC SYSTEM =====
            new() { Type = "overrideMusic", Label = "Override Music", Category = "action", Icon = "🎵", IsAdvanced = false,
                Description = "Override background music with specified track",
                Properties = new() {
                    new NodeProperty { Name = "musicSet", Type = "string", Label = "Music Set", DefaultValue = "MUSIC_COMBAT" }
                },
                CodeTemplate = "Quest:OverrideMusic(\"{musicSet}\")\n{CHILDREN}" },

            new() { Type = "stopMusicOverride", Label = "Stop Music Override", Category = "action", Icon = "⏹️🎵", IsAdvanced = false,
                Description = "Stop music override and return to normal",
                Properties = new(),
                CodeTemplate = "Quest:StopOverrideMusic()\n{CHILDREN}" },

            new() { Type = "transitionMusic", Label = "Transition Music", Category = "action", Icon = "🎼", IsAdvanced = true,
                Description = "Smoothly transition to different music theme",
                Properties = new() {
                    new NodeProperty { Name = "theme", Type = "string", Label = "Theme Name", DefaultValue = "THEME_PEACEFUL" }
                },
                CodeTemplate = "Quest:TransitionToTheme(\"{theme}\")\n{CHILDREN}" },

            new() { Type = "enableDangerMusic", Label = "Danger Music", Category = "action", Icon = "⚠️🎵", IsAdvanced = true,
                Description = "Enable or disable combat/danger music",
                Properties = new() {
                    new NodeProperty { Name = "enabled", Type = "bool", Label = "Enabled", DefaultValue = "true" }
                },
                CodeTemplate = "Quest:EnableDangerMusic({enabled})\n{CHILDREN}" },

            // ===== CAMERA SYSTEM =====
            new() { Type = "cameraOrbitEntity", Label = "Camera Orbit Entity", Category = "action", Icon = "🎥", IsAdvanced = false,
                Description = "Orbit camera around an entity",
                Properties = new() {
                    new NodeProperty { Name = "distance", Type = "float", Label = "Distance", DefaultValue = "5.0" },
                    new NodeProperty { Name = "height", Type = "float", Label = "Height", DefaultValue = "2.0" },
                    new NodeProperty { Name = "speed", Type = "float", Label = "Speed", DefaultValue = "1.0" },
                    new NodeProperty { Name = "duration", Type = "float", Label = "Duration", DefaultValue = "3.0" }
                },
                CodeTemplate = "Quest:CameraCircleAroundThing(Me, {distance}, {height}, {speed}, {duration})\n{CHILDREN}" },

            new() { Type = "cameraOrbitPosition", Label = "Camera Orbit Position", Category = "action", Icon = "🎥📍", IsAdvanced = true,
                Description = "Orbit camera around world position",
                Properties = new() {
                    new NodeProperty { Name = "x", Type = "float", Label = "X", DefaultValue = "0" },
                    new NodeProperty { Name = "y", Type = "float", Label = "Y", DefaultValue = "0" },
                    new NodeProperty { Name = "z", Type = "float", Label = "Z", DefaultValue = "0" },
                    new NodeProperty { Name = "distance", Type = "float", Label = "Distance", DefaultValue = "5.0" },
                    new NodeProperty { Name = "height", Type = "float", Label = "Height", DefaultValue = "2.0" },
                    new NodeProperty { Name = "speed", Type = "float", Label = "Speed", DefaultValue = "1.0" }
                },
                CodeTemplate = "Quest:CameraCircleAroundPos({x={x}, y={y}, z={z}}, {distance}, {height}, {speed})\n{CHILDREN}" },

            new() { Type = "cameraLookAtEntity", Label = "Camera Look At Entity", Category = "action", Icon = "👁️🎥", IsAdvanced = false,
                Description = "Move camera to look at entity",
                Properties = new() {
                    new NodeProperty { Name = "distance", Type = "float", Label = "Distance", DefaultValue = "3.0" },
                    new NodeProperty { Name = "height", Type = "float", Label = "Height", DefaultValue = "1.5" },
                    new NodeProperty { Name = "duration", Type = "float", Label = "Duration", DefaultValue = "1.0" }
                },
                CodeTemplate = "Quest:CameraMoveToPosAndLookAtThing(Me:GetPos(), Me, {distance}, {height}, {duration})\n{CHILDREN}" },

            new() { Type = "cameraLookAtPosition", Label = "Camera Look At Position", Category = "action", Icon = "👁️📍", IsAdvanced = true,
                Description = "Move camera to look at world position",
                Properties = new() {
                    new NodeProperty { Name = "lookX", Type = "float", Label = "Look At X", DefaultValue = "0" },
                    new NodeProperty { Name = "lookY", Type = "float", Label = "Look At Y", DefaultValue = "0" },
                    new NodeProperty { Name = "lookZ", Type = "float", Label = "Look At Z", DefaultValue = "0" },
                    new NodeProperty { Name = "duration", Type = "float", Label = "Duration", DefaultValue = "1.0" }
                },
                CodeTemplate = "Quest:CameraMoveToPosAndLookAtPos(Me:GetPos(), {x={lookX}, y={lookY}, z={lookZ}}, {duration})\n{CHILDREN}" },

            new() { Type = "cameraResetToHero", Label = "Reset Camera To Hero", Category = "action", Icon = "🔄🎥", IsAdvanced = false,
                Description = "Return camera to default third-person view behind hero",
                Properties = new(),
                CodeTemplate = "Quest:CameraResetToViewBehindHero()\n{CHILDREN}" },

            new() { Type = "cameraUseCameraPoint", Label = "Use Camera Point", Category = "action", Icon = "📷", IsAdvanced = true,
                Description = "Use a predefined camera point from the level",
                Properties = new() {
                    new NodeProperty { Name = "cameraPoint", Type = "string", Label = "Camera Point Name", DefaultValue = "CAMERA_POINT_1" }
                },
                CodeTemplate = "local camPoint = Quest:GetNamedThing(\"{cameraPoint}\")\nQuest:CameraUseCameraPoint(camPoint)\n{CHILDREN}" },

            new() { Type = "cameraConversation", Label = "Conversation Camera", Category = "action", Icon = "🎬💬", IsAdvanced = true,
                Description = "Set up camera for dialogue scene",
                Properties = new() {
                    new NodeProperty { Name = "distance", Type = "float", Label = "Distance", DefaultValue = "2.0" }
                },
                CodeTemplate = "Quest:CameraDoConversation(Me, hero, {distance})\n{CHILDREN}" },

            // ===== SCREEN EFFECTS =====
            new() { Type = "screenFadeOut", Label = "Screen Fade Out", Category = "action", Icon = "⬛", IsAdvanced = false,
                Description = "Fade screen to black",
                Properties = new() {
                    new NodeProperty { Name = "duration", Type = "float", Label = "Duration", DefaultValue = "1.0" }
                },
                CodeTemplate = "Quest:FadeScreenOutUntilNextCallToFadeScreenIn({duration})\n{CHILDREN}" },

            new() { Type = "screenFadeIn", Label = "Screen Fade In", Category = "action", Icon = "⬜", IsAdvanced = false,
                Description = "Fade screen back in from black",
                Properties = new() {
                    new NodeProperty { Name = "duration", Type = "float", Label = "Duration", DefaultValue = "1.0" }
                },
                CodeTemplate = "Quest:EndCutFade({duration})\n{CHILDREN}" },

            new() { Type = "radialBlur", Label = "Radial Blur", Category = "action", Icon = "🌀", IsAdvanced = true,
                Description = "Apply radial blur effect from center",
                Properties = new() {
                    new NodeProperty { Name = "intensity", Type = "float", Label = "Intensity (0-1)", DefaultValue = "0.5" },
                    new NodeProperty { Name = "duration", Type = "float", Label = "Fade Duration", DefaultValue = "0.5" }
                },
                CodeTemplate = "Quest:RadialBlurFadeTo({intensity}, {duration})\n{CHILDREN}" },

            new() { Type = "radialBlurOff", Label = "Radial Blur Off", Category = "action", Icon = "🔲", IsAdvanced = true,
                Description = "Fade out radial blur effect",
                Properties = new() {
                    new NodeProperty { Name = "duration", Type = "float", Label = "Fade Duration", DefaultValue = "0.5" }
                },
                CodeTemplate = "Quest:RadialBlurFadeOut({duration})\n{CHILDREN}" },

            new() { Type = "colorFilter", Label = "Color Filter", Category = "action", Icon = "🎨", IsAdvanced = true,
                Description = "Apply color filter overlay to screen",
                Properties = new() {
                    new NodeProperty { Name = "r", Type = "float", Label = "Red (0-1)", DefaultValue = "1.0" },
                    new NodeProperty { Name = "g", Type = "float", Label = "Green (0-1)", DefaultValue = "1.0" },
                    new NodeProperty { Name = "b", Type = "float", Label = "Blue (0-1)", DefaultValue = "1.0" },
                    new NodeProperty { Name = "a", Type = "float", Label = "Alpha (0-1)", DefaultValue = "0.3" },
                    new NodeProperty { Name = "duration", Type = "float", Label = "Fade Duration", DefaultValue = "0.5" }
                },
                CodeTemplate = "Quest:ScreenFilterFadeTo({r}, {g}, {b}, {a}, {duration})\n{CHILDREN}" },

            new() { Type = "colorFilterOff", Label = "Color Filter Off", Category = "action", Icon = "🔲🎨", IsAdvanced = true,
                Description = "Remove color filter from screen",
                Properties = new() {
                    new NodeProperty { Name = "duration", Type = "float", Label = "Fade Duration", DefaultValue = "0.5" }
                },
                CodeTemplate = "Quest:ScreenFilterFadeOut({duration})\n{CHILDREN}" },

            new() { Type = "letterbox", Label = "Letterbox On", Category = "action", Icon = "🎬", IsAdvanced = true,
                Description = "Enable cinematic letterbox bars",
                Properties = new() {
                    new NodeProperty { Name = "fadeIn", Type = "bool", Label = "Fade In", DefaultValue = "true" }
                },
                CodeTemplate = "Quest:StartLetterBox({fadeIn})\n{CHILDREN}" },

            new() { Type = "letterboxOff", Label = "Letterbox Off", Category = "action", Icon = "📺", IsAdvanced = true,
                Description = "Remove cinematic letterbox bars",
                Properties = new(),
                CodeTemplate = "Quest:EndLetterBox()\n{CHILDREN}" },

            new() { Type = "playMovie", Label = "Play Movie", Category = "action", Icon = "🎞️", IsAdvanced = true,
                Description = "Play an AVI movie file",
                Properties = new() {
                    new NodeProperty { Name = "movieName", Type = "string", Label = "Movie Name", DefaultValue = "intro" }
                },
                CodeTemplate = "Quest:PlayAVIMovie(\"{movieName}\")\n{CHILDREN}" }
        };
    }

    public static List<NodeDefinition> GetConditionNodes()
    {
        return new List<NodeDefinition>
        {
            new() { Type = "checkState", Label = "Check State", Category = "condition", Icon = "?", IsAdvanced = false, HasBranching = true,
                Description = "Check if state variable equals value",
                Properties = new() {
                    new NodeProperty { Name = "name", Type = "string", Label = "State Name", DefaultValue = "questStarted" },
                    new NodeProperty { Name = "value", Type = "string", Label = "Expected Value", DefaultValue = "true" }
                },
                CodeTemplate = "if Quest:GetStateBool(\"{name}\") == {value} then\n{TRUE}\nelse\n{FALSE}\nend" },

            new() { Type = "checkGlobal", Label = "Check Global", Category = "condition", Icon = "?", IsAdvanced = true, HasBranching = true,
                Description = "Check if global variable equals value",
                Properties = new() {
                    new NodeProperty { Name = "name", Type = "string", Label = "Global Name", DefaultValue = "globalEvent" },
                    new NodeProperty { Name = "value", Type = "string", Label = "Expected Value", DefaultValue = "true" }
                },
                CodeTemplate = "if Quest:GetGlobalBool(\"{name}\") == {value} then\n{TRUE}\nelse\n{FALSE}\nend" },

            new() { Type = "checkHeroGold", Label = "Check Hero Gold", Category = "condition", Icon = "?", IsAdvanced = false, HasBranching = true,
                Description = "Check hero's gold amount",
                Properties = new() {
                    new NodeProperty { Name = "operator", Type = "select", Label = "Operator", DefaultValue = ">=",
                        Options = new List<string> { "==", "!=", ">", ">=", "<", "<=" } },
                    new NodeProperty { Name = "amount", Type = "int", Label = "Gold Amount", DefaultValue = "100" }
                },
                CodeTemplate = "if Quest:GetHeroGold() {operator} {amount} then\n{TRUE}\nelse\n{FALSE}\nend" },

            new() { Type = "checkHeroMorality", Label = "Check Morality", Category = "condition", Icon = "?", IsAdvanced = true, HasBranching = true,
                Description = "Check hero's morality value",
                Properties = new() {
                    new NodeProperty { Name = "operator", Type = "select", Label = "Operator", DefaultValue = ">",
                        Options = new List<string> { "==", "!=", ">", ">=", "<", "<=" } },
                    new NodeProperty { Name = "threshold", Type = "float", Label = "Threshold", DefaultValue = "0" }
                },
                CodeTemplate = "if Quest:GetHeroMorality() {operator} {threshold} then\n{TRUE}\nelse\n{FALSE}\nend" },

            new() { Type = "checkHeroHealth", Label = "Check Health", Category = "condition", Icon = "?", IsAdvanced = true, HasBranching = true,
                Description = "Check hero's current health",
                Properties = new() {
                    new NodeProperty { Name = "operator", Type = "select", Label = "Operator", DefaultValue = "<",
                        Options = new List<string> { "==", "!=", ">", ">=", "<", "<=" } },
                    new NodeProperty { Name = "threshold", Type = "float", Label = "Threshold", DefaultValue = "50" }
                },
                CodeTemplate = "if Quest:GetHeroHealth() {operator} {threshold} then\n{TRUE}\nelse\n{FALSE}\nend" },

            new() { Type = "checkHasItem", Label = "Has Item", Category = "condition", Icon = "?", IsAdvanced = false, HasBranching = true,
                Description = "Check if hero has specific item",
                Properties = new() {
                    new NodeProperty { Name = "item", Type = "string", Label = "Item", DefaultValue = "OBJECT_APPLE", Options = new List<string>(GameData.Objects) }
                },
                CodeTemplate = "if Quest:IsPlayerCarryingItemOfType(\"{item}\") then\n{TRUE}\nelse\n{FALSE}\nend" },

            new() { Type = "checkIsAlive", Label = "Is Alive", Category = "condition", Icon = "?", IsAdvanced = false, HasBranching = true,
                Description = "Check if entity is alive",
                Properties = new(),
                CodeTemplate = "if Me:IsAlive() then\n{TRUE}\nelse\n{FALSE}\nend" },

            new() { Type = "checkYesNoAnswer", Label = "Check Answer", Category = "condition", Icon = "?", IsAdvanced = false, HasBranching = true,
                Description = "Check yes/no question answer and branch (Yes/No/Unsure)",
                Properties = new(),
                BranchLabels = new List<string> { "Yes", "No", "Unsure" },
                CodeTemplate = "if answer == 0 then\n{Yes}\nelseif answer == 1 then\n{No}\nelse\n{Unsure}\nend" },
            
            new() { Type = "checkRegionLoaded", Label = "Region Loaded", Category = "condition", Icon = "?", IsAdvanced = true, HasBranching = true,
                Description = "Check if region is currently loaded",
                Properties = new() {
                    new NodeProperty { Name = "region", Type = "string", Label = "Region", DefaultValue = "Oakvale", Options = new List<string>(GameData.Regions) }
                },
                CodeTemplate = "if Quest:IsRegionLoaded(\"{region}\") then\n{TRUE}\nelse\n{FALSE}\nend" },

            new() { Type = "checkQuestComplete", Label = "Quest Complete", Category = "condition", Icon = "?", IsAdvanced = true, HasBranching = true,
                Description = "Check if another quest is completed",
                Properties = new() {
                    new NodeProperty { Name = "questName", Type = "string", Label = "Quest Name", DefaultValue = "QUEST_NAME" }
                },
                CodeTemplate = "if Quest:IsQuestComplete(\"{questName}\") then\n{TRUE}\nelse\n{FALSE}\nend" },

            new() { Type = "checkBoastTaken", Label = "Boast Taken", Category = "condition", Icon = "?", IsAdvanced = true, HasBranching = true,
                Description = "Check if boast was accepted",
                Properties = new() {
                    new NodeProperty { Name = "boastId", Type = "int", Label = "Boast ID", DefaultValue = "1" }
                },
                CodeTemplate = "if Quest:IsBoastTaken({boastId}, \"{QUEST_NAME}\") then\n{TRUE}\nelse\n{FALSE}\nend" },

            // ===== AUDIO/CONVERSATION CONDITIONS =====
            new() { Type = "checkSoundPlaying", Label = "Sound Playing", Category = "condition", Icon = "?🔊", IsAdvanced = true, HasBranching = true,
                Description = "Check if a sound is currently playing (requires soundId from Play Sound nodes)",
                Properties = new(),
                CodeTemplate = "if soundId and Quest:IsSoundPlaying(soundId) then\n{TRUE}\nelse\n{FALSE}\nend" },

            new() { Type = "checkConversationActive", Label = "Conversation Active", Category = "condition", Icon = "?💬", IsAdvanced = true, HasBranching = true,
                Description = "Check if conversation is still active",
                Properties = new(),
                CodeTemplate = "if convoId and Quest:IsConversationActive(convoId) then\n{TRUE}\nelse\n{FALSE}\nend" },

            new() { Type = "checkDangerMusicEnabled", Label = "Danger Music Enabled", Category = "condition", Icon = "?🎵", IsAdvanced = true, HasBranching = true,
                Description = "Check if danger/combat music is enabled",
                Properties = new(),
                CodeTemplate = "if Quest:IsDangerMusicEnabled() then\n{TRUE}\nelse\n{FALSE}\nend" },

            new() { Type = "checkCameraScripted", Label = "Camera Scripted", Category = "condition", Icon = "?🎥", IsAdvanced = true, HasBranching = true,
                Description = "Check if camera is currently in scripted mode",
                Properties = new(),
                CodeTemplate = "if Quest:IsCameraInScriptedMode() then\n{TRUE}\nelse\n{FALSE}\nend" },

            new() { Type = "checkEntityInCombat", Label = "Entity In Combat", Category = "condition", Icon = "?⚔️", IsAdvanced = false, HasBranching = true,
                Description = "Check if this entity is in combat",
                Properties = new(),
                CodeTemplate = "if Me:IsInCombat() then\n{TRUE}\nelse\n{FALSE}\nend" },

            new() { Type = "checkHeroInCombat", Label = "Hero In Combat", Category = "condition", Icon = "?🗡️", IsAdvanced = false, HasBranching = true,
                Description = "Check if hero is in combat",
                Properties = new(),
                CodeTemplate = "if hero:IsInCombat() then\n{TRUE}\nelse\n{FALSE}\nend" },

            new() { Type = "checkDistanceToHero", Label = "Distance To Hero", Category = "condition", Icon = "?📏", IsAdvanced = false, HasBranching = true,
                Description = "Check distance between entity and hero",
                Properties = new() {
                    new NodeProperty { Name = "operator", Type = "select", Label = "Operator", DefaultValue = "<",
                        Options = new List<string> { "<", "<=", ">", ">=" } },
                    new NodeProperty { Name = "distance", Type = "float", Label = "Distance", DefaultValue = "5.0" }
                },
                CodeTemplate = "if Quest:GetDistanceBetweenThings(Me, hero) {operator} {distance} then\n{TRUE}\nelse\n{FALSE}\nend" }
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
                CodeTemplate = "Quest:Pause({seconds})\n{CHILDREN}" },
            
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
                CodeTemplate = "{functionName}()\n{CHILDREN}" },

            new() { Type = "defineEvent", Label = "Define Event", Category = "custom", Icon = "🎯", IsAdvanced = false,
                Description = "Create a custom event that can be called from anywhere in the graph",
                Properties = new() {
                    new NodeProperty { Name = "eventName", Type = "string", Label = "Event Name", DefaultValue = "MyCustomEvent" }
                },
                CodeTemplate = "-- Event: {eventName}\nfunction Event_{eventName}()\n{CHILDREN}\nend" },

            new() { Type = "callEvent", Label = "Call Event", Category = "custom", Icon = "📡", IsAdvanced = false,
                Description = "Trigger a custom event defined elsewhere",
                Properties = new() {
                    new NodeProperty { Name = "eventName", Type = "string", Label = "Event Name", DefaultValue = "MyCustomEvent" }
                },
                CodeTemplate = "Event_{eventName}()\n{CHILDREN}" }
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
    public bool HasBranching { get; set; } = false; // True for nodes with True/False outputs
    public List<string>? BranchLabels { get; set; } = null; // Custom branch labels (e.g., "Yes", "No", "Unsure")
}

public class NodeProperty
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public object? DefaultValue { get; set; }
    public List<string>? Options { get; set; }
}
