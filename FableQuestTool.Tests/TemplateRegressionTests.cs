using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using FableQuestTool.Models;
using FableQuestTool.Services;
using Xunit;

namespace FableQuestTool.Tests;

public sealed class TemplateRegressionTests
{
    private const string UpdateSnapshotsEnvVar = "FQT_UPDATE_TEMPLATE_SNAPSHOTS";
    private static readonly JsonSerializerOptions CloneOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    [Fact]
    public void BuiltInTemplates_LoadValidateCompileExport_AllPass()
    {
        TemplateService templateService = new TemplateService();
        ProjectValidator validator = new ProjectValidator();
        CodeGenerator generator = new CodeGenerator();
        ExportService exportService = new ExportService(generator);

        List<QuestTemplate> templates = GetTemplates(templateService);
        Assert.NotEmpty(templates);

        foreach (QuestTemplate template in templates)
        {
            Assert.NotNull(template.Template);

            QuestProject project = CloneProject(template.Template!);
            List<ValidationIssue> issues = validator.Validate(project);
            List<ValidationIssue> errors = issues.Where(i => i.Severity == ValidationSeverity.Error).ToList();
            Assert.True(errors.Count == 0,
                $"Template '{template.Name}' has validation errors: {string.Join(" | ", errors.Select(e => e.Message))}");

            string questScript = generator.GenerateQuestScript(project);
            Assert.False(string.IsNullOrWhiteSpace(questScript));

            foreach (QuestEntity entity in project.Entities)
            {
                string entityScript = generator.GenerateEntityScript(project, entity);
                Assert.False(string.IsNullOrWhiteSpace(entityScript));
            }

            using TestTempDirectory temp = new TestTempDirectory();
            string outputFolder = exportService.Export(project, temp.Path);
            Assert.True(Directory.Exists(outputFolder), $"Template '{template.Name}' export folder does not exist.");
        }
    }

    [Fact]
    public void BuiltInTemplates_RoundTripSaveLoad_NoFileDiff()
    {
        TemplateService templateService = new TemplateService();
        ProjectFileService projectFileService = new ProjectFileService();
        List<QuestTemplate> templates = GetTemplates(templateService);

        foreach (QuestTemplate template in templates)
        {
            QuestProject original = CloneProject(template.Template!);

            using TestTempDirectory temp = new TestTempDirectory();
            string firstPath = Path.Combine(temp.Path, $"{SanitizeFileName(template.Name)}.first.fqtproj");
            string secondPath = Path.Combine(temp.Path, $"{SanitizeFileName(template.Name)}.second.fqtproj");

            projectFileService.Save(firstPath, original);
            QuestProject loaded = projectFileService.Load(firstPath);
            projectFileService.Save(secondPath, loaded);

            string firstText = NormalizeLineEndings(File.ReadAllText(firstPath));
            string secondText = NormalizeLineEndings(File.ReadAllText(secondPath));
            Assert.Equal(firstText, secondText);
        }
    }

    [Fact]
    public void BuiltInTemplates_GeneratedLua_MatchesSnapshots()
    {
        TemplateService templateService = new TemplateService();
        CodeGenerator generator = new CodeGenerator();
        List<QuestTemplate> templates = GetTemplates(templateService);

        foreach (QuestTemplate template in templates)
        {
            QuestProject project = CloneProject(template.Template!);
            string templateFolder = Path.Combine("TemplateSnapshots", SanitizeFileName(template.Name));

            string questScript = generator.GenerateQuestScript(project);
            AssertSnapshot(Path.Combine(templateFolder, "Quest.lua"), questScript);

            foreach (QuestEntity entity in project.Entities.OrderBy(e => e.ScriptName, StringComparer.Ordinal))
            {
                string entityScript = generator.GenerateEntityScript(project, entity);
                string entityFileName = $"{SanitizeFileName(entity.ScriptName)}.lua";
                AssertSnapshot(Path.Combine(templateFolder, "Entities", entityFileName), entityScript);
            }

            if (generator.NeedsContainerEntityScript(project))
            {
                ContainerReward container = project.Rewards.Container!;
                string containerScript = generator.GenerateContainerEntityScript(project, container);
                string containerFileName = $"{SanitizeFileName(container.ContainerScriptName)}.lua";
                AssertSnapshot(Path.Combine(templateFolder, "Entities", containerFileName), containerScript);
            }
        }
    }

    [Fact]
    public void BuiltInTemplates_Export_ProducesExpectedFileLayout()
    {
        TemplateService templateService = new TemplateService();
        CodeGenerator generator = new CodeGenerator();
        ExportService exportService = new ExportService(generator);
        List<QuestTemplate> templates = GetTemplates(templateService);

        foreach (QuestTemplate template in templates)
        {
            QuestProject project = CloneProject(template.Template!);

            using TestTempDirectory temp = new TestTempDirectory();
            string questFolder = exportService.Export(project, temp.Path);

            Assert.Equal(Path.Combine(temp.Path, project.Name), questFolder);
            Assert.True(File.Exists(Path.Combine(questFolder, $"{project.Name}.lua")));
            Assert.True(File.Exists(Path.Combine(questFolder, "_quests_registration.lua")));

            string entitiesFolder = Path.Combine(questFolder, "Entities");
            Assert.True(Directory.Exists(entitiesFolder));

            foreach (QuestEntity entity in project.Entities)
            {
                Assert.True(File.Exists(Path.Combine(entitiesFolder, $"{entity.ScriptName}.lua")),
                    $"Missing exported entity script for '{entity.ScriptName}' in template '{template.Name}'.");
            }

            if (generator.NeedsContainerEntityScript(project))
            {
                string containerName = project.Rewards.Container!.ContainerScriptName;
                Assert.True(File.Exists(Path.Combine(entitiesFolder, $"{containerName}.lua")),
                    $"Missing exported container script '{containerName}' in template '{template.Name}'.");
            }
        }
    }

    private static List<QuestTemplate> GetTemplates(TemplateService templateService)
    {
        return templateService.GetAllTemplates()
            .Where(t => t.Template != null)
            .OrderBy(t => t.Category, StringComparer.Ordinal)
            .ThenBy(t => t.Name, StringComparer.Ordinal)
            .ToList();
    }

    private static QuestProject CloneProject(QuestProject project)
    {
        string json = JsonSerializer.Serialize(project, CloneOptions);
        return JsonSerializer.Deserialize<QuestProject>(json, CloneOptions) ?? new QuestProject();
    }

    private static void AssertSnapshot(string relativeFixturePath, string actual)
    {
        string snapshotPath = TestPaths.GetFixturePath(relativeFixturePath);
        bool updateSnapshots = string.Equals(
            Environment.GetEnvironmentVariable(UpdateSnapshotsEnvVar),
            "1",
            StringComparison.Ordinal);

        if (updateSnapshots)
        {
            string? directory = Path.GetDirectoryName(snapshotPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(snapshotPath, actual);
        }

        Assert.True(File.Exists(snapshotPath),
            $"Snapshot not found: {snapshotPath}. Set {UpdateSnapshotsEnvVar}=1 and re-run tests to generate snapshots.");

        string expected = NormalizeLineEndings(File.ReadAllText(snapshotPath));
        string normalizedActual = NormalizeLineEndings(actual);
        Assert.Equal(expected, normalizedActual);
    }

    private static string NormalizeLineEndings(string text)
    {
        return text.Replace("\r\n", "\n").Replace("\r", "\n").TrimEnd('\n');
    }

    private static string SanitizeFileName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "unnamed";
        }

        char[] invalid = Path.GetInvalidFileNameChars();
        char[] chars = value.Select(c => invalid.Contains(c) ? '_' : c).ToArray();
        return new string(chars).Replace(' ', '_');
    }
}
