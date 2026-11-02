---
name: motion-quality-bar
description: "10-26 user — basic move/jump must reach ORIGINAL AnyPortrait hero quality; analyze frame-by-frame (0.04s) with numbers, not impressions"
metadata:
  node_type: memory
  type: feedback
  originSessionId: 8b62ae35-8290-4d53-8ae8-27ae6990900e
  modified: 2026-10-26T01:06:23.580Z
---

User (10-26): "다른 거 빼고 기본 이동·점프부터 완벽하게", reference = the original AnyPortrait hero's motion ("원본 수준"), and "동작 분석할 때 프레임 단위로 제대로 봐봐". Asked to test both ① cel animation and ② rig + hand-drawn parts and leave videos.

**Why:** Stepped pose-swap rig (±1px, poses held 0.08s) looked stiff vs original mesh-deform rig (continuous scissor legs 2↔9 art px, jump anticipation crouch, continuous in-air changes). My strip-glance reviews missed this.

**How to apply:** Before judging motion, capture same-condition 0.04s×30 strips for original and candidate (`bash Tools/compare_moves.sh <tag> [original]`), track numbers (`Tools/pixelfx/track_feet.py`), view frame sheets, and send side-by-side MP4 (`Tools/pixelfx/make_compare_video.py`). Details: PixelRigDesign §3.6. Related: [[hero-polish-loop]], [[test-daytime]].

10-26 decision: user picked ① cel ("셀이 압도적으로 예쁘다") for walk/run/jump — PixelLab PixMiniMax drafts (Docs/ArtCompare/Motion/cel), then loop-close, palette cleanup, per-frame hand socket, play via PixelRig.
10-26 equipment decision: DNF-style per-slot layers (B plan) — transfer_outfit per equipment over all cel frames (~35 gen each), extract overlay + erase mask (Tools/pixelfx/extract_layer.py), composite at runtime on equip change. Spec: CelEquipmentPlan 「확정」.
10-26 저녁 사용자 판정: 맨손 걷기 셀 **합격**. 불만 = 무기 든 이동이 부자연스러움, 점프가 다리 모은 자세(캐릭터와 안 맞음), 대기가 다리 꼰 자세·투구 흔들림 → 똑바로 선 대기·다리 벌린 점프로 교체 진행.
