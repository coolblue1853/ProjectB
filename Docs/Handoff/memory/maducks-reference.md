---
name: maducks-reference
description: "MadDucks project is ProjectB's read-only \"parent\" reference — never modify it"
metadata:
  node_type: memory
  type: project
  originSessionId: 7f303872-6cc7-4749-abaf-5659414b9850
  modified: 2026-10-18T00:41:51.548Z
---

`C:\Users\user\Downloads\PurpleCow_MadDucks-main\PurpleCow_MadDucks-main` (Unity 6000.3, CookApps company project, zip download) is the "parent" project. Pull any useful features, rules (.claude/CLAUDE.md, .claude-rules/, .claude/agents/) and packages into ProjectB.

No company servers (spec-data server, FarmStack, auth, analytics) — ProjectB is personal; anything server-backed in MadDucks must be re-done local (CSV/ES3 files).

NEVER modify, create files in, or commit anything inside the MadDucks folder. Read-only.

Company code must not leak: ProjectB GitHub repo (coolblue1853/ProjectB) was public on 2026-10-18; user chose to make it private. Until confirmed private, don't push MadDucks-derived proprietary code/docs (open-source packages like unity-mcp are fine).

Other decisions 2026-10-18: upgrade ProjectB to Unity 6 (6000.4.0f1 installed); Chrome/Whale browser OK to use (Whale is "웨일"); GPT image generation has no cap now; fix bugs autonomously and verify in the real editor; after questions run, keep working automatically without waiting. Related: [[usage-budget]] [[git-autocommit]]

Scope (2026-10-18): not just feature cleanup/asset hookup — also optimization and refactoring of messy legacy code; may research and apply modern algorithms/design patterns from the web.
