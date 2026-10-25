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

        ImportWeapons();
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
    /// <summary>
    /// 6포즈 이동 주기(끊기는 포즈). 배열은 포즈별 픽셀값. 배낭은 엉덩이가 올라올 때 한 포즈 늦게 따라온다(상대값 = min(0, 이전 - 현재)).
    /// </summary>
    private static AnimDef Cycle(string name, float frame, float blade, float[] legFx, float[] legFy, float[] legBx, float[] legBy, float[] hipsY, float[] armFx, float hipsX)
    {
        int n = legFx.Length;
        var def = new AnimDef(name, frame * n, true).Step();
        def.K("LegF", 'x', Keys(legFx, frame)).K("LegF", 'y', Keys(legFy, frame))
            .K("LegB", 'x', Keys(legBx, frame)).K("LegB", 'y', Keys(legBy, frame))
            .K("Hips", 'y', Keys(hipsY, frame)).K("Hips", 'x', Keys(Fill(n, hipsX), frame));
        var armB = new float[n];
        var pack = new float[n];
        for (int i = 0; i < n; i++)
        {
            armB[i] = -armFx[i];
            // 아래로만 처진다(위로 밀리면 배낭 윗선이 머리카락 위로 삐져나왔다): 몸이 올라오는 포즈에서 한 박자 늦게 1px 처짐
            pack[i] = Mathf.Min(0f, hipsY[(i + n - 1) % n] - hipsY[i]);
        }
        def.K("Hips/ShoulderF", 'x', Keys(armFx, frame)).K("Hips/ShoulderB", 'x', Keys(armB, frame))
            .K("Hips/Pack", 'y', Keys(pack, frame))
            .Arm(0f, 0f, blade, frame * n, 0f, blade);
        return def;
    }

    /// <summary>한 포즈짜리 클립(끊김). 값은 (x, y) 픽셀, arms 는 양 어깨 같은 이동, blade 는 앞팔 칼날 각도</summary>
    private static AnimDef Pose(string name, float len, float blade, (float x, float y) hips = default, (float x, float y) legF = default,
        (float x, float y) legB = default, (float x, float y) arms = default, float pack = 0f)
    {
        var def = new AnimDef(name, len, false).Step();
        def.K("Hips", 'x', 0f, hips.x, len, hips.x).K("Hips", 'y', 0f, hips.y, len, hips.y)
            .K("LegF", 'x', 0f, legF.x, len, legF.x).K("LegF", 'y', 0f, legF.y, len, legF.y)
            .K("LegB", 'x', 0f, legB.x, len, legB.x).K("LegB", 'y', 0f, legB.y, len, legB.y)
            .K("Hips/ShoulderF", 'x', 0f, arms.x, len, arms.x).K("Hips/ShoulderF", 'y', 0f, arms.y, len, arms.y)
            .K("Hips/ShoulderB", 'x', 0f, arms.x, len, arms.x).K("Hips/ShoulderB", 'y', 0f, arms.y, len, arms.y)
            .K("Hips/Pack", 'y', 0f, pack, len, pack)
            .Arm(0f, 0f, blade, len, 0f, blade);
        return def;
    }

    /// <summary>포즈 배열 → (시간, 값) 쌍. 마지막에 첫 포즈를 한 번 더 붙여 고리를 닫는다</summary>
    private static float[] Keys(float[] poses, float frame)
    {
        var k = new float[(poses.Length + 1) * 2];
        for (int i = 0; i <= poses.Length; i++)
        {
            k[i * 2] = i * frame;
            k[i * 2 + 1] = poses[i % poses.Length];
        }
        return k;
    }

    private static float[] Fill(int n, float v)
    {
        var a = new float[n];
        for (int i = 0; i < n; i++)
        {
            a[i] = v;
        }
        return a;
    }

    private static List<AnimDef> Clips()
    {
        return new List<AnimDef>
        {
            // 이동 동작 = 끊기는 포즈(Step: 보간 없음) + 팔은 회전 없이 1px 이동. 10-25 사용자 "걷기·점프가 너무 이상하다":
            // 팔을 돌리면 가는 팔이 대(大)자로 뻗고 회전 도트가 지글거렸다, 부드러운 보간은 다리가 1px 사이를 미끄러졌다.
            // 대기: 숨쉬기 1px, 길게 쉬고 짧게 넘어간다(첫 포즈를 길게 — 레퍼런스 250/150ms 리듬), 배낭은 한 박자 늦게 따라온다
            new AnimDef("Idle", 1.3f, true).Step()
                .K("Hips", 'y', 0f, 0f, 0.6f, -1f, 1.1f, 0f, 1.3f, 0f)
                .K("Hips/Pack", 'y', 0f, 0f, 1.1f, -1f, 1.22f, 0f, 1.3f, 0f)
                .Arm(0f, 0f, 45f, 1.3f, 0f, 45f),
            // 걷기 6포즈(0.09초씩): 접지 A → 내려앉음 → 교차(뒷발 듦) → 접지 B → 내려앉음 → 교차(앞발 듦).
            // 보폭 ±2px, 팔은 반대 다리와 엇갈려 1px, 배낭은 몸보다 한 박자 늦게(2차 움직임)
            Cycle("Walk", 0.09f, 45f,
                new float[] { 2, 1, 0, -2, -1, 0 }, new float[] { 0, 0, 0, 0, 0, 1 },
                new float[] { -2, -1, 0, 2, 1, 0 }, new float[] { 0, 0, 1, 0, 0, 0 },
                new float[] { 0, -1, 0, 0, -1, 0 }, new float[] { -1, -1, 0, 1, 1, 0 }, 0f),
            // 달리기 6포즈(0.07초씩): 교차 때 발을 2px 들고 몸은 1px 앞으로. 칼은 앞 위 45°(뒤로 눕히면 손이 몸 앞이라 칼날이 얼굴·가슴을 가로질렀다)
            Cycle("Run", 0.07f, 45f,
                new float[] { 2, 1, -1, -2, -1, 1 }, new float[] { 0, 0, 0, 0, 1, 2 },
                new float[] { -2, -1, 1, 2, 1, -1 }, new float[] { 0, 1, 2, 0, 0, 0 },
                new float[] { 0, -1, 0, 0, -1, 0 }, new float[] { -1, -1, 0, 1, 1, 0 }, 1f),
            // 점프: 리그(PixelRig)가 세로 속도로 고른다 — 도약(첫 0.06초 늘임) → 상승 → 꼭대기 → 낙하, 착지하면 0.1초 눌림(Land).
            // 변형은 한두 프레임만(셀레스트식), 1px 단위
            Pose("Jump", 0.1f, 90f, legF: (1, 1), legB: (-1, 2), arms: (0, 1)),
            Pose("JumpLaunch", 0.06f, 90f, hips: (0, 1), arms: (0, 1)),
            Pose("JumpApex", 0.1f, 45f, legF: (1, 2), legB: (0, 2), arms: (0, 1)),
            Pose("JumpFall", 0.1f, 90f, legF: (1, 0), legB: (-1, 1), arms: (0, 2)),
            // 피격: 몸이 뒤로 1px 젖혀지고 팔이 들리며 칼은 뒤로(리그가 피격 신호 뒤 0.22초 보여 준다)
            Pose("Hurt", 0.1f, 135f, hips: (-1, 0), legF: (-1, 0), legB: (-2, 0), arms: (-1, 1), pack: -1),
            // 사망 1단계: 무릎 꿇음(몸 2px 내려앉고 칼끝 아래로). 2단계(뒤로 쓰러짐)는 리그가 몸 전체를 90° 눕힌다
            Pose("DeathKneel", 0.1f, -90f, hips: (0, -2), legF: (1, 0), legB: (-1, 0)),
            Pose("Land", 0.1f, 45f, hips: (0, -1), legF: (1, 0), legB: (-1, 0)), // 팔·배낭은 몸통과 같이(더 내리면 뒷어깨·배낭 사이가 비었다, find_rig_holes)
            // 대시 1포즈: 몸 2px 앞으로, 다리 뒤로 끌림, 팔 1px 뒤로, 칼 뒤로 눕힘
            new AnimDef("Dash", 0.1f, false).Step()
                .K("Hips", 'x', 0f, 2f, 0.1f, 2f)
                .K("LegF", 'x', 0f, -1f, 0.1f, -1f)
                .K("LegB", 'x', 0f, -2f, 0.1f, -2f)
                .K("LegB", 'y', 0f, 1f, 0.1f, 1f)
                .K("Hips/ShoulderF", 'x', 0f, -1f, 0.1f, -1f)
                .K("Hips/ShoulderB", 'x', 0f, -1f, 0.1f, -1f)
                .Arm(0f, 0f, 180f, 0.1f, 0f, 180f),
            // 로프: 팔은 정확히 180°(머리 위로 곧게 — 어중간한 각도는 도트가 지글거렸다), 오를 때는 팔·다리 1~2px 번갈아
            new AnimDef("RopeStay", 0.6f, true).Step()
                .Arm(0f, 180f, 90f, 0.6f, 180f, 90f)
                .K("Hips/ShoulderB", 'r', 0f, 180f, 0.6f, 180f)
                .K("LegF", 'y', 0f, 1f, 0.6f, 1f),
            new AnimDef("RopeUp", 0.4f, true).Step()
                .Arm(0f, 180f, 90f, 0.4f, 180f, 90f)
                .K("Hips/ShoulderB", 'r', 0f, 180f, 0.4f, 180f)
                .K("Hips/ShoulderF", 'y', 0f, 1f, 0.2f, 0f, 0.4f, 1f)
                .K("Hips/ShoulderB", 'y', 0f, 0f, 0.2f, 1f, 0.4f, 0f)
                .K("LegF", 'y', 0f, 2f, 0.2f, 0f, 0.4f, 2f)
                .K("LegB", 'y', 0f, 0f, 0.2f, 2f, 0.4f, 0f),
            // 공격 = 끊기는 핵심 포즈(예비 → 타격 → 이어짐 → 회복), 각도는 45° 배수만(도트 칼·팔이 깔끔하게).
            // 속도감은 베기 궤적 이펙트가 채운다. Arm(시간, 팔 각도, 칼날 각도) — 칼날 0 = 앞, 90 = 위, 180 = 뒤, -45 = 앞 아래
            new AnimDef("Stab", 0.36f, false).Step()
                .Arm(0f, -45f, 180f, 0.07f, 90f, 0f, 0.2f, 90f, 0f, 0.3f, 45f, 45f, 0.36f, 45f, 45f)
                .K("Hips/ShoulderB", 'r', 0f, 45f, 0.07f, -45f, 0.3f, 0f, 0.36f, 0f)
                .K("Hips", 'x', 0f, -1f, 0.07f, 2f, 0.2f, 1f, 0.3f, 0f, 0.36f, 0f)
                .K("LegF", 'x', 0f, 0f, 0.07f, 2f, 0.3f, 1f, 0.36f, 1f)
                .K("LegB", 'x', 0f, 0f, 0.07f, -1f, 0.3f, 0f, 0.36f, 0f),
            new AnimDef("SwingDown", 0.38f, false).Step()
                .Arm(0f, 180f, 135f, 0.07f, 90f, 0f, 0.13f, 45f, -45f, 0.28f, 45f, 0f, 0.38f, 45f, 45f)
                .K("Hips/ShoulderB", 'r', 0f, -45f, 0.07f, 0f, 0.38f, 0f)
                .K("Hips", 'x', 0f, -1f, 0.07f, 1f, 0.13f, 1f, 0.28f, 0f, 0.38f, 0f)
                .K("Hips", 'y', 0f, 0f, 0.07f, 0f, 0.13f, -1f, 0.28f, 0f, 0.38f, 0f)
                .K("LegF", 'x', 0f, 0f, 0.07f, 2f, 0.28f, 1f, 0.38f, 1f),
            new AnimDef("SwingUp", 0.38f, false).Step()
                .Arm(0f, 0f, -45f, 0.07f, 90f, 45f, 0.13f, 180f, 90f, 0.28f, 135f, 90f, 0.38f, 45f, 45f)
                .K("Hips/ShoulderB", 'r', 0f, 0f, 0.07f, -45f, 0.28f, 0f, 0.38f, 0f)
                .K("Hips", 'y', 0f, -1f, 0.07f, 0f, 0.13f, 1f, 0.28f, 0f, 0.38f, 0f)
                .K("LegF", 'x', 0f, 0f, 0.07f, 1f, 0.28f, 0f, 0.38f, 0f),
            new AnimDef("SwingSide", 0.38f, false).Step()
                .Arm(0f, -45f, 180f, 0.07f, 90f, 0f, 0.13f, 90f, -45f, 0.28f, 45f, 0f, 0.38f, 45f, 45f)
                .K("Hips/ShoulderB", 'r', 0f, 45f, 0.07f, -45f, 0.28f, 0f, 0.38f, 0f)
                .K("Hips", 'x', 0f, -1f, 0.07f, 2f, 0.13f, 2f, 0.28f, 1f, 0.38f, 0f)
                .K("LegF", 'x', 0f, 0f, 0.07f, 2f, 0.28f, 1f, 0.38f, 1f)
                .K("LegB", 'x', 0f, 0f, 0.07f, -1f, 0.28f, 0f, 0.38f, 0f),
            // 활 쏘기(활을 쥐면 리그가 Cast 대신 이것): 앞팔로 활을 앞에 세워 들고(활은 똑바로 선다), 뒷팔이 시위를 2px 당겼다 놓으며 몸 1px 반동
            new AnimDef("Shoot", 0.42f, false).Step()
                .Arm(0f, 90f, 90f, 0.42f, 90f, 90f)
                .K("Hips/ShoulderB", 'r', 0f, 90f, 0.1f, 90f, 0.24f, 90f, 0.42f, 45f)
                .K("Hips/ShoulderB", 'x', 0f, 0f, 0.1f, -2f, 0.24f, 0f, 0.42f, 0f)
                .K("Hips", 'x', 0f, 0f, 0.24f, -1f, 0.32f, 0f, 0.42f, 0f),
            // 막기(ShieldUp 스킬): 몸 1px 낮추고 뒷팔(방패)을 앞으로 90°, 앞팔은 칼을 위로 세워 대기
            new AnimDef("Guard", 0.45f, false).Step()
                .Arm(0f, 45f, 90f, 0.45f, 45f, 90f)
                .K("Hips/ShoulderB", 'r', 0f, 90f, 0.45f, 90f)
                .K("Hips", 'y', 0f, -1f, 0.45f, -1f)
                .K("Hips", 'x', 0f, 1f, 0.45f, 1f)
                .K("LegF", 'x', 0f, 1f, 0.45f, 1f)
                .K("LegB", 'x', 0f, -1f, 0.45f, -1f),
            // 회전베기(Spining 스킬): 팔이 앞 → 위 → 뒤를 90°씩 두 번(칼날은 팔 방향), 아래 방향은 건너뛴다(칼끝이 땅을 뚫었다), 끝에 대기 자세로
            new AnimDef("Spin", 0.45f, false).Step()
                .Arm(0f, 90f, 0f, 0.06f, 180f, 90f, 0.12f, 270f, 180f, 0.18f, 450f, 360f, 0.24f, 540f, 450f, 0.3f, 630f, 540f,
                     0.36f, 810f, 720f, 0.45f, 720f, 765f)
                .K("Hips", 'y', 0f, -1f, 0.4f, -1f, 0.45f, 0f),
            new AnimDef("Cast", 0.45f, false).Step()
                .Arm(0f, 90f, 90f, 0.1f, 180f, 90f, 0.45f, 180f, 90f)
                .K("Hips/ShoulderB", 'r', 0f, 0f, 0.1f, -45f, 0.45f, -45f)
                .K("Hips", 'y', 0f, 0f, 0.1f, 1f, 0.45f, 1f),
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
            SetCurve(clip, bone, "m_LocalPosition.x", def.Get(bone, 'x'), r.x, 1f / Ppu, def.Length, def.Stepped);
            SetCurve(clip, bone, "m_LocalPosition.y", def.Get(bone, 'y'), r.y, 1f / Ppu, def.Length, def.Stepped);
            SetCurve(clip, bone, "localEulerAnglesRaw.z", def.Get(bone, 'r'), 0f, 1f, def.Length, def.Stepped);
        }
        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = def.Loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        EditorUtility.SetDirty(clip);
        return clip;
    }

    private static void SetCurve(AnimationClip clip, string bone, string prop, float[] keys, float baseValue, float scale, float length, bool stepped)
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
            var mode = stepped ? AnimationUtility.TangentMode.Constant : AnimationUtility.TangentMode.Linear;
            AnimationUtility.SetKeyLeftTangentMode(curve, i, mode);
            AnimationUtility.SetKeyRightTangentMode(curve, i, mode);
        }
        clip.SetCurve(bone, typeof(Transform), prop, curve);
    }

    private class AnimDef
    {
        public readonly string Name;
        public readonly float Length;
        public readonly bool Loop;
        private readonly Dictionary<string, float[]> _keys = new Dictionary<string, float[]>();
        public bool Stepped { get; private set; }

        /// <summary>보간 없이 키 포즈를 그대로 유지(도트 이동 동작은 끊기는 포즈가 자연스럽다)</summary>
        public AnimDef Step()
        {
            Stepped = true;
            return this;
        }

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

    /// <summary>도트 무기 그림(Tools/pixelfx/draw_weapon_pixel.py 산출) 임포트: 24 PPU, 점 필터, 피벗 = json 의 손잡이 픽셀 중심</summary>
    private static void ImportWeapons()
    {
        const string dir = "Assets/Resources/PixelRig/Weapons";
        if (!Directory.Exists(dir))
        {
            return;
        }
        foreach (string json in Directory.GetFiles(dir, "*.json"))
        {
            string png = Path.ChangeExtension(json, ".png").Replace('\\', '/');
            var meta = JsonUtility.FromJson<WeaponMeta>(File.ReadAllText(json));
            var ti = AssetImporter.GetAtPath(png) as TextureImporter;
            if (ti == null)
            {
                continue;
            }
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.spritePixelsPerUnit = 24f;
            ti.filterMode = FilterMode.Point;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.mipmapEnabled = false;
            ti.alphaIsTransparency = true;
            var settings = new TextureImporterSettings();
            ti.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spriteMeshType = SpriteMeshType.FullRect;
            ti.SetTextureSettings(settings);
            ti.spritePivot = new Vector2((meta.grip[0] + 0.5f) / meta.size[0], 1f - (meta.grip[1] + 0.5f) / meta.size[1]);
            ti.SaveAndReimport();
        }
    }

    [System.Serializable]
    private class WeaponMeta
    {
        public int[] grip;
        public int[] size;
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
