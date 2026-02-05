using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace FableQuestTool.Views;

/// <summary>
/// Validates quest name format for file-safe identifiers.
/// </summary>
public sealed class QuestNameRule : ValidationRule
{
    private static readonly Regex NamePattern = new("^[A-Za-z][A-Za-z0-9_]*$");

    /// <summary>
    /// Executes Validate.
    /// </summary>
    public override ValidationResult Validate(object? value, CultureInfo cultureInfo)
    {
        string text = value?.ToString() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(text))
        {
            return new ValidationResult(false, "Quest name is required.");
        }

        if (!NamePattern.IsMatch(text))
        {
            return new ValidationResult(false, "Use letters, numbers, underscores. Must start with a letter.");
        }

        return ValidationResult.ValidResult;
    }
}

/// <summary>
/// Validates quest IDs for the custom quest range.
/// </summary>
public sealed class QuestIdRule : ValidationRule
{
    /// <summary>
    /// Executes Validate.
    /// </summary>
    public override ValidationResult Validate(object? value, CultureInfo cultureInfo)
    {
        if (!int.TryParse(value?.ToString(), out int id))
        {
            return new ValidationResult(false, "Quest ID must be a number.");
        }

        if (id < 50000 || id > 99999)
        {
            return new ValidationResult(false, "Quest ID must be 50000-99999.");
        }

        return ValidationResult.ValidResult;
    }
}
