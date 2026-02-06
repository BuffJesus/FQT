using System.Linq;
using FableQuestTool.Services;
using Xunit;

namespace FableQuestTool.Tests;

public sealed class ApiParserTests
{
    [Fact]
    public void ParseHeaderFile_LoadsFunctionsAndCategories()
    {
        string headerPath = System.IO.Path.Combine(
            TestPaths.GetRepoRoot(),
            "FSE_Source",
            "ALL-INTERFACE-FUNCTIONS-FOR-FSE.h");

        ApiParser parser = new ApiParser();
        var functions = parser.ParseHeaderFile(headerPath);

        Assert.NotEmpty(functions);
        Assert.Contains(functions, f => f.Category == "Entity API");
        Assert.Contains(functions, f => f.Category == "Quest API");
        Assert.Contains(functions, f => f.Name == "AcquireControl");
    }

    [Fact]
    public void ParseHeaderFile_ParsesOptionalParameters()
    {
        string headerPath = System.IO.Path.Combine(
            TestPaths.GetRepoRoot(),
            "FSE_Source",
            "ALL-INTERFACE-FUNCTIONS-FOR-FSE.h");

        ApiParser parser = new ApiParser();
        var functions = parser.ParseHeaderFile(headerPath);

        var playAnim = functions.FirstOrDefault(f => f.Name == "PlayAnimation_NonBlocking");
        Assert.NotNull(playAnim);
        Assert.Contains(playAnim!.Parameters, p => p.Name == "stayOnLastFrame" && p.IsOptional);

        var speakAndWait = functions.FirstOrDefault(f => f.Name == "SpeakAndWait");
        Assert.NotNull(speakAndWait);
        Assert.Contains(speakAndWait!.Parameters, p => p.Name == "selectionMethod" && p.IsOptional);
    }
}
