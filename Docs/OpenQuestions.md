# 열린 질문 (Open Questions)

질문이 생기면 여기에 쌓아두고, 답이 없어도 되는 작업부터 계속 진행합니다.
각 항목은 **[임시 가정]** 을 두고 진행하며, 답변을 받으면 문서/코드에 반영합니다.

| 상태 | 표기 |
|---|---|
| 답변 대기 | ⏳ |
| 답변 완료(반영 필요) | 💬 |
| 반영 완료 | ✅ |

---

## 환경 / 도구

### Q-001 💬 Unity 에디터 버전
- 프로젝트는 **2021.3.21f1** 인데, 이 PC의 Unity Hub에는 **6000.4.0f1** 만 설치되어 있음.
- Unity 6으로 열면 프로젝트가 업그레이드되며(URP, Input System, AnyPortrait, Behavior Designer 등 호환 이슈 가능) 되돌리기 어렵다.
- **[임시 가정]** 에디터를 열지 않고 코드/데이터/문서만 수정한다. 컴파일 검증은 불가하므로 변경은 보수적으로 한다.
- **답변: Unity 6로 업그레이드.** 6000.4.0f1로 진행 중.

### Q-002 💬 에셋스토어 / ChatGPT 접근
- 내장 브라우저에서 Unity 에셋스토어가 **로그인 페이지**로 이동함 (로그인은 직접 해주셔야 함).
- 로컬 캐시(`%APPDATA%/Unity/Asset Store-5.x`)에서 확인된 보유 패키지: DOTween Pro, Easy Save 3, Feel, AnyPortrait, All In 1 Sprite Shader, Odin Inspector.
- **[임시 가정]** 로그인 전까지는 위 캐시된 패키지만 후보로 검토한다.
- **답변: Chrome 사용 OK** (웨일 브라우저에 Claude 확장 설치됨).

### Q-003 ✅ git push 인증
- 셸의 git(Git Credential Manager)에 GitHub 자격 증명이 없어서 `git push`가 실패함. Fork는 자체 계정을 사용하는 것으로 보임.
- **[임시 가정]** 커밋은 로컬에 계속 쌓고, 인증이 해결되면 한꺼번에 push한다.
- 사용자 답: Fork로 push하면 됨 → Claude는 Fork를 직접 조작할 수 없으므로 **커밋까지만** 하고 push는 Fork에서.
- 완전 자동 push를 원하면: 터미널에서 `git push` 한 번 실행해 GCM 로그인 → 이후 Claude가 직접 push 가능.

### Q-004 ⏳ Claude 룰스(CLAUDE.md / 리뷰어 규칙)
- 사용자가 별도 git/리뷰 규칙을 가져다줄 예정.
- **[임시 가정]** 그 전까지는 main에 기능 단위 커밋, 기존 메시지 스타일 `(수정)/(추가) ...` 유지.

### Q-005 ✅ GPT 이미지 사용량 70% 측정 방법
- ChatGPT Plus는 이미지 생성 한도를 %로 보여주지 않음.
- **답변: 상한 없이 사용.** 생성 기록만 `Docs/ArtLog.md`에 남김.

### Q-006 ⏳ 자정 이후 Claude 10% 예산 집행 방식
- 주간 리셋: 매주 일 21:00 (KST). 이후 평일 예산 = 주간의 10%.
- **[임시 가정]** 평일 하루 약 2%씩. 각 작업 시작 시 사용량을 확인하고 일일 몫을 넘으면 중단.

### Q-007 ⏳ 저장소 private 전환
- ProjectB 저장소가 **public**. 매드덕스(회사 프로젝트)에서 가져온 코드/문서가 공개될 수 있음 → **private로 전환하기로 결정**.
- 웨일의 GitHub는 다른 계정으로 로그인되어 있어 저장소 설정 페이지가 404로 보임 → 직접 전환 필요:
  GitHub → coolblue1853/ProjectB → Settings → 맨 아래 Danger Zone → Change visibility → Private.
- **[임시 가정]** 전환 확인 전까지 매드덕스 유래 독점 코드·문서는 **로컬 커밋만** 하고 push 보류. 오픈소스(unity-mcp 등)는 push.

### Q-008 💬 unity-mcp 도입
- 답변: 회사에서 확인할 수 없으면 넣어도 됨 → 오픈소스, 로컬 전용, `claude mcp add --scope local`로 등록(저장소에 흔적 없음), 텔레메트리 끔.

### Q-009 ✅ 자동 실행 권한 모드 → 루틴 불필요(사용자). 예약 작업은 일시정지, 세션에서 상시 연속 진행
- 예약 작업 `projectb-auto-improve`(평일 10·14·18·22시)는 기본 권한 모드로 실행되어 도구 승인을 물어볼 수 있음.
- 요청: 앱의 Scheduled에서 이 작업을 열어 권한 모드를 Auto로 바꾸거나, "Run now"를 한 번 눌러 필요한 도구를 미리 승인.

### Q-201 ⏳ 주말 자동 실행
- 평일 10% 예산만 지정됨. **[임시 가정]** 주말에는 자동 실행하지 않음.

### Q-010 ✅ 에셋스토어 패키지 다운로드 → 사용자 에디터 로그인 후 내부 API로 18종 자동 다운로드
- Unity 에디터가 로그인되어 있지 않아 Asset Store 패키지를 받을 수 없음. 현재 로컬 캐시에 있는 건 6개뿐(그중 All In 1 Sprite Shader만 새로 도입).
- 요청: Unity 에디터 우상단 **Sign in** → Window > Package Manager > My Assets에서 아래를 **Download만**(Import는 Claude가 대화상자 없이 처리):
  - 몬스터/보스(도트): Monsters_Creatures_Fantasy, Evil Wizard 2, Bringer Of Death (free), Medieval King Pack 2, Martial Hero, 2D Pixel - RPG Monster Pack
  - 맵(도트): Magic Cliffs Pixel Art Environment, Underwater Fantasy
  - 이펙트: Magic Slashes FX, Particle Pack
  - UI/폰트: Free Pixel Font - Thaleah, Text Animator for Unity
  - 사운드: 8-Bit Sfx, Free Frost Mage SFX, SoundBits Free Sound FX, Fantasy Music Suite 2, Free Fantasy Adventure Music Pack
  - 기타: Quest Machine
- **[임시 가정]** 받기 전까지는 기존 에셋 + 코드 개선 위주로 진행.

## 기획 (밸런스 / 시스템)

### Q-104 ⏳ Lv1 사냥터 = SideRoad로 교체
- SideRoad 맵은 NearForest 전체 + 왼쪽 확장 구역(보스 포함)이라 확장판으로 판단. 마을 포털을 SideRoad로 연결하고 NearForest는 비활성.
- **[임시 가정]** 의도된 최신 맵은 SideRoad. 아니라면 NearForest로 되돌리기 쉬움(마을 포털 openMap만 교체).
- 참고: 프로브에서 timeScale 조작·맵 강제 활성화 등을 할 때 'PlayerLoop called recursively' 에러가 1회 관찰됨(도구 컨텍스트에서 수십 개 스포너 동시 Init). 실제 포털 이동에서 재발하면 PoolCheck 활성화를 프레임 분산할 것.

### Q-105 ⏳ 플레이어 조작 세부 (PlayerStateDesign 5절)
- Q-A 피격이 공격·스킬을 끊는가? → **[임시 가정]** 끊는다(슈퍼아머면 안 끊음).
- Q-B 발판에서 떨어진 뒤 2단 점프? → **[임시 가정]** 코요테 0.08s 후에는 공중 점프 1회만(메이플식).
- Q-C 같은 버프 재사용: 중첩/갱신? → **[임시 가정]** 현행대로 중첩.
- Q-D 홀드 중 다른 스킬 키 → **[임시 가정]** 무시.

### Q-106 ⏳ 타격감 세부 (GameFeelPlan의 열린 질문 참고)
- **[임시 가정]** 히트스톱은 전역 timeScale이 아닌 국소 정지(적·공격자 애니 속도)로, 보스 처치만 전역 슬로우. Feel 패키지는 1단계에서 도입하지 않음(ProCamera2D 흔들림과 중복).

### Q-107 ⏳ Lv2 '마법 절벽' 콘텐츠 (Lv2ContentPlan 7절)
- Q-L1 캐릭터 PPU 24 vs 타일 PPU 16 밀도 차이 → **[임시 가정]** 허용
- Q-L2 신규 세트 단계 → **[임시 가정]** 2/4/6(6단계는 무기 포함)
- Q-L3 CliffArmor 아트 → **[임시 가정]** 기존 텍스처 색조 변환 임시본
- Q-L4 Lv2 입구 → **[임시 가정]** SideRoad 서쪽 끝
- Q-L5 피격·사망 애니 없는 Fox 포함 → **[임시 가정]** 포함(셰이더로 대체)
- Q-L6 Lv1 무기 5~10뎀 확정 여부 → **[임시 가정]** 확정

### Q-101 💬 정수 나눗셈으로 무력화된 % 스탯
- 공속/이속/받피증/치명피해/증뎀/스킬뎀/방무/최종뎀이 `int/100` 이라 100% 미만 값은 전부 0으로 처리됨 → 사실상 효과 없음.
- 고치면 **체감 밸런스가 크게 바뀜** (치명타가 처음으로 1.2배 이상이 됨, 방무 50%가 '방어 100% 무시'에서 정상으로 바뀜 등).
- **답변: 버그는 알아서 수정.** 수정 후 스펙시트 기준값으로 재조정.

### Q-102 ⏳ 세트 구성
- `RustedSwordandShield`(무기)의 tfName이 `RabbitArmor` → Rabbit 세트가 6부위(5셋 효과는 방어구 5개만으로도 발동). 의도인지?
- `RatArmor`, `ScareArmor` 는 세트 효과 정의가 없음.
- 반지/목걸이 슬롯이 1개씩. 던파/디아식 세트라면 장신구 세트도 고려할지?
- **[임시 가정]** 무기는 세트에서 제외하지 않고 그대로 둔다. Rat/Scare 세트 효과는 스펙시트에 **제안안**으로만 작성(데이터 미반영).

### Q-103 ⏳ 게임의 큰 방향
- 메이플식 사냥터 반복 + 던파/디아식 세트 파밍이 핵심 루프로 이해함. 생존 요소(포만감, 낮/밤, 횃불)는 계속 유지할지?
- 레벨/경험치 시스템은 없음(`level` 세이브 필드만 존재). 메이플식 레벨업을 넣을지, 장비로만 성장할지?
- **[임시 가정]** 성장 = 장비 + 세트 효과. 레벨은 미도입. 생존 요소는 유지.
