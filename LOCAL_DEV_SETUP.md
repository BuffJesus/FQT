# Local Development Setup with FSE

This document describes how to set up FSE files locally for development and testing. These files are **NOT** included in the public repository per the FSE author's request.

## Setup Instructions

### 1. Download FSE

Download the latest FSE release from the official repository:
https://github.com/eeeeeAeoN/FableScriptExtender

### 2. Set Up FSE_Binaries (Optional - for local testing)

If you want to test the old auto-install behavior locally:

1. Create the directory: `FableQuestTool/FSE_Binaries/`
2. Copy the following files into it:
   - `FSE_Launcher.exe`
   - `FableScriptExtender.dll`

**Note:** These files are gitignored and will NOT be committed to the repository.

### 3. Set Up FSE_Source (Optional - for reference)

If you want to keep the FSE source code for reference:

1. Clone or download FSE source from: https://github.com/eeeeeAeoN/FableScriptExtender
2. Place it in the `FSE_Source/` directory at the project root

**Note:** This directory is gitignored and will NOT be committed to the repository.

### 4. Include FSE_Binaries in Build (Optional)

If you want the build to include FSE_Binaries for local testing, uncomment these lines in `FableQuestTool/FableQuestTool.csproj`:

```xml
<ItemGroup>
    <None Include="FSE_Binaries\**\*">
        <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
</ItemGroup>
```

**IMPORTANT:** Do NOT commit these changes to the public repository.

## Why This Setup?

- **Respects FSE Author's Request:** FSE is not bundled with the public FQT distribution
- **Allows Local Development:** You can still test and develop with FSE locally
- **Prevents Accidental Commits:** `.gitignore` ensures FSE files stay local
- **Public Users Get Latest:** Users are directed to download FSE from the official source

## Git Safety

The following entries in `.gitignore` prevent FSE files from being committed:

```
# FSE files for local development/testing (not included in public repo)
FSE_Source/
FableQuestTool/FSE_Binaries/
```

If you accidentally stage these files, git will ignore them.
