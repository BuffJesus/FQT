# Game Data Reference Files

This directory contains reference files from Fable: The Lost Chapters game data to assist with quest development.

## Contents

### FinalAlbion.qst
Main quest registry file from the game. Shows how all native quests are registered.

**Location:** `data/Levels/FinalAlbion.qst`

**Purpose:** Reference for quest registration syntax and understanding which quests are enabled in the game.

### Level Files (.lev)

Level files define the physical structure and layout of game regions.

**Included regions:**
- **StartOakValeEast.lev** - Eastern section of childhood Oakvale
- **StartOakValeWest.lev** - Western section of childhood Oakvale
- **StartOakvaleMemorialGarden.lev** - Memorial Garden section of childhood Oakvale
- **BarrowFields.lev** - Lookout Point area (adult game)

**Location:** `data/Levels/FinalAlbion/`

**Purpose:**
- Verify correct region names for quest development
- Reference for understanding region structure
- Identify available markers and spawn points

### Thing Files (.tng)

TNG (Thing) files define objects, NPCs, markers, and interactive elements within each region.

**Included:**
- **StartOakValeEast.tng** - Objects/NPCs in East section
- **StartOakValeWest.tng** - Objects/NPCs in West section
- **StartOakvaleMemorialGarden.tng** - Objects/NPCs in Memorial Garden
- **BarrowFields.tng** - Objects/NPCs in BarrowFields

**Location:** `data/Levels/FinalAlbion/`

**Purpose:**
- Find marker script names (e.g., `MK_OVID_DAD`) for entity spawning
- Identify existing NPCs and objects in regions
- Reference for creature definitions and spawn points

## Usage in Quest Development

### Finding Region Names
Region names in the level files are the actual names used by FSE:
- **Childhood Oakvale** = `"StartOakVale"` (NOT "Oakvale")
- **Adult Lookout Point** = `"BarrowFields"`

Use these exact names in your quest's `Quest:AddQuestRegion()` calls.

### Finding Spawn Markers
TNG files contain marker definitions like:
```
Thing "MK_OVID_DAD"
  DefinitionType "Marker"
  ScriptName "MK_OVID_DAD"
  ...
```

Use these script names to get spawn positions:
```lua
local marker = Quest:GetThingWithScriptName("MK_OVID_DAD")
local pos = marker:GetPos()
Quest:CreateCreature("CREATURE_DEF", pos, "EntityScriptName")
```

### Verifying Region Sections
StartOakVale consists of three sections:
1. StartOakValeEast
2. StartOakValeWest
3. StartOakvaleMemorialGarden

When creating quests for childhood, use `"StartOakVale"` as the region name - FSE will handle loading all three sections.

## Important Notes

- These files are **reference only** - do not modify the game's original files
- Level (.lev) and TNG (.tng) files are binary formats - use hex editor or specialized tools to view
- FinalAlbion.qst is text-based and human-readable
- All files are from the Steam version of Fable: The Lost Chapters

## See Also

- **QUEST_DEVELOPMENT_GUIDE.md** - Quest development patterns and best practices
- **DISCORD_DEVELOPER_PATTERNS.md** - Community examples and patterns
- **SampleQuests/** - Working quest examples using these regions
