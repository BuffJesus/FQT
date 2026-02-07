using System.IO;
using System.Text.RegularExpressions;
using FableQuestTool.Models;

namespace FableQuestTool.Services;

/// <summary>
/// Parses FSE header files into API metadata for the in-app reference view.
/// </summary>
public class ApiParser
{
    /// <summary>
    /// Parses a header file and returns discovered API functions.
    /// </summary>
    public List<ApiFunction> ParseHeaderFile(string filePath)
    {
        var functions = new List<ApiFunction>();
        
        if (!File.Exists(filePath))
        {
            return functions;
        }

        string[] lines = File.ReadAllLines(filePath);
        string currentCategory = "General";

        foreach (string line in lines)
        {
            string trimmed = line.Trim();

            // Detect category headers
            if (trimmed.Contains("ENTITY-API"))
            {
                currentCategory = "Entity API";
            }
            else if (trimmed.Contains("QUEST-API"))
            {
                currentCategory = "Quest API";
            }
            else if (trimmed.Contains("GAME-API"))
            {
                currentCategory = "Game API";
            }
            else if (trimmed.Contains("HERO-API"))
            {
                currentCategory = "Hero API";
            }

            // Parse function declarations
            if (IsFunctionDeclaration(trimmed))
            {
                var function = ParseFunction(trimmed, currentCategory);
                if (function != null)
                {
                    functions.Add(function);
                }
            }
        }

        return functions;
    }

    private bool IsFunctionDeclaration(string line)
    {
        // Basic check for function declaration
        return line.Contains("(") && line.Contains(")") && line.EndsWith(";") 
            && !line.StartsWith("//") && !line.StartsWith("/*") && !line.StartsWith("#");
    }

    private ApiFunction? ParseFunction(string line, string category)
    {
        try
        {
            // Remove semicolon and trim
            line = line.TrimEnd(';').Trim();

            // Extract return type and function signature
            int openParen = line.IndexOf('(');
            if (openParen == -1) return null;

            string beforeParen = line.Substring(0, openParen).Trim();
            string[] parts = beforeParen.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 2) return null;

            string functionName = parts[parts.Length - 1];
            string returnType = string.Join(" ", parts.Take(parts.Length - 1));

            // Extract parameters
            int closeParen = line.LastIndexOf(')');
            string paramString = line.Substring(openParen + 1, closeParen - openParen - 1).Trim();
            
            var parameters = ParseParameters(paramString);

            return new ApiFunction
            {
                Name = functionName,
                ReturnType = returnType,
                Category = category,
                Parameters = parameters,
                Description = GenerateDescription(functionName, returnType, parameters, category),
                Example = GenerateExample(functionName, parameters, category)
            };
        }
        catch
        {
            return null;
        }
    }

    private List<ApiParameter> ParseParameters(string paramString)
    {
        var parameters = new List<ApiParameter>();
        
        if (string.IsNullOrWhiteSpace(paramString))
        {
            return parameters;
        }

        // Split by comma, but handle template parameters
        var parts = SplitParameters(paramString);

        foreach (string part in parts)
        {
            string trimmed = part.Trim();
            if (string.IsNullOrWhiteSpace(trimmed)) continue;

            bool hasDefaultValue = trimmed.Contains("=");
            bool isOptional = trimmed.Contains("sol::optional") || hasDefaultValue;

            if (hasDefaultValue)
            {
                int equalsIndex = trimmed.IndexOf('=');
                if (equalsIndex > 0)
                {
                    trimmed = trimmed.Substring(0, equalsIndex).Trim();
                }
            }
            
            // Extract type and name
            string[] tokens = trimmed.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length < 2) continue;

            string paramType = string.Join(" ", tokens.Take(tokens.Length - 1));
            string paramName = tokens[tokens.Length - 1].TrimEnd('*', '&');

            parameters.Add(new ApiParameter
            {
                Type = paramType,
                Name = paramName,
                IsOptional = isOptional
            });
        }

        return parameters;
    }

    private List<string> SplitParameters(string paramString)
    {
        var results = new List<string>();
        int depth = 0;
        int start = 0;

        for (int i = 0; i < paramString.Length; i++)
        {
            if (paramString[i] == '<') depth++;
            else if (paramString[i] == '>') depth--;
            else if (paramString[i] == ',' && depth == 0)
            {
                results.Add(paramString.Substring(start, i - start));
                start = i + 1;
            }
        }

        if (start < paramString.Length)
        {
            results.Add(paramString.Substring(start));
        }

        return results;
    }

    private string GenerateDescription(string functionName, string returnType, List<ApiParameter> parameters, string category)
    {
        bool isNonBlocking = functionName.EndsWith("_NonBlocking", StringComparison.Ordinal);
        string baseName = isNonBlocking
            ? functionName[..^"_NonBlocking".Length]
            : functionName;

        if (DescriptionOverrides.TryGetValue(baseName, out string? overrideDescription))
        {
            return AppendAsyncNote(overrideDescription, isNonBlocking, category);
        }

        string entityContext = GetEntityContext(parameters, category);
        string normalizedName = baseName;
        if (normalizedName.StartsWith("Entity", StringComparison.Ordinal))
        {
            normalizedName = normalizedName["Entity".Length..];
            if (string.IsNullOrWhiteSpace(entityContext))
            {
                entityContext = "the target entity";
            }
        }

        string description;
        if (normalizedName.StartsWith("MsgIs", StringComparison.Ordinal))
        {
            string eventName = Humanize(normalizedName["MsgIs".Length..]).ToLowerInvariant();
            description = $"Returns true if {EventSubject(category)} {eventName}.";
        }
        else if (normalizedName.StartsWith("MsgReceived", StringComparison.Ordinal))
        {
            string eventName = Humanize(normalizedName["MsgReceived".Length..]).ToLowerInvariant();
            description = $"Returns true if {EventSubject(category)} received {eventName}.";
        }
        else if (normalizedName.StartsWith("MsgWho", StringComparison.Ordinal))
        {
            string eventName = Humanize(normalizedName["MsgWho".Length..]).ToLowerInvariant();
            description = $"Returns the entity that {eventName}.";
        }
        else if (normalizedName.StartsWith("Is", StringComparison.Ordinal))
        {
            string condition = Humanize(normalizedName["Is".Length..]).ToLowerInvariant();
            string subject = Subject(category, entityContext);
            description = string.IsNullOrWhiteSpace(subject)
                ? $"Returns true if {condition}."
                : $"Returns true if {subject} {condition}.";
        }
        else if (normalizedName.StartsWith("Get", StringComparison.Ordinal))
        {
            string target = Humanize(normalizedName["Get".Length..]).ToLowerInvariant();
            if (returnType.Contains("table", StringComparison.OrdinalIgnoreCase) && target.Contains("pos", StringComparison.Ordinal))
            {
                description = $"Returns the position of {Subject(category, entityContext)} as a table {{x, y, z}}.";
            }
            else if (returnType.Contains("table", StringComparison.OrdinalIgnoreCase))
            {
                description = $"Returns a table containing {target}.";
            }
            else
            {
                description = $"Returns {target}.";
            }
        }
        else if (normalizedName.StartsWith("Set", StringComparison.Ordinal))
        {
            string target = Humanize(normalizedName["Set".Length..]).ToLowerInvariant();
            description = $"Sets {target}{ContextSuffix(category, entityContext)}.";
        }
        else if (normalizedName.StartsWith("Reset", StringComparison.Ordinal))
        {
            string target = Humanize(normalizedName["Reset".Length..]).ToLowerInvariant();
            description = $"Resets {target}{ContextSuffix(category, entityContext)} to default.";
        }
        else if (normalizedName.StartsWith("Add", StringComparison.Ordinal))
        {
            string target = Humanize(normalizedName["Add".Length..]).ToLowerInvariant();
            description = $"Adds {target}.";
        }
        else if (normalizedName.StartsWith("Remove", StringComparison.Ordinal))
        {
            string target = Humanize(normalizedName["Remove".Length..]).ToLowerInvariant();
            description = $"Removes {target}.";
        }
        else if (normalizedName.StartsWith("Clear", StringComparison.Ordinal))
        {
            string target = Humanize(normalizedName["Clear".Length..]).ToLowerInvariant();
            description = $"Clears {target}{ContextSuffix(category, entityContext)}.";
        }
        else if (normalizedName.StartsWith("Activate", StringComparison.Ordinal))
        {
            string target = Humanize(normalizedName["Activate".Length..]).ToLowerInvariant();
            description = $"Activates {target}.";
        }
        else if (normalizedName.StartsWith("Deactivate", StringComparison.Ordinal))
        {
            string target = Humanize(normalizedName["Deactivate".Length..]).ToLowerInvariant();
            description = $"Deactivates {target}.";
        }
        else if (normalizedName.StartsWith("GiveHero", StringComparison.Ordinal))
        {
            string target = Humanize(normalizedName["GiveHero".Length..]).ToLowerInvariant();
            description = $"Gives the hero {target}.";
        }
        else if (normalizedName.StartsWith("Give", StringComparison.Ordinal))
        {
            string target = Humanize(normalizedName["Give".Length..]).ToLowerInvariant();
            description = $"Gives {target}.";
        }
        else if (normalizedName.StartsWith("Take", StringComparison.Ordinal))
        {
            string target = Humanize(normalizedName["Take".Length..]).ToLowerInvariant();
            description = $"Takes {target}.";
        }
        else if (normalizedName.StartsWith("Show", StringComparison.Ordinal))
        {
            string target = Humanize(normalizedName["Show".Length..]).ToLowerInvariant();
            description = $"Shows {target}.";
        }
        else if (normalizedName.StartsWith("Play", StringComparison.Ordinal))
        {
            string target = Humanize(normalizedName["Play".Length..]).ToLowerInvariant();
            description = $"Plays {target}.";
        }
        else if (normalizedName.StartsWith("Camera", StringComparison.Ordinal))
        {
            string target = Humanize(normalizedName["Camera".Length..]).ToLowerInvariant();
            description = string.IsNullOrWhiteSpace(target)
                ? "Controls the camera."
                : $"Controls the camera: {target}.";
        }
        else if (normalizedName.StartsWith("Create", StringComparison.Ordinal))
        {
            string target = Humanize(normalizedName["Create".Length..]).ToLowerInvariant();
            description = $"Creates {target}.";
        }
        else if (normalizedName.StartsWith("Start", StringComparison.Ordinal))
        {
            string target = Humanize(normalizedName["Start".Length..]).ToLowerInvariant();
            description = $"Starts {target}.";
        }
        else if (normalizedName.StartsWith("Stop", StringComparison.Ordinal))
        {
            string target = Humanize(normalizedName["Stop".Length..]).ToLowerInvariant();
            description = $"Stops {target}.";
        }
        else if (returnType == "void")
        {
            description = $"Performs {Humanize(normalizedName).ToLowerInvariant()}.";
        }
        else
        {
            description = $"Returns {returnType}: {Humanize(normalizedName).ToLowerInvariant()}.";
        }

        return AppendAsyncNote(description, isNonBlocking, category);
    }

    private static string AppendAsyncNote(string description, bool isNonBlocking, string category)
    {
        if (!isNonBlocking)
        {
            return description;
        }

        string note = " Returns immediately; action continues in the background.";
        if (category == "Entity API")
        {
            note += " Requires AcquireControl first.";
        }

        return description + note;
    }

    private static string EventSubject(string category)
    {
        return category == "Entity API" ? "this entity" : "the entity";
    }

    private static string Subject(string category, string entityContext)
    {
        if (category == "Entity API")
        {
            return "this entity is";
        }

        if (!string.IsNullOrWhiteSpace(entityContext))
        {
            return $"{entityContext} is";
        }

        return string.Empty;
    }

    private static string ContextSuffix(string category, string entityContext)
    {
        if (category == "Entity API")
        {
            return " for this entity";
        }

        if (!string.IsNullOrWhiteSpace(entityContext))
        {
            return $" for {entityContext}";
        }

        return string.Empty;
    }

    private static string GetEntityContext(List<ApiParameter> parameters, string category)
    {
        if (category == "Entity API")
        {
            return "this entity";
        }

        int entityParams = parameters.Count(p => p.Type.Contains("CScriptThing", StringComparison.Ordinal));
        if (entityParams >= 2)
        {
            return "the two entities";
        }

        return entityParams == 1 ? "the target entity" : string.Empty;
    }

    private static string Humanize(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return string.Empty;
        }

        string withSpaces = Regex.Replace(name, "([a-z0-9])([A-Z])", "$1 $2");
        withSpaces = Regex.Replace(withSpaces, "([A-Z]+)([A-Z][a-z])", "$1 $2");
        return withSpaces.Replace("_", " ").Trim();
    }

    private static readonly Dictionary<string, string> DescriptionOverrides = new(StringComparer.OrdinalIgnoreCase)
    {
        ["AcquireControl"] = "Acquires scripted control of this entity so actions can be issued from Lua.",
        ["ReleaseControl"] = "Releases scripted control and returns the entity to normal AI behavior.",
        ["TakeExclusiveControl"] = "Prevents other systems from controlling this entity while scripted.",
        ["MakeBehavioral"] = "Attaches this entity to the quest behavior system.",
        ["ClearCommands"] = "Clears the current command queue for this entity.",
        ["ClearAllActions"] = "Stops all current actions on this entity.",
        ["ClearAllActionsIncludingLoopingAnimations"] = "Stops all actions, including looping animations, on this entity.",
        ["SpeakAndWait"] = "Plays the specified dialogue and waits for it to finish.",
        ["GainControlAndSpeak"] = "Acquires control, then plays the specified dialogue and waits for completion.",
        ["CreateThread"] = "Starts a Lua function in a new parallel thread.",
        ["Pause"] = "Waits for the specified duration while yielding to the game loop.",
        ["NewScriptFrame"] = "Yields control back to the game loop for this script frame.",
        ["ShowMessage"] = "Displays an on-screen message for the given duration."
    };

    private string GenerateExample(string functionName, List<ApiParameter> parameters, string category)
    {
        string paramList = string.Join(", ", parameters.Select(p => GetExampleValue(p)));
        
        if (category == "Entity API")
        {
            return $"Me:{functionName}({paramList})";
        }
        else if (category == "Quest API")
        {
            return $"Quest:{functionName}({paramList})";
        }
        else if (category == "Hero API")
        {
            return $"Hero:{functionName}({paramList})";
        }
        else
        {
            return $"{functionName}({paramList})";
        }
    }

    private string GetExampleValue(ApiParameter param)
    {
        if (param.Type.Contains("string"))
        {
            return $"\"{param.Name}\"";
        }
        else if (param.Type.Contains("bool"))
        {
            return "true";
        }
        else if (param.Type.Contains("int") || param.Type.Contains("float"))
        {
            return "0";
        }
        else if (param.Type.Contains("CScriptThing"))
        {
            return "targetThing";
        }
        else if (param.Type.Contains("table"))
        {
            return "{x=0, y=0, z=0}";
        }
        else
        {
            return param.Name;
        }
    }
}
