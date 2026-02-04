# ACTION PLAN (FQT + FSE)

Last updated: 2026-02-04
Scope: Fable Quest Tool (FQT) WPF app and bundled/reference Fable Script Extender (FSE) sources in this repo.
Goal: Close the most critical gaps first (broken/missing features, correctness, legal/docs), then improve quality and maintainability.

---

## P0 — Critical (ship-blocking)

### 1) Implement thread generation in CodeGenerator
- **Why**: User-defined threads are exposed but generated as TODO, so feature is effectively non-functional.
- **Target files**: `FableQuestTool/Services/CodeGenerator.cs`
- **Deliverables**:
  - Generate Lua for `QuestThread` nodes, including optional region parameter and sequencing rules.
  - Define how thread graphs map to Lua (single entry node? multiple roots?).
  - Add unit tests for at least 2 thread scenarios.
- **Acceptance**:
  - No `-- TODO: Implement thread logic` output.
  - Tests pass and prove valid Lua is produced.

### 2) Add explicit license and third-party notices
- **Why**: README references a LICENSE file that does not exist. Bundled binaries and vendor code require attribution.
- **Target files**: `LICENSE`, `THIRD_PARTY_NOTICES.md`
- **Deliverables**:
  - Choose and add a project license.
  - Add third-party notices for:
    - `FableQuestTool/tools/*` (WadBridge, FableMod)
    - `FSE_Source/Vendor/*` (Lua, sol2)
    - NuGet dependencies (Nodify, CommunityToolkit.Mvvm)
- **Acceptance**:
  - README references valid license file.
  - Third-party notices list versions and licenses.

### 3) Fix docs that point to missing/incorrect paths
- **Why**: README says `SampleQuests/` at repo root, but samples live in `FSE_Source/SampleQuests/`.
- **Target files**: `README.md`
- **Deliverables**:
  - Correct sample quest path.
  - Clarify relationship between FQT and `FSE_Source/` (reference only vs. source of truth).
- **Acceptance**:
  - No broken paths in README.

---

## P1 — High (product quality & correctness)

### 4) Expand test coverage on code generation and deployment
- **Why**: Only two tests exist; core workflows are unverified.
- **Target files**: `FableQuestTool.Tests/*`
- **Deliverables**:
  - Tests for quest script generation (Init/Main/OnPersist, rewards, quest cards).
  - Tests for entity scripts (control modes, object rewards, variable nodes).
  - Tests for DeploymentService register/update/remove logic (quests.lua + FinalAlbion.qst).
- **Acceptance**:
  - Test suite covers core user flows without requiring a game install.

### 5) Resolve README encoding artifacts
- **Why**: Garbled characters indicate wrong encoding and reduce trust.
- **Target files**: `README.md`
- **Deliverables**:
  - Normalize file encoding (UTF-8) and fix symbols/arrows.
- **Acceptance**:
  - README renders correctly on GitHub and local editors.

### 6) Document project file format and compatibility policy
- **Why**: Early development implies breaking changes; users need migration guidance.
- **Target files**: new `DOCS/PROJECT_FORMAT.md` (or README section)
- **Deliverables**:
  - Describe project schema, versioning, and migration expectations.
- **Acceptance**:
  - Document includes a schema version field and change policy.

---

## P2 — Medium (developer experience & maintainability)

### 7) Document FSE build prerequisites and compatibility
- **Why**: `FSE_Source/README.md` is technical but lacks build steps and version matrix.
- **Target files**: `FSE_Source/README.md`
- **Deliverables**:
  - Build toolchain/version requirements (MSVC, SDK, etc.).
  - Compatibility notes (Steam version only, known exe hashes if possible).
- **Acceptance**:
  - Clear instructions to build FSE from source.

### 8) Establish data provenance for GameData lists
- **Why**: Game data lists appear static; no source of truth described.
- **Target files**: `FableQuestTool/Data/GameData.cs` and docs
- **Deliverables**:
  - Document how lists are sourced and updated.
  - Optional: add a script to regenerate lists.
- **Acceptance**:
  - Clear update process without manual guesswork.

### 9) Add structured logging and error reporting plan
- **Why**: Early dev; debugging user issues is difficult without logs.
- **Target files**: `FableQuestTool/Services/*`
- **Deliverables**:
  - Define logging destinations and verbosity levels.
  - Add a user-friendly error report mechanism.
- **Acceptance**:
  - Errors surface with actionable messages and logs stored in a known location.

---

## P3 — Nice-to-have (polish & future capabilities)

### 10) Add UI affordances for advanced node behaviors
- **Why**: Node graph is powerful, but advanced behaviors are opaque.
- **Target files**: `FableQuestTool/Views/*`, `ViewModels/*`, `Data/NodeDefinitions.cs`
- **Deliverables**:
  - Inline help/tooltip previews for node templates.
  - Warnings for unsupported features (e.g., parallel nodes running sequentially).
- **Acceptance**:
  - UI surfaces advanced behavior constraints clearly.

### 11) Improve distribution packaging guidance
- **Why**: README references release workflow but lacks versioning/release steps.
- **Target files**: `README.md`, new `DOCS/RELEASE.md`
- **Deliverables**:
  - Release checklist, versioning, and artifact naming.
- **Acceptance**:
  - New contributors can create consistent releases.

---

## Suggested execution order
1) P0.1 Thread generation
2) P0.2 License + third-party notices
3) P0.3 README path fixes
4) P1.4 Test expansion
5) P1.5 README encoding
6) P1.6 Project format docs
7) P2.7 FSE build docs
8) P2.8 Data provenance
9) P2.9 Logging plan
10) P3.10 UI affordances
11) P3.11 Release guidance

---

## Optional: quick wins (same day)
- Fix README sample path and encoding.
- Add missing LICENSE stub (even provisional) + THIRD_PARTY_NOTICES skeleton.
- Replace thread TODO with a minimal implementation and a failing test to lock behavior.
