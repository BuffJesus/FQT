# Testing Plan

Last updated: 2026-02-05
Scope: Fable Quest Tool (FQT) WPF app and non-game dependencies in this repo.
Goal: Provide a concrete, repeatable test strategy that covers core logic, file formats,
      and deployment without requiring a live game install for most runs.

---

## Principles

- Fast feedback: keep unit tests under a few seconds; run on every change.
- Offline-first: most tests run without a Fable install or FSE runtime.
- Deterministic outputs: prefer golden files for Lua generation and format parsing.
- Layered confidence: unit tests -> integration tests -> UI smoke -> optional in-game.

---

## Test Layers

### 1) Unit tests (fast, deterministic)
Target: pure logic, parsers, validators, generators.

- Code generation: quest script, entity script, container script.
- Name validation and project validation rules.
- Parsing: API headers, TNG files, QST files, INI files.
- Data helpers: region mappings, coordinate conversion, quest ID allocation.
- Format readers: BigArchive/BigBank/BigEntry/BigReader.
- Template and node definition integrity (see coverage map below).

### 2) Integration tests (filesystem + temp folders)
Target: end-to-end flows without the game.

- Project save/load round trip.
- Export pipeline produces expected Lua and assets.
- Deployment to a temp FSE folder (fake Fable install).
- quests.lua / FinalAlbion.qst / FSE_Master.lua update logic.

### 3) UI and view-model tests
Target: command wiring and state transitions.

- ViewModel command enable/disable based on state.
- Error states from services appear in view-model properties.
- Selected entity/node state updates and undo/redo (when added).

### 4) Manual smoke tests (short checklist)
Target: major user flows using the built app.

- Create new quest, add entity node graph, save/reopen, export.
- Load sample quest and regenerate Lua.
- Deploy to a staged FSE directory (not the real game folder).
- Search/filter in API reference, templates, and entity browser.

### 5) Optional in-game validation
Target: final sanity before release.

- Launch via FSE, verify quest activation.
- Entity scripts run (dialogue, rewards, triggers).
- Quest completion and rewards delivered.

---

## Coverage Map (by module)

### Core
- `FableQuestTool/Core/IniFile.cs`: read/write round trip with comments and ordering.
- `FableQuestTool/Core/FileWrite.cs`: atomic write behavior and error handling.
- `FableQuestTool/Core/Guard.cs`: expected exceptions for invalid inputs.

### Config and IO
- `FableQuestTool/Config/FableConfig.cs`: path validation and FSE folder resolution.
- `FableQuestTool/Services/ProjectFileService.cs`: serialize/deserialize project files.
- `FableQuestTool/Services/ProjectValidator.cs`: missing fields and invalid references.

### Data and Definitions
- `FableQuestTool/Data/GameData.cs`: lists contain required baseline entries.
- `FableQuestTool/Data/NodeDefinitions.cs`: every node type has a template and
  valid inputs/outputs; required properties are present.
- `FableQuestTool/Services/GameDataCatalogService.cs`: filtering and lookup logic.

### Formats
- `FableQuestTool/Formats/BigArchive.cs`
- `FableQuestTool/Formats/BigBank.cs`
- `FableQuestTool/Formats/BigEntry.cs`
- `FableQuestTool/Formats/BigReader.cs`
- `FableQuestTool/Formats/QstFile.cs`
Use `FSE_Source/SampleQuests/NewQuest/FinalAlbion.qst.example` as a fixture and
assert read/write stability.

### Parsing and Catalogs
- `FableQuestTool/Services/ApiParser.cs`: parse the header in `FSE_Source/ALL-INTERFACE-FUNCTIONS-FOR-FSE.h`
  and validate categories + parameter handling.
- `FableQuestTool/Services/TngParser.cs`: parse minimal TNG fixtures and filter by category.
- `FableQuestTool/Services/LevelDataService.cs`: directory scanning and invalid path handling.
- `FableQuestTool/Services/RegionTngMapping.cs`: region-to-file mapping correctness.

### Code Generation
- `FableQuestTool/Services/CodeGenerator.cs`: golden-file tests for:
  - Quest Init/Main/OnPersist generation.
  - Thread generation and exit behavior.
  - Entity graph translation (single node, branching, loops).
  - Reward logic: gold, renown, items, containers.
  - Control modes and object behaviors.

### Deployment and Export
- `FableQuestTool/Services/DeploymentService.cs`: deploy/remove/enable/disable to temp FSE folders.
- `FableQuestTool/Services/ExportService.cs`: exported file layout and generated content.
- `FableQuestTool/Services/WadBridgeClient.cs`: error handling and process invocation stubs.

### ViewModels
- `FableQuestTool/ViewModels/*`: command availability and selection changes.
- Focus on `MainViewModel`, `EntityEditorViewModel`, `QuestManagerViewModel`,
  and `ApiReferenceViewModel` for the biggest user flows.

---

## Test Data and Fixtures

- `FSE_Source/SampleQuests/*`: existing Lua samples for snapshot comparisons.
- Minimal fake FSE folder structure for deployment tests:
  - `FSE/quests.lua` with a small baseline table.
  - `FSE/Master/FSE_Master.lua` with a minimal `Main()` stub.
  - `FinalAlbion.qst` from the sample fixture.
- Small synthetic project JSON files for save/load round-trip tests.
- Minimal TNG snippets stored under `FableQuestTool.Tests/Fixtures/`.

---

## Execution Plan (phased)

Phase 1: Core correctness (unit tests)
- Build unit coverage for CodeGenerator, NameValidation, ApiParser, TngParser.
- Add fixtures and golden files for code generation.

Phase 2: Integration workflows
- Project save/load round trip tests.
- Deployment to temp folder, including register/unregister logic.

Phase 3: UI and usability checks
- ViewModel command state tests.
- Manual checklist for the major flows.

Phase 4: In-game validation
- Smoke a single quest in Fable via FSE.

---

## Minimal Commands

- Run unit tests: `dotnet test FableQuestTool.Tests/FableQuestTool.Tests.csproj`
- Run all tests: `dotnet test`

### Guarded Snapshot Refresh

Use the helper script to intentionally rewrite snapshot fixtures.

- Codegen snapshots only:
`powershell -ExecutionPolicy Bypass -File .\scripts\update-snapshots.ps1 -ConfirmToken I_UNDERSTAND_SNAPSHOT_UPDATE`

- Codegen + template snapshots:
`powershell -ExecutionPolicy Bypass -File .\scripts\update-snapshots.ps1 -IncludeTemplateSnapshots -ConfirmToken I_UNDERSTAND_SNAPSHOT_UPDATE`

The script exits without changing files unless the exact confirmation token is supplied.

---

## Open Questions

- Should we split tests into fast and slow categories (xUnit Traits) for CI?
- Do we want golden Lua snapshots or assertion-based checks only?
- Is there a stable, redistributable test QST file for format tests?
