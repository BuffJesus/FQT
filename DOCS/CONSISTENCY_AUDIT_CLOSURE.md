# Consistency Audit Closure

This document maps each Deliverable 1 audit axis to the concrete test and source enforcement currently in the repo.

## 1) Node schema <-> pin typing <-> graph invariants <-> UI connection rules

### Source contracts
- Node schema and property types: `FableQuestTool/Data/NodeDefinitions.cs`
- Connector typing and mapping: `FableQuestTool/ViewModels/ConnectorViewModel.cs`, `FableQuestTool/ViewModels/NodeViewModel.cs`
- Connection invariants and enforcement: `FableQuestTool/ViewModels/EntityTabViewModel.cs` (`FinishConnection` path)

### Tests enforcing contract
- `FableQuestTool.Tests/ConsistencyAuditTests.cs`
  - `NodeDefinitions_HaveUniqueTypeIdsAndSupportedPropertyTypes`
- `FableQuestTool.Tests/ViewModelEntityTabTests.cs`
  - `EntityTab_FinishConnection_RejectsExecToDataTypeMismatch`
  - `EntityTab_FinishConnection_AllowsWildcardDataConnection`
  - `EntityTab_FinishConnection_EnforcesSingleIncomingConnectionPerInput`
  - `EntityTab_FinishConnection_RejectsConnectingIntoTriggerNode`

## 2) Node registry <-> serialization <-> migrations <-> templates

### Source contracts
- Registry/types: `FableQuestTool/Data/NodeDefinitions.cs`
- Serialization and migrations: `FableQuestTool/Services/ProjectFileService.cs`, `FableQuestTool/Services/ProjectFileData.cs`
- Template source/load: `FableQuestTool/Services/TemplateService.cs`

### Tests enforcing contract
- `FableQuestTool.Tests/ConsistencyAuditTests.cs`
  - `BuiltInTemplates_UseKnownNodeTypesOrDocumentedDynamicTypes`
- `FableQuestTool.Tests/TemplateRegressionTests.cs`
  - `BuiltInTemplates_RoundTripSaveLoad_NoFileDiff`
  - `BuiltInTemplates_LoadValidateCompileExport_AllPass`
- `FableQuestTool.Tests/ProjectFileServiceTests.cs`
  - `Load_AppliesMigrationsAndPersistsUpdatedFile`
  - `Load_DoesNotRewriteWhenNoMigrationIsRequired`

## 3) Codegen <-> FSE call usage <-> escaping/scoping rules

### Source contracts
- Generator pipeline and per-node emission: `FableQuestTool/Services/CodeGenerator.cs`
- Escaping, placeholder substitution, variable resolution:
  - `Escape`
  - `ReplacePlaceholderWithVariable`
  - `TryResolveVariableReference`

### Tests enforcing contract
- `FableQuestTool.Tests/CodeGeneratorContractTests.cs`
  - `GenerateQuestScript_EscapesObjectiveTextForLuaStringLiteral`
  - `GenerateEntityScript_EscapesTextPropertyValues`
  - `GenerateEntityScript_ReplacesQuotedVariablePlaceholderWithLuaExpression`
  - `GenerateQuestScript_AddsActionQueueContract_WhenQueueNodeIsPresent`
  - `GenerateEntityScript_EmitsDiagnosticComment_ForUnknownNodeType`
- `FableQuestTool.Tests/TemplateRegressionTests.cs`
  - `BuiltInTemplates_GeneratedLua_MatchesSnapshots`

## 4) Export/Deploy/Launch <-> settings <-> filesystem reality

### Source contracts
- Export layout: `FableQuestTool/Services/ExportService.cs`
- Deploy/toggle/delete/launch and file mutation: `FableQuestTool/Services/DeploymentService.cs`
- Fable/FSE path resolution: `FableQuestTool/Config/FableConfig.cs`

### Tests enforcing contract
- `FableQuestTool.Tests/TemplateRegressionTests.cs`
  - `BuiltInTemplates_Export_ProducesExpectedFileLayout`
- `FableQuestTool.Tests/ExportServiceTests.cs`
- `FableQuestTool.Tests/DeploymentServiceTests.cs`
- `FableQuestTool.Tests/DeploymentToggleTests.cs`
- `FableQuestTool.Tests/ConfigTests.cs`
  - includes launcher-missing and normalized path behavior

## 5) Structured diagnostics contracts

### Source contracts
- Diagnostics catalog: `FableQuestTool/Core/DiagnosticCatalog.cs`
- Validation diagnostics: `FableQuestTool/Services/ProjectValidator.cs`
- Codegen diagnostics: `FableQuestTool/Services/CodeGenerator.cs`
- IO diagnostics: `FableQuestTool/Services/ProjectFileService.cs`, `FableQuestTool/Services/DeploymentService.cs`, `FableQuestTool/ViewModels/MainViewModel.cs`

### Tests enforcing contract
- `FableQuestTool.Tests/DiagnosticsCatalogTests.cs`
  - `AllDiagnosticsInSource_AreRegisteredInCatalog`
  - `CatalogEntries_UseExpectedCodeFormat`
- `FableQuestTool.Tests/ProjectValidatorTests.cs`
  - validates `FQT-VAL-*` presence
- `FableQuestTool.Tests/ProjectFileServiceTests.cs`
  - validates `FQT-IO-001`
- `FableQuestTool.Tests/DeploymentServiceTests.cs`
  - validates deployment/launch IO code prefixes
- `FableQuestTool.Tests/CodeGeneratorContractTests.cs`
  - validates `FQT-CG-001` emission path

## Current status

- Full test suite baseline at close of this audit pass:
  - `dotnet test FQT.sln -v minimal`
  - Result: `Passed 167, Failed 0`
