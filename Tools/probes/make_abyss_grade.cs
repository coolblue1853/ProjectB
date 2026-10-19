// 어비스 컬러 그레이딩 프로필 생성/갱신 (에디트·플레이 모두 가능)
string path = "Assets/Resources/PostFx/AbyssGrade.asset";
var prof = AssetDatabase.LoadAssetAtPath<UnityEngine.Rendering.VolumeProfile>(path);
if (prof == null) { prof = ScriptableObject.CreateInstance<UnityEngine.Rendering.VolumeProfile>(); AssetDatabase.CreateAsset(prof, path); }
System.Func<System.Type, UnityEngine.Rendering.VolumeComponent> get = t => { UnityEngine.Rendering.VolumeComponent c; if (!prof.TryGet(t, out c)) { c = prof.Add(t, true); AssetDatabase.AddObjectToAsset(c, prof); } c.active = true; return c; };
var ca = (UnityEngine.Rendering.Universal.ColorAdjustments)get(typeof(UnityEngine.Rendering.Universal.ColorAdjustments));
ca.saturation.Override(-12f); ca.contrast.Override(0f); ca.postExposure.Override(0.1f); ca.colorFilter.Override(new Color(0.96f, 0.99f, 1f));
var st = (UnityEngine.Rendering.Universal.SplitToning)get(typeof(UnityEngine.Rendering.Universal.SplitToning));
st.shadows.Override(new Color(0.20f, 0.42f, 0.47f)); st.highlights.Override(new Color(0.93f, 0.80f, 0.62f)); st.balance.Override(10f);
var smh = (UnityEngine.Rendering.Universal.ShadowsMidtonesHighlights)get(typeof(UnityEngine.Rendering.Universal.ShadowsMidtonesHighlights));
smh.shadows.Override(new Vector4(0.94f, 1.02f, 1.08f, 0.1f)); smh.highlights.Override(new Vector4(1.03f, 1.0f, 0.95f, 0f));
var vg = (UnityEngine.Rendering.Universal.Vignette)get(typeof(UnityEngine.Rendering.Universal.Vignette));
vg.intensity.Override(0.2f); vg.smoothness.Override(0.65f); vg.color.Override(new Color(0.04f, 0.08f, 0.10f));
var bl = (UnityEngine.Rendering.Universal.Bloom)get(typeof(UnityEngine.Rendering.Universal.Bloom));
bl.threshold.Override(0.8f); bl.intensity.Override(0.9f); bl.scatter.Override(0.65f); bl.tint.Override(new Color(0.78f, 1f, 0.96f)); // 생물발광 포인트(눈·반딧불·도깨비불)
EditorUtility.SetDirty(prof); AssetDatabase.SaveAssets();
return "profile ok comps=" + prof.components.Count;
