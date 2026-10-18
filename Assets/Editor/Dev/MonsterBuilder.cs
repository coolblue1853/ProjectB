#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace ProjectB.EditorTools
{
    /// <summary>
    /// 도트 스프라이트시트 → 몬스터 프리팹 생성기 (Lv2ContentPlan 5.3~5.4).
    /// 기존 몬스터 프리팹을 템플릿으로 복제해 Behavior Tree·자식 구조·스크립트를 그대로 쓰고,
    /// 애니메이션은 템플릿 컨트롤러의 AnimatorOverrideController로 클립만 교체한다.
    /// 스케일은 템플릿 루트 스케일을 유지하고 PPU로 화면 크기를 맞춘다(자식·감지 범위가 템플릿과 동일하게 유지됨).
    /// </summary>
    public static class MonsterBuilder
    {
        public class Spec
        {
            public string Name;              // 새 프리팹 이름 (예: CliffGoblin_BD)
            public string Template;          // 템플릿 프리팹 이름 (예: RatThief_BD)
            public string SheetFolder;       // 스프라이트시트 폴더
            public float WorldPxPerUnit = 24f; // 목표 텍셀 밀도 (Lv2ContentPlan 1.3)
            public int FrameHeight = 150;
            public int FeetRowFromTop = 101; // 1프레임 알파 bbox 하단(위에서부터)
            public Vector2 BodyPx = new Vector2(33, 36);
            public bool FlipX;
            public float Fps = 12f;
            public int MaxHp = 100;
            public int Def = 1;
            public bool SuperArmor;
            public bool Boss;
            // 템플릿 클립 이름 → 시트 파일 이름(확장자 제외). 시트는 이름_0.. 슬라이스 또는 Multiple
            public Dictionary<string, string> ClipMap = new Dictionary<string, string>();
            public string DeathSheet = "Death";
        }

        const string PrefabDir = "Assets/Prefab/Enemy_BD/";
        const string AnimRoot = "Assets/Sprite/Enemy/Lv2/";

        public static string Build(Spec s)
        {
            var log = new StringBuilder();
            GameObject template = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabDir + s.Template + ".prefab");
            if (template == null)
            {
                return "템플릿 없음: " + s.Template;
            }
            float templateScale = Mathf.Abs(template.transform.localScale.x);
            float ppu = s.WorldPxPerUnit * templateScale;
            var tbox = template.GetComponent<BoxCollider2D>();
            float feetLocal = tbox != null ? tbox.offset.y - tbox.size.y / 2f : -1f;
            int feetRowFromBottom = s.FrameHeight - s.FeetRowFromTop;
            float pivotY = (feetRowFromBottom - feetLocal * ppu) / s.FrameHeight;
            log.Append(s.Name + ": ppu=" + ppu.ToString("F2") + " pivotY=" + pivotY.ToString("F3") + " ");

            // 1) 시트 임포트 설정: PPU + 커스텀 피벗(발이 템플릿 발 위치에 오도록)
            foreach (string sheet in s.ClipMap.Values.Concat(new[] { s.DeathSheet }).Distinct())
            {
                ConfigureSheet(SheetPath(s, sheet), ppu, pivotY);
            }

            // 2) 클립 생성
            string animDir = AnimRoot + s.Name.Replace("_BD", "") + "/";
            Directory.CreateDirectory(animDir);
            var clips = new Dictionary<string, AnimationClip>();
            foreach (var kv in s.ClipMap)
            {
                bool loop = !kv.Key.ToLower().Contains("attack");
                clips[kv.Key] = MakeClip(SheetPath(s, kv.Value), animDir + s.Name.Replace("_BD", "") + "_" + kv.Value.Replace(" ", "") + ".anim", s.Fps, loop);
            }

            // 3) Override Controller
            var tAnim = template.GetComponent<Animator>();
            var baseCtrl = tAnim.runtimeAnimatorController;
            var ovr = new AnimatorOverrideController(baseCtrl) { name = s.Name.Replace("_BD", "") + "_Override" };
            var pairs = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            ovr.GetOverrides(pairs);
            for (int i = 0; i < pairs.Count; i++)
            {
                if (clips.TryGetValue(pairs[i].Key.name, out AnimationClip c) && c != null)
                {
                    pairs[i] = new KeyValuePair<AnimationClip, AnimationClip>(pairs[i].Key, c);
                }
                else
                {
                    log.Append("[미매핑 " + pairs[i].Key.name + "] ");
                }
            }
            ovr.ApplyOverrides(pairs);
            string ovrPath = animDir + ovr.name + ".overrideController";
            AssetDatabase.DeleteAsset(ovrPath);
            AssetDatabase.CreateAsset(ovr, ovrPath);

            // 4) 프리팹 복제·수정
            string prefabPath = PrefabDir + s.Name + ".prefab";
            if (!File.Exists(prefabPath))
            {
                AssetDatabase.CopyAsset(PrefabDir + s.Template + ".prefab", prefabPath);
            }
            GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                root.name = s.Name;
                root.GetComponent<Animator>().runtimeAnimatorController = ovr;
                var sr = root.GetComponent<SpriteRenderer>();
                Sprite first = FirstSprite(SheetPath(s, s.ClipMap.Values.First()));
                if (sr != null)
                {
                    sr.sprite = first;
                    sr.flipX = s.FlipX;
                }
                var box = root.GetComponent<BoxCollider2D>();
                if (box != null)
                {
                    box.size = new Vector2(s.BodyPx.x / ppu, s.BodyPx.y / ppu);
                    box.offset = new Vector2(0f, feetLocal + box.size.y / 2f);
                }
                var eh = root.GetComponent<EnemyHealth>();
                if (eh != null)
                {
                    eh.maxHP = s.MaxHp;
                    eh.nowHP = s.MaxHp;
                    eh.enemyDef = s.Def;
                    eh.isSuperArmor = s.SuperArmor;
                    eh.isBossMob = s.Boss;
                }
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }

            // 5) 사망 연출 프리팹: 템플릿과 무관한 단순 구조 (Death 클립 재생 → DeadBody 디졸브 → 반환)
            string deadPath = BuildDeadBody(s, ppu, pivotY, animDir);
            log.Append("prefab=" + prefabPath + " dead=" + deadPath);
            AssetDatabase.SaveAssets();
            return log.ToString();
        }

        static string SheetPath(Spec s, string sheet) => s.SheetFolder.TrimEnd('/') + "/" + sheet + ".png";

        static void ConfigureSheet(string path, float ppu, float pivotY)
        {
            var imp = AssetImporter.GetAtPath(path) as TextureImporter;
            if (imp == null)
            {
                Debug.LogWarning("[PB][Build] 시트 없음: " + path);
                return;
            }
            imp.spritePixelsPerUnit = ppu;
            imp.filterMode = FilterMode.Point;
            imp.textureCompression = TextureImporterCompression.Uncompressed;
            imp.mipmapEnabled = false;
#pragma warning disable 618
            var metas = imp.spritesheet;
            for (int i = 0; i < metas.Length; i++)
            {
                metas[i].alignment = (int)SpriteAlignment.Custom;
                metas[i].pivot = new Vector2(0.5f, pivotY);
            }
            imp.spritesheet = metas;
#pragma warning restore 618
            imp.SaveAndReimport();
        }

        static List<Sprite> Sprites(string path)
        {
            return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>()
                .OrderBy(sp => { var m = Regex.Match(sp.name, @"(\d+)$"); return m.Success ? int.Parse(m.Value) : 0; })
                .ToList();
        }

        static Sprite FirstSprite(string path) => Sprites(path).FirstOrDefault();

        static AnimationClip MakeClip(string sheetPath, string clipPath, float fps, bool loop)
        {
            List<Sprite> sprites = Sprites(sheetPath);
            if (sprites.Count == 0)
            {
                Debug.LogWarning("[PB][Build] 스프라이트 없음: " + sheetPath);
                return null;
            }
            var clip = new AnimationClip { frameRate = fps };
            var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
            var keys = new ObjectReferenceKeyframe[sprites.Count + 1];
            for (int i = 0; i < sprites.Count; i++)
            {
                keys[i] = new ObjectReferenceKeyframe { time = i / fps, value = sprites[i] };
            }
            keys[sprites.Count] = new ObjectReferenceKeyframe { time = sprites.Count / fps, value = sprites[sprites.Count - 1] };
            AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            AssetDatabase.DeleteAsset(clipPath);
            AssetDatabase.CreateAsset(clip, clipPath);
            return clip;
        }

        static string BuildDeadBody(Spec s, float ppu, float pivotY, string animDir)
        {
            string path = "Assets/Prefab/Enemy/Lv2/" + s.Name.Replace("_BD", "") + "DeadBody.prefab";
            Directory.CreateDirectory("Assets/Prefab/Enemy/Lv2");
            AnimationClip death = MakeClip(SheetPath(s, s.DeathSheet), animDir + s.Name.Replace("_BD", "") + "_DeathOnce.anim", s.Fps, false);
            var ctrl = AnimatorController.CreateAnimatorControllerAtPath(animDir + s.Name.Replace("_BD", "") + "_Death.controller");
            ctrl.layers[0].stateMachine.AddState("Death").motion = death;

            var root = new GameObject(s.Name.Replace("_BD", "") + "DeadBody");
            root.AddComponent<DeadBody>();
            var body = new GameObject("Body");
            body.transform.SetParent(root.transform, false);
            float templateScale = Mathf.Abs(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabDir + s.Template + ".prefab").transform.localScale.x);
            body.transform.localScale = Vector3.one * templateScale;
            var sr = body.AddComponent<SpriteRenderer>();
            sr.sprite = death != null ? FirstSprite(SheetPath(s, s.DeathSheet)) : null;
            sr.flipX = s.FlipX;
            var tsr = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabDir + s.Template + ".prefab").GetComponent<SpriteRenderer>();
            if (tsr != null)
            {
                sr.sortingLayerID = tsr.sortingLayerID;
                sr.sortingOrder = tsr.sortingOrder;
            }
            body.AddComponent<Animator>().runtimeAnimatorController = ctrl;
            var rb = body.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic; // 시체는 제자리에서 Death 애니 → 디졸브
            PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return path;
        }
    }
}
#endif
