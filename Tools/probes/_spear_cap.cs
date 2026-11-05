// AUTO-110: SpearTrap 상한 3 확인 (플레이 모드)
for (int i = 0; i < 5; i++) { SpearTrap.Spawn(new Vector3(i, 0, 0), 1f, 1, 4f, 0.5f); }
return "live=" + SpearTrap.LiveCount + " active=" + UnityEngine.Object.FindObjectsByType<SpearTrap>(FindObjectsSortMode.None).Length;
