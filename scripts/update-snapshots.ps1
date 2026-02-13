param(
    [switch]$IncludeTemplateSnapshots,
    [string]$ConfirmToken
)

$ErrorActionPreference = "Stop"

$requiredToken = "I_UNDERSTAND_SNAPSHOT_UPDATE"
if ($ConfirmToken -ne $requiredToken) {
    Write-Host "Snapshot update aborted."
    Write-Host "This script rewrites test fixture snapshots."
    Write-Host "Re-run with: -ConfirmToken $requiredToken"
    exit 1
}

$repoRoot = Split-Path -Parent $PSScriptRoot
Push-Location $repoRoot
try {
    Write-Host "Updating code generator snapshots..."
    $env:FQT_UPDATE_SNAPSHOTS = "1"
    dotnet test "FableQuestTool.Tests/FableQuestTool.Tests.csproj" --filter "FullyQualifiedName~CodeGeneratorSnapshotTests" -v minimal
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }

    if ($IncludeTemplateSnapshots) {
        Write-Host "Updating template regression snapshots..."
        $env:FQT_UPDATE_TEMPLATE_SNAPSHOTS = "1"
        dotnet test "FableQuestTool.Tests/FableQuestTool.Tests.csproj" --filter "FullyQualifiedName~TemplateRegressionTests.BuiltInTemplates_GeneratedLua_MatchesSnapshots" -v minimal
        if ($LASTEXITCODE -ne 0) {
            exit $LASTEXITCODE
        }
    }
}
finally {
    Remove-Item Env:FQT_UPDATE_SNAPSHOTS -ErrorAction SilentlyContinue
    Remove-Item Env:FQT_UPDATE_TEMPLATE_SNAPSHOTS -ErrorAction SilentlyContinue
    Pop-Location
}

Write-Host "Snapshot update completed."
