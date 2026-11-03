# 신규 맵 2장 보존 (Lv1_DeepForestCore, Lv1_MistSwamp)

- 게임은 `GameObject.Find("Map")` 아래 자식으로 맵을 찾고 포털이 씬 오브젝트(closeMap/openMap/proCamera/player)를 참조 -> SampleScene 안에 있어야 동작. 별도 씬/프리팹 분리 안 함.
- 11-03 18:10 확인: 두 맵 모두 에디터 메모리와 디스크 SampleScene(16:48 저장)에 존재. 씬은 커밋 금지(사용자 변경 혼재).
- 백업 프리팹: `Assets/Backup/Floor1Maps/Lv1_DeepForestCore.prefab`, `Lv1_MistSwamp.prefab` (타일·스포너·드롭 포함. 포털의 씬 참조는 끊김 - 참고/복구용, 런타임 미사용).
- 재생성: `bash Tools/u.sh cs Tools/probes/build_f1_newmaps.cs` (에디트 모드, 원본 Lv1_Swamp/Lv1_DeepForest 필요, 씬 저장은 따로).
- 프리팹 재-export: 에디트 모드에서 맵 루트를 `PrefabUtility.SaveAsPrefabAsset(go, 경로, out ok)` (SaveAsPrefabAssetAndConnect 금지).
