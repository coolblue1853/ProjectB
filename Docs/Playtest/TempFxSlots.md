# 첫 1시간 임시 무기 스킬 FX 슬롯

2026-11-01 조사. 그림은 Claude가 GPT로 별도 제작한다. 아래 크기·색은 제작 주문용 임시 제안이며 시각 품질 판정은 미검증이다.

## 대응·동작 규칙

- 스킬 ID = 실제 `Skill.skillName`의 공백만 `_`로 치환한다. 오타·대소문자·작은따옴표를 유지한다(`Slash_Attck`, `Hunter's_Stride`).
- `Assets/Art/Temp/FX/<ID>.png` 또는 `<ID>_f<N>.png` 하나만 둔다. N=1~64, 등폭 가로 시트, 왼쪽→오른쪽. 투명 배경, 프레임별 중심 고정, 여백 포함 같은 크기. 중복 ID·잘못된 폭은 기존 FX로 복귀한다.
- Point/무압축/밉맵 없음, 36 PPU. 단일 이미지는 0.2초, 시트는 20fps(N/20초). 오른쪽을 보는 원본이며 왼쪽 시전은 좌우 반전. 그림 크기는 원본 px 그대로이며 기존 castVfxScale을 적용하지 않는다.
- 기존 `Skill.SkillEffect`의 첫 피벗·바닥 보정 위치에서 시전 시 1회 재생한다. 홀드도 시작 1회다. PNG가 있으면 castVfx만 대체하고, 없으면 기존 castVfx를 그대로 재생한다. 기존 지연 FX·투사체·장판·소환수·피해 판정은 유지한다. 즉 기존 판정 스프라이트까지 전부 교체하는 기능은 아니다.
- 임포터가 Resources/FX/TempSkillFxLibrary.asset에 텍스처 참조를 기록하므로 플레이어 빌드에도 포함된다. 에디터 정지 상태에서 PNG 추가·교체·삭제 후 refresh한다. 플레이 중 교체는 지원하지 않는다.
- 기본 3타·스미어 v7·타격 v7·모션 데이터는 수정하지 않는다. 근거: `Assets/Script/Weapon/Skill.cs:418`, `Assets/Script/Combat/Vfx/Vfx.cs:111`, `Assets/Script/Combat/Vfx/TempSkillFxLibrary.cs:20`.

## 현재 연결 조사

Wolf=Pack Hunter, Grove=Ancient Grove, Bog=Bog Stalker. 2/6세트는 스탯, 4세트는 기존 스킬 쿨다운 강화이며 독립 시전 스킬은 없다. 근거: `Assets/Resources/SetItem.csv:14~16`, `:20~22`, `:26~28`. 세트 강화 대상 외에도 해당 세트 무기의 전체 슬롯을 함께 조사했다.

문제는 코드·에셋 연결에 근거한 분류다. 공용 테마 FX 재사용은 정상 공통 기능이며, “눈에 안 띔”의 실제 화면 판정은 미검증이다. “없음”은 연결상 별도 문제를 확인하지 못했다는 뜻이다.

| 스킬 ID | 무기·세트 | 발동 방식 | 현재 이펙트(프리팹·스프라이트·절차 함수) | 문제 | 근거 |
|---|---|---|---|---|---|
| Smash_Down | RustedSword / — | A 키·단발 | cast: 없음<br>`GroundAttack.prefab`<br>sprite: `Assets/Sprite/Effect/GroundAttack/GroundAttack1.png` | 없음 | `Assets/Resources/Weapon/RustedSword.prefab:94`<br>`Assets/Resources/Weapon/DamageObject/GroundAttack.prefab:79` |
| Slash_Attck | RustedSword / — | S 키·단발 | cast: 없음<br>`SlashAttack.prefab`<br>sprite: `Assets/Sprite/Effect/SlashAttack/SlashAttack3.png` | 없음 | `Assets/Resources/Weapon/RustedSword.prefab:192`<br>`Assets/Resources/Weapon/DamageObject/SlashAttack.prefab:79` |
| Scattering_Leaves | LeafAdae / — | A 키·단발·방사 투사체 | cast: 없음<br>`LeafAdea/LeafSkillStraight.prefab`<br>sprite: `Assets/Sprite/Enemy/LeafMole/LeafAttck1.png` | 없음 | `Assets/Resources/Weapon/LeafAdae.prefab:95`<br>`Assets/Resources/Weapon/DamageObject/LeafAdea/LeafSkillStraight.prefab:80` |
| Haste | LeafAdae / — | S 키·단발·버프/회복 | cast: 없음<br>`LeafAdea/AttSpeedBuff.prefab`<br>sprite: `Assets/Sprite/Effect/GroundAttack/GroundAttack1.png` | 없음 | `Assets/Resources/Weapon/LeafAdae.prefab:195`<br>`Assets/Resources/Weapon/DamageObject/LeafAdea/AttSpeedBuff.prefab:78` |
| Falling_Leaf_Step | LeafAdae / — | D 키·단발·버프/회복 | cast: 없음<br>`LeafAdea/SpeedBuff.prefab`<br>sprite: `Assets/Sprite/Effect/SpeedUp/SpeedUp1.png` | 없음 | `Assets/Resources/Weapon/LeafAdae.prefab:671`<br>`Assets/Resources/Weapon/DamageObject/LeafAdea/SpeedBuff.prefab:78` |
| Fang_Rush | FangDagger / Wolf | A 키·순차 찌르기 기본 6회(프리팹 10개 중 objectMaxCount=6, 장비 추가타 반영) | cast: DustBrown<br>`DoubleDagger/DaggerPowerSteb.prefab`<br>sprite: `Assets/Sprite/Effect/PowerStab/PowerStab1.png` | 다른 무기 것 재사용 | `Assets/Resources/Weapon/FangDagger.prefab:121`<br>`Assets/Resources/Weapon/DamageObject/DoubleDagger/DaggerPowerSteb.prefab:79` |
| Pack_Step | FangDagger / Wolf | S 키·단발·버프/회복 | cast: 없음<br>`DoubleDagger/HitCountBuff.prefab`<br>sprite: `Assets/Sprite/Effect/SpeedUp/SpeedUp1.png` | 다른 무기 것 재사용 | `Assets/Resources/Weapon/FangDagger.prefab:230`<br>`Assets/Resources/Weapon/DamageObject/DoubleDagger/HitCountBuff.prefab:78` |
| Hunter's_Stride | FangDagger / Wolf | D 키·단발·버프/회복 | cast: 없음<br>`DoubleDagger/SpeedBuff.prefab`<br>sprite: `Assets/Sprite/Effect/SpeedUp/SpeedUp1.png` | 다른 무기 것 재사용 | `Assets/Resources/Weapon/FangDagger.prefab:330`<br>`Assets/Resources/Weapon/DamageObject/DoubleDagger/SpeedBuff.prefab:78` |
| Howl_of_the_Pack | FangDagger / Wolf | F 키·선딜 0.3초 뒤 늑대 소환 | cast: BurstBlue; 선딜 delayEffectPrefab<br>`FangDagger/SummonWolf.prefab` → `FangDagger/WolfSpirit.prefab` 2기<br>sprite: `Assets/Art/Monsters/ForestWolf/Run.png`; 등장·소멸 SmokeGray | 없음 | `Assets/Resources/Weapon/FangDagger.prefab:1335`·`:1338`<br>`Assets/Resources/Weapon/DamageObject/FangDagger/SummonWolf.prefab:157`·`:159`<br>`Assets/Resources/Weapon/DamageObject/FangDagger/WolfSpirit.prefab:88`·`:152`·`:153` |
| Mauling_Rush | BearClaw / Grove | A 키·돌진 후 순차 할퀴기 총 6개 판정 | cast: DustBrown<br>`BearClaw/BearClawScareDashAttack.prefab`<br>`BearClaw/BearClawClowAttackDashL.prefab`<br>sprite: `Assets/Sprite/Effect/ClowAttack/clowAttack1.png`<br>`BearClaw/BearClawClowAttackDashR.prefab`<br>sprite: `Assets/Sprite/Effect/ClowAttack/clowAttackRight1.png` | 다른 무기 것 재사용 | `Assets/Resources/Weapon/BearClaw.prefab:109`<br>`Assets/Resources/Weapon/DamageObject/BearClaw/BearClawScareDashAttack.prefab:87`<br>`Assets/Resources/Weapon/DamageObject/BearClaw/BearClawClowAttackDashL.prefab:87`<br>`Assets/Resources/Weapon/DamageObject/BearClaw/BearClawClowAttackDashR.prefab:87` |
| Rending_Upswipe | BearClaw / Grove | S 키·단발 | cast: SlashRed → WSlashSide(적색)<br>`BearClaw/BearClawScareDashUpAttack.prefab`<br>sprite: `Assets/Sprite/Effect/ClowAttack/ClowUpper1.png` | 다른 무기 것 재사용 | `Assets/Resources/Weapon/BearClaw.prefab:214`<br>`Assets/Resources/Weapon/DamageObject/BearClaw/BearClawScareDashUpAttack.prefab:87` |
| Bear_Roar | BearClaw / Grove | D 키·단발·버프/회복 | cast: BurstRed<br>`LeafAdea/AttSpeedBuff.prefab`<br>sprite: `Assets/Sprite/Effect/GroundAttack/GroundAttack1.png` | 다른 무기 것 재사용 | `Assets/Resources/Weapon/BearClaw.prefab:861`<br>`Assets/Resources/Weapon/DamageObject/LeafAdea/AttSpeedBuff.prefab:78` |
| Root_Step | RootStaff / Grove | A 키·단발 | cast: BurstGreen<br>`LeafStaff/TelePort.prefab`<br>sprite: `Assets/Sprite/Effect/LeafTele/LeafTele1.png` | 다른 무기 것 재사용 | `Assets/Resources/Weapon/RootStaff.prefab:94`<br>`Assets/Resources/Weapon/DamageObject/LeafStaff/TelePort.prefab:80` |
| Ancient_Roots | RootStaff / Grove | S 키·지면 확인 후 순차 뿌리 총 9개 판정 | cast: PillarGreen<br>`RootStaff/DeepRoot.prefab`<br>sprite: `Assets/Sprite/Effect/RootAttack/RootRise1.png` | 다른 무기 것 재사용 | `Assets/Resources/Weapon/RootStaff.prefab:231`<br>`Assets/Resources/Weapon/DamageObject/RootStaff/DeepRoot.prefab:88` |
| Sap_Mend | RootStaff / Grove | D 키·단발·지면 확인·버프/회복 | cast: RuneGreen<br>`LeafStaff/HealGround.prefab`<br>sprite: `Assets/Sprite/Effect/HealGround/HealGround1.png` | 다른 무기 것 재사용 | `Assets/Resources/Weapon/RootStaff.prefab:512`<br>`Assets/Resources/Weapon/DamageObject/LeafStaff/HealGround.prefab:77` |
| Jaw_Shake | CrocJawAxe / Bog | A 키·단발 | cast: SlashRed → WSlashSide(적색)<br>`CrocJawAxe/CrocJawAxeSSideDown.prefab`<br>sprite: `Assets/Sprite/Effect/ScareSide/SSideDown1.png` | 다른 무기 것 재사용 | `Assets/Resources/Weapon/CrocJawAxe.prefab:94`<br>`Assets/Resources/Weapon/DamageObject/CrocJawAxe/CrocJawAxeSSideDown.prefab:87` |
| Bog_Eruption | CrocJawAxe / Bog | S 키·단발 | cast: DustBrown<br>`CrocJawAxe/CrocJawAxeScareSideGround.prefab`<br>sprite: `Assets/Sprite/Effect/ScareSide/SideGround1.png` | 다른 무기 것 재사용 | `Assets/Resources/Weapon/CrocJawAxe.prefab:194`<br>`Assets/Resources/Weapon/DamageObject/CrocJawAxe/CrocJawAxeScareSideGround.prefab:87` |
| Death_Roll | CrocJawAxe / Bog | F 키·단발 | cast: DustBrown<br>`CrocJawAxe/CrocJawAxeSalshSlide.prefab`<br>sprite: `Assets/Sprite/Effect/ScareSide/Slash1.png` | 다른 무기 것 재사용 | `Assets/Resources/Weapon/CrocJawAxe.prefab:294`<br>`Assets/Resources/Weapon/DamageObject/CrocJawAxe/CrocJawAxeSalshSlide.prefab:88` |
| Tail_Whirl | CrocJawAxe / Bog | D 키·홀드(최대 5초), 시작 FX 1회 | cast: DustBrown<br>`CrocJawAxe/CrocJawAxeSpiningSlash.prefab`<br>sprite: `Assets/Sprite/Effect/ScareSide/SpiningSlash1.png` | 다른 무기 것 재사용 | `Assets/Resources/Weapon/CrocJawAxe.prefab:813`<br>`Assets/Resources/Weapon/DamageObject/CrocJawAxe/CrocJawAxeSpiningSlash.prefab:112` |
| Boar_Charge | TuskKnuckles / Wolf | A 키·단발 | cast: DustBrown<br>`TuskKnuckles/TuskDash.prefab`<br>sprite: `Assets/Sprite/Effect/Dash/DashEffect1.png` | 다른 무기 것 재사용 | `Assets/Resources/Weapon/TuskKnuckles.prefab:94`<br>`Assets/Resources/Weapon/DamageObject/TuskKnuckles/TuskDash.prefab:87` |
| Hoof_Step | TuskKnuckles / Wolf | S 키·단발 | cast: 없음<br>`Leather Knucle/BackDash.prefab`<br>sprite: `Assets/Sprite/Effect/Dash/DashEffect1.png` | 다른 무기 것 재사용 | `Assets/Resources/Weapon/TuskKnuckles.prefab:194`<br>`Assets/Resources/Weapon/DamageObject/Leather Knucle/BackDash.prefab:79` |
| Tusk_Uppercut | TuskKnuckles / Wolf | D 키·단발 | cast: DustBrown<br>`TuskKnuckles/TuskUpperCut.prefab`<br>sprite: `Assets/Sprite/Effect/UpperCut/UpperCut1.png` | 다른 무기 것 재사용 | `Assets/Resources/Weapon/TuskKnuckles.prefab:619`<br>`Assets/Resources/Weapon/DamageObject/TuskKnuckles/TuskUpperCut.prefab:87` |
| Tongue_Lash | FrogTongueWhip / Bog | A 키·연속 찌르기 2개 판정 | cast: SlashRed → WSlashSide(적색)<br>`FrogTongueWhip/FrogTongueWhipDoubleSteb.prefab`<br>sprite: `Assets/Sprite/Effect/PowerStab/PowerStab1.png` | 다른 무기 것 재사용 | `Assets/Resources/Weapon/FrogTongueWhip.prefab:97`<br>`Assets/Resources/Weapon/DamageObject/FrogTongueWhip/FrogTongueWhipDoubleSteb.prefab:87` |
| Leap_Slam | FrogTongueWhip / Bog | S 키·단발 | cast: DustBrown<br>`FrogTongueWhip/FrogTongueWhipSpearDashAttack 1.prefab`<br>sprite: `Assets/Sprite/Effect/SpearDash/SpearDash1.png` | 다른 무기 것 재사용 | `Assets/Resources/Weapon/FrogTongueWhip.prefab:198`<br>`Assets/Resources/Weapon/DamageObject/FrogTongueWhip/FrogTongueWhipSpearDashAttack 1.prefab:87` |
| Crab_Scuttle | PetalPincer / Bog | A 키·단발 | cast: DustBrown<br>`PetalPincer/PetalPincerDash.prefab`<br>sprite: `Assets/Sprite/Effect/Dash/DashEffect1.png` | 다른 무기 것 재사용 | `Assets/Resources/Weapon/PetalPincer.prefab:94`<br>`Assets/Resources/Weapon/DamageObject/PetalPincer/PetalPincerDash.prefab:87` |
| Shell_Back | PetalPincer / Bog | S 키·단발 | cast: 없음<br>`PetalPincer/PetalPincerBackDash.prefab`<br>sprite: `Assets/Sprite/Effect/Dash/DashEffect1.png` | 다른 무기 것 재사용 | `Assets/Resources/Weapon/PetalPincer.prefab:194`<br>`Assets/Resources/Weapon/DamageObject/PetalPincer/PetalPincerBackDash.prefab:87` |
| Pincer_Flurry | PetalPincer / Bog | D 키·단발 | cast: CrackRed → WSlashDown(상하 반전)<br>`PetalPincer/PetalPincerUpperCut.prefab`<br>sprite: `Assets/Sprite/Effect/UpperCut/UpperCut1.png` | 다른 무기 것 재사용 | `Assets/Resources/Weapon/PetalPincer.prefab:613`<br>`Assets/Resources/Weapon/DamageObject/PetalPincer/PetalPincerUpperCut.prefab:87` |

공용 cast FX의 절차 생성: 마법(Burst/Pillar/Rune)은 `Tools/pixelfx/gen_magic.py`, 먼지·폭발은 `Tools/pixelfx/gen_polish_fx.py`. enum 근거 `Assets/Script/Combat/Vfx/VfxLibrary.cs:7~24`; 구형 베기의 WSlash 치환은 `Assets/Script/Combat/Vfx/Vfx.cs`의 `TryUpgradeLegacy`. 각 판정 스프라이트는 위 표의 Animator가 추가로 프레임을 바꿀 수 있다.

## GPT용 한 줄 그림 주문

공통: 초반용 작고 소박한 픽셀 FX, 약간 몽환적이고 섬뜩하게, 유아틱·얼굴·캐릭터·큰 광원·글자 금지, 투명 배경, 블러 없이 단단한 픽셀 가장자리. 아래는 제작용 주문만이며 PNG는 생성하지 않았다.

- `Smash_Down_f4.png`: 프레임당 64×48px, 4프레임 가로 시트, 왼쪽→오른쪽, 짧은 아래찍기 쐐기, 색 회갈색 #796A59, 흐린 회청 #82979D, 뼈색 #D6CEAD; 초반용 소박한 투명 픽셀 FX.
- `Slash_Attck_f4.png`: 프레임당 64×48px, 4프레임 가로 시트, 왼쪽→오른쪽, 작은 횡베기 자국, 색 회갈색 #796A59, 흐린 회청 #82979D, 뼈색 #D6CEAD; 초반용 소박한 투명 픽셀 FX.
- `Scattering_Leaves_f4.png`: 프레임당 64×48px, 4프레임 가로 시트, 왼쪽→오른쪽, 흩어지는 잎 조각, 색 이끼 녹색 #687B51, 탁한 청록 #527B78, 뼈색 #D6CEAD; 초반용 소박한 투명 픽셀 FX.
- `Haste_f4.png`: 프레임당 48×48px, 4프레임 가로 시트, 왼쪽→오른쪽, 가느다란 잎맥 상승선, 색 이끼 녹색 #687B51, 탁한 청록 #527B78, 뼈색 #D6CEAD; 초반용 소박한 투명 픽셀 FX.
- `Falling_Leaf_Step_f4.png`: 프레임당 48×48px, 4프레임 가로 시트, 왼쪽→오른쪽, 발목의 잎 잔흔, 색 이끼 녹색 #687B51, 탁한 청록 #527B78, 뼈색 #D6CEAD; 초반용 소박한 투명 픽셀 FX.
- `Fang_Rush_f4.png`: 프레임당 64×48px, 4프레임 가로 시트, 왼쪽→오른쪽, 겹친 송곳니 찌르기, 색 회갈색 #796A59, 흐린 회청 #82979D, 뼈색 #D6CEAD; 초반용 소박한 투명 픽셀 FX.
- `Pack_Step_f4.png`: 프레임당 48×48px, 4프레임 가로 시트, 왼쪽→오른쪽, 발밑의 짧은 발톱 자국, 색 회갈색 #796A59, 흐린 회청 #82979D, 뼈색 #D6CEAD; 초반용 소박한 투명 픽셀 FX.
- `Hunter's_Stride_f4.png`: 프레임당 48×48px, 4프레임 가로 시트, 왼쪽→오른쪽, 가느다란 추적 바람, 색 회갈색 #796A59, 흐린 회청 #82979D, 뼈색 #D6CEAD; 초반용 소박한 투명 픽셀 FX.
- `Howl_of_the_Pack_f4.png`: 프레임당 64×48px, 4프레임 가로 시트, 중심에서 바깥, 작은 늑대 울음 파문, 색 회갈색 #796A59, 흐린 회청 #82979D, 뼈색 #D6CEAD; 초반용 소박한 투명 픽셀 FX.
- `Mauling_Rush_f4.png`: 프레임당 64×48px, 4프레임 가로 시트, 왼쪽→오른쪽, 짧은 세 갈래 할퀴기, 색 회갈색 #796A59, 흐린 회청 #82979D, 뼈색 #D6CEAD; 초반용 소박한 투명 픽셀 FX.
- `Rending_Upswipe_f4.png`: 프레임당 64×48px, 4프레임 가로 시트, 아래→위, 아래에서 위로 긁는 발톱, 색 회갈색 #796A59, 흐린 회청 #82979D, 뼈색 #D6CEAD; 초반용 소박한 투명 픽셀 FX.
- `Bear_Roar_f4.png`: 프레임당 48×48px, 4프레임 가로 시트, 중심에서 바깥, 작게 퍼지는 거친 울음 파문, 색 회갈색 #796A59, 흐린 회청 #82979D, 뼈색 #D6CEAD; 초반용 소박한 투명 픽셀 FX.
- `Root_Step_f4.png`: 프레임당 64×48px, 4프레임 가로 시트, 왼쪽→오른쪽, 낮게 흩어지는 뿌리 먼지, 색 이끼 녹색 #687B51, 탁한 청록 #527B78, 뼈색 #D6CEAD; 초반용 소박한 투명 픽셀 FX.
- `Ancient_Roots_f4.png`: 프레임당 64×48px, 4프레임 가로 시트, 아래→위, 땅에서 돋는 가느다란 뿌리, 색 이끼 녹색 #687B51, 탁한 청록 #527B78, 뼈색 #D6CEAD; 초반용 소박한 투명 픽셀 FX.
- `Sap_Mend_f4.png`: 프레임당 48×48px, 4프레임 가로 시트, 왼쪽→오른쪽, 작은 수액 방울과 잎맥, 색 이끼 녹색 #687B51, 탁한 청록 #527B78, 뼈색 #D6CEAD; 초반용 소박한 투명 픽셀 FX.
- `Jaw_Shake_f4.png`: 프레임당 64×48px, 4프레임 가로 시트, 왼쪽→오른쪽, 작게 맞물리는 턱 모양 쐐기, 색 이끼 녹색 #687B51, 탁한 청록 #527B78, 뼈색 #D6CEAD; 초반용 소박한 투명 픽셀 FX.
- `Bog_Eruption_f4.png`: 프레임당 64×48px, 4프레임 가로 시트, 왼쪽→오른쪽, 낮은 진흙 파편, 색 이끼 녹색 #687B51, 탁한 청록 #527B78, 뼈색 #D6CEAD; 초반용 소박한 투명 픽셀 FX.
- `Death_Roll_f4.png`: 프레임당 64×48px, 4프레임 가로 시트, 왼쪽→오른쪽, 짧은 비스듬한 회전 호, 색 이끼 녹색 #687B51, 탁한 청록 #527B78, 뼈색 #D6CEAD; 초반용 소박한 투명 픽셀 FX.
- `Tail_Whirl_f4.png`: 프레임당 64×48px, 4프레임 가로 시트, 중심에서 바깥, 가느다란 꼬리 회전 잔흔, 색 이끼 녹색 #687B51, 탁한 청록 #527B78, 뼈색 #D6CEAD; 초반용 소박한 투명 픽셀 FX.
- `Boar_Charge_f4.png`: 프레임당 64×48px, 4프레임 가로 시트, 왼쪽→오른쪽, 낮게 밀리는 엄니 먼지, 색 회갈색 #796A59, 흐린 회청 #82979D, 뼈색 #D6CEAD; 초반용 소박한 투명 픽셀 FX.
- `Hoof_Step_f4.png`: 프레임당 64×48px, 4프레임 가로 시트, 왼쪽→오른쪽, 짧은 발굽 먼지, 색 회갈색 #796A59, 흐린 회청 #82979D, 뼈색 #D6CEAD; 초반용 소박한 투명 픽셀 FX.
- `Tusk_Uppercut_f4.png`: 프레임당 64×48px, 4프레임 가로 시트, 아래→위, 아래에서 위로 치는 엄니 쐐기, 색 회갈색 #796A59, 흐린 회청 #82979D, 뼈색 #D6CEAD; 초반용 소박한 투명 픽셀 FX.
- `Tongue_Lash_f4.png`: 프레임당 64×48px, 4프레임 가로 시트, 왼쪽→오른쪽, 가느다란 혀 채찍 잔흔, 색 이끼 녹색 #687B51, 탁한 청록 #527B78, 뼈색 #D6CEAD; 초반용 소박한 투명 픽셀 FX.
- `Leap_Slam_f4.png`: 프레임당 64×48px, 4프레임 가로 시트, 왼쪽→오른쪽, 작은 착지 진흙 쐐기, 색 이끼 녹색 #687B51, 탁한 청록 #527B78, 뼈색 #D6CEAD; 초반용 소박한 투명 픽셀 FX.
- `Crab_Scuttle_f4.png`: 프레임당 64×48px, 4프레임 가로 시트, 왼쪽→오른쪽, 낮게 옆으로 흩어지는 모래, 색 이끼 녹색 #687B51, 탁한 청록 #527B78, 뼈색 #D6CEAD; 초반용 소박한 투명 픽셀 FX.
- `Shell_Back_f4.png`: 프레임당 64×48px, 4프레임 가로 시트, 왼쪽→오른쪽, 뒤로 빠지는 짧은 모래 자국, 색 이끼 녹색 #687B51, 탁한 청록 #527B78, 뼈색 #D6CEAD; 초반용 소박한 투명 픽셀 FX.
- `Pincer_Flurry_f4.png`: 프레임당 64×48px, 4프레임 가로 시트, 왼쪽→오른쪽, 작게 교차하는 집게 자국, 색 이끼 녹색 #687B51, 탁한 청록 #527B78, 뼈색 #D6CEAD; 초반용 소박한 투명 픽셀 FX.


## 검증 기록 (2026-11-01)

- Unity 스크립트 컴파일 완료, C# 컴파일 오류 0. 최종 EditMode 테스트 427/427 통과, 실패 0·건너뜀 0, 4.886초. 작업 ID `937e465edb9c4971af4243362c2079c0`.
- 고유 이름 `__TempFxSlotProbe_20261101.png`의 빈 더미 1장으로 카탈로그 → PixelFxClip 로드 확인: 1프레임, 8×8px, 36 PPU, 0.2초. 삭제 후 파일 없음·해당 ID의 GetClip=null(기존 FX 복귀) 확인. 더미 PNG와 meta 삭제 완료.
- 최종 프로브: playing=False, compiling=False, 카탈로그 27개. 작업 중 외부에서 추가된 FX PNG 27개는 Codex가 제작·수정하지 않았다.
- 세이브 3개 백업·복원 MD5 일치 3/3: SaveFile `F1CBD3EDF7E845CACE5442F6641C5B2F`, SaveSlot2 `4683BD8B4FFE564E86535BDF7861504B`, SaveSlot3 `19F8B41DA36D93C79D675FB3CFD1325C`.
- **미해결:** 콘솔 clear 직후 Unity 네이티브 `Access version should be odd when acquiring lock` 19건 재발. C# 컴파일 오류와 별개이며 콘솔 전체 error=0은 미달성이다. 임시 PNG 추가 전에도 같은 오류를 관찰했다.
- **미검증:** 인게임 시각 품질·27스킬 실제 시전·홀드/소환/장판의 화면 겹침·플레이어 빌드 실행. 이번 연결은 시전 FX 자리만 제공하며 최종 시각 판정은 Claude/사용자가 한다.
- 일회성 프로브·호출 스크립트·백업은 `Logs/temp_fx_*`, `Logs/TempFxSaveBackup/`에만 두며 커밋 제외. 커밋·push는 하지 않았다.
