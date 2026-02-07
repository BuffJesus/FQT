# FQT User Guide

## Overview
FQT is a visual quest authoring tool for Fable with a node-based entity editor,
a structured Quest Setup workflow, and direct export/deploy into the game.
This guide covers the common workflow and the most important features.

## Getting Started (Cheat Sheet)
Create a quest in 5 minutes:
1) File -> New
2) Quest Setup -> Basic Info (Name + ID)
3) Quest Setup -> Quest Card (Objective Text + Region)
4) Entities -> Add Entity -> place trigger + actions
5) File -> Export and Deploy

If something fails:
- Tools -> Validate Project
- Tools -> Preview Generated Lua
- Tools -> Configure Fable Path

## Quick Start
1) File -> New to create a project.
2) Quest Setup -> Basic Info to name the quest and set an ID.
3) Quest Setup -> Quest Card to configure objective text and regions.
4) Entities tab -> add entities and build behavior with nodes.
5) File -> Export and Deploy to send the quest to your Fable install.

## Keyboard Shortcuts
- Ctrl+N: New project
- Ctrl+O: Open project
- Ctrl+S: Save
- Ctrl+Shift+S: Save As
- Ctrl+E: Export and Deploy
- Ctrl+Shift+V: Validate Project
- Ctrl+P: Preview Generated Lua
- F1: Open User Guide

## Quest Setup Tab
The left list controls which section is visible. Use Advanced Mode for
Optional Challenges and Background Tasks.

### Basic Info
- Quest Name: internal script name (no spaces, unique).
- Display Name: player-facing name.
- Quest ID: numeric ID (recommend 50000+ for custom quests).
- Description: internal description (not shown to players).
- Quest Enabled: unchecked means it will not deploy.

### Quest Card
- Quest Card Object: which quest card item is granted.
- Objective Text: text shown in quest log.
- Objective Regions: used for breadcrumb guidance.
- World Map Offset: leave at 0,0 to auto-calc, or set manually.
- Start/End Screen: enable cinematic quest intro/outro.
- Gold/Story/Guild flags: affect display and quest log ordering.

### Regions
Add regions referenced by your quest. These help with map guidance and data
lookups in other sections.

### Rewards
- Gold/Experience/Renown/Morality: numeric rewards.
- Direct Reward Items: grant items directly to hero on completion.
- Container Rewards: spawn a container with items. Use Spawn Location to
  place near marker/entity or a fixed position.
- Reward Abilities: grant hero abilities on completion.

### Optional Challenges (Advanced Mode)
Boasts are optional objectives that grant extra rewards.

### States
State variables are persisted per quest and are commonly used for logic gates.

### Background Tasks (Advanced Mode)
Threads run functions periodically until an exit condition is met.

## Entities Tab (Node Editor)
This is the core of quest logic and behavior.

## Visual Scripting Basics
Think of nodes as steps in a flow. Triggers start the flow, actions do work,
conditions branch the flow, and flow nodes control execution order.

Node Types
- Trigger: Starts execution (events like "When Hero Talks").
- Action: Performs a task (give item, play animation).
- Condition: Checks something and branches True/False.
- Flow: Organizes execution order (Sequence, Delay, Loop).
- Custom: Define and call your own events.
- Variable: Get/Set data stored on the entity.

Execution Tips
- Triggers do not need input pins.
- Most actions need an Exec input and output.
- Use reroute nodes to keep wires readable.
- Use states for quest-wide logic gates.

### Entity Settings
- Script Name: unique identifier used in Lua.
- Definition Name: game definition to bind or spawn.
- Entity Type / Spawn Method / Region / Marker: define creation or binding.
- Quest Target Options: highlight or show marker on minimap.

### Add Entity
Use the "+ Add Entity" button to create a new entity tab.

### Node Menu
Right-click on the graph (or use the Add Node menu) to insert nodes.
Use the search box to filter nodes. Favorites and Recents are pinned at top.
Hover over a node in the menu to see what it does.

### Graph Tips
- Drag from an output pin to create a connection.
- Double-click a wire to add a reroute node.
- Ctrl+drag on a wire to insert a reroute inline.
- Use Align/Distribute when multiple nodes are selected.

### Variables
Use the Variables panel to create per-entity variables.
Drag variables into properties to bind them, or drop on the graph to create
Get/Set nodes.

### Properties Panel
Selecting a node shows its properties on the right. Use "Bind" to tie a property
to a variable instead of a literal value.

## Templates Tab
Templates provide ready-made quest structures. Choose a template, review the
details, then click "Use This Template" to start a new quest.

## API Reference Tab
Search and browse FSE functions. Each entry includes a description, return
type, parameters, and example usage. Use this to validate node behavior and
write custom Lua snippets.

## Export and Deploy
Use File -> Export and Deploy (or the Deploy button) to push your quest into
Fable. Validation runs before deploy. Fix errors and try again if needed.

## Preview Generated Lua
Tools -> Preview Generated Lua shows the final scripts for the quest and each
entity. Use this for debugging or advanced customization.

## Validation
Tools -> Validate Project checks naming, required fields, and conflicts.
Fix errors before deploying.

## Troubleshooting
- If deploy fails, confirm your Fable path in Tools -> Configure Fable Path.
- If nodes do not behave, check entity script names and node properties.
- Use the API Reference and Lua Preview to verify generated output.
- If advanced sections are missing, enable Advanced Mode in the toolbar.

## Tips and Best Practices
- Keep quest names and entity script names unique and consistent.
- Use states for branching and progression logic.
- Use templates to scaffold large quests quickly.
- Save often and use Save As for experimental branches.

## Beginner Tips (Top 10)
1) Start with a template if you are unsure about structure.
2) Keep entity names short and consistent (NPC_Guard, Gate_Trigger).
3) Use states to remember what the player has done.
4) Use "When Killed By Hero" for clean kill logic.
5) Use Objective Regions so the breadcrumb trail works.
6) If you see no output, check Advanced Mode and section filters.
7) Validate early; fix naming issues before building large graphs.
8) Keep wires tidy with reroute nodes.
9) Preview Lua to confirm what will be exported.
10) Save often; use Save As for variants.

## Common Node Patterns
Talk -> Give Item
- Trigger: When Hero Talks
- Action: Speak To Hero
- Action: Give Item

Gate by State
- Condition: Check State (Bool)
- True: Continue
- False: Show Message / Do Nothing

Increment a Counter
- Action: Set State (Int) with +1 (use a variable or compute in Lua)
- Condition: Check State (Int) >= target

Start -> Delay -> Action
- Trigger: When Triggered
- Flow: Delay
- Action: (any)

## Example Quest Setups
These examples are intentionally simple and are meant to show the flow of
nodes, not the final production layout.

### Example 1: Simple Fetch Quest
Goal: Talk to an NPC, collect an item, return it, complete quest.

Quest Setup
- Basic Info: Name = FetchQuest, ID = 50010
- Quest Card: Objective Text = "Bring the herb to the healer"
- Rewards: Gold = 200, Renown = 50

Entities
1) NPC_Healer (bind existing)
   - Trigger: When Hero Talks
   - Action: Speak To Hero (gives instructions)
   - Action: Set State (Bool) -> hasHerb = false

2) Herb_Item (object or marker)
   - Trigger: When Used By Hero
   - Action: Give Item -> HERB_ITEM
   - Action: Set State (Bool) -> hasHerb = true

3) NPC_Healer (return logic)
   - Trigger: When Hero Talks
   - Condition: Check State (Bool) hasHerb == true
   - True: Take Item -> HERB_ITEM
   - True: Complete Quest
   - False: Show Message -> "You still need the herb."

Notes
- Use one NPC entity with two logic branches, or split into two entities.
- Use states to gate the completion path.

Flow Diagram (ASCII)
[Hero Talks] -> [Give Instructions] -> [Set hasHerb=false]
[Hero Uses Herb] -> [Give Herb] -> [Set hasHerb=true]
[Hero Talks] -> [Check hasHerb]
   True -> [Take Herb] -> [Complete Quest]
   False -> [Show Message]

### Example 2: Kill and Report
Goal: Kill a target and report back to quest giver.

Quest Setup
- Objective Text = "Defeat the bandit leader"
- Reward = Gold/Experience

Entities
1) Bandit_Leader (spawn or bind)
   - Trigger: When Killed By Hero
   - Action: Set State (Bool) -> leaderDead = true

2) NPC_GuardCaptain
   - Trigger: When Hero Talks
   - Condition: Check State (Bool) leaderDead == true
   - True: Speak To Hero (reward text)
   - True: Complete Quest
   - False: Speak To Hero (reminder)

Notes
- Use "When Killed By Hero" rather than "When Killed" for cleaner gating.
- If multiple targets, use integer state and increment on each death.

Flow Diagram (ASCII)
[Bandit Killed] -> [Set leaderDead=true]
[Hero Talks] -> [Check leaderDead]
   True -> [Complete Quest]
   False -> [Reminder Message]

### Example 3: Escort with Region Trigger
Goal: Escort an NPC to a destination region.

Quest Setup
- Objective Text = "Escort the trader to the city gates"

Entities
1) NPC_Trader
   - Trigger: When Hero Talks
   - Action: Follow Hero
   - Action: Show Message -> "Stay close."

2) CityGate_Trigger (region trigger)
   - Trigger: When Triggered
   - Condition: Check State (Bool) escortStarted == true
   - True: Set State (Bool) escortComplete = true
   - True: Complete Quest

3) NPC_Trader (start flag)
   - Trigger: When Hero Talks
   - Action: Set State (Bool) escortStarted = true

Notes
- Use a region trigger entity or a marker volume for the destination.
- Optionally stop following on completion using Stop Following.

Flow Diagram (ASCII)
[Hero Talks] -> [Follow Hero] -> [Set escortStarted=true]
[Gate Triggered] -> [Check escortStarted]
   True -> [Complete Quest]

### Example 4: Timed Quest (Countdown)
Goal: Reach a marker before time runs out.

Quest Setup
- Objective Text = "Reach the town gate before time runs out"

Entities
1) Quest_Start (trigger)
   - Trigger: When Triggered
   - Action: Set State (Int) -> timeLeft = 30
   - Action: Show Message -> "You have 30 seconds!"

2) Countdown_Thread (background task)
   - Thread Function: TickTimer
   - Interval: 1.0
   - Exit State: timerDone (bool)

3) TickTimer (Lua function or node event)
   - Action: Set State (Int) timeLeft = timeLeft - 1
   - Condition: Check State (Int) timeLeft <= 0
   - True: Set State (Bool) timerDone = true
   - True: Fail Quest

4) Gate_Trigger
   - Trigger: When Triggered
   - Condition: Check State (Bool) timerDone == false
   - True: Set State (Bool) timerDone = true
   - True: Complete Quest

Notes
- You can implement the timer in Lua for more control, then call from a node.
- Use Background Tasks for repeated checks.

Flow Diagram (ASCII)
[Start Trigger] -> [Set timeLeft=30] -> [Show Message]
[TickTimer] -> [timeLeft -= 1] -> [Check timeLeft <= 0]
   True -> [Fail Quest]
[Gate Triggered] -> [Check timerDone=false]
   True -> [Complete Quest]

### Example 5: Branching Choice
Goal: Let the player choose a path and reward accordingly.

Entities
1) NPC_Mayor
   - Trigger: When Hero Talks
   - Action: Yes/No Question -> "Will you help the guards?"

2) NPC_Mayor
   - Trigger: When Hero Talks
   - Condition: Check Answer
   - Yes: Set State (Bool) helpGuards = true
   - Yes: Show Message -> "Meet the captain."
   - No: Set State (Bool) helpGuards = false
   - No: Show Message -> "Maybe another time."

Notes
- Use Check Answer immediately after the question node.
- Store the result in a state for later branches.

Flow Diagram (ASCII)
[Hero Talks] -> [Yes/No Question]
[Hero Talks] -> [Check Answer]
   Yes -> [Set helpGuards=true] -> [Show Message]
   No  -> [Set helpGuards=false] -> [Show Message]

### Example 6: Container Rewards + Minimap Marker
Goal: Defeat a boss, then collect loot from a chest.

Quest Setup
- Rewards: Enable container rewards (Container object, items).

Entities
1) Boss
   - Trigger: When Killed By Hero
   - Action: Set State (Bool) bossDead = true
   - Action: Show Message -> "The chest is now available."

2) RewardChest (container object)
   - Entity Settings: Quest Target + Show on Minimap
   - Condition: Check State (Bool) bossDead == true
   - True: (allow interaction / show marker)

Notes
- Use Quest Target + Minimap marker to guide players to the chest.
- Use container rewards to spawn items in the world.

Flow Diagram (ASCII)
[Boss Killed] -> [Set bossDead=true] -> [Show Message]
[Chest Available] -> [Check bossDead] -> [Allow Interaction]

## Common Mistakes
- Quest Name has spaces or duplicates (breaks deploy).
- Entity Script Name is missing or reused.
- Forgetting to enable Advanced Mode for advanced sections.
- Using "When Killed" when you need "When Killed By Hero".
- Not setting Objective Regions (breadcrumb guidance won't appear).

## Glossary
- Quest State: A saved variable tied to the quest.
- Trigger: A node that starts execution.
- Action: A node that performs a task.
- Condition: A node that checks a value and branches.
- Flow Node: A node that controls execution order.
