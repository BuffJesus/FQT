# Game Data Lists

Last updated: 2026-02-04

This document describes the source and maintenance expectations for static game
lists in `FableQuestTool/Data/GameData.cs`.

---

## What is included

`GameData.cs` contains static lists used to populate dropdowns and validate input:

- Regions
- Creatures
- Quest cards
- Objects
- Abilities

---

## Current source of truth

These lists are currently curated manually based on:
- Known working samples in `FSE_Source/SampleQuests/`
- Existing Fable/FSE community knowledge
- Empirical testing (when available)

There is no automated extraction pipeline yet.

---

## Update process (manual)

1. Identify the missing/incorrect entry (region, creature, object, etc.).
2. Verify the correct internal name via FSE logging or known working quests.
3. Add/update the entry in `FableQuestTool/Data/GameData.cs`.
4. If the entry is used in node templates, validate code generation and behavior.
5. Document the change in this file with a short note if it’s a special case.

---

## Recommended future improvements

- Add a small tooling script to regenerate lists from a known data source.
- Store list metadata (source, date, version) alongside each category.
- Validate list entries against a canonical registry during CI.
