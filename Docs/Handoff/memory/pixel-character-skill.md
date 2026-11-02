---
name: pixel-character-skill
description: "10-25 character pixel art = skill pixel-character + CharacterPixelRules §0 확정 레시피: 원본 앵커를 edit_image_pro_flash로 편집해 컨셉 교체(같은 자세·비율), 세대별 4컨셉·사용자 판정"
metadata:
  node_type: memory
  type: feedback
  originSessionId: 8b62ae35-8290-4d53-8ae8-27ae6990900e
  modified: 2026-10-25T07:30:30.881Z
---

For any character (hero/NPC) sprite, follow `.claude-rules/Game/CharacterPixelRules.md` **§0 확정 레시피** (run via skill `.claude/skills/pixel-character/`):
- `edit_image_pro_flash` with the anchor `Docs/ArtCompare/hero_style_anchor_64.png` as the start image: "Redraw this same character as <concept> … Expression: … No fox ears, no fox tail. Keep EXACTLY the same pose, facing direction, body proportions, head size, size and position…". Weapons go on the back or hip.
- Then `flatten_keep_detail.py --merge 14 --speck 28 --head-auto`, then `cap_colors.py --colors 20`, then a compare sheet against the original.
- Each generation: 4 NEW cool RPG-set archetypes (web search ok), per-character expressions, 20 gens, user approval and good/bad, log in `Docs/ArtCompare/CharacterGen/gen_log.md`.

**Why:** the user approved gen9 ("거의 완벽, 딱 이런 형태로 다양하게") and gen10 ("4개 다 합격, 원하는 형태로 잘 따라온다"). Rejected approaches:
- text-only reproduction ("여우라는 점만 닮았다");
- a fixed base body plus inpainting ("체형 고정은 오히려 안 좋았다");
- AI-edited base body;
- fresh create plus style_image, because pose and proportions vary and the result is hard to compare.

The user wants same direction, same pose and the original's proportions so comparison is easy. Judge results yourself and never ask the user to pick a variant.

**How to apply:** start from §0; turn every user feedback into a rule there. Related: [[game-pillars]], [[abyss-mood-not-childish]], [[art-ref-criteria]].
