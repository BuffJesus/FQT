namespace FableQuestTool.Models;

/// <summary>
/// Represents a reusable quest template in the template library.
///
/// Quest templates provide pre-configured starting points for common quest types.
/// They include default entities, behaviors, states, and configuration that can
/// be customized after creation. Templates help users quickly create quests
/// without starting from scratch.
/// </summary>
/// <remarks>
/// Templates are stored in the Templates directory and loaded by TemplateService.
/// Users can create their own templates by saving existing quests as templates.
///
/// Common template categories:
/// - Combat: Kill quests, arena battles, boss fights
/// - Escort: Protect NPCs, caravan defense
/// - Fetch: Item collection, delivery missions
/// - Story: Narrative-driven quests with dialogue
/// </remarks>
public class QuestTemplate
{
    /// <summary>
    /// Display name of the template shown in the template browser.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description explaining what this template provides and its intended use.
    /// Shown to users when browsing templates.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Category for organizing templates in the browser.
    /// Examples: "Combat", "Escort", "Fetch", "Story", "Custom"
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Suggested difficulty level for quests created from this template.
    /// Examples: "Easy", "Medium", "Hard", "Heroic"
    /// </summary>
    public string Difficulty { get; set; } = string.Empty;

    /// <summary>
    /// The actual quest project data used as the template.
    /// Copied and customized when a user creates a new quest from this template.
    /// </summary>
    public QuestProject Template { get; set; } = new();
}
