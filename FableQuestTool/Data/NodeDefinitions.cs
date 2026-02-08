using System.Collections.Generic;
using FableQuestTool.Models;

namespace FableQuestTool.Data;

public static class NodeDefinitions
{
    private static readonly List<NodeDefinition> AllNodesCache = BuildAllNodes();

    /// <summary>
    /// Executes GetAllNodes.
    /// </summary>
    public static List<NodeDefinition> GetAllNodes()
    {
        return new List<NodeDefinition>(AllNodesCache);
    }

    private static List<NodeDefinition> BuildAllNodes()
    {
        List<NodeDefinition> nodes = new();
        nodes.AddRange(GetTriggerNodes());
        nodes.AddRange(GetActionNodes());
        nodes.AddRange(GetConditionNodes());
        nodes.AddRange(GetFlowNodes());
        return nodes;
    }

    /// <summary>
    /// Executes GetTriggerNodes.
    /// </summary>
    public static List<NodeDefinition> GetTriggerNodes()
    {
        return new List<NodeDefinition>
        {
            new() { Type = "onHeroTalks", Label = "When Hero Talks", Category = "trigger", Icon = "??", IsAdvanced = false,
                Description = "Triggered when the hero initiates conversation with this entity",
                Properties = new(),
                CodeTemplate = "if Me:IsTalkedToByHero() then\n{CHILDREN}\nend" },
            
            new() { Type = "onHeroHits", Label = "When Hero Hits", Category = "trigger", Icon = "??", IsAdvanced = false,
                Description = "Triggered when the hero hits this entity with any attack",
                Properties = new(),
                CodeTemplate = "if Me:MsgIsHitByHero() then\n{CHILDREN}\nend" },
            
            new() { Type = "onHeroHitsWithFlourish", Label = "When Flourish Hit", Category = "trigger", Icon = "?", IsAdvanced = true,
                Description = "Triggered when the hero hits with a flourish attack",
                Properties = new(),
                CodeTemplate = "if Me:MsgIsHitByHeroWithFlourish() then\n{CHILDREN}\nend" },
            
            new() { Type = "onHeroHitsWithWeapon", Label = "When Hit With Weapon", Category = "trigger", Icon = "???", IsAdvanced = true,
                Description = "Triggered when hit with a specific weapon type",
                Properties = new() { 
                    new NodeProperty { Name = "weaponName", Type = "string", Label = "Weapon Name", DefaultValue = "WEAPON_IRON_SWORD" }
                },
                CodeTemplate = "if Me:MsgIsHitByHeroWithWeapon(\"{weaponName}\") then\n{CHILDREN}\nend" },
            
            new() { Type = "onEntityKilled", Label = "When Killed", Category = "trigger", Icon = "??", IsAdvanced = false,
                Description = "Triggered when this entity dies",
                Properties = new(),
                CodeTemplate = "if Me:IsDead() then\n{CHILDREN}\nend" },
            
            new() { Type = "onKilledByHero", Label = "When Killed By Hero", Category = "trigger", Icon = "??", IsAdvanced = false,
                Description = "Triggered when killed specifically by the hero",
                Properties = new(),
                CodeTemplate = "if Me:IsKilledByHero() then\n{CHILDREN}\nend" },
            
            new() { Type = "onProximity", Label = "When Hero Nearby", Category = "trigger", Icon = "??", IsAdvanced = false,
                Description = "Triggered when hero comes within specified distance",
                Properties = new() {
                    new NodeProperty { Name = "distance", Type = "float", Label = "Distance", DefaultValue = "5.0" }
                },
                CodeTemplate = "if Quest:IsDistanceBetweenThingsUnder(hero, Me, {distance}) then\n{CHILDREN}\nend" },
            
            new() { Type = "onItemPresented", Label = "When Item Given", Category = "trigger", Icon = "??", IsAdvanced = false,
                Description = "Triggered when hero presents the specified item to this entity",
                Properties = new() {
                    new NodeProperty { Name = "item", Type = "string", Label = "Item", DefaultValue = "OBJECT_APPLE", Options = new List<string>(GameData.Objects) }
                },
                CodeTemplate = "local wasPresented = Me:MsgIsPresentedWithItem()\nlocal presentedItemName = g_PresentedItemName\nif wasPresented then\n    Quest:Log(\"Presented item: \" .. tostring(presentedItemName))\n    g_PresentedItemName = nil\nend\nif wasPresented and presentedItemName == \"{item}\" then\n{CHILDREN}\nend" },
            
            new() { Type = "onHeroUsed", Label = "When Used By Hero", Category = "trigger", Icon = "??", IsAdvanced = false,
                Description = "Triggered when hero uses/activates this entity",
                Properties = new(),
                CodeTemplate = "if Me:MsgIsUsedByHero() then\n{CHILDREN}\nend" },
            
            new() { Type = "onTriggered", Label = "When Triggered", Category = "trigger", Icon = "?", IsAdvanced = false,
                Description = "Triggered when hero enters a trigger region",
                Properties = new(),
                CodeTemplate = "if Me:MsgIsTriggeredByHero() then\n{CHILDREN}\nend" },
            
            new() { Type = "onKnockedOut", Label = "When Knocked Out", Category = "trigger", Icon = "??", IsAdvanced = true,
                Description = "Triggered when this entity is knocked unconscious",
                Properties = new(),
                CodeTemplate = "if Me:MsgIsKnockedOutByHero() then\n{CHILDREN}\nend" },
            
            new() { Type = "onKicked", Label = "When Kicked", Category = "trigger", Icon = "??", IsAdvanced = true,
                Description = "Triggered when this entity is kicked",
                Properties = new(),
                CodeTemplate = "if Me:MsgIsKicked() then\n{CHILDREN}\nend" },
            
            new() { Type = "onAwareOfHero", Label = "When Aware of Hero", Category = "trigger", Icon = "???", IsAdvanced = true,
                Description = "Triggered when entity becomes aware of hero's presence",
                Properties = new(),
                CodeTemplate = "if Me:IsAwareOfHero() then\n{CHILDREN}\nend" },
            
            new() { Type = "onRegionLoaded", Label = "When Region Loads", Category = "trigger", Icon = "???", IsAdvanced = true,
                Description = "Triggered when specified region loads",
                Properties = new() {
                    new NodeProperty { Name = "region", Type = "string", Label = "Region", DefaultValue = "Oakvale", Options = new List<string>(GameData.Regions) }
                },
                CodeTemplate = "if Quest:IsRegionLoaded(\"{region}\") then\n{CHILDREN}\nend" },
            
            new() { Type = "onStateChange", Label = "When State Changes", Category = "trigger", Icon = "??", IsAdvanced = true,
                Description = "Triggered when a state variable reaches a specific value",
                Properties = new() {
                    new NodeProperty { Name = "stateName", Type = "string", Label = "State Name", DefaultValue = "questStarted" },
                    new NodeProperty { Name = "value", Type = "string", Label = "Value", DefaultValue = "true" }
                },
                CodeTemplate = "if Quest:GetStateBool(\"{stateName}\") == {value} then\n{CHILDREN}\nend" },
            new() { Type = "onStateChangeBool", Label = "When State Changes (Bool)", Category = "trigger", Icon = "??", IsAdvanced = true,
                Description = "Triggered when a boolean state variable reaches a specific value",
                Properties = new() {
                    new NodeProperty { Name = "stateName", Type = "string", Label = "State Name", DefaultValue = "questStarted" },
                    new NodeProperty { Name = "value", Type = "bool", Label = "Value", DefaultValue = "true" }
                },
                CodeTemplate = "if Quest:GetStateBool(\"{stateName}\") == {value} then\n{CHILDREN}\nend" },

            new() { Type = "onStateChangeInt", Label = "When State Changes (Int)", Category = "trigger", Icon = "??", IsAdvanced = true,
                Description = "Triggered when an integer state variable reaches a specific value",
                Properties = new() {
                    new NodeProperty { Name = "stateName", Type = "string", Label = "State Name", DefaultValue = "killCount" },
                    new NodeProperty { Name = "value", Type = "int", Label = "Value", DefaultValue = "0" }
                },
                CodeTemplate = "if Quest:GetStateInt(\"{stateName}\") == {value} then\n{CHILDREN}\nend" },

            new() { Type = "onStateChangeFloat", Label = "When State Changes (Float)", Category = "trigger", Icon = "??", IsAdvanced = true,
                Description = "Triggered when a float state variable reaches a specific value",
                Properties = new() {
                    new NodeProperty { Name = "stateName", Type = "string", Label = "State Name", DefaultValue = "progress" },
                    new NodeProperty { Name = "value", Type = "float", Label = "Value", DefaultValue = "0.0" }
                },
                CodeTemplate = "if tonumber(Quest:GetStateString(\"{stateName}\")) == {value} then\n{CHILDREN}\nend" },

            new() { Type = "onStateChangeString", Label = "When State Changes (String)", Category = "trigger", Icon = "??", IsAdvanced = true,
                Description = "Triggered when a string state variable reaches a specific value",
                Properties = new() {
                    new NodeProperty { Name = "stateName", Type = "string", Label = "State Name", DefaultValue = "questStage" },
                    new NodeProperty { Name = "value", Type = "string", Label = "Value", DefaultValue = "started" }
                },
                CodeTemplate = "if Quest:GetStateString(\"{stateName}\") == \"{value}\" then\n{CHILDREN}\nend" }
        };
    }

    /// <summary>
    /// Executes GetActionNodes.
    /// </summary>
    public static List<NodeDefinition> GetActionNodes()
    {
        return new List<NodeDefinition>
        {
            // ===== DIALOGUE =====
            new() { Type = "showDialogue", Label = "Show Dialogue", Category = "action", Icon = "??", IsAdvanced = false,
                Description = "Display dialogue text from this entity",
                Properties = new() {
                    new NodeProperty { Name = "text", Type = "text", Label = "Dialogue Text", DefaultValue = "Hello, hero!" }
                },
                CodeTemplate = "Me:SpeakAndWait(\"{text}\")\n{CHILDREN}" },
            
            new() { Type = "speakToHero", Label = "Speak To Hero", Category = "action", Icon = "???", IsAdvanced = false,
                Description = "Entity speaks directly to the hero",
                Properties = new() {
                    new NodeProperty { Name = "text", Type = "text", Label = "Text", DefaultValue = "Greetings!" }
                },
                CodeTemplate = "Me:Speak(hero, \"{text}\")\n{CHILDREN}" },

            // ===== REWARDS =====
            new() { Type = "giveReward", Label = "Give Reward", Category = "action", Icon = "??", IsAdvanced = false,
                Description = "Give gold and/or items to hero",
                Properties = new() {
                    new NodeProperty { Name = "gold", Type = "int", Label = "Gold", DefaultValue = "100" },
                    new NodeProperty { Name = "item", Type = "string", Label = "Item (optional)", DefaultValue = "", Options = new List<string>(GameData.Objects) },
                    new NodeProperty { Name = "amount", Type = "int", Label = "Item Amount", DefaultValue = "1" }
                },
                CodeTemplate = "if {gold} > 0 then Quest:GiveHeroGold({gold}) end\nlocal itemToGive = \"{item}\"\nif itemToGive ~= \"\" and itemToGive ~= nil then Quest:GiveHeroObject(\"{item}\", {amount}) end\n{CHILDREN}" },

            new() { Type = "giveItem", Label = "Give Item", Category = "action", Icon = "??", IsAdvanced = false,
                Description = "Give specific item(s) to hero",
                Properties = new() {
                    new NodeProperty { Name = "item", Type = "string", Label = "Item", DefaultValue = "OBJECT_HEALTH_POTION", Options = new List<string>(GameData.Objects) },
                    new NodeProperty { Name = "amount", Type = "int", Label = "Amount", DefaultValue = "1" }
                },
                CodeTemplate = "Quest:GiveHeroObject(\"{item}\", {amount})\n{CHILDREN}" },

            new() { Type = "takeItem", Label = "Take Item", Category = "action", Icon = "??", IsAdvanced = false,
                Description = "Remove item from hero's inventory or possession",
                Properties = new() {
                    new NodeProperty { Name = "item", Type = "string", Label = "Item", DefaultValue = "OBJECT_APPLE", Options = new List<string>(GameData.Objects) }
                },
                CodeTemplate = "local ok = pcall(function() Quest:ConfiscateItemsOfTypeFromHero(\"{item}\") end)\nlocal stillHas = false\nif Quest:GetNumberOfItemsOfTypeInInventory(\"{item}\") > 0 then\n    stillHas = true\nelseif Quest:IsObjectInHeroPossession(\"{item}\") then\n    stillHas = true\nelseif Quest:IsPlayerCarryingItemOfType(\"{item}\") then\n    stillHas = true\nend\nif not ok or stillHas then\n    Quest:TakeObjectFromHero(\"{item}\")\nend\n{CHILDREN}" },

            // ===== STATE MANAGEMENT =====
            new() { Type = "setState", Label = "Set State", Category = "action", Icon = "??", IsAdvanced = false,
                Description = "Set a quest state variable",
                Properties = new() {
                    new NodeProperty { Name = "name", Type = "string", Label = "Variable Name", DefaultValue = "questStarted" },
                    new NodeProperty { Name = "value", Type = "string", Label = "Value", DefaultValue = "true" }
                },
                CodeTemplate = "Quest:SetStateBool(\"{name}\", {value})\n{CHILDREN}" },
            new() { Type = "setStateBool", Label = "Set State (Bool)", Category = "action", Icon = "??", IsAdvanced = false,
                Description = "Set a boolean quest state variable",
                Properties = new() {
                    new NodeProperty { Name = "name", Type = "string", Label = "Variable Name", DefaultValue = "questStarted" },
                    new NodeProperty { Name = "value", Type = "bool", Label = "Value", DefaultValue = "true" }
                },
                CodeTemplate = "Quest:SetStateBool(\"{name}\", {value})\n{CHILDREN}" },

            new() { Type = "setStateInt", Label = "Set State (Int)", Category = "action", Icon = "??", IsAdvanced = true,
                Description = "Set an integer quest state variable",
                Properties = new() {
                    new NodeProperty { Name = "name", Type = "string", Label = "Variable Name", DefaultValue = "killCount" },
                    new NodeProperty { Name = "value", Type = "int", Label = "Value", DefaultValue = "0" }
                },
                CodeTemplate = "Quest:SetStateInt(\"{name}\", {value})\n{CHILDREN}" },

            new() { Type = "setStateFloat", Label = "Set State (Float)", Category = "action", Icon = "??", IsAdvanced = true,
                Description = "Set a float quest state variable",
                Properties = new() {
                    new NodeProperty { Name = "name", Type = "string", Label = "Variable Name", DefaultValue = "progress" },
                    new NodeProperty { Name = "value", Type = "float", Label = "Value", DefaultValue = "0.0" }
                },
                CodeTemplate = "Quest:SetStateString(\"{name}\", tostring({value}))\n{CHILDREN}" },

            new() { Type = "setStateString", Label = "Set State (String)", Category = "action", Icon = "??", IsAdvanced = true,
                Description = "Set a string quest state variable",
                Properties = new() {
                    new NodeProperty { Name = "name", Type = "string", Label = "Variable Name", DefaultValue = "questStage" },
                    new NodeProperty { Name = "value", Type = "string", Label = "Value", DefaultValue = "started" }
                },
                CodeTemplate = "Quest:SetStateString(\"{name}\", \"{value}\")\n{CHILDREN}" },

            new() { Type = "setGlobalState", Label = "Set Global", Category = "action", Icon = "??", IsAdvanced = true,
                Description = "Set a global state variable (persists across quests)",
                Properties = new() {
                    new NodeProperty { Name = "name", Type = "string", Label = "Variable Name", DefaultValue = "globalFlag" },
                    new NodeProperty { Name = "value", Type = "string", Label = "Value", DefaultValue = "true" }
                },
                CodeTemplate = "Quest:SetGlobalBool(\"{name}\", {value})\n{CHILDREN}" },

            // ===== MESSAGES =====
            new() { Type = "showMessage", Label = "Show Message", Category = "action", Icon = "??", IsAdvanced = false,
                Description = "Display on-screen message",
                Properties = new() {
                    new NodeProperty { Name = "text", Type = "text", Label = "Message", DefaultValue = "Objective Updated" },
                    new NodeProperty { Name = "duration", Type = "float", Label = "Duration", DefaultValue = "3.0" }
                },
                CodeTemplate = "Quest:ShowMessage(\"{text}\", {duration})\n{CHILDREN}" },
            
            new() { Type = "showTitleMessage", Label = "Show Title Message", Category = "action", Icon = "??", IsAdvanced = false,
                Description = "Display large title message",
                Properties = new() {
                    new NodeProperty { Name = "text", Type = "text", Label = "Title", DefaultValue = "New Objective" },
                    new NodeProperty { Name = "duration", Type = "float", Label = "Duration", DefaultValue = "5.0" }
                },
                CodeTemplate = "Quest:AddScreenTitleMessage(\"{text}\", {duration}, true)\n{CHILDREN}" },
            
            new() { Type = "guildmasterMessage", Label = "Guildmaster Says", Category = "action", Icon = "??", IsAdvanced = true,
                Description = "Show message from guild master",
                Properties = new() {
                    new NodeProperty { Name = "key", Type = "string", Label = "Message Key", DefaultValue = "QUEST_MESSAGE" }
                },
                CodeTemplate = "Quest:HeroReceiveMessageFromGuildMaster(\"{key}\", \"Class\", true, true)\n{CHILDREN}" },
            
            new() { Type = "yesNoQuestion", Label = "Yes/No Question", Category = "action", Icon = "?", IsAdvanced = false,
                Description = "Ask hero a yes/no question (use with Check Answer node to handle response)",
                Properties = new() {
                    new NodeProperty { Name = "question", Type = "text", Label = "Question", DefaultValue = "Do you accept?" },
                    new NodeProperty { Name = "yes", Type = "text", Label = "Yes Text", DefaultValue = "Yes" },
                    new NodeProperty { Name = "no", Type = "text", Label = "No Text", DefaultValue = "No" },
                    new NodeProperty { Name = "unsure", Type = "text", Label = "Unsure Text", DefaultValue = "I'm not sure" }
                },
                CodeTemplate = "answer = Quest:GiveHeroYesNoQuestion(\"{question}\", \"{yes}\", \"{no}\", \"{unsure}\")\nQuest:EndMovieSequence()\n{CHILDREN}" },

            new() { Type = "showStartScreen", Label = "Show Start Screen", Category = "action", Icon = "??", IsAdvanced = false,
                Description = "Display the quest start screen for this quest",
                Properties = new() {
                    new NodeProperty { Name = "questCard", Type = "string", Label = "Quest Card", DefaultValue = "OBJECT_QUEST_CARD_GENERIC", Options = new List<string>(GameData.QuestCards) },
                    new NodeProperty { Name = "giveCard", Type = "bool", Label = "Give Quest Card", DefaultValue = "false" },
                    new NodeProperty { Name = "showHeroGuide", Type = "bool", Label = "Show Hero Guide", DefaultValue = "true" },
                    new NodeProperty { Name = "isStory", Type = "bool", Label = "Story Quest", DefaultValue = "false" },
                    new NodeProperty { Name = "isGold", Type = "bool", Label = "Gold Quest", DefaultValue = "false" }
                },
                CodeTemplate = "local questCard = \"{questCard}\"\nif {giveCard} and questCard ~= \"\" then\n    Quest:GiveQuestCardDirectly(questCard, \"{QUEST_NAME}\", true)\nend\nif {showHeroGuide} then\n    Quest:SetHeroGuideShowsQuestCards(true)\nend\nQuest:KickOffQuestStartScreen(\"{QUEST_NAME}\", {isStory}, {isGold})\n{CHILDREN}" },

            // ===== QUEST TARGETS =====
            new() { Type = "showMinimapMarker", Label = "Show Minimap Marker", Category = "action", Icon = "?", IsAdvanced = false,
                Description = "Show a minimap marker on this entity",
                Properties = new() {
                    new NodeProperty { Name = "markerName", Type = "string", Label = "Marker Name", DefaultValue = "QuestTarget" }
                },
                CodeTemplate = "Quest:MiniMapAddMarker(Me, \"{markerName}\")\n{CHILDREN}" },

            new() { Type = "hideMinimapMarker", Label = "Hide Minimap Marker", Category = "action", Icon = "?", IsAdvanced = false,
                Description = "Remove the minimap marker from this entity",
                Properties = new(),
                CodeTemplate = "Quest:MiniMapRemoveMarker(Me)\n{CHILDREN}" },

            new() { Type = "highlightQuestTarget", Label = "Highlight Quest Target", Category = "action", Icon = "??", IsAdvanced = false,
                Description = "Highlight this entity as a quest target (green glow)",
                Properties = new(),
                CodeTemplate = "Quest:SetThingHasInformation(Me, true)\n{CHILDREN}" },

            new() { Type = "clearQuestTargetHighlight", Label = "Clear Quest Target Highlight", Category = "action", Icon = "??", IsAdvanced = false,
                Description = "Remove the quest target highlight from this entity",
                Properties = new(),
                CodeTemplate = "Quest:ClearThingHasInformation(Me)\n{CHILDREN}" },

            new() { Type = "highlightQuestTargetByName", Label = "Highlight Quest Target (By Name)", Category = "action", Icon = "??", IsAdvanced = false,
                Description = "Highlight another entity using its script name",
                Properties = new() {
                    new NodeProperty { Name = "targetScriptName", Type = "string", Label = "Target Script Name", DefaultValue = "QuestTarget" }
                },
                CodeTemplate = "local target = Quest:GetThingWithScriptName(\"{targetScriptName}\")\nif target ~= nil then\n    Quest:SetThingHasInformation(target, true)\nend\n{CHILDREN}" },

            new() { Type = "clearQuestTargetHighlightByName", Label = "Clear Quest Target (By Name)", Category = "action", Icon = "??", IsAdvanced = false,
                Description = "Clear highlight on another entity using its script name",
                Properties = new() {
                    new NodeProperty { Name = "targetScriptName", Type = "string", Label = "Target Script Name", DefaultValue = "QuestTarget" }
                },
                CodeTemplate = "local target = Quest:GetThingWithScriptName(\"{targetScriptName}\")\nif target ~= nil then\n    Quest:ClearThingHasInformation(target)\nend\n{CHILDREN}" },

            new() { Type = "showMinimapMarkerByName", Label = "Show Minimap Marker (By Name)", Category = "action", Icon = "?", IsAdvanced = false,
                Description = "Show a minimap marker for another entity using its script name",
                Properties = new() {
                    new NodeProperty { Name = "targetScriptName", Type = "string", Label = "Target Script Name", DefaultValue = "QuestTarget" },
                    new NodeProperty { Name = "markerName", Type = "string", Label = "Marker Name", DefaultValue = "QuestTarget" }
                },
                CodeTemplate = "local target = Quest:GetThingWithScriptName(\"{targetScriptName}\")\nif target ~= nil then\n    Quest:MiniMapAddMarker(target, \"{markerName}\")\nend\n{CHILDREN}" },

            new() { Type = "hideMinimapMarkerByName", Label = "Hide Minimap Marker (By Name)", Category = "action", Icon = "?", IsAdvanced = false,
                Description = "Remove a minimap marker for another entity using its script name",
                Properties = new() {
                    new NodeProperty { Name = "targetScriptName", Type = "string", Label = "Target Script Name", DefaultValue = "QuestTarget" }
                },
                CodeTemplate = "local target = Quest:GetThingWithScriptName(\"{targetScriptName}\")\nif target ~= nil then\n    Quest:MiniMapRemoveMarker(target)\nend\n{CHILDREN}" },

            // ===== CINEMATIC - Movie Sequence (No frame checks during movie mode!) =====
            new() { Type = "startMovieSequence", Label = "Start Movie Sequence", Category = "action", Icon = "??", IsAdvanced = false,
                Description = "Begin cinematic mode (prevents interruptions during cutscenes/conversations)",
                Properties = new(),
                CodeTemplate = "Quest:StartMovieSequence()\nQuest:Pause(0.1)\n{CHILDREN}" },

            new() { Type = "endMovieSequence", Label = "End Movie Sequence", Category = "action", Icon = "?", IsAdvanced = false,
                Description = "End cinematic mode and return control to player",
                Properties = new(),
                CodeTemplate = "Quest:EndMovieSequence()\n{CHILDREN}" },

            // letterbox starts cinematic mode
            new() { Type = "letterbox", Label = "Start Cinematic", Category = "action", Icon = "??", IsAdvanced = true,
                Description = "Start cinematic mode with letterbox bars (use Letterbox Off to remove)",
                Properties = new(),
                CodeTemplate = "Quest:StartMovieSequence()\nQuest:Pause(0.1)\n{CHILDREN}" },

            // letterboxOff ends both letterbox AND movie sequence
            new() { Type = "letterboxOff", Label = "Letterbox Off", Category = "action", Icon = "??", IsAdvanced = true,
                Description = "End cinematic mode and remove letterbox bars",
                Properties = new(),
                CodeTemplate = "Quest:EndLetterBox()\nQuest:EndMovieSequence()\n{CHILDREN}" },

            // ===== CONVERSATIONS (DEPRECATED - Use SpeakAndWait instead) =====
            // NOTE: Conversation system APIs don't work reliably from entity scripts and can cause hangs.
            // For multi-line dialogue, use multiple "Show Dialogue" nodes (which use SpeakAndWait).
            // SpeakAndWait automatically manages movie sequences and letterbox bars.

            new() { Type = "speakWithOptions", Label = "Speak (Advanced)", Category = "action", Icon = "??", IsAdvanced = true,
                Description = "Entity speaks with full audio and display options",
                Properties = new() {
                    new NodeProperty { Name = "text", Type = "text", Label = "Text/Voice Key", DefaultValue = "Hello, hero!" },
                    new NodeProperty { Name = "use2DSound", Type = "bool", Label = "Use 2D Sound", DefaultValue = "false" },
                    new NodeProperty { Name = "useVoice", Type = "bool", Label = "Use Voice", DefaultValue = "false" },
                    new NodeProperty { Name = "displayOnScreen", Type = "bool", Label = "Display On Screen", DefaultValue = "true" }
                },
                CodeTemplate = "Me:Speak(hero, \"{text}\", {use2DSound}, {useVoice}, {displayOnScreen})\n{CHILDREN}" },

            // ===== CAMERA (Quest-level APIs only) =====
            // NOTE: Advanced camera APIs (CameraCircleAroundThing, CameraMoveToPosAndLookAtThing) crash from entity scripts.
            // For dialogue scenes, use "Conversation Camera" node instead (CameraDoConversation).
            // Only basic camera reset works from entity scripts.

            new() { Type = "cameraResetToHero", Label = "Reset Camera To Hero", Category = "action", Icon = "??", IsAdvanced = false,
                Description = "Return camera to default third-person view behind hero",
                Properties = new() {
                    new NodeProperty { Name = "duration", Type = "float", Label = "Duration", DefaultValue = "1.0" }
                },
                CodeTemplate = "Quest:CameraResetToViewBehindHero({duration})\nQuest:Pause({duration})\n{CHILDREN}" },

            new() { Type = "cameraUseCameraPoint", Label = "Use Camera Point", Category = "action", Icon = "??", IsAdvanced = true,
                Description = "Use a predefined camera point from the level",
                Properties = new() {
                    new NodeProperty { Name = "cameraPoint", Type = "string", Label = "Camera Point Name", DefaultValue = "CAMERA_POINT_1" },
                    new NodeProperty { Name = "duration", Type = "float", Label = "Transition Duration", DefaultValue = "1.0" },
                    new NodeProperty { Name = "easeIn", Type = "int", Label = "Ease In Type", DefaultValue = "0" },
                    new NodeProperty { Name = "easeOut", Type = "int", Label = "Ease Out Type", DefaultValue = "0" }
                },
                CodeTemplate = "local camPoint = Quest:GetThingWithScriptName(\"{cameraPoint}\")\nif camPoint ~= nil then\n    Quest:CameraUseCameraPoint(camPoint, Me, {duration}, {easeIn}, {easeOut})\n    Quest:Pause({duration})\nelse\n    Quest:Log(\"Warning: Camera point '{cameraPoint}' not found\")\nend\n{CHILDREN}" },

            new() { Type = "cameraConversation", Label = "Conversation Camera", Category = "action", Icon = "??", IsAdvanced = true,
                Description = "Set up camera for dialogue scene (0=Default, 1=Close, 2=OTS_Speaker, 3=OTS_Listener) - QUEST THREAD ONLY",
                Properties = new() {
                    new NodeProperty { Name = "cameraOp", Type = "int", Label = "Camera Type", DefaultValue = "3" }
                },
                CodeTemplate = "-- This API only works from Quest threads, NOT entity scripts\n-- Use with a quest thread that monitors DialogueTriggered flag\nlocal stranger = Quest:GetThingWithScriptName(\"ENTITY_SCRIPT_NAME\")\nlocal hero = Quest:GetHero()\nif stranger and hero then\n    Quest:CameraDoConversation(stranger, hero, {cameraOp})\n    Quest:Pause(0.5)\nend\n{CHILDREN}" },

            // ===== SCREEN EFFECTS (Just pause for timing, no frame checks) =====
            new() { Type = "screenFadeOut", Label = "Screen Fade Out", Category = "action", Icon = "?", IsAdvanced = false,
                Description = "Fade screen to black",
                Properties = new() {
                    new NodeProperty { Name = "duration", Type = "float", Label = "Duration", DefaultValue = "0.5" }
                },
                CodeTemplate = "Quest:FadeScreenOutUntilNextCallToFadeScreenIn({duration})\nQuest:Pause({duration})\n{CHILDREN}" },

            new() { Type = "screenFadeIn", Label = "Screen Fade In", Category = "action", Icon = "?", IsAdvanced = false,
                Description = "Fade screen back from black",
                Properties = new() {
                    new NodeProperty { Name = "duration", Type = "float", Label = "Duration", DefaultValue = "0.5" }
                },
                CodeTemplate = "Quest:FadeScreenIn({duration})\nQuest:Pause({duration})\n{CHILDREN}" },

            new() { Type = "radialBlur", Label = "Radial Blur", Category = "action", Icon = "??", IsAdvanced = true,
                Description = "Apply radial blur effect (intensity 0-1, inner/outer radius, fade params)",
                Properties = new() {
                    new NodeProperty { Name = "intensity", Type = "float", Label = "Intensity (0-1)", DefaultValue = "0.5" },
                    new NodeProperty { Name = "innerRadius", Type = "float", Label = "Inner Radius", DefaultValue = "0.0" },
                    new NodeProperty { Name = "outerRadius", Type = "float", Label = "Outer Radius", DefaultValue = "1.0" },
                    new NodeProperty { Name = "fadeIn", Type = "float", Label = "Fade In Time", DefaultValue = "0.5" },
                    new NodeProperty { Name = "hold", Type = "float", Label = "Hold Time", DefaultValue = "0.0" },
                    new NodeProperty { Name = "fadeOut", Type = "float", Label = "Fade Out Time", DefaultValue = "0.0" },
                    new NodeProperty { Name = "unknown", Type = "float", Label = "Unknown Param", DefaultValue = "0.0" }
                },
                CodeTemplate = "Quest:RadialBlurFadeTo({intensity}, {innerRadius}, {outerRadius}, {fadeIn}, {hold}, {fadeOut}, {unknown})\nQuest:Pause({fadeIn})\n{CHILDREN}" },

            new() { Type = "radialBlurOff", Label = "Radial Blur Off", Category = "action", Icon = "??", IsAdvanced = true,
                Description = "Cancel any active radial blur effect",
                Properties = new(),
                CodeTemplate = "Quest:CancelRadialBlurFade()\n{CHILDREN}" },

            new() { Type = "colorFilter", Label = "Color Filter", Category = "action", Icon = "??", IsAdvanced = true,
                Description = "Apply color filter to screen (5 params + color: saturation, brightness, contrast, intensity, fade)",
                Properties = new() {
                    new NodeProperty { Name = "saturation", Type = "float", Label = "Saturation", DefaultValue = "1.0" },
                    new NodeProperty { Name = "brightness", Type = "float", Label = "Brightness", DefaultValue = "1.0" },
                    new NodeProperty { Name = "contrast", Type = "float", Label = "Contrast", DefaultValue = "1.0" },
                    new NodeProperty { Name = "intensity", Type = "float", Label = "Intensity (0-1)", DefaultValue = "0.5" },
                    new NodeProperty { Name = "fadeTime", Type = "float", Label = "Fade Time", DefaultValue = "0.5" },
                    new NodeProperty { Name = "r", Type = "float", Label = "Red (0-1)", DefaultValue = "1.0" },
                    new NodeProperty { Name = "g", Type = "float", Label = "Green (0-1)", DefaultValue = "1.0" },
                    new NodeProperty { Name = "b", Type = "float", Label = "Blue (0-1)", DefaultValue = "1.0" }
                },
                CodeTemplate = "Quest:ScreenFilterFadeTo({saturation}, {brightness}, {contrast}, {intensity}, {fadeTime}, {r={r}, g={g}, b={b}})\nQuest:Pause({fadeTime})\n{CHILDREN}" },

            new() { Type = "colorFilterOff", Label = "Color Filter Off", Category = "action", Icon = "??", IsAdvanced = true,
                Description = "Reset color filter to neutral (requires handle from FadeTo - use neutral values instead)",
                Properties = new() {
                    new NodeProperty { Name = "fadeTime", Type = "float", Label = "Fade Time", DefaultValue = "0.5" }
                },
                CodeTemplate = "-- Reset to neutral filter (intensity 0)\nQuest:ScreenFilterFadeTo(1.0, 1.0, 1.0, 0.0, {fadeTime}, {r=1.0, g=1.0, b=1.0})\nQuest:Pause({fadeTime})\n{CHILDREN}" },

            // ===== MUSIC (Just pause for timing) =====
            new() { Type = "overrideMusic", Label = "Override Music", Category = "action", Icon = "??", IsAdvanced = false,
                Description = "Override current music with specified music set",
                Properties = new() {
                    new NodeProperty { Name = "musicSetType", Type = "int", Label = "Music Set Type", DefaultValue = "2" },
                    new NodeProperty { Name = "isCutscene", Type = "bool", Label = "Is Cutscene Music", DefaultValue = "true" },
                    new NodeProperty { Name = "forcePlay", Type = "bool", Label = "Force Play", DefaultValue = "true" }
                },
                CodeTemplate = "Quest:OverrideMusic({musicSetType}, {isCutscene}, {forcePlay})\n{CHILDREN}" },

            new() { Type = "stopMusicOverride", Label = "Stop Music Override", Category = "action", Icon = "??", IsAdvanced = false,
                Description = "Stop overriding music and return to normal",
                Properties = new(),
                CodeTemplate = "Quest:StopOverrideMusic()\n{CHILDREN}" },

            new() { Type = "enableDangerMusic", Label = "Enable Danger Music", Category = "action", Icon = "??", IsAdvanced = true,
                Description = "Enable or disable danger/combat music",
                Properties = new() {
                    new NodeProperty { Name = "enabled", Type = "bool", Label = "Enabled", DefaultValue = "true" }
                },
                CodeTemplate = "Quest:EnableDangerMusic({enabled})\n{CHILDREN}" },

            new() { Type = "playMovie", Label = "Play Movie", Category = "action", Icon = "???", IsAdvanced = true,
                Description = "Play an AVI movie file",
                Properties = new() {
                    new NodeProperty { Name = "movieName", Type = "string", Label = "Movie Name", DefaultValue = "intro" }
                },
                CodeTemplate = "Quest:PlayAVIMovie(\"{movieName}\")\n{CHILDREN}" },

            // ===== ENTITY CONTROL =====
            new() { Type = "makeHostile", Label = "Make Hostile", Category = "action", Icon = "??", IsAdvanced = false,
                Description = "Make this entity hostile to the hero",
                Properties = new(),
                CodeTemplate = "Quest:EntitySetThingAsEnemyOfThing(Me, hero)\n{CHILDREN}" },

            new() { Type = "makeFriendly", Label = "Make Friendly", Category = "action", Icon = "??", IsAdvanced = false,
                Description = "Make this entity friendly to the hero",
                Properties = new(),
                CodeTemplate = "Me:SetFriendsWithEverythingFlag(true)\n{CHILDREN}" },

            new() { Type = "killEntity", Label = "Kill Entity", Category = "action", Icon = "??", IsAdvanced = false,
                Description = "Kill this entity",
                Properties = new(),
                CodeTemplate = "Quest:SetThingAsKilled(Me)\n{CHILDREN}" },

            new() { Type = "teleportEntity", Label = "Teleport Entity", Category = "action", Icon = "?", IsAdvanced = true,
                Description = "Teleport this entity to a marker",
                Properties = new() {
                    new NodeProperty { Name = "marker", Type = "string", Label = "Marker Name", DefaultValue = "MARKER_SPAWN" }
                },
                CodeTemplate = "local marker = Quest:GetThingWithScriptName(\"{marker}\")\nif marker ~= nil then\n    Quest:EntityTeleportToThing(Me, marker)\nelse\n    Quest:Log(\"Warning: Marker '{marker}' not found for teleport\")\nend\n{CHILDREN}" },

            new() { Type = "followHero", Label = "Follow Hero", Category = "action", Icon = "??", IsAdvanced = false,
                Description = "Make entity follow the hero",
                Properties = new() {
                    new NodeProperty { Name = "distance", Type = "float", Label = "Follow Distance", DefaultValue = "3.0" }
                },
                CodeTemplate = "Me:FollowThing(hero, {distance}, true)\n{CHILDREN}" },

            new() { Type = "stopFollowing", Label = "Stop Following", Category = "action", Icon = "??", IsAdvanced = false,
                Description = "Stop following a target entity",
                Properties = new() {
                    new NodeProperty { Name = "target", Type = "string", Label = "Target Script Name", DefaultValue = "Hero" }
                },
                CodeTemplate = "local followTarget = \"{target}\"\nlocal followThing = nil\nif followTarget == \"Hero\" then\n    followThing = Quest:GetHero()\nelse\n    followThing = Quest:GetThingWithScriptName(followTarget)\nend\nif followThing ~= nil then\n    Me:StopFollowingThing(followThing)\nend\n{CHILDREN}" },

            new() { Type = "sheatheWeapons", Label = "Sheathe Weapons", Category = "action", Icon = "??", IsAdvanced = true,
                Description = "Put weapons away",
                Properties = new(),
                CodeTemplate = "Quest:EntitySheatheWeapons(Me)\n{CHILDREN}" },

            new() { Type = "unsheatheWeapons", Label = "Unsheathe Weapons", Category = "action", Icon = "???", IsAdvanced = true,
                Description = "Draw weapons",
                Properties = new(),
                CodeTemplate = "Quest:EntityUnsheatheWeapons(Me)\n{CHILDREN}" },

            // ===== MOVEMENT =====
            new() { Type = "moveToMarker", Label = "Move To Marker", Category = "action", Icon = "??", IsAdvanced = false,
                Description = "Walk to specified marker",
                Properties = new() {
                    new NodeProperty { Name = "marker", Type = "string", Label = "Marker Name", DefaultValue = "MARKER_1" }
                },
                CodeTemplate = "local marker = Quest:GetThingWithScriptName(\"{marker}\")\nif marker ~= nil then\n    Me:MoveToThing(marker)\nelse\n    Quest:Log(\"Warning: Marker '{marker}' not found\")\nend\n{CHILDREN}" },

            new() { Type = "moveToPosition", Label = "Move To Position", Category = "action", Icon = "??", IsAdvanced = true,
                Description = "Move to specific coordinates",
                Properties = new() {
                    new NodeProperty { Name = "x", Type = "float", Label = "X", DefaultValue = "0" },
                    new NodeProperty { Name = "y", Type = "float", Label = "Y", DefaultValue = "0" },
                    new NodeProperty { Name = "z", Type = "float", Label = "Z", DefaultValue = "0" }
                },
                CodeTemplate = "Me:MoveToPosition({x={x}, y={y}, z={z}}, 1.0, 1)\n{CHILDREN}" },

            new() { Type = "teleportToMarker", Label = "Teleport To Marker", Category = "action", Icon = "?", IsAdvanced = true,
                Description = "Instantly teleport to a marker",
                Properties = new() {
                    new NodeProperty { Name = "marker", Type = "string", Label = "Marker Name", DefaultValue = "MARKER_SPAWN" }
                },
                CodeTemplate = "local marker = Quest:GetThingWithScriptName(\"{marker}\")\nif marker ~= nil then\n    Quest:EntityTeleportToThing(Me, marker)\nelse\n    Quest:Log(\"Warning: Marker '{marker}' not found for teleport\")\nend\n{CHILDREN}" },

            // ===== ANIMATIONS =====
            new() { Type = "playAnimation", Label = "Play Animation", Category = "action", Icon = "??", IsAdvanced = true,
                Description = "Play an animation (non-blocking)",
                Properties = new() {
                    new NodeProperty { Name = "anim", Type = "string", Label = "Animation", DefaultValue = "idle" },
                    new NodeProperty { Name = "stayOnLastFrame", Type = "bool", Label = "Stay On Last Frame", DefaultValue = "false" },
                    new NodeProperty { Name = "allowLooking", Type = "bool", Label = "Allow Looking", DefaultValue = "true" }
                },
                CodeTemplate = "Me:PlayAnimation(\"{anim}\", {stayOnLastFrame}, {allowLooking})\n{CHILDREN}" },

            new() { Type = "playAnimationBlocking", Label = "Play Animation (Wait)", Category = "action", Icon = "?", IsAdvanced = true,
                Description = "Play an animation and wait for completion",
                Properties = new() {
                    new NodeProperty { Name = "anim", Type = "string", Label = "Animation", DefaultValue = "idle" },
                    new NodeProperty { Name = "stayOnLastFrame", Type = "bool", Label = "Stay On Last Frame", DefaultValue = "false" },
                    new NodeProperty { Name = "allowLooking", Type = "bool", Label = "Allow Looking", DefaultValue = "true" }
                },
                CodeTemplate = "Me:GainControlAndPlayAnimation(\"{anim}\", true, {stayOnLastFrame}, {allowLooking})\n{CHILDREN}" },

            new() { Type = "playLoopingAnim", Label = "Play Looping Anim", Category = "action", Icon = "??", IsAdvanced = true,
                Description = "Play animation multiple times",
                Properties = new() {
                    new NodeProperty { Name = "anim", Type = "string", Label = "Animation", DefaultValue = "idle" },
                    new NodeProperty { Name = "loops", Type = "int", Label = "Loop Count", DefaultValue = "1" },
                    new NodeProperty { Name = "useMovement", Type = "bool", Label = "Use Movement", DefaultValue = "false" },
                    new NodeProperty { Name = "allowLooking", Type = "bool", Label = "Allow Looking", DefaultValue = "true" }
                },
                CodeTemplate = "Me:PlayLoopingAnimation(\"{anim}\", {loops}, {useMovement}, {allowLooking})\n{CHILDREN}" },

            // ===== QUEST COMPLETION =====
            new() { Type = "completeQuest", Label = "Complete Quest", Category = "action", Icon = "?", IsAdvanced = false,
                Description = "Mark quest as completed and give rewards (Note: Rewards must be configured in quest settings)",
                Properties = new() {
                    new NodeProperty { Name = "showScreen", Type = "bool", Label = "Show Completion Screen", DefaultValue = "true" }
                },
                CodeTemplate = "Quest:SetStateBool(\"QuestCompleted\", true)\nbreak\n{CHILDREN}" },
            
            new() { Type = "failQuest", Label = "Fail Quest", Category = "action", Icon = "?", IsAdvanced = false,
                Description = "Mark quest as failed",
                Properties = new() {
                    new NodeProperty { Name = "message", Type = "text", Label = "Failure Message", DefaultValue = "Quest Failed" }
                },
                CodeTemplate = "Quest:SetQuestAsFailed(\"{QUEST_NAME}\", true, \"{message}\", true)\n{CHILDREN}" },
            
            new() { Type = "wait", Label = "Wait", Category = "action", Icon = "??", IsAdvanced = false,
                Description = "Pause execution for specified seconds",
                Properties = new() {
                    new NodeProperty { Name = "seconds", Type = "float", Label = "Seconds", DefaultValue = "1.0" }
                },
                CodeTemplate = "Quest:Pause({seconds})\n{CHILDREN}" },

            // ===== ABILITIES =====
            new() { Type = "giveAbility", Label = "Give Ability", Category = "action", Icon = "?", IsAdvanced = false,
                Description = "Grant hero a combat ability",
                Properties = new() {
                    new NodeProperty { Name = "abilityId", Type = "int", Label = "Ability ID", DefaultValue = "1", Options = new List<string>(GameData.Abilities) }
                },
                CodeTemplate = "Quest:GiveHeroAbility({abilityId}, true)\n{CHILDREN}" },
            
            new() { Type = "giveExpression", Label = "Give Expression", Category = "action", Icon = "??", IsAdvanced = true,
                Description = "Unlock a hero expression",
                Properties = new() {
                    new NodeProperty { Name = "expression", Type = "string", Label = "Expression Name", DefaultValue = "LAUGH" },
                    new NodeProperty { Name = "level", Type = "int", Label = "Level", DefaultValue = "1" }
                },
                CodeTemplate = "Quest:GiveHeroExpression(\"{expression}\", {level})\n{CHILDREN}" }
        };
    }

    /// <summary>
    /// Executes GetConditionNodes.
    /// </summary>
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
            new() { Type = "checkStateBool", Label = "Check State (Bool)", Category = "condition", Icon = "?", IsAdvanced = false, HasBranching = true,
                Description = "Check if boolean state variable equals value",
                Properties = new() {
                    new NodeProperty { Name = "name", Type = "string", Label = "State Name", DefaultValue = "questStarted" },
                    new NodeProperty { Name = "value", Type = "bool", Label = "Expected Value", DefaultValue = "true" }
                },
                CodeTemplate = "if Quest:GetStateBool(\"{name}\") == {value} then\n{TRUE}\nelse\n{FALSE}\nend" },

            new() { Type = "checkStateInt", Label = "Check State (Int)", Category = "condition", Icon = "?", IsAdvanced = true, HasBranching = true,
                Description = "Check if integer state variable equals value",
                Properties = new() {
                    new NodeProperty { Name = "name", Type = "string", Label = "State Name", DefaultValue = "killCount" },
                    new NodeProperty { Name = "value", Type = "int", Label = "Expected Value", DefaultValue = "0" }
                },
                CodeTemplate = "if Quest:GetStateInt(\"{name}\") == {value} then\n{TRUE}\nelse\n{FALSE}\nend" },

            new() { Type = "checkStateFloat", Label = "Check State (Float)", Category = "condition", Icon = "?", IsAdvanced = true, HasBranching = true,
                Description = "Check if float state variable equals value",
                Properties = new() {
                    new NodeProperty { Name = "name", Type = "string", Label = "State Name", DefaultValue = "progress" },
                    new NodeProperty { Name = "value", Type = "float", Label = "Expected Value", DefaultValue = "0.0" }
                },
                CodeTemplate = "if tonumber(Quest:GetStateString(\"{name}\")) == {value} then\n{TRUE}\nelse\n{FALSE}\nend" },

            new() { Type = "checkStateString", Label = "Check State (String)", Category = "condition", Icon = "?", IsAdvanced = true, HasBranching = true,
                Description = "Check if string state variable equals value",
                Properties = new() {
                    new NodeProperty { Name = "name", Type = "string", Label = "State Name", DefaultValue = "questStage" },
                    new NodeProperty { Name = "value", Type = "string", Label = "Expected Value", DefaultValue = "started" }
                },
                CodeTemplate = "if Quest:GetStateString(\"{name}\") == \"{value}\" then\n{TRUE}\nelse\n{FALSE}\nend" },

            new() { Type = "checkGlobal", Label = "Check Global", Category = "condition", Icon = "??", IsAdvanced = true, HasBranching = true,
                Description = "Check a global state variable",
                Properties = new() {
                    new NodeProperty { Name = "name", Type = "string", Label = "Variable Name", DefaultValue = "globalFlag" },
                    new NodeProperty { Name = "value", Type = "string", Label = "Expected Value", DefaultValue = "true" }
                },
                CodeTemplate = "if Quest:GetGlobalBool(\"{name}\") == {value} then\n{TRUE}\nelse\n{FALSE}\nend" },

            new() { Type = "checkHasItem", Label = "Has Item", Category = "condition", Icon = "??", IsAdvanced = false, HasBranching = true,
                Description = "Check if hero has specific item in inventory or possession",
                Properties = new() {
                    new NodeProperty { Name = "item", Type = "string", Label = "Item", DefaultValue = "OBJECT_APPLE", Options = new List<string>(GameData.Objects) }
                },
                CodeTemplate = "local hasItem = false\nif Quest:IsObjectInHeroPossession(\"{item}\") then\n    hasItem = true\nelseif Quest:GetNumberOfItemsOfTypeInInventory(\"{item}\") > 0 then\n    hasItem = true\nelseif Quest:IsPlayerCarryingItemOfType(\"{item}\") then\n    hasItem = true\nend\nif hasItem then\n{TRUE}\nelse\n{FALSE}\nend" },

            new() { Type = "checkIsAlive", Label = "Is Alive", Category = "condition", Icon = "?", IsAdvanced = false, HasBranching = true,
                Description = "Check if entity is alive",
                Properties = new(),
                CodeTemplate = "if Me:IsAlive() then\n{TRUE}\nelse\n{FALSE}\nend" },

            new() { Type = "checkYesNoAnswer", Label = "Check Answer", Category = "condition", Icon = "?", IsAdvanced = false, HasBranching = true,
                Description = "Check yes/no question answer and branch (Yes/No/Unsure) - Use after Yes/No Question node",
                Properties = new(),
                BranchLabels = new List<string> { "Yes", "No", "Unsure" },
                CodeTemplate = "if answer ~= nil then\n    if answer == 0 then\n{Yes}\n    elseif answer == 1 then\n{No}\n    else\n{Unsure}\n    end\nend" },
            
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
                CodeTemplate = "if Quest:IsQuestCompleted(\"{questName}\") then\n{TRUE}\nelse\n{FALSE}\nend" },

            new() { Type = "checkBoastTaken", Label = "Boast Taken", Category = "condition", Icon = "??", IsAdvanced = true, HasBranching = true,
                Description = "Check if hero has taken a specific boast",
                Properties = new() {
                    new NodeProperty { Name = "boastID", Type = "int", Label = "Boast ID", DefaultValue = "1" },
                    new NodeProperty { Name = "questName", Type = "string", Label = "Quest Name", DefaultValue = "QUEST_NAME" }
                },
                CodeTemplate = "if Quest:IsBoastTaken({boastID}, \"{questName}\") then\n{TRUE}\nelse\n{FALSE}\nend" },

            new() { Type = "checkCameraScripted", Label = "Camera Scripted", Category = "condition", Icon = "??", IsAdvanced = true, HasBranching = true,
                Description = "Check if camera is currently in scripted mode",
                Properties = new(),
                CodeTemplate = "if Quest:IsCameraInScriptedMode() then\n{TRUE}\nelse\n{FALSE}\nend" },

            new() { Type = "checkDistanceToHero", Label = "Distance To Hero", Category = "condition", Icon = "??", IsAdvanced = false, HasBranching = true,
                Description = "Check if distance between entity and hero is under threshold",
                Properties = new() {
                    new NodeProperty { Name = "distance", Type = "float", Label = "Distance", DefaultValue = "5.0" }
                },
                CodeTemplate = "if Quest:IsDistanceBetweenThingsUnder(Me, hero, {distance}) then\n{TRUE}\nelse\n{FALSE}\nend" }
        };
    }

    /// <summary>
    /// Executes GetFlowNodes.
    /// </summary>
    public static List<NodeDefinition> GetFlowNodes()
    {
        return new List<NodeDefinition>
        {
            new() { Type = "branch", Label = "Branch", Category = "flow", Icon = "?", IsAdvanced = false, HasBranching = true,
                Description = "Executes True or False path based on a boolean value",
                Properties = new() {
                    new NodeProperty { Name = "condition", Type = "bool", Label = "Condition", DefaultValue = "true" }
                },
                CodeTemplate = "if {condition} then\n{TRUE}\nelse\n{FALSE}\nend" },

            new() { Type = "sequence", Label = "Sequence", Category = "flow", Icon = "?", IsAdvanced = false,
                Description = "Execute children in sequential order",
                Properties = new(),
                CodeTemplate = "{CHILDREN}" },
            
            new() { Type = "parallel", Label = "Parallel", Category = "flow", Icon = "?", IsAdvanced = true,
                Description = "Execute children in parallel (falls back to sequential in entity scripts)",
                Properties = new(),
                CodeTemplate = "-- Parallel execution\n{CHILDREN}" },
            
            new() { Type = "loop", Label = "Loop", Category = "flow", Icon = "??", IsAdvanced = false,
                Description = "Repeat children N times",
                Properties = new() {
                    new NodeProperty { Name = "count", Type = "int", Label = "Loop Count", DefaultValue = "3" }
                },
                CodeTemplate = "for i = 1, {count} do\n{CHILDREN}\nif Me:IsNull() then break end\nif not Quest:NewScriptFrame(Me) then break end\nend" },
            
            new() { Type = "whileLoop", Label = "While Loop", Category = "flow", Icon = "?", IsAdvanced = true,
                Description = "Repeat while condition is true",
                Properties = new() {
                    new NodeProperty { Name = "condition", Type = "text", Label = "Condition (Lua)", DefaultValue = "Me:IsAlive()" }
                },
                CodeTemplate = "while {condition} do\n{CHILDREN}\nQuest:Pause(0)\nif Me:IsNull() then break end\nif not Quest:NewScriptFrame(Me) then break end\nend" },
            
            new() { Type = "delay", Label = "Delay", Category = "flow", Icon = "??", IsAdvanced = false,
                Description = "Wait before continuing",
                Properties = new() {
                    new NodeProperty { Name = "seconds", Type = "float", Label = "Seconds", DefaultValue = "1.0" }
                },
                CodeTemplate = "Quest:Pause({seconds})\n{CHILDREN}" },
            
            new() { Type = "randomChoice", Label = "Random Choice", Category = "flow", Icon = "??", IsAdvanced = true,
                Description = "Execute children with random selection (use multiple children for branching)",
                Properties = new() {
                    new NodeProperty { Name = "maxChoice", Type = "int", Label = "Number of Choices", DefaultValue = "3" }
                },
                CodeTemplate = "-- Random choice (1 to {maxChoice})\nlocal randomChoice = math.random(1, {maxChoice})\n{CHILDREN}" },
            
            new() { Type = "callFunction", Label = "Call Function", Category = "flow", Icon = "??", IsAdvanced = true,
                Description = "Call a custom Lua function",
                Properties = new() {
                    new NodeProperty { Name = "functionName", Type = "string", Label = "Function Name", DefaultValue = "CustomFunction" }
                },
                CodeTemplate = "{functionName}()\n{CHILDREN}" },

            new() { Type = "defineEvent", Label = "Define Event", Category = "custom", Icon = "??", IsAdvanced = false,
                Description = "Create a custom event that can be called from anywhere in the graph",
                Properties = new() {
                    new NodeProperty { Name = "eventName", Type = "string", Label = "Event Name", DefaultValue = "MyCustomEvent" }
                },
                CodeTemplate = "-- Event: {eventName}\nfunction Event_{eventName}()\n{CHILDREN}\nend" },

            new() { Type = "callEvent", Label = "Call Event", Category = "custom", Icon = "??", IsAdvanced = false,
                Description = "Trigger a custom event defined elsewhere",
                Properties = new() {
                    new NodeProperty { Name = "eventName", Type = "string", Label = "Event Name", DefaultValue = "MyCustomEvent" }
                },
                CodeTemplate = "Event_{eventName}()\n{CHILDREN}" }
        };
    }
}

/// <summary>
/// Describes a node type, its UI metadata, and its code template.
/// </summary>
public class NodeDefinition
{
    /// <summary>
    /// Gets or sets Type.
    /// </summary>
    public string Type { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets Label.
    /// </summary>
    public string Label { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets Category.
    /// </summary>
    public string Category { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets Icon.
    /// </summary>
    public string Icon { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets IsAdvanced.
    /// </summary>
    public bool IsAdvanced { get; set; }
    /// <summary>
    /// Gets or sets Description.
    /// </summary>
    public string Description { get; set; } = string.Empty;
    /// <summary>
    /// Executes This member.
    /// </summary>
    public List<NodeProperty> Properties { get; set; } = new();
    /// <summary>
    /// Gets or sets CodeTemplate.
    /// </summary>
    public string CodeTemplate { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets HasBranching.
    /// </summary>
    public bool HasBranching { get; set; } = false;
    /// <summary>
    /// Gets or sets BranchLabels.
    /// </summary>
    public List<string>? BranchLabels { get; set; } = null;
    /// <summary>
    /// Gets or sets ValueType.
    /// </summary>
    public string? ValueType { get; set; }
}

/// <summary>
/// Describes a configurable property for a node type.
/// </summary>
public class NodeProperty
{
    /// <summary>
    /// Gets or sets Name.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets Type.
    /// </summary>
    public string Type { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets Label.
    /// </summary>
    public string Label { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets DefaultValue.
    /// </summary>
    public object? DefaultValue { get; set; }
    /// <summary>
    /// Gets or sets Options.
    /// </summary>
    public List<string>? Options { get; set; }
}

