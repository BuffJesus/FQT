using System.Collections.Generic;

namespace FableQuestTool.Core;

/// <summary>
/// Source-of-truth catalog for structured diagnostics emitted by FQT.
/// </summary>
public static class DiagnosticCatalog
{
    public static IReadOnlyDictionary<string, string> Entries { get; } = new Dictionary<string, string>
    {
        ["FQT-VAL-001"] = "Project is null.",
        ["FQT-VAL-002"] = "Quest or symbol name validation failure.",
        ["FQT-VAL-003"] = "Quest ID below recommended range.",
        ["FQT-VAL-004"] = "Quest has no regions configured.",
        ["FQT-VAL-005"] = "Entity has no behavior nodes.",
        ["FQT-VAL-006"] = "Entity has no trigger nodes.",
        ["FQT-VAL-007"] = "Entity uses unknown node type.",
        ["FQT-VAL-008"] = "Parallel node warns about sequential runtime behavior.",
        ["FQT-VAL-009"] = "Connection references missing node.",
        ["FQT-VAL-010"] = "Deployment blocked by invalid names.",

        ["FQT-CG-001"] = "Codegen encountered unknown node type.",
        ["FQT-CG-002"] = "Codegen emitted placeholder for empty behavior graph.",
        ["FQT-CG-003"] = "Codegen emitted placeholder for missing trigger nodes.",

        ["FQT-IO-001"] = "Project file payload is invalid.",
        ["FQT-IO-002"] = "Fable install path not configured for deployment.",
        ["FQT-IO-003"] = "FSE installation check failed during deployment.",
        ["FQT-IO-004"] = "FSE folder path could not be resolved.",
        ["FQT-IO-005"] = "Failed to update quests.lua during deployment.",
        ["FQT-IO-006"] = "Failed to update FinalAlbion.qst during deployment.",
        ["FQT-IO-007"] = "Failed to update FSE_Master.lua activation block.",
        ["FQT-IO-008"] = "Unhandled deployment exception.",
        ["FQT-IO-009"] = "Fable install path not configured for launch/delete.",
        ["FQT-IO-010"] = "Launch blocked by missing FSE installation.",
        ["FQT-IO-011"] = "FSE launcher executable not found.",
        ["FQT-IO-012"] = "Unhandled launch exception.",
        ["FQT-IO-013"] = "Fable path not configured in service helper.",
        ["FQT-IO-014"] = "quests.lua quest entry parse failure.",
        ["FQT-IO-015"] = "quests.lua read/write exception.",
        ["FQT-IO-016"] = "FSE_Master.lua missing.",
        ["FQT-IO-017"] = "Main function not found in FSE_Master.lua.",
        ["FQT-IO-018"] = "Main function end not found in FSE_Master.lua.",
        ["FQT-IO-019"] = "FSE_Master.lua read/write exception.",
        ["FQT-IO-020"] = "FinalAlbion.qst file not found.",
        ["FQT-IO-021"] = "FinalAlbion.qst update exception.",
        ["FQT-IO-022"] = "Quest name invalid for delete file operations.",
        ["FQT-IO-023"] = "FSE folder path unresolved for delete.",
        ["FQT-IO-024"] = "Quest not found in installation during delete.",
        ["FQT-IO-025"] = "Unhandled delete exception.",
        ["FQT-IO-026"] = "Fable path not configured for toggle.",
        ["FQT-IO-027"] = "FSE folder path unresolved for toggle.",
        ["FQT-IO-028"] = "Unhandled toggle exception.",

        ["FQT-IO-101"] = "UI open-project error.",
        ["FQT-IO-102"] = "UI save completed but file missing.",
        ["FQT-IO-103"] = "UI save-project error.",
        ["FQT-IO-104"] = "UI delete error.",
        ["FQT-IO-105"] = "UI deploy error.",
        ["FQT-IO-106"] = "UI launch error.",
        ["FQT-IO-107"] = "UI catalog export error.",
        ["FQT-IO-108"] = "UI open-samples error."
    };
}
