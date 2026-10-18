// 도깨비불 밤 발광: 런타임에 피격 머티리얼(M_EnemySprite)로 바뀌므로 머티리얼 대신 파란 점광원(Light2D)을 붙인다
var path = "Assets/Prefab/Enemy_BD/Wisp_BD.prefab";
var root = PrefabUtility.LoadPrefabContents(path);
var old = root.transform.Find("WispGlow"); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
var go = new GameObject("WispGlow"); go.transform.SetParent(root.transform, false);
var l = go.AddComponent<UnityEngine.Rendering.Universal.Light2D>();
l.lightType = UnityEngine.Rendering.Universal.Light2D.LightType.Point;
l.shadowsEnabled = false; { var lso = new SerializedObject(l); lso.FindProperty("m_NormalMapQuality").intValue = (int)UnityEngine.Rendering.Universal.Light2D.NormalMapQuality.Disabled; lso.ApplyModifiedPropertiesWithoutUndo(); }
l.color = new Color(0.45f, 0.75f, 1f); l.intensity = 1.6f; l.pointLightOuterRadius = 3.5f; l.pointLightInnerRadius = 0.3f;
PrefabUtility.SaveAsPrefabAsset(root, path); PrefabUtility.UnloadPrefabContents(root);
return "glow added";
