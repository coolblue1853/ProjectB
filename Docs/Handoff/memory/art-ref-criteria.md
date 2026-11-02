---
name: art-ref-criteria
description: 10-19 user wants Claude to pre-pick Pinterest refs by documented criteria (ArtStyle.md §0); user only marks good/bad
metadata:
  node_type: memory
  type: feedback
  originSessionId: 8b62ae35-8290-4d53-8ae8-27ae6990900e
  modified: 2026-10-19T11:53:48.303Z
---

User said "너의 판단기준을 올려두면 내가 매번 찾을 필요도 없을테니" — Claude should select art references itself using `.claude-rules/Game/ArtStyle.md` §0 criteria, present numbered contact sheets, and let the user mark only bad/good; update §0 after every round.

**Why:** user doesn't want to hunt refs each time; round-1 bads were cute/chibi sticker sheets, cartoon big-eye faces, whole-body lava/fire glow, realistic painted human — goods are moss/root/antler overgrown eerie creatures with 1–2 glowing points, and Maple-proportion dark chibis with big hats + gold trim.

**How to apply:** collect candidates ONLY as Pinterest related pins of the user's own original pins, via Claude in Chrome (Whale, logged in) using RelatedModulesResource — round 8 this way was "전반적으로 다 나쁘지 않네"; search-engine collection (rounds 2–7) kept getting worse. Show numbered sheets (Docs/ArtCompare/pin_round*), record verdicts in §0. Refs set subject/mood only — every PixelLab generation must follow ONE unified art spec (ArtStyle '하나의 아트워크로 통일': fixed density, selective outline, top-left light + hue shift, master palette ≤16 colors/mob, fixed pixflux params + style suffix, style-anchor comparison). Related: [[abyss-mood-not-childish]], [[monster-art-pipeline]], [[polish-existing-first]].

**Character rendering anchor (2026-10-25):** user pinned 2 pixel lineups (PixelTheWise girls, pins 584834701625061220 · 887068457842485519; files Docs/ArtCompare/hero_style_anchor_*) — "이런 느낌의 도트 그래픽으로 캐릭터가". Characters render like this: 56–64px tall, 3.5–4 heads, full 1px dark colored outline, 3–4 tone cel shading, saturated readable color blocks, big hair shapes, oversized bright weapon. Overrides the muted-palette rule for characters only (mobs/backgrounds stay muted).

**Character FORM final (2026-10-25, user "이런 느낌이 베스트"):** pins 632896553930144417 · 1143492161662892815 (files Docs/ArtCompare/hero_form_best_*). User means shape/deformation/pixel scale, not color: 2-head super-deformed chibi, in-game ~32–36px tall, black 1px outline, big eyes w/ highlight, stubby limbs, 2–3 tone flat shading. Overrides earlier 3.5–4 head anchors and the "no chibi / no black outline" bans for characters only; mood stays dark via costume/props.

**Character COLOR feel (2026-10-25, same two pins, "저 색이 아니라 저런 색감"):** 11–15 colors/char, one dominant hue + white/light gray + tiny complementary accent, saturation 0.5–0.7, bright values (~40% V>0.8), shade within hue (2–3 steps, darks shift red/purple), contrast from black outline not dark fills, no muddy browns/grays. Hero navy coat = saturated mid-value indigo, gold brooch = accent. Mobs/backgrounds stay muted so characters pop.
