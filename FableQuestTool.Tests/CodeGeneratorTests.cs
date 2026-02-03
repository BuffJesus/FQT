using FableQuestTool.Models;
using FableQuestTool.Services;
using Xunit;

namespace FableQuestTool.Tests;

public sealed class CodeGeneratorTests
{
    [Fact]
    public void GenerateOnPersist_IncludesStateRoundTrip()
    {
        QuestProject project = new QuestProject();
        project.States.Add(new QuestState { Name = "TestBool", Type = "bool", Persist = true, DefaultValue = true });
        project.States.Add(new QuestState { Name = "TestInt", Type = "int", Persist = true, DefaultValue = 3 });
        project.States.Add(new QuestState { Name = "TestFloat", Type = "float", Persist = true, DefaultValue = 1.5 });
        project.States.Add(new QuestState { Name = "TestString", Type = "string", Persist = true, DefaultValue = "hello" });

        CodeGenerator generator = new CodeGenerator();
        string script = generator.GenerateQuestScript(project);

        Assert.Contains("Quest:PersistTransferBool(context, \"TestBool\", TestBool_value)", script);
        Assert.Contains("Quest:PersistTransferInt(context, \"TestInt\", TestInt_value)", script);
        Assert.Contains("Quest:PersistTransferString(context, \"TestFloat\", TestFloat_value)", script);
        Assert.Contains("Quest:PersistTransferString(context, \"TestString\", TestString_value)", script);
    }
}
