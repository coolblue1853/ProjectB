---
name: limbus-fx-reference
description: "10-28 사용자 — 이펙트 기준은 스컬이 아니라 림버스 컴퍼니; 형태는 도트, 디테일은 셰이더·파티클 VFX로 림버스 퀄리티 목표"
metadata:
  node_type: memory
  type: feedback
  originSessionId: 8b62ae35-8290-4d53-8ae8-27ae6990900e
  modified: 2026-10-28T12:41:10.108Z
---

이펙트(베기·타격 등)의 품질 목표는 **림버스 컴퍼니**다(사용자 PC에 설치됨, Unity 6000.3.12f1 어드레서블 캐시 `AppData\LocalLow\Unity\ProjectMoon_LimbusCompany`). 형태(실루엣)는 도트로 잡고, 디테일은 셰이더·파티클 VFX로 채운다. 림버스는 도트가 아니지만 형태와 색감이 우수하다. 동작 레퍼런스로도 림버스를 꺼내 쓴다.

10-28 결과: v7(타원 고리·바늘 꼬리·안쪽 톱니·바늘 찌르기)에 사용자 "진짜 한 100배는 나아진 것 같다" — 림버스 캡처·텍스처를 분석해 형태 문법을 옮기는 방식이 검증됐다. 다른 이펙트도 이 방식(캡처 → 문법 정리 → 절차 생성 도트 + 받침 VFX)으로.

**Why:** 10-28 "스컬보다는 림버스를 보고 형태는 도트로 디테일은 vfx로 … 림버스 이펙트 퀄리티를 내는 걸 목표로". 스컬 기준 이펙트는 "너무 밝기만 하다"는 평이었다.
**How to apply:** 추출물은 스컬과 같은 규칙으로 저장소 밖 `C:\Users\user\LimbusRef` 에만 두고 커밋 금지(참고 전용). 추출은 UnityPy(`FALLBACK_UNITY_VERSION = "6000.3.12f1"`) 또는 AssetStudioModCLI `--unity-version 6000.3.12f1`. 모션 쪽 스컬 기준([[skul-reference]])은 이펙트에 한해 이 규칙으로 바뀐다. [[stick-first-pipeline]]
