# PR Review Guide

This guide summarizes the current change-set and proposes a reviewer order that minimizes risk.

## Scope

Primary goals completed in this pass:
- Added template-based regression coverage and snapshots.
- Added consistency-audit tests across registry/schema/UI/codegen/deploy.
- Added structured diagnostics (`FQT-VAL-*`, `FQT-CG-*`, `FQT-IO-*`) to validation/codegen/io paths.
- Added diagnostics source-of-truth catalog and drift guard test.
- Aligned legacy codegen tests/snapshots with current generator behavior.

## Risk Areas

### Low risk: documentation and guard rails
- `DOCS/CONSISTENCY_AUDIT_CLOSURE.md`
- `DOCS/PR_REVIEW_GUIDE.md`
- `FableQuestTool/Core/DiagnosticCatalog.cs`
- `FableQuestTool.Tests/DiagnosticsCatalogTests.cs`

### Medium risk: test-only behavior contracts
- `FableQuestTool.Tests/TemplateRegressionTests.cs`
- `FableQuestTool.Tests/ConsistencyAuditTests.cs`
- `FableQuestTool.Tests/CodeGeneratorContractTests.cs`
- `FableQuestTool.Tests/ProjectFileServiceTests.cs` (new migration/diagnostics assertions)
- `FableQuestTool.Tests/ViewModelEntityTabTests.cs` (connection invariants)
- `FableQuestTool.Tests/ConfigTests.cs`, `FableQuestTool.Tests/DeploymentServiceTests.cs`, `FableQuestTool.Tests/DeploymentToggleTests.cs` (diagnostics assertions)
- `FableQuestTool.Tests/CodeGeneratorTests.cs` (expectation alignment)
- `FableQuestTool.Tests/Fixtures/Snapshots/*.lua` (snapshot refresh)

### Higher risk: production diagnostics message surface
- `FableQuestTool/Services/ProjectValidator.cs`
- `FableQuestTool/Services/ProjectFileService.cs`
- `FableQuestTool/Services/CodeGenerator.cs`
- `FableQuestTool/Services/DeploymentService.cs`
- `FableQuestTool/ViewModels/MainViewModel.cs`

Risk note:
- Changes in production files are mostly additive prefixes/codes and do not alter core control flow.
- `ProjectValidator` shape changed (`ValidationIssue` now includes optional `Code`), but existing call sites remain compatible.

## Recommended Review Order

1. `FableQuestTool/Core/DiagnosticCatalog.cs` + `FableQuestTool.Tests/DiagnosticsCatalogTests.cs`
2. `FableQuestTool/Services/ProjectValidator.cs` + `FableQuestTool.Tests/ProjectValidatorTests.cs`
3. `FableQuestTool/Services/ProjectFileService.cs` + migration/diagnostics tests in `FableQuestTool.Tests/ProjectFileServiceTests.cs`
4. `FableQuestTool/Services/CodeGenerator.cs` + `FableQuestTool.Tests/CodeGeneratorContractTests.cs` + `FableQuestTool.Tests/CodeGeneratorTests.cs`
5. Snapshot diff review in `FableQuestTool.Tests/Fixtures/Snapshots/*.lua`
6. `FableQuestTool/Services/DeploymentService.cs` + deployment/toggle/config tests
7. `FableQuestTool/ViewModels/MainViewModel.cs` for UI-facing diagnostics text prefixes
8. Final pass over `FableQuestTool.Tests/TemplateRegressionTests.cs` and `DOCS/CONSISTENCY_AUDIT_CLOSURE.md`

## Suggested Verification Commands

1. Targeted diagnostics and template checks:
`dotnet test FableQuestTool.Tests/FableQuestTool.Tests.csproj --filter "FullyQualifiedName~DiagnosticsCatalogTests|FullyQualifiedName~TemplateRegressionTests" -v minimal`

2. Full suite:
`dotnet test FQT.sln -v minimal`

Expected at close of this pass:
- Full suite green: `Passed 167, Failed 0`.

## Follow-up (optional hardening)

1. Add a CI step that fails if `DiagnosticCatalog` is out of sync with source.
2. Add a policy test that all `ValidationSeverity.Error` issues require non-null `Code`.
3. Add a script to auto-refresh snapshots with explicit reviewer confirmation.
