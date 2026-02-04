# Release Guidance

Last updated: 2026-02-04

This document describes a basic release workflow for Fable Quest Tool (FQT).

---

## Versioning

- Use semantic versioning: `MAJOR.MINOR.PATCH`.
- Increment:
  - **MAJOR** for breaking changes to project format or API.
  - **MINOR** for new features.
  - **PATCH** for fixes and small improvements.

---

## Release checklist

1. Update version number (where applicable) and changelog notes.
2. Build the standalone release:

```bash
dotnet publish FableQuestTool/FableQuestTool.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

3. Smoke test the build:
   - Launch FQT
   - Create a new project
   - Generate and preview Lua
   - Deploy (if you have a local Fable install)

4. Package the publish output as a ZIP.
5. Publish the release and attach the ZIP.

---

## Notes

- Ensure `LICENSE` and `THIRD_PARTY_NOTICES.md` are up to date.
- If you change the project file format, update `DOCS/PROJECT_FORMAT.md` and note
  the change in the release notes.
