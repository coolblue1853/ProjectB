# 세이브 슬롯 점검 (11-03 18:10, 내용 미열람, md5 앞 8자리)

경로: `C:\Users\user\AppData\LocalLow\DefaultCompany\ProjectB`

| 파일 | mtime | 크기 | md5 |
|---|---|---|---|
| SaveFile.es3 | 17:35:03 | 70182 | f1cbd3ed |
| SaveFile.es3.bac | 17:35:03 | 70182 | f1cbd3ed |
| SaveSlot2.es3 | 17:35:03 | 59169 | eefb5f86 |
| SaveSlot2.es3.bac | 17:35:03 | 59151 | 2f489328 |
| SaveSlot3.es3 | 17:35:03 | 76519 | b222881f |
| SaveSlot3.es3.bac | 17:35:03 | 75563 | f1a9ac9e |

비교(과거 사본 대비)
- `Logs/pt_save/*.es3`(17:29 사본)와 현재 `.es3` 3개 md5 동일.
- `Logs/ptbot/backup`(14:18, 옛 ptbot 백업): SaveFile 현재와 동일. SaveSlot3.es3(b222881f)·.bac(f1a9ac9e) 동일. **SaveSlot2.es3 만 다름**(옛 2f489328, 크기 59151 -> 현재 eefb5f86, 59169). 현재 SaveSlot2.es3.bac 는 옛 .es3 와 동일(2f489328) = 그 사이 한 번 저장되어 .bac 가 직전본이 됨. 정상 저장 흔적, 손상 징후 없음.
- 모든 mtime 이 17:35:03 으로 같음 = 한꺼번에 복사(ptbot restore 의 `cp`)된 흔적. 슬롯 진행 시각은 mtime 으로 못 믿는다.
- 16:42 시점 .bac 원본은 남아 있지 않아 직접 비교 불가.

대책: ptbot.sh 가 이제 최초 원본(`Logs/pt_save/orig`)을 보존한다(첫 실행 시 현재 상태가 orig 가 됨 — 지금 상태가 "원본"으로 굳으니 의도한 세이브일 때 첫 실행할 것).
