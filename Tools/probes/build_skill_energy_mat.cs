// Resources/FX/M_SkillEnergy.mat 생성 (SkillEnergy2D 셰이더를 빌드에 포함시키는 참조 겸 공유 머티리얼)
var sh = Shader.Find("ProjectB/SkillEnergy2D");
if (sh == null) return "shader missing";
var path = "Assets/Resources/FX/M_SkillEnergy.mat";
var m = AssetDatabase.LoadAssetAtPath<Material>(path);
if (m == null) { m = new Material(sh); AssetDatabase.CreateAsset(m, path); } else m.shader = sh;
// 셰이더 기본값으로 재설정 (셰이더 튜닝 후 재실행)
for (int i = 0; i < sh.GetPropertyCount(); i++) {
  var n = sh.GetPropertyName(i); var t = sh.GetPropertyType(i);
  if (t == UnityEngine.Rendering.ShaderPropertyType.Float || t == UnityEngine.Rendering.ShaderPropertyType.Range) m.SetFloat(n, sh.GetPropertyDefaultFloatValue(i));
  else if (t == UnityEngine.Rendering.ShaderPropertyType.Color) m.SetColor(n, sh.GetPropertyDefaultVectorValue(i));
}
EditorUtility.SetDirty(m); AssetDatabase.SaveAssets();
return "mat ok supported=" + sh.isSupported + " passes=" + m.passCount + " | " + ProjectB.EditorTools.Lv2ItemBuilder.ApplyVfx();
