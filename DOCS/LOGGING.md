# Logging and Error Reporting Plan

Last updated: 2026-02-04

This document outlines a pragmatic logging plan for Fable Quest Tool (FQT).
The goal is to improve supportability without adding heavy dependencies.

---

## Goals

- Capture actionable errors for deployment and code generation.
- Provide a known log location for user support.
- Keep logs lightweight and opt-in for verbose output.

---

## Proposed log destinations

- **Default log file**: `%LOCALAPPDATA%/FQT/logs/fqt.log`
- **Session log**: `%LOCALAPPDATA%/FQT/logs/fqt-YYYYMMDD-HHMMSS.log`
- **User-facing error dialog**: include a one-line error code and log path

---

## What to log (minimum viable)

- App startup/version and environment info
- Project load/save exceptions
- Code generation exceptions
- Deployment pipeline steps and failures (file writes, registration)
- FSE detection failures

---

## Logging levels

- `Info` — normal operations
- `Warn` — non-fatal issues, recoverable fallbacks
- `Error` — operation failed, user needs to act
- `Debug` — verbose info behind a setting

---

## Implementation sketch

- Introduce a small `ILog` interface in `FableQuestTool/Core/`.
- Provide a file logger implementation with rolling session logs.
- Wrap top-level commands (save/deploy/launch) with try/catch + log.
- Add a user setting for “Enable verbose logging”.

---

## Follow-up tasks

- Add log file viewer UI in the Tools menu.
- Add a “Copy error details” button in dialogs.
