using System;

namespace FableQuestTool.Core;

public static class Guard
{
    public static void NotNull<T>(T value, string paramName) where T : class
    {
        if (value is null)
        {
            throw new ArgumentNullException(paramName);
        }
    }

    /// <summary>
    /// Executes NotNullOrEmpty.
    /// </summary>
    public static void NotNullOrEmpty(string value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be null or empty.", paramName);
        }
    }
}
