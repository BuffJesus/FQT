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
                Description = GenerateDescription(functionName, returnType),
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

    private string GenerateDescription(string functionName, string returnType)
    {
        // Generate basic description from function name
        string name = functionName.Replace("_", " ");
        
        if (functionName.StartsWith("Is") || functionName.StartsWith("Msg"))
        {
            return $"Checks if {name.Substring(2).ToLower()}.";
        }
        else if (functionName.StartsWith("Get"))
        {
            return $"Gets {name.Substring(3).ToLower()}.";
        }
        else if (functionName.StartsWith("Set"))
        {
            return $"Sets {name.Substring(3).ToLower()}.";
        }
        else if (returnType == "void")
        {
            return $"Performs: {name.ToLower()}.";
        }
        else
        {
            return $"Returns {returnType}: {name.ToLower()}.";
        }
    }

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
