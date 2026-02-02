# Fable Quest Tool (FQT)

A visual quest editor for Fable: The Lost Chapters that generates Lua scripts using the Fable Script Extender (FSE).

> ⚠️ **Early Development Warning**
> This tool is in the very early stages of development. You may encounter bugs, missing features, and breaking changes. Use at your own risk and always back up your work.

## Overview

FQT is a WPF-based visual editor that allows you to create quests for Fable: The Lost Chapters using a node-based graph interface. The tool generates FSE-compatible Lua scripts that can be deployed directly into the game.

### Features

- **Visual Node Graph Editor** - Design quest logic using an intuitive node-based interface
- **Entity Management** - Browse, create, and edit game entities
- **Template System** - Reusable quest patterns and entity templates
- **Code Generation** - Automatically generates FSE-compatible Lua scripts
- **Quest Manager** - Deploy and manage quests in your game installation
- **API Reference** - Built-in documentation for FSE functions
- **Sample Quests** - Includes working examples demonstrating various quest patterns

## Requirements

- Windows OS (uses WPF)
- Fable: The Lost Chapters (PC version)
- [Fable Script Extender (FSE)](https://github.com/eeeeeAeoN/FableScriptExtender) by eeeeeAeoN

## Installation

### For Users (Prebuilt Release)

1. Download the latest release from the [Releases](https://github.com/BuffJesus/FQT/releases/tag/fable) page
2. Extract the ZIP file to your preferred location
3. Run `FableQuestTool.exe`

No additional dependencies or .NET runtime installation required - everything is included in the standalone executable.

### For Developers (Build from Source)

**Requirements:**
- .NET 8.0 SDK or higher
- Visual Studio 2022 or JetBrains Rider

**Steps:**
1. Clone this repository
2. Open `FQT.sln` in your IDE
3. Build the solution in Debug or Release configuration
4. Run from the IDE or execute `FableQuestTool.exe` from the build output directory

**Creating a Release Build:**

To create a standalone release that includes all dependencies:

```bash
dotnet publish FableQuestTool/FableQuestTool.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

The output will be in `FableQuestTool/bin/Release/net8.0-windows/win-x64/publish/`

## Usage

### Creating a Quest

1. **New Project** - Go to File → New to create a new quest project
2. **Design Quest Logic** - Use the node graph editor to design your quest flow:
   - Add nodes for events, dialogues, spawning entities, etc.
   - Connect nodes to define quest progression
   - Configure node properties in the inspector
3. **Create Entities** - Define NPCs, items, and other entities:
   - Use Tools → Browse Entities to view available entities
   - Create custom entity behaviors using entity templates
4. **Export and Deploy** - File → Export and Deploy to generate Lua files and deploy to FSE

### Managing Quests

- **Tools → Manage Deployed Quests** - View and manage quests in your FSE installation
- **Tools → Launch FSE** - Launch the game with FSE enabled

### Sample Quests

The `SampleQuests/` folder contains working examples:
- **NewQuest** - Complete example demonstrating best practices
- **MyFirstQuest** - Basic quest with NPCs and dialogue
- **MySecondQuest** - Bulletin board quest with item tracking
- **GhostGrannyNecklaceLUA** - Complex multi-NPC quest
- **WaspBossLUA** - Boss fight mechanics
- **DemonDoorLUA** - Conditional access quest

See `SampleQuests/README.md` for detailed explanations of each example.

## Project Structure

```
FQT/
├── FableQuestTool/       # Main WPF application
│   ├── Data/             # Node definitions and data models
│   ├── Services/         # Code generation and business logic
│   ├── Views/            # UI views and controls
│   ├── ViewModels/       # MVVM view models
│   └── Resources/        # Styles and themes
├── FSE_Source/           # FSE C++ source code reference
└── SampleQuests/         # Example quests
```

## Known Issues

As this tool is in early development, expect:
- Potential crashes or unexpected behavior
- Incomplete or missing features
- Limited error handling
- Breaking changes in future updates
- Incomplete documentation

Always save your work frequently and keep backups of your quest files.

## Credits

- **Fable Script Extender (FSE)** - Created by [eeeeeAeoN](https://github.com/eeeeeAeoN/FableScriptExtender)
- **FQT** - Quest editor tool built on top of FSE

## Dependencies

- [Nodify](https://github.com/miroiu/nodify) - Node-based UI library
- [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) - MVVM toolkit
- WPF & Windows Forms (.NET 8.0)

## Contributing

This project is in active development. If you encounter bugs or have suggestions, please open an issue on the repository.

## License

See LICENSE file for details. Note that FSE has its own license - refer to the [FSE repository](https://github.com/eeeeeAeoN/FableScriptExtender) for more information.
