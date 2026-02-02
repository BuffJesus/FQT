using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FableQuestTool.Models;

/// <summary>
/// Represents a complete quest project in FableQuestTool.
///
/// QuestProject is the root data structure that contains all information needed to define
/// a custom quest for Fable: The Lost Chapters. It includes quest metadata, configuration,
/// rewards, boasts, state variables, entities, and execution threads.
///
/// When saved, this object is serialized to a .fqtproj JSON file. When deployed, the
/// CodeGenerator service converts this project into Lua scripts that run in the FSE runtime.
/// </summary>
/// <remarks>
/// Quest IDs should be unique and >= 50000 to avoid conflicts with base game quests.
/// The Name property is used as the internal Lua identifier, while DisplayName is shown to players.
/// </remarks>
public sealed partial class QuestProject : ObservableObject
{
    /// <summary>
    /// Internal quest name used as the Lua script identifier.
    /// Must be a valid Lua identifier (no spaces, starts with letter).
    /// </summary>
    /// <example>"BanditHunt" or "RescueMission"</example>
    [ObservableProperty]
    [property: JsonPropertyName("Name")]
    private string name = "NewQuest";

    /// <summary>
    /// Unique numeric identifier for the quest.
    /// Must be >= 50000 to avoid conflicts with base game quests (1-49999).
    /// This ID is used by the Fable engine to track quest progress.
    /// </summary>
    [ObservableProperty]
    [property: JsonPropertyName("Id")]
    private int id = 50000;

    /// <summary>
    /// Human-readable quest name displayed to players in the quest log and UI.
    /// Supports spaces and special characters unlike the internal Name property.
    /// </summary>
    public string DisplayName { get; set; } = "New Quest";

    /// <summary>
    /// Quest description shown to players when viewing quest details.
    /// Typically explains the quest objective and backstory.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// List of region names where this quest takes place.
    /// Regions are Fable's level/area identifiers (e.g., "LOOKOUT_POINT", "BOWERSTONE_SOUTH").
    /// Used for quest registration and entity spawning.
    /// </summary>
    public ObservableCollection<string> Regions { get; set; } = new();

    /// <summary>
    /// Definition name of the quest card object given to the player.
    /// Quest cards are inventory items that track active quests.
    /// Default is "OBJECT_QUEST_CARD_GENERIC" but custom cards can be created.
    /// </summary>
    public string QuestCardObject { get; set; } = "OBJECT_QUEST_CARD_GENERIC";

    /// <summary>
    /// Objective text displayed on the quest card and in the quest log.
    /// Should be a concise description of what the player needs to do.
    /// </summary>
    public string ObjectiveText { get; set; } = string.Empty;

    /// <summary>
    /// Primary region shown as the quest destination on the world map.
    /// Used for quest marker placement and navigation hints.
    /// </summary>
    public string ObjectiveRegion1 { get; set; } = string.Empty;

    /// <summary>
    /// Secondary region for quests spanning multiple areas.
    /// Optional - leave empty for single-region quests.
    /// </summary>
    public string ObjectiveRegion2 { get; set; } = string.Empty;

    /// <summary>
    /// X-axis pixel offset for the quest marker on the world map.
    /// Used to fine-tune marker placement within a region.
    /// </summary>
    [ObservableProperty]
    [property: JsonPropertyName("WorldMapOffsetX")]
    private int worldMapOffsetX;

    /// <summary>
    /// Y-axis pixel offset for the quest marker on the world map.
    /// Used to fine-tune marker placement within a region.
    /// </summary>
    [ObservableProperty]
    [property: JsonPropertyName("WorldMapOffsetY")]
    private int worldMapOffsetY;

    /// <summary>
    /// Whether to display the quest start cinematic screen when the quest begins.
    /// Shows quest title, description, and rewards in a dramatic presentation.
    /// </summary>
    public bool UseQuestStartScreen { get; set; }

    /// <summary>
    /// Whether to display the quest completion screen when the quest ends.
    /// Shows final rewards and quest summary.
    /// </summary>
    public bool UseQuestEndScreen { get; set; }

    /// <summary>
    /// Marks this as a story quest (main storyline).
    /// Story quests have special handling and cannot be abandoned.
    /// </summary>
    public bool IsStoryQuest { get; set; }

    /// <summary>
    /// Marks this as a gold quest (paid quest from Guild).
    /// Gold quests require payment to accept and have different UI treatment.
    /// </summary>
    public bool IsGoldQuest { get; set; }

    /// <summary>
    /// Whether to give the quest card directly to the player without going through Guild.
    /// Useful for quests triggered by world events rather than Guild board.
    /// </summary>
    public bool GiveCardDirectly { get; set; }

    /// <summary>
    /// Whether this quest appears on the Heroes Guild quest board.
    /// Set to false for quests that start through other means (talking to NPCs, etc.).
    /// </summary>
    public bool IsGuildQuest { get; set; } = false;

    /// <summary>
    /// Whether this quest is currently enabled and can be started.
    /// Disabled quests are not registered with the game engine.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Rewards given to the player upon quest completion.
    /// Includes gold, experience, renown, items, and abilities.
    /// </summary>
    public QuestRewards Rewards { get; set; } = new();

    /// <summary>
    /// Collection of boasts (optional challenges) for this quest.
    /// Boasts are additional objectives that grant bonus rewards if completed.
    /// Players can select boasts before starting a Guild quest.
    /// </summary>
    public ObservableCollection<QuestBoast> Boasts { get; set; } = new();

    /// <summary>
    /// Collection of state variables used by this quest.
    /// States track quest progress and can be persisted across game saves.
    /// Accessed in Lua via Quest:GetState() and Quest:SetState().
    /// </summary>
    public ObservableCollection<QuestState> States { get; set; } = new();

    /// <summary>
    /// Collection of entities (NPCs, objects, creatures) managed by this quest.
    /// Each entity has its own behavior defined through a visual node graph.
    /// </summary>
    public ObservableCollection<QuestEntity> Entities { get; set; } = new();

    /// <summary>
    /// Collection of quest threads (parallel execution contexts).
    /// Threads allow running multiple scripts simultaneously in different regions.
    /// Useful for quests that span multiple areas or have concurrent objectives.
    /// </summary>
    public ObservableCollection<QuestThread> Threads { get; set; } = new();
}
