using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 새 주인공 픽셀 리그 프리팹·애니 클립 생성기 (PixelRigDesign.md).
/// 입력: Assets/Art/Player/PixelRig/Adventurer/ 의 부위 PNG + pivots.json (Tools/pixelfx/split_rig_parts.py 산출물).
/// 출력: Assets/Resources/PixelRig/Adventurer.prefab, Assets/Art/Player/PixelRig/Anims/*.anim.
/// 애니를 고치려면 아래 Clips() 표의 값(픽셀·도)을 바꾸고 다시 굽는다: PixelRigBuilder.BuildAdventurer().
/// </summary>
public static class PixelRigBuilder
{
    private const string ArtDir = "Assets/Art/Player/PixelRig/Adventurer";
    private const string AnimDir = "Assets/Art/Player/PixelRig/Anims";
    private const string PrefabPath = "Assets/Resources/PixelRig/Adventurer.prefab";
    private const float Ppu = 36f;
    private const int Size = 64;
    private const int FeetRow = 55; // 발바닥 = 54행 아래 경계 → 리그 원점
    // 무기 소켓 배율 — 예전 AnyPortrait 루트(3)보다 조금 작게: 새 주인공 키(2유닛)에 칼 길이가 맞도록
    private const float WeaponScale = 2.4f;

    [System.Serializable]
    private class PivotFile
    {
        public int[] size;
        public bool mirror;
        public PivotMap pivots;
    }

    [System.Serializable]
    private class PivotMap
    {
        public int[] hips, pack, head, shoulder_l, hand_l, shoulder_r, hand_r, leg_l, leg_r;
    }

    [MenuItem("Tools/ProjectB/Pixel Rig/Build Adventurer")]
    public static string BuildAdventurer()
    {
        PivotMap pv = JsonUtility.FromJson<PivotFile>(File.ReadAllText(ArtDir + "/pivots.json")).pivots;
        int ox = pv.hips[0];

        ImportPart("pack", pv.pack);
        ImportPart("torso", pv.hips);
        ImportPart("head", pv.head);
        ImportPart("arm_l", pv.shoulder_l);
        ImportPart("arm_r", pv.shoulder_r);
        ImportPart("leg_l", pv.leg_l);
        ImportPart("leg_r", pv.leg_r);

        GameObject root = new GameObject("Adventurer");
        var bones = new List<Transform>();
        var renderers = new List<SpriteRenderer>();

        Transform legB = Bone("LegB", root.transform, pv.leg_r, null, ox, bones);
        Transform legF = Bone("LegF", root.transform, pv.leg_l, null, ox, bones);
        Transform hips = Bone("Hips", root.transform, pv.hips, null, ox, bones);
        Transform pack = Bone("Pack", hips, pv.pack, pv.hips, ox, bones);
        Transform head = Bone("Head", hips, pv.head, pv.hips, ox, bones);
        Transform shB = Bone("ShoulderB", hips, pv.shoulder_r, pv.hips, ox, bones);
        Transform handB = Bone("HandB", shB, pv.hand_r, pv.shoulder_r, ox, bones);
        Transform shF = Bone("ShoulderF", hips, pv.shoulder_l, pv.hips, ox, bones);
        Transform handF = Bone("HandF", shF, pv.hand_l, pv.shoulder_l, ox, bones);

        renderers.Add(Part(pack, "pack", 36));
        renderers.Add(Part(shB, "arm_r", 38)); // 팔 = 소매+손, 어깨 위쪽을 축으로 돈다(손만 떼면 공중에 떠 보였다)
        renderers.Add(Part(legB, "leg_r", 39));
        renderers.Add(Part(legF, "leg_l", 40));
        renderers.Add(Part(hips, "torso", 42));
        renderers.Add(Part(head, "head", 44));
        renderers.Add(Part(shF, "arm_l", 46));

        // 무기 소켓: 기존 무기 스프라이트는 "주먹에 수직으로 세운" 기준(소켓 회전 0 = 칼날 위)
        Transform socketR = Socket("RightWeapon", handF);
        Transform socketL = Socket("LeftWeapon", handB);

        PixelRig rig = root.AddComponent<PixelRig>();
        var so = new SerializedObject(rig);
        SetArray(so.FindProperty("_renderers"), renderers.ToArray());
        SetArray(so.FindProperty("_bones"), bones.ToArray());
        so.FindProperty("_socketRight").objectReferenceValue = socketR;
        so.FindProperty("_socketLeft").objectReferenceValue = socketL;
        so.FindProperty("_ppu").floatValue = Ppu;
        so.FindProperty("_weaponOrderFront").intValue = 45;
        so.FindProperty("_weaponOrderBack").intValue = 37;

        // 클립 굽기 — 정지 자세 값을 먼저 읽어 두고, 표의 값은 그 위에 더한다
        var rest = new Dictionary<string, Vector3>();
        foreach (Transform b in bones)
        {
            rest[AnimationUtility.CalculateTransformPath(b, root.transform)] = b.localPosition;
        }
        Directory.CreateDirectory(AnimDir);
        List<AnimDef> defs = Clips();
        SerializedProperty clips = so.FindProperty("_clips");
        clips.arraySize = defs.Count;
        for (int i = 0; i < defs.Count; i++)
        {
            AnimationClip clip = Bake(defs[i], rest);
            SerializedProperty e = clips.GetArrayElementAtIndex(i);
            e.FindPropertyRelative("name").stringValue = defs[i].Name;
            e.FindPropertyRelative("clip").objectReferenceValue = clip;
            e.FindPropertyRelative("loop").boolValue = defs[i].Loop;
        }
        so.ApplyModifiedPropertiesWithoutUndo();

        Directory.CreateDirectory(Path.GetDirectoryName(PrefabPath));
        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);
        AssetDatabase.SaveAssets();
        return "PixelRig built: " + defs.Count + " clips, " + bones.Count + " bones";
    }

    // ── 키프레임 표 ───────────────────────────────────────────────────────────
    // 뼈: Hips, Hips/Head, Hips/Pack, Hips/ShoulderF, Hips/ShoulderB, LegF, LegB
    // 채널: x·y = 아트 픽셀(오른쪽·위가 +), r = 도(반시계 +). 몸통(Hips)·머리는 돌리지 않는다 — 도트 통째 회전은 외곽이 지글거린다(위치 이동으로 표현). 캐릭터는 오른쪽을 본다 → 어깨 r + = 팔을 앞으로 든다.
    private static List<AnimDef> Clips()
    {
        return new List<AnimDef>
        {
            new AnimDef("Idle", 1.2f, true)
                .K("Hips", 'y', 0f, 0f, 0.6f, -1f, 1.2f, 0f)
                .Arm(0f, 0f, 60f, 0.6f, 3f, 60f, 1.2f, 0f, 60f)
                .K("Hips/ShoulderB", 'r', 0f, 0f, 0.6f, -3f, 1.2f, 0f),
            new AnimDef("Walk", 0.5f, true)
                .K("LegF", 'x', 0f, 1f, 0.25f, -1f, 0.5f, 1f)
                .K("LegF", 'y', 0f, 0f, 0.125f, 1f, 0.25f, 0f, 0.5f, 0f)
                .K("LegB", 'x', 0f, -1f, 0.25f, 1f, 0.5f, -1f)
                .K("LegB", 'y', 0f, 0f, 0.25f, 0f, 0.375f, 1f, 0.5f, 0f)
                .K("Hips", 'y', 0f, 0f, 0.125f, -1f, 0.25f, 0f, 0.375f, -1f, 0.5f, 0f)
                .Arm(0f, -10f, 60f, 0.25f, 10f, 60f, 0.5f, -10f, 60f)
                .K("Hips/ShoulderB", 'r', 0f, 10f, 0.25f, -10f, 0.5f, 10f),
            new AnimDef("Run", 0.36f, true)
                .K("LegF", 'x', 0f, 2f, 0.18f, -2f, 0.36f, 2f)
                .K("LegF", 'y', 0f, 0f, 0.09f, 2f, 0.18f, 0f, 0.36f, 0f)
                .K("LegB", 'x', 0f, -2f, 0.18f, 2f, 0.36f, -2f)
                .K("LegB", 'y', 0f, 0f, 0.18f, 0f, 0.27f, 2f, 0.36f, 0f)
                .K("Hips", 'y', 0f, 0f, 0.09f, -1f, 0.18f, 0f, 0.27f, -1f, 0.36f, 0f)
                .K("Hips", 'x', 0f, 1f, 0.36f, 1f)
                .Arm(0f, -20f, 150f, 0.18f, -5f, 150f, 0.36f, -20f, 150f)
                .K("Hips/ShoulderB", 'r', 0f, 20f, 0.18f, -20f, 0.36f, 20f),
            new AnimDef("Jump", 0.2f, false)
                .K("LegF", 'x', 0f, 0f, 0.1f, 1f)
                .K("LegF", 'y', 0f, 0f, 0.1f, 2f)
                .K("LegB", 'x', 0f, 0f, 0.1f, -1f)
                .K("LegB", 'y', 0f, 0f, 0.1f, 3f)
                .Arm(0f, 0f, 60f, 0.1f, 35f, 80f)
                .K("Hips/ShoulderB", 'r', 0f, 0f, 0.1f, -30f),
            new AnimDef("Dash", 0.15f, false)
                .K("Hips", 'x', 0f, 0f, 0.06f, 2f)
                .K("LegF", 'x', 0f, 0f, 0.06f, -1f)
                .K("LegF", 'y', 0f, 0f, 0.06f, 1f)
                .K("LegB", 'x', 0f, 0f, 0.06f, -3f)
                .K("LegB", 'y', 0f, 0f, 0.06f, 1f)
                .Arm(0f, 0f, 60f, 0.06f, -35f, 170f)
                .K("Hips/ShoulderB", 'r', 0f, 0f, 0.06f, -35f),
            new AnimDef("RopeStay", 0.6f, true)
                .Arm(0f, 160f, 90f, 0.6f, 160f, 90f)
                .K("Hips/ShoulderB", 'r', 0f, -160f, 0.6f, -160f)
                .K("LegF", 'y', 0f, 1f, 0.6f, 1f),
            new AnimDef("RopeUp", 0.4f, true)
                .Arm(0f, 150f, 90f, 0.2f, 175f, 90f, 0.4f, 150f, 90f)
                .K("Hips/ShoulderB", 'r', 0f, -175f, 0.2f, -150f, 0.4f, -175f)
                .K("LegF", 'y', 0f, 2f, 0.2f, 0f, 0.4f, 2f)
                .K("LegB", 'y', 0f, 0f, 0.2f, 2f, 0.4f, 0f),
            // 공격: Arm(시간, 팔 각도, 칼날 각도) — 칼날 0 = 앞 수평, 90 = 위, 180 = 뒤, -90 = 아래
            new AnimDef("Stab", 0.36f, false)
                .Arm(0f, 0f, 60f, 0.06f, -40f, 160f, 0.12f, 90f, 0f, 0.3f, 85f, 0f, 0.36f, 60f, 20f)
                .K("Hips/ShoulderB", 'r', 0f, 0f, 0.12f, -30f, 0.36f, -20f)
                .K("Hips", 'x', 0f, 0f, 0.06f, -1f, 0.12f, 2f, 0.36f, 1f)
                .K("LegF", 'x', 0f, 0f, 0.12f, 2f, 0.36f, 1f)
                .K("LegB", 'x', 0f, 0f, 0.12f, -1f, 0.36f, 0f),
            new AnimDef("SwingDown", 0.38f, false)
                .Arm(0f, 0f, 60f, 0.08f, 170f, 120f, 0.16f, 30f, -60f, 0.3f, 20f, -60f, 0.38f, 10f, -40f)
                .K("Hips/ShoulderB", 'r', 0f, 0f, 0.08f, -40f, 0.16f, 20f, 0.38f, 0f)
                .K("Hips", 'x', 0f, 0f, 0.08f, -1f, 0.16f, 1f, 0.38f, 0f)
                .K("Hips", 'y', 0f, 0f, 0.16f, -1f, 0.38f, 0f)
                .K("LegF", 'x', 0f, 0f, 0.16f, 2f, 0.38f, 1f),
            new AnimDef("SwingUp", 0.38f, false)
                .Arm(0f, 0f, 60f, 0.06f, -30f, -100f, 0.16f, 165f, 100f, 0.38f, 150f, 90f)
                .K("Hips/ShoulderB", 'r', 0f, 0f, 0.16f, -30f, 0.38f, -20f)
                .K("Hips", 'y', 0f, 0f, 0.16f, 1f, 0.38f, 0f)
                .K("LegF", 'x', 0f, 0f, 0.16f, 1f, 0.38f, 0f),
            new AnimDef("SwingSide", 0.38f, false)
                .Arm(0f, 0f, 60f, 0.08f, -60f, 180f, 0.18f, 100f, 0f, 0.38f, 95f, 0f)
                .K("Hips/ShoulderB", 'r', 0f, 0f, 0.18f, -40f, 0.38f, -30f)
                .K("Hips", 'x', 0f, 0f, 0.18f, 2f, 0.38f, 1f)
                .K("LegF", 'x', 0f, 0f, 0.18f, 2f, 0.38f, 1f)
                .K("LegB", 'x', 0f, 0f, 0.18f, -1f, 0.38f, 0f),
            new AnimDef("Cast", 0.45f, false)
                .Arm(0f, 0f, 60f, 0.15f, 120f, 90f, 0.45f, 115f, 90f)
                .K("Hips/ShoulderB", 'r', 0f, 0f, 0.15f, -100f, 0.45f, -95f)
                .K("Hips", 'y', 0f, 0f, 0.15f, 1f, 0.45f, 1f),
        };
    }

    // ── 굽기 ─────────────────────────────────────────────────────────────────
    private static readonly string[] AnimBones = { "Hips", "Hips/Head", "Hips/Pack", "Hips/ShoulderF", "Hips/ShoulderB", "Hips/ShoulderF/HandF", "LegF", "LegB" };

    private static AnimationClip Bake(AnimDef def, Dictionary<string, Vector3> rest)
    {
        string path = AnimDir + "/" + def.Name + ".anim";
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if (clip == null)
        {
            clip = new AnimationClip();
            AssetDatabase.CreateAsset(clip, path);
        }
        clip.ClearCurves();
        clip.frameRate = 60f;
        foreach (string bone in AnimBones)
        {
            Vector3 r = rest[bone];
            SetCurve(clip, bone, "m_LocalPosition.x", def.Get(bone, 'x'), r.x, 1f / Ppu, def.Length);
            SetCurve(clip, bone, "m_LocalPosition.y", def.Get(bone, 'y'), r.y, 1f / Ppu, def.Length);
            SetCurve(clip, bone, "localEulerAnglesRaw.z", def.Get(bone, 'r'), 0f, 1f, def.Length);
        }
        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = def.Loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        EditorUtility.SetDirty(clip);
        return clip;
    }

    private static void SetCurve(AnimationClip clip, string bone, string prop, float[] keys, float baseValue, float scale, float length)
    {
        var curve = new AnimationCurve();
        if (keys == null)
        {
            curve.AddKey(0f, baseValue);
            curve.AddKey(length, baseValue);
        }
        else
        {
            for (int i = 0; i + 1 < keys.Length; i += 2)
            {
                curve.AddKey(keys[i], baseValue + keys[i + 1] * scale);
            }
        }
        for (int i = 0; i < curve.length; i++)
        {
            AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
            AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
        }
        clip.SetCurve(bone, typeof(Transform), prop, curve);
    }

    private class AnimDef
    {
        public readonly string Name;
        public readonly float Length;
        public readonly bool Loop;
        private readonly Dictionary<string, float[]> _keys = new Dictionary<string, float[]>();

        public AnimDef(string name, float length, bool loop)
        {
            Name = name;
            Length = length;
            Loop = loop;
        }

        /// <summary>(시간, 값) 쌍을 이어 적는다</summary>
        public AnimDef K(string bone, char channel, params float[] timeValue)
        {
            _keys[bone + ":" + channel] = timeValue;
            return this;
        }

        /// <summary>앞팔 (시간, 팔 각도 A, 칼날 각도 B) 세 개씩 — 어깨 r = A, 손목 r = B - 90 - A (무기 그림은 손목 0에서 칼날이 위)</summary>
        public AnimDef Arm(params float[] tab)
        {
            var arm = new float[tab.Length / 3 * 2];
            var wrist = new float[tab.Length / 3 * 2];
            for (int i = 0, j = 0; i + 2 < tab.Length; i += 3, j += 2)
            {
                arm[j] = tab[i];
                arm[j + 1] = tab[i + 1];
                wrist[j] = tab[i];
                wrist[j + 1] = tab[i + 2] - 90f - tab[i + 1];
            }
            _keys["Hips/ShoulderF:r"] = arm;
            _keys["Hips/ShoulderF/HandF:r"] = wrist;
            return this;
        }

        public float[] Get(string bone, char channel)
        {
            float[] v;
            return _keys.TryGetValue(bone + ":" + channel, out v) ? v : null;
        }
    }

    // ── 뼈·부위 ──────────────────────────────────────────────────────────────
    /// <summary>피벗 픽셀(x, y)의 왼쪽 아래 꼭짓점 = 관절점. 리그 원점은 (엉덩이 x, 발바닥 행).</summary>
    private static Vector2 PixelToLocal(int[] p, int ox)
    {
        return new Vector2((p[0] - ox) / Ppu, (FeetRow - (p[1] + 1)) / Ppu);
    }

    private static Transform Bone(string name, Transform parent, int[] pivot, int[] parentPivot, int ox, List<Transform> bones)
    {
        var t = new GameObject(name).transform;
        t.SetParent(parent, false);
        Vector2 pos = PixelToLocal(pivot, ox) - (parentPivot != null ? PixelToLocal(parentPivot, ox) : Vector2.zero);
        t.localPosition = pos;
        bones.Add(t);
        return t;
    }

    private static SpriteRenderer Part(Transform bone, string part, int order)
    {
        var sr = bone.gameObject.AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ArtDir + "/" + part + ".png");
        sr.sortingOrder = order;
        return sr;
    }

    private static Transform Socket(string name, Transform hand)
    {
        var t = new GameObject(name).transform;
        t.SetParent(hand, false);
        t.localScale = new Vector3(WeaponScale, WeaponScale, 1f);
        return t;
    }

    private static void ImportPart(string part, int[] pivot)
    {
        string path = ArtDir + "/" + part + ".png";
        var ti = (TextureImporter)AssetImporter.GetAtPath(path);
        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Single;
        ti.spritePixelsPerUnit = Ppu;
        ti.filterMode = FilterMode.Point;
        ti.textureCompression = TextureImporterCompression.Uncompressed;
        ti.mipmapEnabled = false;
        ti.alphaIsTransparency = true;
        var settings = new TextureImporterSettings();
        ti.ReadTextureSettings(settings);
        settings.spriteAlignment = (int)SpriteAlignment.Custom;
        settings.spriteMeshType = SpriteMeshType.FullRect;
        ti.SetTextureSettings(settings);
        ti.spritePivot = new Vector2(pivot[0] / (float)Size, (Size - (pivot[1] + 1)) / (float)Size);
        ti.SaveAndReimport();
    }

    private static void SetArray<T>(SerializedProperty prop, T[] items) where T : Object
    {
        prop.arraySize = items.Length;
        for (int i = 0; i < items.Length; i++)
        {
            prop.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
        }
    }
}
