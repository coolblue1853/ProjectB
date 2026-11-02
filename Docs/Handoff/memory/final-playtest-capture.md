---
name: final-playtest-capture
description: 11-01 사용자 — 최종 수정은 직접 플레이테스트 + 스샷/영상으로 이상한 부분까지 검토
metadata:
  type: feedback
---

최종 수정(커밋 전 마지막 확인)은 숫자 프로브만으로 끝내지 말고, 직접 플레이 모드에서 해당 장면을 스크린샷 또는 연속 캡처(영상·GIF·프레임 시트)로 찍어 눈으로 이상한 부분이 없는지까지 검토한다.

**Why:** 사용자가 11-01 "최종 수정엔 직접 플레이테스트로 해서 스샷 혹은 영상 찍어서 이상한 부분 없는지까지 검토해"라고 지시. 숫자는 맞아도 화면이 어색한 경우를 걸러야 한다.
**How to apply:** 프로브로 장면을 세팅한 뒤 DOVirtual.DelayedCall + ScreenCapture.CaptureScreenshot 으로 0.1초 간격 프레임을 Logs/playtest/ 에 찍고 시트·GIF로 묶어 본다. 낮 시간대로([[test-daytime]]). Codex 지시서에도 같은 기준을 넣는다.
