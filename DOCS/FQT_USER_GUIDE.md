FABLE QUEST TOOL (FQT) USER GUIDE

Overview
FQT is a visual quest authoring tool for Fable with a node-based entity editor,
a structured Quest Setup workflow, and direct export/deploy into the game.
This guide covers the common workflow and the most important features.

Quick Start
1) File -> New to create a project.
2) Quest Setup -> Basic Info to name the quest and set an ID.
3) Quest Setup -> Quest Card to configure objective text and regions.
4) Entities tab -> add entities and build behavior with nodes.
5) File -> Export and Deploy to send the quest to your Fable install.

Keyboard Shortcuts
- Ctrl+N: New project
- Ctrl+O: Open project
- Ctrl+S: Save
- Ctrl+Shift+S: Save As
- Ctrl+E: Export and Deploy
- Ctrl+Shift+V: Validate Project
- Ctrl+P: Preview Generated Lua

Quest Setup Tab
The left list controls which section is visible. Use Advanced Mode for
Optional Challenges and Background Tasks.

Basic Info
- Quest Name: internal script name (no spaces, unique).
- Display Name: player-facing name.
- Quest ID: numeric ID (recommend 50000+ for custom quests).
- Description: internal description (not shown to players).
- Quest Enabled: unchecked means it will not deploy.

Quest Card
- Quest Card Object: which quest card item is granted.
- Objective Text: text shown in quest log.
- Objective Regions: used for breadcrumb guidance.
- World Map Offset: leave at 0,0 to auto-calc, or set manually.
- Start/End Screen: enable cinematic quest intro/outro.
- Gold/Story/Guild flags: affect display and quest log ordering.

Regions
Add regions referenced by your quest. These help with map guidance and data
lookups in other sections.

Rewards
- Gold/Experience/Renown/Morality: numeric rewards.
- Direct Reward Items: grant items directly to hero on completion.
- Container Rewards: spawn a container with items. Use Spawn Location to
  place near marker/entity or a fixed position.
- Reward Abilities: grant hero abilities on completion.

Optional Challenges (Advanced Mode)
Boasts are optional objectives that grant extra rewards.

States
State variables are persisted per quest and are commonly used for logic gates.

Background Tasks (Advanced Mode)
Threads run functions periodically until an exit condition is met.

Entities Tab (Node Editor)
This is the core of quest logic and behavior.

Entity Settings
- Script Name: unique identifier used in Lua.
- Definition Name: game definition to bind or spawn.
- Entity Type / Spawn Method / Region / Marker: define creation or binding.
- Quest Target Options: highlight or show marker on minimap.

Add Entity
Use the "+ Add Entity" button to create a new entity tab.

Node Menu
Right-click on the graph (or use the Add Node menu) to insert nodes.
Use the search box to filter nodes. Favorites and Recents are pinned at top.
Hover over a node in the menu to see what it does.

Graph Tips
- Drag from an output pin to create a connection.
- Double-click a wire to add a reroute node.
- Ctrl+drag on a wire to insert a reroute inline.
- Use Align/Distribute when multiple nodes are selected.

Variables
Use the Variables panel to create per-entity variables.
Drag variables into properties to bind them, or drop on the graph to create
Get/Set nodes.

Properties Panel
Selecting a node shows its properties on the right. Use "Bind" to tie a property
to a variable instead of a literal value.

Templates Tab
Templates provide ready-made quest structures. Choose a template, review the
details, then click "Use This Template" to start a new quest.

API Reference Tab
Search and browse FSE functions. Each entry includes a description, return
type, parameters, and example usage. Use this to validate node behavior and
write custom Lua snippets.

Export and Deploy
Use File -> Export and Deploy (or the Deploy button) to push your quest into
Fable. Validation runs before deploy. Fix errors and try again if needed.

Preview Generated Lua
Tools -> Preview Generated Lua shows the final scripts for the quest and each
entity. Use this for debugging or advanced customization.

Validation
Tools -> Validate Project checks naming, required fields, and conflicts.
Fix errors before deploying.

Troubleshooting
- If deploy fails, confirm your Fable path in Tools -> Configure Fable Path.
- If nodes do not behave, check entity script names and node properties.
- Use the API Reference and Lua Preview to verify generated output.
- If advanced sections are missing, enable Advanced Mode in the toolbar.

Tips and Best Practices
- Keep quest names and entity script names unique and consistent.
- Use states for branching and progression logic.
- Use templates to scaffold large quests quickly.
- Save often and use Save As for experimental branches.
