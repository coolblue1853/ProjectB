# 마스터 모션 38 × 스컬 참고 애니 대응표

`Tools/skulref/m38_refs.py` 결과(10-27). 시트: `C:\Users\user\SkulRef\compare\m38\NN_이름.png`(저장소 밖, 참고 전용). 형식: 묶음폴더/애니(프레임 수). 묶음 폴더는 `SkulRef/lib/character/<소분류>/` 아래.
로프(11·12)는 스컬에 없음 → 우리 뒷모습 비율 보정안(MotionSet §6.3)으로 간다.

| # | 모션 | 스컬 후보(묶음/애니, 프레임) |
|---|---|---|
| 1 | 대기 | Appear_1b3ab02012/Idle(10), Rolling_20bf862898/Idle(8), Skill_2dd8031910/Idle(16), Skill1_3ac1287745/Idle(8), JumpAttack_76f1446398/Idle(15), NaturesGrab_7bc11a7522/Idle(22) |
| 2 | 걷기 | Appear_1b3ab02012/Walk(10), BluntStrike_2082af4195/Walk(8), Rolling_20bf862898/Walk(8), Skill_2dd8031910/Walk(24), LittleBone_57c1fb1632/Walk(8), Casting_6388d48342/Walk(8) |
| 3 | 달리기 | AirSlash_5dfbfb9740/Dash(8), Skill1_959def1736/Dash(9), basic_d1c04e8459/Dash(10), ChargeAttack_067a067504/Dash(7), Attack_A_1c1e9d6940/Dash(7), Rolling_20bf860832/Dash(7) |
| 4 | 제동 | Dash_8719107323/Dash_End(6), Move_dc66c32679/Move_End(6) |
| 5 | 점프 웅크림 | Appear_1b3ab02012/Jump(6), JumpAttack_76f1446398/Jump(6), Fire_b5da7f7495/Jump(6), Rolling_20bf860832/Jump(4), NaturesGrab_7bc11a7522/Jump(4), Dash_8719107323/Jump(4) |
| 6 | 공중 | JumpAttack_76f1446398/FallRepeat(9), NaturesGrab_7bc11a7522/FallRepeat(8), Appear_1b3ab02012/FallRepeat(6), Skill_2dd8031910/FallRepeat(6), Fire_b5da7f7495/FallRepeat(6), Rolling_20bf860832/FallRepeat(4) |
| 7 | 착지 | NaturesGrab_7bc11a7522/JumpAttack_Land(18), Skill_f139320932/JumpAttack_Land(7), Pierce_b4ee5e2680/JumpAttack_Land(6), Fire_b5da7f7495/JumpAttack_Land(5), ChargeAttack_c3d2b65148/JumpAttack_Land(5), basic_d1c04e8459/JumpAttack_Land(5) |
| 8 | 대시 | AirSlash_5dfbfb9740/Dash(8), Skill1_959def1736/Dash(9), basic_d1c04e8459/Dash(10), ChargeAttack_067a067504/Dash(7), Attack_A_1c1e9d6940/Dash(7), Rolling_20bf860832/Dash(7) |
| 9 | 피격 | Attack2_dc66c37384/Hit2(7), Attack2_dc66c37384/Hit1(6), Attack2_dc66c37384/Hit01(2), Attack2_dc66c37384/Hit02(2), Attack2_dc66c37384/Hit(1), Parts_data7871/Hit01(9) |
| 10 | 사망 | GiveWeapon_1e15e25401/Dead(10), Watch_data4706/Dead(11), Parts_dc66c34296/Dead(69), _dc66c30439/Dead(12), Meteor_dc66c37141/Die(10), Stamping_dc66c30051/Die(5) |
| 11 | 로프 매달리기 |  |
| 12 | 로프 오르기 |  |
| 13 | A1 내려베기 | Skill1_0242a60819/AttackA(8), Appear_1b3ab02012/AttackA(10), Skill_2dd8031910/AttackA(16), Empty_3387621514/AttackA(8), JumpAttack_76f1446398/AttackA(15), NaturesGrab_7bc11a7522/AttackA(28) |
| 14 | A2 올려베기 | Skill1_0242a60819/AttackB(8), Appear_1b3ab02012/AttackB(10), Skill_2dd8031910/AttackB(16), Punch_4a88894685/AttackB(10), AirSlash_5dfbfb9740/AttackB(16), Charge_5dfbfb2815/AttackB(16) |
| 15 | A3 찌르기 | Pierce_b4ee5e2680/Pierce_Attack_Skill(6), BuddhaCyclone_df855c9501/RapidThrust_Attack(6), BuddhaCyclone_df855c1809/RapidThrust_Attack_Enhanced(6), Cyclone_df855c7258/Thrust_Attack(4), RapidThrust_e6f90d2930/RapidThrust_Attack(6), RapidThrust_e6f90d2930/RapidThrust_Attack_Enhanced(6) |
| 16 | A4 양손 내려찍기 | NaturesGrab_7bc11a7522/Stomp_Attack(16), Pierce_b4ee5e2680/Stomp_Attack_Skill(6), BuddhaCyclone_df855c7583/HeavenEarthSlam_Attack(5), Cyclone_df855c7258/Slam_Attack(5), BuddhaCyclone_df855c1809/HeavenEarthSlam_Attack_Enhanced(5), EvilAnnihilation_9169e71902/Stomp_Attack(4) |
| 17 | A5 양손 수평 휘두르기 | Guard_e34bb85224/Slash_Horizontal(6), Appear_1b3ab02012/HorizontalSlash_Attack(5), Dash_8719107323/Slash_Horizontal_A_Active(4), BuddhaCyclone_df855c1809/Swing_Attack(4), Cyclone_df855c7258/Swing_Attack(3), RapidThrust_e6f90d2930/Swing_Attack(4) |
| 18 | A6 연속 찌르기 | Charge_5dfbfb2815/Ranbu_Attack(12), BuddhaCyclone_df855c9501/RapidThrust_Attack(6), BuddhaCyclone_df855c1809/RapidThrust_Attack_Enhanced(6), RapidThrust_e6f90d2930/RapidThrust_Attack(6), RapidThrust_e6f90d2930/RapidThrust_Attack_Enhanced(6) |
| 19 | A7 교차 베기 | Hibernation_f150452975/Slash_Cross_Attack(4), GlacialBlade_f150459923/Slash_Cross_Attack(4) |
| 20 | A8 공중 내려치기 | Skill_2dd8031910/JumpAttack(13), JumpAttack_76f1446398/JumpAttack(8), NaturesGrab_7bc11a7522/JumpAttack(12), Attack_a713052405/JumpAttack(8), Pierce_b4ee5e2680/JumpAttack(8), Skill1_b8ba9d0379/JumpAttack(8) |
| 21 | A9 돌진 찌르기 | Rush_834af86423/Rush(12), Rush_834af86423/Tackle(6), Attack_a713052405/DashAttack_A(6), Attack_a713052405/DashAttack_B(6), Attack_a713052405/DashAttack_C(6), Attack_a713052405/DashAttack_D(6) |
| 22 | A10 회전 베기 | Charge_5dfbfb2815/SpinAttack(4), BuddhaCyclone_df855c9501/BuddhaCyclone(3), BuddhaCyclone_df855c9501/Cyclone(3), Cyclone_df855c7258/Cyclone(3), BuddhaCyclone_df855c1809/BuddhaCyclone_Enhanced(3), AirSlash_5dfbfb9740/SpinAttack(1) |
| 23 | A11 시전 | Casting_a8fd116063/Casting(2), Attack2_dc66c37384/Casting(2), Casting_d567027468/Casting_End(15), Casting_d567027468/Casting_Intro(18), Casting_d567027468/Casting_Ready(25), Landing_data7873/Casting_Ready(25) |
| 24 | A12 던지기 | Attack_A_1c1e9d6940/Skill1(9), Empty_3387620891/Skill1(14), Punch_4a88894685/Skill1(11), Skill1_4a88894914/Skill1(11), ChargeAttack_b8b2dc0378/Skill1(7), Rolling_20bf862898/Skill1(6) |
| 25 | A13 활 | ChargeAttack_067a062898/ChargeAttack_Completed(3), ChargeAttack_067a062898/FastShot_Attack(3), ChargeAttack_067a062898/SlowShot_Attack(2) |
| 26 | A14 막기 | JumpAttack_76f1446398/Parry(9), Parry_fb9cb75324/Parry(11), Attack2_dc66c37384/Guard(5), Guard_e34bb85224/Guard(4), BodyBlow_8d81b32098/Parry(3), ChargeAttack_c3d2b65148/Parry(3) |
| 27 | A15 모으기 | Attack_A_d728860324/Concentration(4), Attack_A_d728860324/Concentration_Ready(4), ChargeAttack_067a062898/ChargeAttack_Charging(2), AirSlash_5dfbfb9740/Charge_Ready(2), Charge_5dfbfb2815/Charge_Ready(2), Casting_6388d43788/ChargeAttack_Charging(2) |
| 28 | A16 도약 내려찍기 | NaturesGrab_7bc11a7522/JumpAttack_Land(18), Skill_f139320932/JumpAttack_Land(7), Pierce_b4ee5e2680/JumpAttack_Land(6), Fire_b5da7f7495/JumpAttack_Land(5), ChargeAttack_c3d2b65148/JumpAttack_Land(5), basic_d1c04e8459/JumpAttack_Land(5) |
| 29 | A17 백스텝 | Attack2_dc66c37384/BackStep(1), _dc66c30929/BackStep(10), Adventurer_dc66c31523/BackStep(2), Meteor_dc66c37141/Backstep(2) |
| 30 | A18 주먹 3타 | Skill1_0242a60819/AttackA(8), Appear_1b3ab02012/AttackA(10), Skill_2dd8031910/AttackA(16), Empty_3387621514/AttackA(8), JumpAttack_76f1446398/AttackA(15), NaturesGrab_7bc11a7522/AttackA(28) |
| 31 | A19 발도 | Parry_fb9cb75324/ilseom_Attack(7) |
| 32 | R1 어깨 겨눔 | DrawCard_ec8e164740/Revolver_Ready(7), Fire_b5da7f7495/Fire_Attack(6), DrawCard_ec8e164740/Revolver_Ready_Loop(1) |
| 33 | R2 양손 들기 | Guard_e34bb85224/Skill_Ready(5), Skill_f139320932/Skill_Ready(4), DrawCard_ec8e164740/Skill_Ready(2), Casting_d567027468/Casting_Ready(25), Landing_data7873/Casting_Ready(25), Groggy_dc66c38205/Buff_Ready(8) |
| 34 | R3 잡기 | NaturesGrab_7bc11a7522/NaturesGrab_Completed_Grab(15), Grab_f9a9424661/Grab_Forward(7), FlameChain_2ef2da4894/Grab_Attack(6), NaturesGrab_7bc11a7522/NaturesGrab_Completed_Grab_Fail(1), Grab_dc66c35020/Grab_Attack(10), Grab_dc66c35020/DashGrab_Attack(4) |
| 35 | R4 앉기(탈것) | FlameChain_2ef2da4894/HellBike(4), Sit_4bb3011630/Sit_1 Loop x(5), Sit_4bb3011630/Sit_2 Loop X(3), Sit_4bb3011630/Sit_Talk(2), Sit_4bb3011630/Sit_3 Loop(1), Sit_4bb3011630/Sit_4 Loop(1) |
| 36 | R5 넘어짐·일어남 | LittleBone_57c1fb1632/Reborn1(Castle)(27), Reborn1(Castle)_8719109565/Reborn1(Castle)(30), Groggy_dc66c38205/Groggy_Ready(10), Grapple_dc66c37281/Groggy_Ready(6) |
| 37 | R6 웅크려 놓기 | Attack_A_1c1e9d6940/Interaction_CursedChest(20), LittleBone_57c1fb1632/GetScroll(52), LittleBone_57c1fb1632/GetSkull(45), Groggy_dc66c38205/GetOut(51) |
| 38 | R7 발차기 | BodyBlow_8d81b32098/Kick_Attack(4), Guard_e34bb85224/Kick(4) |
