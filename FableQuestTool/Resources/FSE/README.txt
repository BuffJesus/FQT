FSE (Fable Script Extender) Framework Files
============================================

This folder contains the FSE framework template files used by FableQuestTool.

REQUIRED: FableScriptExtender.dll
---------------------------------
To use FSE with Fable: The Lost Chapters, you need the FSE DLL.

Option 1: Build from source
  1. Open FSE_Source/FableScriptExtender.sln in Visual Studio
  2. Build in Release|Win32 configuration
  3. Copy FableScriptExtender.dll to your Fable installation directory

Option 2: Obtain pre-built binary
  Download FableScriptExtender.dll from the FSE releases and place it in
  your Fable installation directory (same folder as Fable.exe).

LAUNCHING THE GAME
------------------
FableQuestTool includes a built-in launcher that:
  1. Starts Fable.exe
  2. Injects FableScriptExtender.dll automatically
  3. Enables your custom Lua quest scripts

Simply click "Launch FSE" in the tool after deploying your quest.

Folder Structure
----------------
When deployed, FSE creates this structure in your Fable installation:

[Fable Installation]/
  FableScriptExtender.dll  - The FSE DLL (you provide this)
  FSE/
    quests.lua             - Quest registry (auto-managed by tool)
    Master/
      FSE_Master.lua       - Master control script
    [YourQuest]/           - Your quest folders (created by deployment)
      [YourQuest].lua
      Entities/
        [Entity].lua

COMPATIBILITY
-------------
FSE is compatible with Fable: The Lost Chapters (Steam version only).
