# Fable Quest Tool (FQT)

A visual quest editor for Fable: The Lost Chapters that generates Lua scripts using Fable Script Extender (FSE).

Early development warning: expect bugs, missing features, and breaking changes. Back up your work.

## Requirements

- Windows (WPF)
- Fable: The Lost Chapters (PC)
- Fable Script Extender (FSE): https://github.com/eeeeeAeoN/FableScriptExtender

## How To Use

1. Install FSE into your Fable folder.
2. Run `FableQuestTool.exe`.
3. File -> New, build your node graph and entities.
4. File -> Export and Deploy.
5. Tools -> Launch FSE.

## Install FSE (Required)

1. Download FSE from the official repository.
2. Extract into your Fable install folder.
3. Ensure these exist:
   - `FSE_Launcher.exe`
   - `FableScriptExtender.dll`
   - `Mods.ini` configured for FSE
   - `FSE` folder

## Build From Source

Requirements:
- .NET 8 SDK
- Visual Studio 2022 or JetBrains Rider

Steps:
1. Open `FQT.sln`.
2. Build Debug/Release.
3. Run `FableQuestTool.exe`.

Release publish:

```bash
dotnet publish FableQuestTool/FableQuestTool.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

Output:
`FableQuestTool/bin/Release/net8.0-windows/win-x64/publish/`

## Credits

- Fable Script Extender (FSE) by eeeeeAeoN
- Fable Quest Tool (FQT)

## License

See `LICENSE`. FSE has its own license in its repository.
