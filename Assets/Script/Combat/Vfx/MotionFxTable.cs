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
        new Strike("GreatDown", 8, VfxId.MSmear0, new SmearAura.Shape { arc = true, c = new Vector2(-7.2f, 7.8f), ea = 38f, eb = 30.4f, tilt = 0f, a0 = 143.4f, a1 = -6.6f }),
        new Strike("GreatSide", 4, VfxId.MSmear1, new SmearAura.Shape { arc = true, c = new Vector2(-2.3f, 26.7f), ea = 37.4f, eb = 29.9f, tilt = 0f, a0 = -218.2f, a1 = -11.2f }),
        new Strike("GreatSideOne", 4, VfxId.MSmear2, new SmearAura.Shape { arc = true, c = new Vector2(-3.6f, 24.4f), ea = 32.5f, eb = 26f, tilt = 0f, a0 = -223.4f, a1 = 23.8f }),
        new Strike("DaggerCross", 5, VfxId.MSmear3, new SmearAura.Shape { arc = true, c = new Vector2(-4.9f, 16.6f), ea = 34.4f, eb = 27.5f, tilt = 0f, a0 = 198f, a1 = 48f }),
        new Strike("DaggerCross", 5, VfxId.MSmear4, new SmearAura.Shape { arc = true, c = new Vector2(-4.7f, 17.2f), ea = 33.8f, eb = 27f, tilt = 0f, a0 = 197.7f, a1 = 47.7f }),
        new Strike("Spin", 4, VfxId.MSmear5, new SmearAura.Shape { arc = true, c = new Vector2(-2.8f, 27.2f), ea = 25.4f, eb = 20.3f, tilt = 0f, a0 = 212.9f, a1 = 11.7f }),
        new Strike("Spin", 9, VfxId.MSmear6, new SmearAura.Shape { arc = true, c = new Vector2(-0.3f, 18f), ea = 33.6f, eb = 26.9f, tilt = 0f, a0 = 215.7f, a1 = -23.5f }),
        new Strike("LowSweep", 6, VfxId.MSmear7, new SmearAura.Shape { arc = true, c = new Vector2(1.1f, 21.6f), ea = 35.7f, eb = 28.5f, tilt = 0f, a0 = -223.8f, a1 = -16.8f }),
        new Strike("Iaido", 6, VfxId.MSmear8, new SmearAura.Shape { arc = true, c = new Vector2(4.4f, 25.8f), ea = 34.1f, eb = 27.3f, tilt = 0f, a0 = -207.9f, a1 = -11.3f }),
        new Strike("AirSlash", 5, VfxId.MSmear9, new SmearAura.Shape { arc = true, c = new Vector2(-3.2f, 15.3f), ea = 36.4f, eb = 29.1f, tilt = 0f, a0 = 196.1f, a1 = -33.9f }),
        new Strike("GreatUpper", 5, VfxId.MSmear10, new SmearAura.Shape { arc = true, c = new Vector2(-1.3f, 20.5f), ea = 31.4f, eb = 25.1f, tilt = 0f, a0 = -177.8f, a1 = 63.7f }),
        new Strike("Lunge", 3, VfxId.MSmear11, new SmearAura.Shape { arc = false, p0 = new Vector2(7f, 14f), p1 = new Vector2(61.5f, 14f) }),
        new Strike("DashSlash", 6, VfxId.MSmear12, new SmearAura.Shape { arc = true, c = new Vector2(4.6f, 18.3f), ea = 33.9f, eb = 27.1f, tilt = 0f, a0 = -210.1f, a1 = 2.6f }),
        new Strike("Punch", 5, VfxId.MSmear13, new SmearAura.Shape { arc = false, p0 = new Vector2(3f, 21f), p1 = new Vector2(57.5f, 21f) }),
    };

    public static readonly Impact[] Impacts =
    {
        new Impact("GreatDown", 8, VfxId.GroundShock, 22f, 0.8f),
        new Impact("LeapSlam", 6, VfxId.GroundShock, 20f, 1f),
        new Impact("LeapSlam", 6, VfxId.DustBrown, 6f, 0.9f),
        new Impact("Slam", 6, VfxId.GroundShock, 18f, 0.9f),
        new Impact("ShieldBash", 5, VfxId.DustBrown, -4f, 0.7f),
        new Impact("Iaido", 6, VfxId.StepDust, -6f, 1f),
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
