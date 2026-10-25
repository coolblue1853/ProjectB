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
        Assert.AreEqual(VfxId.WThrust, id);
        Assert.IsFalse(flip);
        Assert.IsTrue(WeaponFx.TryMelee("RightHandVashUp_OneHand", VfxId.SlashRed, out id, out flip));
        Assert.AreEqual(VfxId.WSlashDown, id);
        Assert.IsTrue(flip, "올려베기는 세로 초승달 상하 반전");
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
    public void RigClips_WalkAndRunSwapLegs()
    {
        // 걷기·달리기는 다리 그림을 포즈마다 바꾼다(통째 이동으로 돌아가면 다리가 몸에서 떨어져 보였다)
        foreach (var clip in RigClips())
        {
            if (clip.name != "Walk" && clip.name != "Run")
            {
                continue;
            }
            var keys = UnityEditor.AnimationUtility.GetObjectReferenceCurve(clip,
                UnityEditor.EditorCurveBinding.PPtrCurve("LegF", typeof(UnityEngine.SpriteRenderer), "m_Sprite"));
            var distinct = new System.Collections.Generic.HashSet<UnityEngine.Object>();
            foreach (var k in keys)
            {
                distinct.Add(k.value);
            }
            Assert.GreaterOrEqual(distinct.Count, 3, clip.name + " 앞다리 그림 종류");
        }
    }
}
