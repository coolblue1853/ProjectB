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

    public const int SlotCount = 16;

    public static readonly Strike[] Strikes =
    {
        new Strike("GreatDown", 5, VfxId.MSmear0, new SmearAura.Shape { arc = true, c = new Vector2(-5f, 9.9f), ea = 38f, eb = 30.4f, tilt = 0f, a0 = 140f, a1 = -10f }),
        new Strike("GreatSide", 3, VfxId.MSmear1, new SmearAura.Shape { arc = true, c = new Vector2(-2.3f, 26.7f), ea = 37.4f, eb = 29.9f, tilt = 0f, a0 = -218.2f, a1 = -11.2f }),
        new Strike("GreatSideOne", 3, VfxId.MSmear2, new SmearAura.Shape { arc = true, c = new Vector2(-3.6f, 23.4f), ea = 32.5f, eb = 26f, tilt = 0f, a0 = -223.4f, a1 = 23.8f }),
        new Strike("DaggerCross", 3, VfxId.MSmear3, new SmearAura.Shape { arc = true, c = new Vector2(-5.1f, 18.3f), ea = 32.3f, eb = 25.8f, tilt = 0f, a0 = 195.6f, a1 = 45.6f }),
        new Strike("DaggerCross", 3, VfxId.MSmear4, new SmearAura.Shape { arc = true, c = new Vector2(1f, 26.5f), ea = 23.7f, eb = 18.9f, tilt = 0f, a0 = 191.3f, a1 = 41.3f }),
        new Strike("Spin", 3, VfxId.MSmear5, new SmearAura.Shape { arc = true, c = new Vector2(-2.8f, 27.2f), ea = 25.4f, eb = 20.3f, tilt = 0f, a0 = 212.9f, a1 = 11.7f }),
        new Strike("Spin", 8, VfxId.MSmear6, new SmearAura.Shape { arc = true, c = new Vector2(-0.3f, 18f), ea = 33.6f, eb = 26.9f, tilt = 0f, a0 = 215.7f, a1 = -23.5f }),
        new Strike("LeapSlam", 6, VfxId.MSmear7, new SmearAura.Shape { arc = true, c = new Vector2(-3.4f, 12.9f), ea = 37.5f, eb = 30f, tilt = 0f, a0 = 200.9f, a1 = -30.2f }),
        new Strike("Iaido", 4, VfxId.MSmear8, new SmearAura.Shape { arc = true, c = new Vector2(4.1f, 24.1f), ea = 33.1f, eb = 26.5f, tilt = 0f, a0 = -204.5f, a1 = -7.9f }),
        new Strike("AirSlash", 3, VfxId.MSmear9, new SmearAura.Shape { arc = true, c = new Vector2(-3.2f, 15f), ea = 35.1f, eb = 28.1f, tilt = 0f, a0 = 195.1f, a1 = -34.9f }),
        new Strike("Slam", 6, VfxId.MSmear10, new SmearAura.Shape { arc = true, c = new Vector2(-3.7f, 15f), ea = 35.8f, eb = 28.7f, tilt = 0f, a0 = 157.7f, a1 = -49.3f }),
        new Strike("Lunge", 2, VfxId.MSmear11, new SmearAura.Shape { arc = false, p0 = new Vector2(7f, 13f), p1 = new Vector2(61.5f, 13f) }),
        new Strike("DaggerFlurry", 2, VfxId.MSmear12, new SmearAura.Shape { arc = false, p0 = new Vector2(1f, 21f), p1 = new Vector2(55.5f, 21f) }),
        new Strike("DaggerFlurry", 4, VfxId.MSmear13, new SmearAura.Shape { arc = false, p0 = new Vector2(2f, 21.1f), p1 = new Vector2(56.3f, 25.9f) }),
        new Strike("DaggerFlurry", 7, VfxId.MSmear14, new SmearAura.Shape { arc = false, p0 = new Vector2(5f, 18.5f), p1 = new Vector2(59.4f, 15.7f) }),
        new Strike("Punch", 3, VfxId.MSmear15, new SmearAura.Shape { arc = false, p0 = new Vector2(3f, 21f), p1 = new Vector2(57.5f, 21f) }),
    };

    public static readonly Impact[] Impacts =
    {
        new Impact("GreatDown", 5, VfxId.GroundShock, 22f, 0.8f),
        new Impact("LeapSlam", 6, VfxId.GroundShock, 20f, 1f),
        new Impact("LeapSlam", 6, VfxId.DustBrown, 6f, 0.9f),
        new Impact("Slam", 6, VfxId.GroundShock, 18f, 0.9f),
        new Impact("ShieldBash", 3, VfxId.DustBrown, -4f, 0.7f),
        new Impact("Iaido", 4, VfxId.StepDust, -6f, 1f),
        new Impact("Lunge", 2, VfxId.StepDust, -6f, 0.9f),
        new Impact("Backstep", 4, VfxId.StepDust, 0f, 0.9f),
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
