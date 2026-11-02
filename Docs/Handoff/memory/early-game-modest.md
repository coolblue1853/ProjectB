---
name: early-game-modest
description: "10-26 user — early-zone mobs, starter gear and effects must look small/modest, not mighty; only unique/legendary items get flashy power looks"
metadata:
  node_type: memory
  type: feedback
  originSessionId: 8b62ae35-8290-4d53-8ae8-27ae6990900e
  modified: 2026-10-26T07:28:44.428Z
---

User (10-26, during mob polish): "초반 몹들이라 너무 강대하거나 크게 보이면 안된다. 캐릭터가 장착하는 장비나 이펙트도 마찬가지. 물론 힘을 준 유니크·레전더리 아이템은 있겠지만."

**Why:** Progression readability — power should visibly grow with tier; early content looking fierce flattens the curve.

**How to apply:** When restyling early mobs (Forest Road / Side Road), keep them small, harmless-looking, recognizable (a rabbit stays a rabbit), only lightly eerie per [[abyss-mood-not-childish]]. Starter/common gear and basic-attack effects stay plain; reserve big silhouettes, glow, spikes for unique/legendary. Related: [[polish-existing-first]].

10-26 clarification: "몹 퀄리티업" = REDRAW mobs fully in the hero's art style (new pixel art + new cel anims like the hero), not recolor. Recolor (mob_recolor.py) was a misread. Plan: per mob edit original frame with hero-style reference (edit_image_pro_flash method=reference) → PixMiniMax idle/walk/attack/hit → write into existing frame PNG names (8x upscale, same canvas) so animators keep working.
