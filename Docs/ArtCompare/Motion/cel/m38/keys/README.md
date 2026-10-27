# 스컬 기준 재작업 키 자세(10-27)
edit_image_pro_flash 로 ref_sword 에서 만든 키 자세(각 5 gen): up_ready(올려베기 준비), up_hit(올려베기 끝), air_hit(공중 내려치기 타격), slam_hit(도약 내려찍기 착지), iaido_hit(발도 끝), guard_hold(막기), charge_hold(모으기), punch_hit(주먹).
다음: PixMiniMax 사이 프레임 — 기준→키(4프레임, 끝 고정), 키→기준(4프레임) / 올려베기는 기준→준비→타격→기준. 결과 폴더 m38/v2_<이름>_<구간>.
refs.txt = 업로드 번호(3일 유효).
사용자(10-27): "비용 들여도 되니까 스컬과 비슷하게 상업적으로 쓸 수 있을 만큼 퀄리티가 나올 때까지 반복해서 계속" — 생성 비용 제한 없이 키 자세·사이 프레임을 반복.
사이 프레임 작업 1차(4프레임, 끝 고정):
v2_up_a 82926c7e-daa3-4a1c-a5c5-8c1b36a45a81 (기준→올려베기 준비)
v2_up_b 8124c66b-34e6-4ef0-a4fe-0025afe1bd2d (준비→올려베기 끝)
v2_up_c 4ee97530-8a3a-4810-965b-3a4cc21a8f2a (끝→기준)
v2_air_a 0e688233-a2f7-4b81-9765-0f628d16c787 (기준→공중 타격)
v2_slam_a 6ea22d07-1e6c-46d4-91f3-50377d63d61e (기준→내려찍기 착지)
v2_iaido_a b29813c1-e713-4e03-8de2-249c2c3e68a1 (기준→발도 끝)
v2_guard_a b3ac390f-0feb-48db-be4d-3b9cf1fe70a4 (기준→막기)
v2_charge_a 9766955b-035b-4f86-8a4a-448bc6dd7587 (기준→모으기)
남은 사이 작업: 공중·내려찍기·발도·막기·모으기·주먹의 키→기준 회복 구간, 주먹 기준→타격. 이후 시전·던지기·백스텝·활·어깨 겨눔·잡기·앉기 키 자세.
1차 결과 판정(v2_sheet1.png): 올려베기 v2 = 웅크림 → 초승달 잔상과 함께 솟구침 → 넓은 보폭 마무리 → 회복, 스컬 기준 통과(다리 문제 해결). 공중·내려찍기·발도·막기·모으기 앞 구간도 자세가 크고 읽힘 — 통과.
사이 프레임 2차(회복 구간·주먹):
v2_air_b 8b669187-f547-4e1b-ba67-976bdfc669f5 (공중 타격→기준)
v2_slam_b 1adb6159-e9c4-4cbf-b6d3-fca4805fe7fc (착지→기준)
v2_iaido_b ef8d1b17-4a52-435c-b1fb-992aa4c2e70c (발도 끝→기준)
v2_guard_b a3dba45c-77de-4c57-9e00-a557143ca986 (막기→기준)
v2_charge_b 00086b0f-85e0-41a0-8a8c-ab8920ac1841 (모으기 떨림 반복, 시작·끝 같음)
v2_punch_a 76ad45df-81f0-44e0-93e7-5144a3693d77 (기준→주먹)
v2_punch_b 3d0cb0f0-c822-425f-8dae-7ebe827cfe97 (주먹→기준)
다음: 받아서 클립 조립 — SwingUp = up_a 2~4 + up_b 1~4 + up_c 1~3(약 10장, 앞 1~2장은 기준과 같아 뺌), 나머지 = a 1~4 + b 1~3. build_cel CLIPS 에서 폴더 여러 개를 이어 쓰도록(예: 리스트 항목 "폴더/번호") 확장 → 장비 전이 → 게임 촬영 → 검토 영상 갱신.
장비 전이 v2: batch_v2_1~4 (m38_batches.json 에 job 번호) — 받은 뒤 build_equip → split → comp_lint → Unity build → 촬영 → 검토 영상(motion38_review) → 커밋
1차 적용 완료(24dee48f): SwingUp·AirSlash·LeapSlam·Iaido·Guard·Charge·Punch v2, 셀 36·빌드 오류 0.
2차 대상(같은 방식): 시전(Cast)·던지기(Throw)·백스텝(Backstep)·어깨 겨눔(ShoulderAim)·잡기(Grab)·앉기(Ride) 키 자세 → 이어서 기존 내려베기·찌르기·양손 내려찍기·대검 휘두르기·단검 2종·돌진 찌르기·회전·발차기도 스컬 기준으로 재점검.
