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
        ImportPoseParts(pv);

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
        // 셀 이동·점프 그림(Q-423): 평소엔 꺼져 있고, 셀 클립이 재생될 때 리그가 부위 대신 켠다. 부위와 같은 목록이라 투명도·피격 번쩍임이 같이 적용된다
        var celGo = new GameObject("Cel");
        celGo.transform.SetParent(root.transform, false);
        var celSr = celGo.AddComponent<SpriteRenderer>();
        celSr.sortingOrder = 42;
        celSr.enabled = false;
        renderers.Add(celSr);
        // 주먹: 무기(앞 45) 위에 다시 그린다 — 무기는 몸·옷·투구보다 위, 주먹보다 아래(10-26 사용자: 무기가 가장 잘 보여야)
        var fistGo = new GameObject("CelFist");
        fistGo.transform.SetParent(root.transform, false);
        var fistSr = fistGo.AddComponent<SpriteRenderer>();
        fistSr.sortingOrder = 46;
        fistSr.enabled = false;
        renderers.Add(fistSr);

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
        so.FindProperty("_cel").objectReferenceValue = celSr;
        so.FindProperty("_celFist").objectReferenceValue = fistSr;
        BuildCel(so.FindProperty("_celClips"), ox);
        BuildEquipLayers();
        so.FindProperty("_airLegF").objectReferenceValue = PartSprite(LegPose("leg_l", 1, 1));
        so.FindProperty("_airLegB").objectReferenceValue = PartSprite(LegPose("leg_r", 0, 1));

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
    /// 연속 이동 주기(보간, 한 주기 Samples 키): 앞다리 x = stride·cos, 뒷다리 반대. 앞으로 나가는 발이 lift 만큼 들리고 tilt° 기운다.
    /// 몸은 교차 때 bob 만큼 오르고(|sin|), 팔은 다리와 엇갈려 arm° 흔든다(칼날 각도 blade 유지). 머리카락은 들썩임에 한 박자 늦게(그림 교체),
    /// 배낭은 1/8 주기 늦게 아래로만 처진다. 뼈 위치는 리그가 1px 격자에 맞추고 회전은 연속.
    /// </summary>
    private static AnimDef SmoothCycle(string name, float period, float blade, float stride, float lift, float tilt, float bob, float arm, float lean)
    {
        const int Samples = 24;
        var def = new AnimDef(name, period, true);
        var lfx = new float[(Samples + 1) * 2];
        var lfy = new float[(Samples + 1) * 2];
        var lfr = new float[(Samples + 1) * 2];
        var lbx = new float[(Samples + 1) * 2];
        var lby = new float[(Samples + 1) * 2];
        var lbr = new float[(Samples + 1) * 2];
        var hy = new float[(Samples + 1) * 2];
        var hx = new float[(Samples + 1) * 2];
        var pk = new float[(Samples + 1) * 2];
        var shB = new float[(Samples + 1) * 2];
        var armTab = new float[(Samples + 1) * 3];
        var hairT = new List<object>();
        int lastHair = int.MinValue;
        for (int i = 0; i <= Samples; i++)
        {
            float t = period * i / Samples;
            float ph = 2f * Mathf.PI * i / Samples;
            float c = Mathf.Cos(ph), sn = Mathf.Sin(ph);
            float body = bob * Mathf.Abs(sn);
            float bodyLate = bob * Mathf.Abs(Mathf.Sin(ph - Mathf.PI / 4f));
            int k = i * 2;
            lfx[k] = lbx[k] = lfy[k] = lby[k] = lfr[k] = lbr[k] = hy[k] = hx[k] = pk[k] = shB[k] = t;
            lfx[k + 1] = stride * c;
            lbx[k + 1] = -stride * c;
            lfy[k + 1] = lift * Mathf.Max(0f, -sn); // 앞다리는 뒤 → 앞으로 나갈 때(주기 후반) 든다
            lby[k + 1] = lift * Mathf.Max(0f, sn);
            lfr[k + 1] = tilt * c;
            lbr[k + 1] = -tilt * c;
            hy[k + 1] = body;
            hx[k + 1] = lean;
            pk[k + 1] = Mathf.Min(0f, bodyLate - body);
            shB[k + 1] = arm * c; // 뒷팔은 앞다리와 같은 쪽(= 뒷다리 반대)
            armTab[i * 3] = t;
            armTab[i * 3 + 1] = -arm * c; // 앞팔은 앞다리와 엇갈린다
            armTab[i * 3 + 2] = blade;
            int hair = HairLag(bob * Mathf.Abs(Mathf.Sin(ph - 2f * Mathf.PI / Samples)), body);
            if (hair != lastHair)
            {
                hairT.Add(t);
                hairT.Add(HairPose(hair));
                lastHair = hair;
            }
        }
        def.K("LegF", 'x', lfx).K("LegF", 'y', lfy).K("LegF", 'r', lfr)
            .K("LegB", 'x', lbx).K("LegB", 'y', lby).K("LegB", 'r', lbr)
            .K("Hips", 'y', hy).K("Hips", 'x', hx).K("Hips/Pack", 'y', pk)
            .K("Hips/ShoulderB", 'r', shB)
            .SK("Hips/Head", hairT.ToArray())
            .Arm(armTab);
        return def;
    }

    /// <summary>
    /// 이동 주기(끊기는 포즈). 다리·팔은 뼈를 옮기지 않고 포즈 변형 그림으로 바꿔 끼운다(Tools/pixelfx/make_pose_parts.py: 마디별 정수 이동 + 외곽 닫기).
    /// legFdx·legBdx = 발 앞뒤 px(-2~2), legFlift·legBlift = 무릎 접어 들기(0~2), armF = 앞손 앞뒤 px(뒷손은 반대, -1~1).
    /// 배낭은 엉덩이가 올라올 때 한 포즈 늦게 따라온다(상대값 = min(0, 이전 - 현재)). 무기 소켓(앞손)은 손 이동을 따라간다.
    /// </summary>
    private static AnimDef Cycle(string name, float frame, float blade, int[] legFdx, int[] legFlift, int[] legBdx, int[] legBlift, float[] hipsY, float[] headY, int[] armF, float hipsX)
    {
        int n = legFdx.Length;
        var def = new AnimDef(name, frame * n, true).Step();
        def.K("Hips", 'y', Keys(hipsY, frame)).K("Hips", 'x', Keys(Fill(n, hipsX), frame))
            .K("Hips/Head", 'y', Keys(headY, frame));
        var pack = new float[n];
        var hand = new float[n];
        var legF = new string[n];
        var legB = new string[n];
        var armFs = new string[n];
        var armBs = new string[n];
        var hair = new string[n];
        var legsY = new float[n];
        for (int i = 0; i < n; i++)
        {
            // 몸이 위로 뜨는 포즈(달리기 체공)는 다리도 같이 띄운다 — 몸만 올리면 가랑이 사이가 갇힌 구멍이 됐다
            legsY[i] = Mathf.Max(0f, hipsY[i]);
            hair[i] = HairPose(HairLag(hipsY[(i + n - 1) % n], hipsY[i]));
            // 아래로만 처진다(위로 밀리면 배낭 윗선이 머리카락 위로 삐져나왔다)
            pack[i] = Mathf.Min(0f, hipsY[(i + n - 1) % n] - hipsY[i]);
            hand[i] = armF[i];
            legF[i] = LegPose("leg_l", legFdx[i], legFlift[i]);
            legB[i] = LegPose("leg_r", legBdx[i], legBlift[i]);
            armFs[i] = ArmPose("arm_l", armF[i]);
            armBs[i] = ArmPose("arm_r", -armF[i]);
        }
        var handB = new float[n];
        for (int i = 0; i < n; i++)
        {
            handB[i] = -armF[i]; // 뒷손 무기(쌍단검 왼손 등)도 손 그림 이동을 따라간다
        }
        def.K("LegF", 'y', Keys(legsY, frame)).K("LegB", 'y', Keys(legsY, frame))
            .K("Hips/ShoulderB/HandB", 'x', Keys(handB, frame))
            .K("Hips/Pack", 'y', Keys(pack, frame)).K("Hips/ShoulderF/HandF", 'x', Keys(hand, frame))
            .S("LegF", frame, legF).S("LegB", frame, legB).S("Hips/ShoulderF", frame, armFs).S("Hips/ShoulderB", frame, armBs)
            .S("Hips/Head", frame, hair)
            .Arm(0f, 0f, blade, frame * n, 0f, blade);
        return def;
    }

    private static string Tag(int dx)
    {
        return (dx > 0 ? "p" : dx < 0 ? "m" : "z") + Mathf.Abs(dx);
    }

    private static string LegPose(string part, int dx, int lift)
    {
        return part + "_" + Tag(dx) + "_u" + lift;
    }

    private static string ArmPose(string part, int dx)
    {
        return part + "_" + Tag(dx);
    }

    /// <summary>머리카락 2차 움직임: +1 정수리 늘임, -1 눌림, 0 원본</summary>
    private static string HairPose(int dy)
    {
        return dy == 0 ? "head" : "head_" + Tag(dy);
    }

    /// <summary>몸이 이번 포즈에 내려갔으면 머리카락이 한 박자 늦게 남아 늘어나고(+1), 올라갔으면 눌린다(-1)</summary>
    private static int HairLag(float prevHips, float hips)
    {
        return hips < prevHips ? 1 : hips > prevHips ? -1 : 0;
    }

    /// <summary>한 포즈짜리 클립(끊김). 값은 (x, y) 픽셀, arms 는 양 어깨 같은 이동, blade 는 앞팔 칼날 각도</summary>
    private static AnimDef Pose(string name, float len, float blade, (float x, float y) hips = default, (float x, float y) legF = default,
        (float x, float y) legB = default, (float x, float y) arms = default, float pack = 0f, int hair = 0)
    {
        var def = new AnimDef(name, len, false).Step();
        if (hair != 0)
        {
            def.S("Hips/Head", len, new[] { HairPose(hair) });
        }
        def.K("Hips", 'x', 0f, hips.x, len, hips.x).K("Hips", 'y', 0f, hips.y, len, hips.y)
            .K("LegF", 'y', 0f, legF.y, len, legF.y)
            .K("LegB", 'y', 0f, legB.y, len, legB.y)
            // 다리 앞뒤(x)는 뼈를 옮기지 않고 포즈 그림으로(통째로 옮기면 바지 밑단이 몸통에서 어긋났다)
            .S("LegF", len, new[] { LegPose("leg_l", Mathf.RoundToInt(legF.x), 0) })
            .S("LegB", len, new[] { LegPose("leg_r", Mathf.RoundToInt(legB.x), 0) })
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
                .SK("Hips/Head", 0f, "head", 0.6f, HairPose(1), 0.72f, "head", 1.1f, HairPose(-1), 1.22f, "head") // 머리카락도 숨에 한 박자 늦게
                .Arm(0f, 0f, 45f, 1.3f, 0f, 45f),
            // 대기 습관(6초 넘게 서 있으면 한 번): 배낭 추켜올리기 — 뒷어깨 으쓱 1px·배낭을 몸 쪽으로 붙이며 1px 들었다 내려놓고 몸 1px 내려앉음
            new AnimDef("IdleFidget", 0.7f, false).Step()
                .K("Hips/ShoulderB", 'y', 0f, 0f, 0.1f, 1f, 0.3f, 1f, 0.4f, 0f, 0.7f, 0f)
                .K("Hips/Pack", 'y', 0f, 0f, 0.1f, 1f, 0.3f, 1f, 0.4f, -1f, 0.5f, 0f, 0.7f, 0f)
                .K("Hips/Pack", 'x', 0f, 0f, 0.1f, 1f, 0.4f, 0f, 0.7f, 0f)
                .K("Hips", 'y', 0f, 0f, 0.4f, -1f, 0.5f, 0f, 0.7f, 0f)
                .Arm(0f, 0f, 45f, 0.7f, 0f, 45f),
            // 걷기·달리기 = 연속 동작(PixelRigDesign 3.6, 10-26 사용자 "원본 수준, 프레임 단위로"): 원본 AnyPortrait 는 두 발 간격이 매 프레임
            // 사인파로 2↔9 아트px 변했다. 정지 포즈 교체(±1px, 0.08초 유지)는 뻣뻣했다 → 다리 앞뒤·들기·기울기, 몸 들썩임, 팔 흔들기를 곡선으로.
            // 발이 닿는 순간(최대 보폭)이 주기 0·½ → 발소리·먼지 포즈(걷기 0·4 / 달리기 0·3)와 맞는다
            SmoothCycle("Walk", 0.64f, 45f, stride: 3f, lift: 1.5f, tilt: 10f, bob: 1f, arm: 15f, lean: 0f),
            SmoothCycle("Run", 0.36f, 45f, stride: 4f, lift: 3f, tilt: 18f, bob: 1.5f, arm: 35f, lean: 1f),
            // 제동: 달리다 멈추면 리그가 0.13초 보여 준다 — 앞발 2px 내딛어 버티고 몸은 뒤로 1px 젖혀 내려앉음, 손은 관성으로 앞, 머리카락 늘임 → 반쯤 회복
            new AnimDef("Brake", 0.14f, false).Step()
                .K("Hips", 'x', 0f, -1f, 0.07f, 0f, 0.14f, 0f)
                .K("Hips", 'y', 0f, -1f, 0.07f, 0f, 0.14f, 0f)
                .K("Hips/ShoulderF/HandF", 'x', 0f, 1f, 0.07f, 1f, 0.14f, 1f)
                .K("Hips/ShoulderB/HandB", 'x', 0f, 1f, 0.07f, 0f, 0.14f, 1f)
                .S("LegF", 0.07f, new[] { LegPose("leg_l", 2, 0), LegPose("leg_l", 1, 0) })
                .S("LegB", 0.07f, new[] { LegPose("leg_r", -1, 0), LegPose("leg_r", 0, 0) })
                .S("Hips/ShoulderF", 0.07f, new[] { ArmPose("arm_l", 1), ArmPose("arm_l", 1) })
                .S("Hips/ShoulderB", 0.07f, new[] { ArmPose("arm_r", 1), ArmPose("arm_r", 0) })
                .S("Hips/Head", 0.07f, new[] { HairPose(1), HairPose(-1) })
                .Arm(0f, 0f, 45f, 0.14f, 0f, 45f),
            // 점프 v2(PixelRigDesign 3.6): 원본은 0.12초 웅크림 예비 → 도약 → 공중 내내 조금씩 변하는 자세 → 다리 뻗어 착지 준비.
            // JumpCrouch = 입력 뒤 실제로 뜨기 전(리그가 세로 속도로 판단), JumpArc = 세로 속도 +8(0) → 0(0.5) → -8(1) 로 리그가 시점을 고르는 연속 자세
            new AnimDef("JumpCrouch", 0.1f, false).Step()
                .K("Hips", 'y', 0f, -2f, 0.1f, -2f)
                .K("Hips/ShoulderB", 'r', 0f, 20f, 0.1f, 20f)
                .S("LegF", 0.1f, new[] { LegPose("leg_l", 1, 1) }).S("LegB", 0.1f, new[] { LegPose("leg_r", -1, 1) })
                .S("Hips/Head", 0.1f, new[] { HairPose(1) })
                .Arm(0f, -20f, 45f, 0.1f, -20f, 45f),
            new AnimDef("JumpArc", 1f, false)
                .K("LegF", 'y', 0f, 1f, 0.5f, 1f, 1f, 0f)
                .K("LegF", 'r', 0f, 20f, 0.5f, 10f, 1f, -5f)
                .K("LegB", 'y', 0f, 0f, 0.5f, 1f, 1f, 1f)
                .K("LegB", 'r', 0f, -15f, 0.5f, -5f, 1f, -10f)
                .K("Hips/ShoulderB", 'r', 0f, -20f, 0.5f, 30f, 1f, 60f)
                .SK("LegF", 0f, LegPose("leg_l", 1, 2), 0.35f, LegPose("leg_l", 1, 1), 0.75f, LegPose("leg_l", 1, 0))
                .SK("LegB", 0f, LegPose("leg_r", -1, 0), 0.35f, LegPose("leg_r", 0, 1), 0.75f, LegPose("leg_r", -1, 1))
                .SK("Hips/Head", 0f, HairPose(-1), 0.3f, "head", 0.7f, HairPose(1))
                .Arm(0f, 20f, 90f, 0.5f, 30f, 80f, 1f, 45f, 70f), // 팔은 45° 까지(70° 면 손이 어깨에서 떨어져 떠 보였다)
            // 점프: 리그(PixelRig)가 세로 속도로 고른다 — 도약(첫 0.06초 늘임) → 상승 → 꼭대기 → 낙하, 착지하면 0.1초 눌림(Land).
            // 변형은 한두 프레임만(셀레스트식), 1px 단위
            // 다리는 통째로 올리면 몸통 뒤로 숨어 사라져 보였다 → 접은 다리 그림(포즈 변형)으로: 상승 = 앞다리 접어 올림·뒷다리 늘어뜨림
            Pose("Jump", 0.1f, 90f, legF: (0, 1), arms: (0, 1))
                .S("LegF", 0.1f, new[] { LegPose("leg_l", 1, 2) }).S("LegB", 0.1f, new[] { LegPose("leg_r", -1, 0) }),
            Pose("JumpLaunch", 0.06f, 90f, hips: (0, 1), arms: (0, 1), hair: -1), // 머리카락: 도약에 눌리고 낙하에 떠오르고 착지에 눌린다(한 박자 늦는 2차 움직임)
            Pose("JumpApex", 0.1f, 45f, legF: (0, 1), legB: (0, 1), arms: (0, 1)) // 꼭대기: 두 다리 살짝 접음
                .S("LegF", 0.1f, new[] { LegPose("leg_l", 1, 1) }).S("LegB", 0.1f, new[] { LegPose("leg_r", 0, 1) }),
            Pose("JumpFall", 0.1f, 90f, legB: (0, 1), arms: (0, 2), hair: 1) // 낙하: 앞다리 펴서 착지 준비, 뒷다리 접음
                .S("LegF", 0.1f, new[] { LegPose("leg_l", 1, 0) }).S("LegB", 0.1f, new[] { LegPose("leg_r", -1, 1) }),
            // 피격: 몸이 뒤로 1px 젖혀지고 팔이 들린다, 칼은 앞 위 그대로(뒤로 눕히면 칼날이 얼굴을 가로질렀다). 리그가 피격 신호 뒤 0.22초 보여 준다
            Pose("Hurt", 0.1f, 45f, hips: (-1, 0), legF: (-1, 0), legB: (-2, 0), arms: (-1, 1), pack: -1),
            // 사망 1단계: 무릎 꿇음(몸 2px 내려앉고 칼끝 아래로). 2단계(뒤로 쓰러짐)는 리그가 몸 전체를 90° 눕힌다
            Pose("DeathKneel", 0.1f, -90f, hips: (0, -2), legF: (1, 0), legB: (-1, 0)),
            Pose("Land", 0.1f, 45f, hips: (0, -1), legF: (1, 0), legB: (-1, 0), hair: -1), // 팔·배낭은 몸통과 같이(더 내리면 뒷어깨·배낭 사이가 비었다, find_rig_holes)
            // 대시 1포즈: 몸 2px 앞으로, 앞다리 접어 끌고 뒷다리는 뒤로 쭉, 손은 뒤로(소매 고정 그림), 칼 뒤로 눕힘, 머리카락 눌림(앞에서 바람)
            new AnimDef("Dash", 0.1f, false).Step()
                .K("Hips", 'x', 0f, 2f, 0.1f, 2f)
                .K("Hips/ShoulderF/HandF", 'x', 0f, -1f, 0.1f, -1f)
                .K("Hips/ShoulderB/HandB", 'x', 0f, -1f, 0.1f, -1f)
                .S("LegF", 0.1f, new[] { LegPose("leg_l", -1, 1) }).S("LegB", 0.1f, new[] { LegPose("leg_r", -2, 0) })
                .S("Hips/ShoulderF", 0.1f, new[] { ArmPose("arm_l", -1) }).S("Hips/ShoulderB", 0.1f, new[] { ArmPose("arm_r", -1) })
                .S("Hips/Head", 0.1f, new[] { HairPose(-1) })
                .Arm(0f, 0f, 180f, 0.1f, 0f, 180f),
            // 로프: 팔은 정확히 180°(머리 위로 곧게 — 어중간한 각도는 도트가 지글거렸다), 오를 때는 팔·다리 1~2px 번갈아
            new AnimDef("RopeStay", 0.6f, true).Step()
                .Arm(0f, 180f, 90f, 0.6f, 180f, 90f)
                .K("Hips/ShoulderB", 'r', 0f, 180f, 0.6f, 180f)
                .S("LegF", 0.6f, new[] { LegPose("leg_l", 0, 1) }), // 앞다리 살짝 접어 로프에 건다(뼈로 올리면 몸통 뒤로 숨었다)
            new AnimDef("RopeUp", 0.4f, true).Step()
                .Arm(0f, 180f, 90f, 0.4f, 180f, 90f)
                .K("Hips/ShoulderB", 'r', 0f, 180f, 0.4f, 180f)
                .K("Hips/ShoulderF", 'y', 0f, 1f, 0.2f, 0f, 0.4f, 1f)
                .K("Hips/ShoulderB", 'y', 0f, 0f, 0.2f, 1f, 0.4f, 0f)
                .S("LegF", 0.2f, new[] { LegPose("leg_l", 0, 2), LegPose("leg_l", 0, 0) }) // 두 다리 번갈아 크게 접기
                .S("LegB", 0.2f, new[] { LegPose("leg_r", 0, 0), LegPose("leg_r", 0, 2) }),
            // 공격 = 끊기는 핵심 포즈(예비 → 타격 → 이어짐 → 회복), 각도는 45° 배수만(도트 칼·팔이 깔끔하게).
            // 속도감은 베기 궤적 이펙트가 채운다. Arm(시간, 팔 각도, 칼날 각도) — 칼날 0 = 앞, 90 = 위, 180 = 뒤, -45 = 앞 아래
            new AnimDef("Stab", 0.36f, false).Step()
                .Arm(0f, -45f, 180f, 0.07f, 90f, 0f, 0.2f, 90f, 0f, 0.3f, 45f, 45f, 0.36f, 45f, 45f)
                .K("Hips/ShoulderB", 'r', 0f, 45f, 0.07f, -45f, 0.3f, 0f, 0.36f, 0f)
                .K("Hips", 'x', 0f, -1f, 0.07f, 2f, 0.2f, 1f, 0.3f, 0f, 0.36f, 0f)
                .SK("LegF", 0f, LegPose("leg_l", 0, 0), 0.07f, LegPose("leg_l", 2, 0), 0.3f, LegPose("leg_l", 1, 0))
                .SK("LegB", 0f, LegPose("leg_r", 0, 0), 0.07f, LegPose("leg_r", -1, 0), 0.3f, LegPose("leg_r", 0, 0)),
            new AnimDef("SwingDown", 0.38f, false).Step()
                .Arm(0f, 180f, 135f, 0.07f, 90f, 0f, 0.13f, 45f, -45f, 0.28f, 45f, 0f, 0.38f, 45f, 45f)
                .K("Hips/ShoulderB", 'r', 0f, -45f, 0.07f, 0f, 0.38f, 0f)
                .K("Hips", 'x', 0f, -1f, 0.07f, 1f, 0.13f, 1f, 0.28f, 0f, 0.38f, 0f)
                .K("Hips", 'y', 0f, 0f, 0.07f, 0f, 0.13f, -1f, 0.28f, 0f, 0.38f, 0f)
                .SK("LegF", 0f, LegPose("leg_l", 0, 0), 0.07f, LegPose("leg_l", 2, 0), 0.28f, LegPose("leg_l", 1, 0))
                .SK("Hips/Head", 0f, "head", 0.13f, HairPose(1), 0.28f, HairPose(-1), 0.33f, "head"), // 내려찍는 순간 머리카락이 늦게 남았다 튄다
            new AnimDef("SwingUp", 0.38f, false).Step()
                .Arm(0f, 0f, -45f, 0.07f, 90f, 45f, 0.13f, 180f, 90f, 0.28f, 135f, 90f, 0.38f, 45f, 45f)
                .K("Hips/ShoulderB", 'r', 0f, 0f, 0.07f, -45f, 0.28f, 0f, 0.38f, 0f)
                .K("Hips", 'y', 0f, -1f, 0.07f, 0f, 0.13f, 1f, 0.28f, 0f, 0.38f, 0f)
                .SK("LegF", 0f, LegPose("leg_l", 0, 0), 0.07f, LegPose("leg_l", 1, 0), 0.28f, LegPose("leg_l", 0, 0))
                .SK("Hips/Head", 0f, HairPose(1), 0.07f, "head", 0.13f, HairPose(-1), 0.28f, HairPose(1), 0.33f, "head"),
            new AnimDef("SwingSide", 0.38f, false).Step()
                .Arm(0f, -45f, 180f, 0.07f, 90f, 0f, 0.13f, 90f, -45f, 0.28f, 45f, 0f, 0.38f, 45f, 45f)
                .K("Hips/ShoulderB", 'r', 0f, 45f, 0.07f, -45f, 0.28f, 0f, 0.38f, 0f)
                .K("Hips", 'x', 0f, -1f, 0.07f, 2f, 0.13f, 2f, 0.28f, 1f, 0.38f, 0f)
                .SK("LegF", 0f, LegPose("leg_l", 0, 0), 0.07f, LegPose("leg_l", 2, 0), 0.28f, LegPose("leg_l", 1, 0))
                .SK("LegB", 0f, LegPose("leg_r", 0, 0), 0.07f, LegPose("leg_r", -1, 0), 0.28f, LegPose("leg_r", 0, 0)),
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
                .SK("LegF", 0f, LegPose("leg_l", 1, 0)).SK("LegB", 0f, LegPose("leg_r", -1, 0)),
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
    private static readonly string[] AnimBones = { "Hips", "Hips/Head", "Hips/Pack", "Hips/ShoulderF", "Hips/ShoulderB", "Hips/ShoulderF/HandF", "Hips/ShoulderB/HandB", "LegF", "LegB" };

    /// <summary>포즈 변형 그림을 바꿔 끼우는 뼈와 원본 그림</summary>
    private static readonly (string bone, string part)[] SwapParts =
    {
        ("LegF", "leg_l"), ("LegB", "leg_r"), ("Hips/ShoulderF", "arm_l"), ("Hips/ShoulderB", "arm_r"), ("Hips/Head", "head"),
    };

    private static Sprite PartSprite(string part)
    {
        var sp = AssetDatabase.LoadAssetAtPath<Sprite>(ArtDir + "/" + part + ".png");
        if (sp == null)
        {
            throw new System.Exception("포즈 그림 없음: " + part + " — python Tools/pixelfx/make_pose_parts.py 먼저");
        }
        return sp;
    }

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
        foreach (var swap in SwapParts)
        {
            ObjectReferenceKeyframe[] keys;
            if (def.GetSprites(swap.bone, out float[] times, out string[] parts))
            {
                keys = new ObjectReferenceKeyframe[parts.Length];
                for (int i = 0; i < parts.Length; i++)
                {
                    keys[i] = new ObjectReferenceKeyframe { time = times[i], value = PartSprite(parts[i]) };
                }
            }
            else
            {
                // 모든 클립이 원본 그림 키를 가진다 — 걷기 뒤 공격·점프에서 변형 그림이 남지 않게
                keys = new[] { new ObjectReferenceKeyframe { time = 0f, value = PartSprite(swap.part) } };
            }
            AnimationUtility.SetObjectReferenceCurve(clip, EditorCurveBinding.PPtrCurve(swap.bone, typeof(SpriteRenderer), "m_Sprite"), keys);
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

        private readonly Dictionary<string, (float[] times, string[] parts)> _sprites = new Dictionary<string, (float[], string[])>();

        /// <summary>뼈의 그림을 포즈마다 바꿔 끼운다(frame 간격, 마지막에 첫 그림으로 고리를 닫는다). 없으면 원본 그림 고정</summary>
        public AnimDef S(string bone, float frame, string[] parts)
        {
            var times = new float[parts.Length + 1];
            var closed = new string[parts.Length + 1];
            for (int i = 0; i <= parts.Length; i++)
            {
                times[i] = i * frame;
                closed[i] = parts[i % parts.Length];
            }
            _sprites[bone] = (times, closed);
            return this;
        }

        /// <summary>(시간, 그림) 쌍으로 바꿔 끼운다 — 공격처럼 키 간격이 고르지 않은 클립용</summary>
        public AnimDef SK(string bone, params object[] timePart)
        {
            var times = new float[timePart.Length / 2];
            var parts = new string[timePart.Length / 2];
            for (int i = 0; i < times.Length; i++)
            {
                times[i] = System.Convert.ToSingle(timePart[i * 2]);
                parts[i] = (string)timePart[i * 2 + 1];
            }
            _sprites[bone] = (times, parts);
            return this;
        }

        public bool GetSprites(string bone, out float[] times, out string[] parts)
        {
            bool ok = _sprites.TryGetValue(bone, out var v);
            times = v.times;
            parts = v.parts;
            return ok;
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

    // ── 셀(Q-423) ────────────────────────────────────────────────────────────
    private const string CelDir = "Assets/Art/Player/PixelRig/Cel";

    [System.Serializable]
    private class CelFrameJson
    {
        public string png;
        public int[] hand;
        public int[] handB;
        public float angle = CelClip.NoAngle;
    }

    [System.Serializable]
    private class CelClipJson
    {
        public float frame;
        public bool loop;
        public CelFrameJson[] frames;
    }

    /// <summary>Tools/pixelfx/build_cel.py 산출물(cel.json + PNG)을 임포트해 리그의 셀 클립으로. 피벗 = 리그 원점(엉덩이 x, 발바닥 행)</summary>
    private static void BuildCel(SerializedProperty arr, int ox)
    {
        string json = CelDir + "/cel.json";
        if (!File.Exists(json))
        {
            arr.arraySize = 0;
            return;
        }
        // JsonUtility 는 최상위 사전을 못 읽어 클립 이름 순서대로 꺼낸다
        string text = File.ReadAllText(json);
        string[] names = { "Idle", "Walk", "Run", "JumpCrouch", "JumpArc", "Land", "Dash", "Hurt", "Death", "Stab", "SwingDown", "SwingUp", "RopeUp", "RopeStay", "GreatDown", "GreatSide", "DaggerFlurry", "DaggerCross" };
        var list = new List<(string, CelClipJson)>();
        foreach (string n in names)
        {
            int k = text.IndexOf("\"" + n + "\"");
            if (k < 0)
            {
                continue;
            }
            int start = text.IndexOf('{', k);
            int depth = 0, end = start;
            for (; end < text.Length; end++)
            {
                if (text[end] == '{') depth++;
                if (text[end] == '}' && --depth == 0) break;
            }
            list.Add((n, JsonUtility.FromJson<CelClipJson>(text.Substring(start, end - start + 1))));
        }
        arr.arraySize = list.Count;
        for (int c = 0; c < list.Count; c++)
        {
            var (name, clip) = list[c];
            SerializedProperty e = arr.GetArrayElementAtIndex(c);
            e.FindPropertyRelative("name").stringValue = name;
            e.FindPropertyRelative("frame").floatValue = clip.frame;
            e.FindPropertyRelative("loop").boolValue = clip.loop;
            SerializedProperty sp = e.FindPropertyRelative("sprites");
            SerializedProperty fp = e.FindPropertyRelative("fists");
            fp.arraySize = clip.frames.Length;
            SerializedProperty ap = e.FindPropertyRelative("angles");
            ap.arraySize = clip.frames.Length;
            SerializedProperty hp = e.FindPropertyRelative("hand");
            SerializedProperty bp = e.FindPropertyRelative("handB");
            sp.arraySize = hp.arraySize = bp.arraySize = clip.frames.Length;
            for (int i = 0; i < clip.frames.Length; i++)
            {
                CelFrameJson f = clip.frames[i];
                ImportPart(f.png, new[] { ox, FeetRow - 1 }, CelDir);
                sp.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(CelDir + "/" + f.png + ".png");
                string fistPng = CelDir + "/" + f.png + "_fist.png";
                if (File.Exists(fistPng))
                {
                    ImportPart(f.png + "_fist", new[] { ox, FeetRow - 1 }, CelDir);
                }
                fp.GetArrayElementAtIndex(i).objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(fistPng);
                hp.GetArrayElementAtIndex(i).vector2Value = CelLocal(f.hand, ox);
                bp.GetArrayElementAtIndex(i).vector2Value = CelLocal(f.handB, ox);
                ap.GetArrayElementAtIndex(i).floatValue = f.angle;
            }
        }
    }

    // ── 장비 레이어(던파식, CelEquipmentPlan) ─────────────────────────────────
    private const string EquipDir = "Assets/Art/Player/PixelRig/Equip";
    private const string EquipOut = "Assets/Resources/PixelRig/Equip";

    [System.Serializable]
    private class EquipFrameJson
    {
        public string clip;
        public int k;
    }

    [System.Serializable]
    private class EquipJson
    {
        public string id;
        public string slot;
        public EquipFrameJson[] frames;
    }

    /// <summary>Tools/pixelfx/build_equip.py 산출물(장비 폴더마다 equip.json + 레이어·마스크 PNG) → Resources/PixelRig/Equip/&lt;id&gt;.asset</summary>
    private static void BuildEquipLayers()
    {
        if (!Directory.Exists(EquipDir))
        {
            return;
        }
        Directory.CreateDirectory(EquipOut);
        foreach (string dir in Directory.GetDirectories(EquipDir))
        {
            string json = dir + "/equip.json";
            if (!File.Exists(json))
            {
                continue;
            }
            var meta = JsonUtility.FromJson<EquipJson>(File.ReadAllText(json));
            string folder = dir.Replace('\\', '/');
            string path = EquipOut + "/" + meta.id + ".asset";
            var asset = AssetDatabase.LoadAssetAtPath<CelEquipLayer>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<CelEquipLayer>();
                AssetDatabase.CreateAsset(asset, path);
            }
            asset.id = meta.id;
            asset.slot = System.Enum.TryParse(meta.slot, true, out CelEquipLayer.Slot sl) ? sl : CelEquipLayer.Slot.Body;
            var frames = new List<CelEquipLayer.Frame>();
            foreach (EquipFrameJson f in meta.frames)
            {
                string name = f.clip + "_" + f.k;
                frames.Add(new CelEquipLayer.Frame
                {
                    clip = f.clip,
                    index = f.k,
                    layer = ImportRaw(folder + "/" + name + ".png"),
                    erase = ImportRaw(folder + "/" + name + "_erase.png"),
                });
            }
            asset.frames = frames.ToArray();
            EditorUtility.SetDirty(asset);
        }
        AssetDatabase.SaveAssets();
    }

    /// <summary>합성용 원본 텍스처: 읽기 가능·무압축·점 필터</summary>
    private static Texture2D ImportRaw(string path)
    {
        // build_equip.py 가 막 쓴 파일은 아직 에셋 DB 에 없다 — 먼저 임포트(안 하면 레이어가 전부 비었다)
        if (File.Exists(path))
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        }
        var ti = (TextureImporter)AssetImporter.GetAtPath(path);
        if (ti == null)
        {
            return null;
        }
        ti.textureType = TextureImporterType.Default;
        ti.isReadable = true;
        ti.filterMode = FilterMode.Point;
        ti.textureCompression = TextureImporterCompression.Uncompressed;
        ti.mipmapEnabled = false;
        ti.npotScale = TextureImporterNPOTScale.None;
        ti.alphaIsTransparency = true;
        ti.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    /// <summary>64 캔버스 픽셀(왼쪽 위 0) → 리그 로컬 유닛(원점 = 엉덩이 x·발바닥, 위 +). 픽셀 가운데</summary>
    private static Vector2 CelLocal(int[] p, int ox)
    {
        return new Vector2((p[0] + 0.5f - ox) / Ppu, (FeetRow - (p[1] + 0.5f)) / Ppu);
    }

    /// <summary>포즈 변형 그림(leg_l_p1_u0 …)은 원본 부위와 같은 피벗</summary>
    private static void ImportPoseParts(PivotMap pv)
    {
        for (int dx = -2; dx <= 2; dx++)
        {
            for (int lift = 0; lift <= 2; lift++)
            {
                ImportPart(LegPose("leg_l", dx, lift), pv.leg_l);
                ImportPart(LegPose("leg_r", dx, lift), pv.leg_r);
            }
            ImportPart(ArmPose("arm_l", dx), pv.shoulder_l);
            ImportPart(ArmPose("arm_r", dx), pv.shoulder_r);
        }
        ImportPart(HairPose(-1), pv.head);
        ImportPart(HairPose(1), pv.head);
    }

    private static void ImportPart(string part, int[] pivot, string dir = ArtDir)
    {
        string path = dir + "/" + part + ".png";
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
        ti.isReadable = dir == CelDir; // 셀은 장비 레이어 합성(CelComposer)에서 픽셀을 읽는다
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
