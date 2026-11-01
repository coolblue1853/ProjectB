# 임시 에셋 목록 (이펙트 필요)

## 이펙트 필요 (장비 1층계 첫 1시간 작업, 2026-10-31)

- 없음. 새 스킬을 만들지 않았고 LeafAdae·FangDagger·BearClaw·RootStaff·CrocJawAxe의 기존 스킬·이펙트를 그대로 쓴다.
- 새 아이템 아이콘 없음(전부 기존 ItemSheet 행과 `Resources/Item/` 아이콘 재사용).
## 마을 정비 (2026-11-01)

- 기록지기: 기존 마을 Shop NPC의 SpriteRenderer.sprite·머티리얼 재사용, 런타임 색조 (0.75, 0.8, 0.95). TownServices에서 연결한다. 새 PNG·모션·이펙트 없음. 정식 캐릭터 그림은 Claude 담당으로 교체 전까지 임시다.
- 기존 루엔·세나는 NpcQuestPlacer의 색조 재사용을 유지한다. 상점 기본 장비·소모품 아이콘도 기존 자산을 그대로 쓴다.

## 첫 1시간 테스트 임시 그림 (2026-11-01, 사용자 검토 전)

- **새 몹 3종** `Assets/Art/Temp/Mobs/Temp_{MossShellBeetle,BranchAntlerFox,SporeBackToad}.png`: ChatGPT 이미지 1장(사용자 다운로드) → `Logs/temp_mobs/process.py`(마젠타 제거·3분할·높이 34/52/40px·20색). 단일 정지 이미지, 왼쪽을 본다. 움직임은 코드(숨쉬기·기울기·피격 번쩍임). 사용자 첫 반응: "고쳐야 할 부분이 있어 보이긴 한데 잘 나왔다".
- **무기 스킬 임시 FX 27장** `Assets/Art/Temp/FX/<스킬ID>_f4.png`: `Tools/pixelfx/gen_temp_skill_fx.py` 절차 생성(GPT 다운로드 막힘). 64×48 또는 48×48, 4프레임, 색 3개(TempFxSlots.md 주문표). 파일을 지우면 기존 FX로 돌아간다. 미리보기 `Logs/tempfx/preview.png`.
- 둘 다 테스트 뒤 사용자 판정을 받아 정식 교체 여부를 정한다.
