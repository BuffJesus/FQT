namespace FableQuestTool.Models;

/// <summary>
/// Represents a boast (optional challenge) for a guild quest.
///
/// Boasts are additional objectives that players can select before starting
/// a guild quest. Completing a boast grants bonus rewards but increases
/// difficulty. Classic Fable boasts include "No Armour", "Without a Scratch",
/// "Solo Mission", etc.
/// </summary>
/// <remarks>
/// Boasts are only available for guild quests (IsGuildQuest = true).
/// Players select boasts at the guild podium before accepting the quest.
/// Failed boasts result in lost gold (the player's wager).
///
/// The quest script is responsible for checking boast conditions and
/// calling Quest:BoastComplete() or Quest:BoastFailed() accordingly.
/// </remarks>
public sealed class QuestBoast
{
    /// <summary>
    /// Unique identifier for this boast within the quest.
    /// Used by the Fable engine to track boast selection and completion.
    /// </summary>
    public int BoastId { get; set; }

    /// <summary>
    /// Display text describing the boast challenge.
    /// Shown to players when selecting boasts.
    /// </summary>
    /// <example>"Complete the quest without taking damage"</example>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Bonus renown points awarded if the boast is completed.
    /// Added to base quest renown on success.
    /// </summary>
    public int RenownReward { get; set; }

    /// <summary>
    /// Bonus gold awarded if the boast is completed.
    /// This is the "winnings" from the player's wager.
    /// </summary>
    public int GoldReward { get; set; }

    /// <summary>
    /// Whether this entry is a boast (true) or a quest requirement (false).
    /// Quest requirements are mandatory and cannot be opted out of.
    /// </summary>
    public bool IsBoast { get; set; }

    /// <summary>
    /// Reference to localized text ID for multi-language support.
    /// If 0, uses the Text property directly.
    /// </summary>
    public int TextId { get; set; }
}
