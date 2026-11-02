---
name: no-focus-steal
description: Never bring the Unity editor (or any window) to the foreground while the user works
metadata:
  type: feedback
---

Don't focus/raise the Unity editor window (no Tools/focus_unity.ps1, no menu items that pop dialogs, no play mode that grabs focus). Drive everything headlessly via unity-mcp (refresh_unity for compile, execute_code to close/avoid dialogs, AssetDatabase.ImportPackage(path,false)).

**Why:** User uses the PC at the same time (gaming/other work); focus steals block their clicks.
**How to apply:** Keep `Tools/focus_guard.ps1` running hidden (mutex; it pushes Unity back if it grabs foreground without a user click/Alt/Tab; log Logs/focus_guard.log). Never cd the shell into a folder Unity will move (causes 'Moving file failed' modal). Complained again 2026-10-18 even after Play Unfocused. Game view set to "Play Unfocused"; if something needs a dialog, find a silent API instead. Related: [[usage-budget]]
