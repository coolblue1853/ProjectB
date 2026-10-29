using UnityEngine;

/// <summary>
/// 마스터 모션 이펙트 표 — Tools/pixelfx/gen_motion_fx.py 가 생성한다(손으로 고치지 말 것).
/// 셀 동작이 재생되면 타격 프레임에 스미어(셀 좌표 타원 고리·바늘, 림버스 문법)와 지면 충격을 띄운다. 기본 3타는 Weapon 이 따로 띄우므로 여기 없다.
/// </summary>
public static class MotionFxTable
{
    public readonly struct Strike
    {
        public readonly string anim;
        public readonly int frame;
        public readonly VfxId id;
        public readonly SmearAura.Shape shape;

        public Strike(string anim, int frame, VfxId id, SmearAura.Shape shape)
        {
            this.anim = anim;
            this.frame = frame;
            this.id = id;
            this.shape = shape;
        }
    }

    public readonly struct Impact
    {
        public readonly string anim;
        public readonly int frame;
        public readonly VfxId id;
        /// <summary>발 앞 거리(셀 px, 바라보는 쪽 +)</summary>
        public readonly float ahead;
        public readonly float scale;

        public Impact(string anim, int frame, VfxId id, float ahead, float scale)
        {
            this.anim = anim;
            this.frame = frame;
            this.id = id;
            this.ahead = ahead;
            this.scale = scale;
        }
    }

    public const int SlotCount = 14;

    public static readonly Strike[] Strikes =
    {
        new Strike("GreatDown", 8, VfxId.MSmear0, new SmearAura.Shape { arc = true, c = new Vector2(-8.3f, 4.8f), ea = 38f, eb = 30.4f, tilt = 0f, a0 = 145.2f, a1 = -4.8f }),
        new Strike("GreatSide", 4, VfxId.MSmear1, new SmearAura.Shape { arc = true, c = new Vector2(-2.3f, 26.7f), ea = 37.4f, eb = 29.9f, tilt = 0f, a0 = -218.2f, a1 = -11.2f }),
        new Strike("GreatSideOne", 5, VfxId.MSmear2, new SmearAura.Shape { arc = true, c = new Vector2(-6.8f, 25.9f), ea = 35.5f, eb = 28.4f, tilt = 0f, a0 = -210.9f, a1 = 19.1f }),
        new Strike("DaggerCross", 5, VfxId.MSmear3, new SmearAura.Shape { arc = true, c = new Vector2(-5.4f, 14.8f), ea = 37.4f, eb = 29.9f, tilt = 0f, a0 = 215.3f, a1 = -37.7f }),
        new Strike("DaggerCross", 5, VfxId.MSmear4, new SmearAura.Shape { arc = true, c = new Vector2(-4.2f, 15.1f), ea = 36.4f, eb = 29.1f, tilt = 0f, a0 = 147.9f, a1 = -39.5f }),
        new Strike("Spin", 4, VfxId.MSmear5, new SmearAura.Shape { arc = true, c = new Vector2(-2.3f, 26.7f), ea = 37.4f, eb = 29.9f, tilt = 0f, a0 = -218.2f, a1 = -11.2f }),
        new Strike("Spin", 8, VfxId.MSmear6, new SmearAura.Shape { arc = true, c = new Vector2(-2.2f, 15.2f), ea = 25.8f, eb = 20.7f, tilt = 0f, a0 = -305.8f, a1 = -5.8f }),
        new Strike("LowSweep", 8, VfxId.MSmear7, new SmearAura.Shape { arc = true, c = new Vector2(0.2f, 22.1f), ea = 36.6f, eb = 29.3f, tilt = 0f, a0 = -195.5f, a1 = -17.3f }),
        new Strike("Iaido", 7, VfxId.MSmear8, new SmearAura.Shape { arc = true, c = new Vector2(7.7f, 16.4f), ea = 25.5f, eb = 20.4f, tilt = 0f, a0 = -292.1f, a1 = 7.9f }),
        new Strike("AirSlash", 5, VfxId.MSmear9, new SmearAura.Shape { arc = true, c = new Vector2(-3.2f, 15.3f), ea = 36.4f, eb = 29.1f, tilt = 0f, a0 = 196.1f, a1 = -33.9f }),
        new Strike("GreatUpper", 9, VfxId.MSmear10, new SmearAura.Shape { arc = true, c = new Vector2(-5.2f, 17.9f), ea = 37.2f, eb = 29.7f, tilt = 0f, a0 = -89f, a1 = 61f }),
        new Strike("Lunge", 3, VfxId.MSmear11, new SmearAura.Shape { arc = false, p0 = new Vector2(7f, 14f), p1 = new Vector2(61.5f, 14f) }),
        new Strike("DashSlash", 7, VfxId.MSmear12, new SmearAura.Shape { arc = true, c = new Vector2(2.3f, 20.1f), ea = 36.6f, eb = 29.2f, tilt = 0f, a0 = -196.3f, a1 = -0.8f }),
        new Strike("Punch", 11, VfxId.MSmear13, new SmearAura.Shape { arc = false, p0 = new Vector2(8f, 17f), p1 = new Vector2(62.5f, 17f) }),
    };

    public static readonly Impact[] Impacts =
    {
        new Impact("GreatDown", 8, VfxId.GroundShock, 22f, 0.8f),
        new Impact("LeapSlam", 6, VfxId.GroundShock, 20f, 1f),
        new Impact("LeapSlam", 6, VfxId.DustBrown, 6f, 0.9f),
        new Impact("Slam", 6, VfxId.GroundShock, 18f, 0.9f),
        new Impact("ShieldBash", 6, VfxId.DustBrown, -4f, 0.7f),
        new Impact("Iaido", 7, VfxId.StepDust, -6f, 1f),
        new Impact("Lunge", 3, VfxId.StepDust, -6f, 0.9f),
        new Impact("Backstep", 6, VfxId.StepDust, 0f, 0.9f),
    };

    public static bool IsMotionSmear(VfxId id)
    {
        return id >= VfxId.MSmear0 && id < VfxId.MSmear0 + SlotCount;
    }

    public static bool TryShape(VfxId id, out SmearAura.Shape shape)
    {
        foreach (Strike s in Strikes)
        {
            if (s.id == id)
            {
                shape = s.shape;
                return true;
            }
        }
        shape = default;
        return false;
    }
}
