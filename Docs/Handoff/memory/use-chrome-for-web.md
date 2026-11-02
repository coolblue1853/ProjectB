---
name: use-chrome-for-web
description: "10-27 사용자 — 웹 검색·레퍼런스 찾기는 Claude in Chrome(사용자 크롬)으로, 내장 브라우저 말고"
metadata:
  node_type: memory
  type: feedback
  originSessionId: 8b62ae35-8290-4d53-8ae8-27ae6990900e
  modified: 2026-10-27T13:29:05.980Z
---

웹에서 레퍼런스를 찾거나 페이지를 볼 때는 Claude in Chrome(`mcp__claude-in-chrome__*`, 사용자의 실제 크롬)을 쓴다. 내장 브라우저(Claude_Browser)로 검색하지 않는다. 새 탭에서 작업하고, 소리 나는 영상은 틀지 않는다 [[no-youtube-autoplay]].

**Why:** 10-27 "크롬 클로드 쓰라고, 검색할 거면" — 내장 브라우저로 유튜브를 열다가 광고 소리가 났고, 사용자는 크롬을 원했다.
**How to apply:** 검색·이미지 수집은 크롬 탭에서. 이미지 저장은 crome 페이지에서 URL을 얻어 curl 로 `C:\Users\user\MotionRef\` 에 받는다.
