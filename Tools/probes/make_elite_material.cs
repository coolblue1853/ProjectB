// 정예 외곽선 재질 생성: M_EnemySprite 복사 + All In 1 OUTBASE_ON·OUTBASEPIXELPERF_ON(빌드에 변형이 남도록 에셋에 키워드). 재실행 안전
var src = AssetDatabase.LoadAssetAtPath<Material>("Assets/Resources/FX/M_EnemySprite.mat");
string path = "Assets/Resources/FX/M_EnemySpriteElite.mat";
var m = AssetDatabase.LoadAssetAtPath<Material>(path);
if (m == null) { m = new Material(src); AssetDatabase.CreateAsset(m, path); } else { m.CopyPropertiesFromMaterial(src); }
m.EnableKeyword("OUTBASE_ON"); m.EnableKeyword("OUTBASEPIXELPERF_ON");
m.SetColor("_OutlineColor", new Color(1f, 0.78f, 0.3f, 1f)); m.SetFloat("_OutlineAlpha", 0.95f); m.SetFloat("_OutlineGlow", 2f); m.SetInt("_OutlinePixelWidth", 1);
EditorUtility.SetDirty(m); AssetDatabase.SaveAssets();
return "elite mat keywords: " + string.Join(",", m.shaderKeywords);
