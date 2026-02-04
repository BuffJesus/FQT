# Project File Format (FQT)

Last updated: 2026-02-04

This document describes the `.fqtproj` file format used by Fable Quest Tool (FQT).
Projects are stored as JSON and map directly to the `QuestProject` model in
`FableQuestTool/Models/QuestProject.cs`.

---

## Format overview

- **File extension**: `.fqtproj`
- **Encoding**: UTF-8
- **Top-level object**: `QuestProject`
- **Purpose**: Stores quest metadata, entities, states, rewards, and thread settings
  for use by the code generator and deployment pipeline.

---

## Compatibility policy

- This project format is **early and subject to change**.
- Backward compatibility is **best effort** until versioning is added.
- The recommended future direction is to add a `SchemaVersion` integer at the
  top level and implement migrations in the project loader.

---

## Top-level fields (selected)

The following fields exist on the `QuestProject` model and are serialized to JSON.
This is not an exhaustive schema, but covers the core fields used by the generator.

- `Name` (string) — Internal quest name (Lua identifier, no spaces)
- `Id` (int) — Numeric quest ID (recommended >= 50000)
- `DisplayName` (string)
- `Description` (string)
- `Regions` (array of string)
- `QuestCardObject` (string)
- `ObjectiveText` (string)
- `ObjectiveRegion1` (string)
- `ObjectiveRegion2` (string)
- `WorldMapOffsetX` (int)
- `WorldMapOffsetY` (int)
- `UseQuestStartScreen` (bool)
- `UseQuestEndScreen` (bool)
- `IsStoryQuest` (bool)
- `IsGoldQuest` (bool)
- `GiveCardDirectly` (bool)
- `IsGuildQuest` (bool)
- `IsEnabled` (bool)
- `Rewards` (object; see `QuestRewards`)
- `Boasts` (array; see `QuestBoast`)
- `States` (array; see `QuestState`)
- `Entities` (array; see `QuestEntity`)
- `Threads` (array; see `QuestThread`)

---

## Thread fields

Each `QuestThread` entry supports the following properties:

- `FunctionName` (string) — Lua function name for the thread
- `Region` (string) — Region where the thread is activated
- `Description` (string) — Documentation only
- `IntervalSeconds` (float) — Pause interval between iterations (default 0.5)
- `ExitStateName` (string) — Optional quest boolean state to stop the thread
- `ExitStateValue` (bool) — Value that triggers exit (default true)

---

## Notes

- Exact JSON property names and serialization behavior are defined by the models
  in `FableQuestTool/Models/` and the project service.
- If you add new fields, update this document and consider forward/backward
  compatibility implications.
