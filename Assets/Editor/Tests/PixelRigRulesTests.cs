using NUnit.Framework;

/// <summary>픽셀 리그 주인공·기본 공격 이펙트 규칙(PixelRigDesign 2절, GameFeelPlan 10.7) 회귀 테스트</summary>
public class PixelRigRulesTests
{
    [TestCase("ShieldUp", "Guard")]
    [TestCase("Spining", "Spin")]
    [TestCase("StaffAttack", "Cast")]
    [TestCase("SpeadBuff", "Cast")]
    [TestCase("RightHandstabbing_OneHand", "Stab")]
    [TestCase("StrongStab", "Stab")]
    [TestCase("RightHandVashUp_OneHand", "SwingUp")]
    [TestCase("upperCut", "SwingUp")]
    [TestCase("GreatSideAttack", "SwingSide")]
    [TestCase("WideVesh", "SwingSide")]
    [TestCase("RightHandVashDown_OneHand", "SwingDown")]
    [TestCase("GreatVashDown", "SwingDown")]
    [TestCase("RopeStay", "RopeStay")]
    [TestCase("RopeDown", "RopeUp")]
    public void MapName_SkillAndAttackNames(string anim, string clip)
    {
        Assert.AreEqual(clip, PixelRig.MapName(anim));
    }

    [Test]
    public void WeaponFx_MeleeSwingPicksTrail()
    {
        Assert.IsTrue(WeaponFx.TryMelee("RightHandstabbing_OneHand", VfxId.SlashWhite, out VfxId id, out bool flip));
        Assert.AreEqual(VfxId.WSmearStab, id); // 10-28 기본 3타 스미어 v7(셀 좌표 스미어)로 교체 — 예전 WThrust 기대값이 남아 있었다
        Assert.IsFalse(flip);
        Assert.IsTrue(WeaponFx.TryMelee("RightHandVashUp_OneHand", VfxId.SlashRed, out id, out flip));
        Assert.AreEqual(VfxId.WSmearUp, id); // 올려베기 전용 스미어(뒤집기 없이)
        Assert.IsFalse(flip);
    }

    [TestCase("GreatVashDown", "GreatDown")]
    [TestCase("DeathSideGround", "LowSweep")]
    [TestCase("StabLR", "DaggerCross")]
    [TestCase("WideVesh", "GreatSideOne")]
    [TestCase("RightHandstabbing_OneHand", null)]
    public void SkillCel_WeaponSkillsUseMasterMotions(string anim, string cel)
    {
        Assert.AreEqual(cel, PixelRig.SkillCel(anim));
    }

    [Test]
    public void WeaponFx_StaffKeepsOwnEffect()
    {
        Assert.IsFalse(WeaponFx.TryMelee("StaffAttack", VfxId.BurstBlue, out _, out _));
        Assert.IsFalse(WeaponFx.TryMelee("StaffAttack", VfxId.None, out _, out _));
    }

    [Test]
    public void PixelRamp_FromSwingColor()
    {
        Assert.AreEqual(PixelRampId.Red, PixelRamp.FromSwing(VfxId.SlashRed));
        Assert.AreEqual(PixelRampId.Gold, PixelRamp.FromSwing(VfxId.SlashGoldWide));
        Assert.AreEqual(PixelRampId.White, PixelRamp.FromSwing(VfxId.SlashWhite));
        Assert.AreEqual(5, PixelRamp.Get(PixelRampId.Blue).Length);
    }

    [Test]
    public void Vfx_LegacySlashUpgrades()
    {
        Assert.IsTrue(Vfx.TryUpgradeLegacy(VfxId.SlashGold, out VfxId to, out PixelRampId ramp, out float mul, out bool flip));
        Assert.AreEqual(VfxId.WSlashDown, to);
        Assert.AreEqual(PixelRampId.Gold, ramp);
        Assert.IsTrue(Vfx.TryUpgradeLegacy(VfxId.CritCross, out to, out ramp, out mul, out flip));
        Assert.AreEqual(VfxId.WHitStar, to);
        Assert.AreEqual(2f, mul);
        Assert.IsFalse(Vfx.TryUpgradeLegacy(VfxId.BurstBlue, out _, out _, out _, out _), "폭발·빛기둥 등은 대체하지 않는다");
    }

    // ── 포즈 그림 교체(PixelRigDesign 10-26 (19)~) ────────────────────────────
    private static readonly string[] SwapBones = { "LegF", "LegB", "Hips/ShoulderF", "Hips/ShoulderB", "Hips/Head" };

    private static System.Collections.Generic.IEnumerable<UnityEngine.AnimationClip> RigClips()
    {
        var prefab = UnityEngine.Resources.Load<UnityEngine.GameObject>("PixelRig/Adventurer");
        Assert.IsNotNull(prefab, "Resources/PixelRig/Adventurer 프리팹");
        var clips = new UnityEditor.SerializedObject(prefab.GetComponent<PixelRig>()).FindProperty("_clips");
        for (int i = 0; i < clips.arraySize; i++)
        {
            yield return (UnityEngine.AnimationClip)clips.GetArrayElementAtIndex(i).FindPropertyRelative("clip").objectReferenceValue;
        }
    }

    [Test]
    public void RigClips_EverySwapBoneHasSpriteCurve()
    {
        // 모든 클립이 다리·팔·머리 그림 키를 가져야 걷기 뒤 공격·점프에서 변형 그림이 남지 않는다
        foreach (var clip in RigClips())
        {
            foreach (string bone in SwapBones)
            {
                var binding = UnityEditor.EditorCurveBinding.PPtrCurve(bone, typeof(UnityEngine.SpriteRenderer), "m_Sprite");
                var keys = UnityEditor.AnimationUtility.GetObjectReferenceCurve(clip, binding);
                Assert.IsNotNull(keys, clip.name + " / " + bone + " 그림 커브 없음");
                Assert.Greater(keys.Length, 0, clip.name + " / " + bone);
                foreach (var k in keys)
                {
                    Assert.IsNotNull(k.value, clip.name + " / " + bone + " 빈 그림 키 @" + k.time);
                }
            }
        }
    }

    [Test]
    public void RigClips_WalkAndRunMoveLegsContinuously()
    {
        // 걷기·달리기는 다리가 앞뒤로 연속 이동·회전한다(PixelRigDesign 3.6: 원본은 발 간격 2↔9 아트px 연속 변화, 정지 포즈 교체는 뻣뻣했다)
        foreach (var clip in RigClips())
        {
            if (clip.name != "Walk" && clip.name != "Run")
            {
                continue;
            }
            var x = UnityEditor.AnimationUtility.GetEditorCurve(clip, UnityEditor.EditorCurveBinding.FloatCurve("LegF", typeof(UnityEngine.Transform), "m_LocalPosition.x"));
            var r = UnityEditor.AnimationUtility.GetEditorCurve(clip, UnityEditor.EditorCurveBinding.FloatCurve("LegF", typeof(UnityEngine.Transform), "localEulerAnglesRaw.z"));
            Assert.IsNotNull(x, clip.name + " 앞다리 x 커브");
            Assert.IsNotNull(r, clip.name + " 앞다리 회전 커브");
            Assert.GreaterOrEqual(x.length, 12, clip.name + " 연속 동작 키 수");
            float minX = float.MaxValue, maxX = float.MinValue, minR = float.MaxValue, maxR = float.MinValue;
            foreach (var k in x.keys) { minX = UnityEngine.Mathf.Min(minX, k.value); maxX = UnityEngine.Mathf.Max(maxX, k.value); }
            foreach (var k in r.keys) { minR = UnityEngine.Mathf.Min(minR, k.value); maxR = UnityEngine.Mathf.Max(maxR, k.value); }
            Assert.GreaterOrEqual((maxX - minX) * 36f, 5.9f, clip.name + " 앞다리 앞뒤 폭(아트 px)");
            Assert.GreaterOrEqual(maxR - minR, 19f, clip.name + " 앞다리 기울기 폭(도)");
        }
    }
}
