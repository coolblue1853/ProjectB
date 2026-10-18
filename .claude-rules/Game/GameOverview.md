# 게임 개요 (Project B)

> 코드 기준으로 역산한 스펙. 확정되지 않은 기획 판단은 `Docs/OpenQuestions.md`의 질문 번호(Q-xxx)를 붙인다.

## 1. 한 줄 요약

메이플스토리식 **횡스크롤 2D 액션 전투** 위에, 던전앤파이터·디아블로식 **장비 세트 효과 파밍**을 얹은 생존형 액션 RPG.

## 2. 핵심 루프

```
마을(상점/대장간/창고) ─포털→ 사냥터(낮/밤 스포너)
   ▲                              │ 처치 → 재료/장비 드롭(확률)
   │                              ▼
   └── 제작(재료→장비) ← 인벤토리 ← 채집(돌/나무/약초)
          │
          ▼
   장비 장착 → 스탯 + 세트 효과(2/5셋) → 더 강한 사냥터·밤 몬스터·보스
```

- **성장 축:** 장비와 세트 효과만으로 성장한다. 레벨·경험치는 없다(Q-103).
- **생존 축:** 포만감이 HP·스태미나 자연 회복의 재료가 된다. 밤에는 횃불로 시야를 확보한다.
- **시간 축:** 하루는 20분이다. 밤(8분)에만 나오는 몬스터와 보스가 있다.

## 3. 콘텐츠 현황

| 영역 | 현재 | 비고 |
|---|---|---|
| 맵 | Town, Lv1_NearForest, Lv1_NearForest_SideRoad | SideRoad로 가는 포털이 끊겨 있음 |
| 몬스터 | 일반 8종 + 보스 1종 + 채집물 3종 | `Game/EnemySpec.md` |
| 아이템 | 유효 81개(장비 48개) | `Game/ItemSpec.md` |
| 세트 | 정의 2개(Rabbit, Leaf), 데이터만 있는 세트 2개(Rat, Scare) | Q-102 |
| 스킬 | 무기마다 4슬롯(A/S/D/F) | `Game/CombatSpec.md` |

## 4. 기술 스택

- Unity 6 (6000.4) + URP 2D. 2021.3.21f1에서 업그레이드했다.
- **입력:** New Input System. 키보드와 게임패드를 지원한다.
- **AI:** Behavior Designer + 커스텀 Task(`Assets/Script/BT/BD_`).
- **연출:** DOTween(시퀀스·콤보), ProCamera2D(카메라·흔들림), DamageNumbersPro, MasterAudio.
- **캐릭터:** AnyPortrait. 장비 장착 시 메시 텍스처를 교체한다.
- **데이터와 저장:** 데이터는 `Resources/*.csv`, 세이브는 Easy Save 3.

## 5. 문서 지도

| 문서 | 내용 |
|---|---|
| `Game/CombatSpec.md` | 이동, 입력, 스탯, 데미지 공식, 콤보·스킬, 상태이상 |
| `Game/ItemSpec.md` | CSV 스키마, 슬롯, 세트 규칙, 인벤토리, 제작, 상점, 세이브 |
| `Game/EnemySpec.md` | 몬스터 스탯, BT 구조, 스폰, 드롭 테이블 |
| `Game/WorldSpec.md` | 낮/밤, 맵·포털, 카메라 |
| `Game/TechDebt.md` | 알려진 버그와 기술 부채 백로그 |
