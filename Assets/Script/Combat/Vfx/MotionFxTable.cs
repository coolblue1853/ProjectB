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

    public const int SlotCount = 15;

    public static readonly Strike[] Strikes =
    {
        new Strike("GreatDown", 5, VfxId.MSmear0, new SmearAura.Shape { arc = true, c = new Vector2(-5f, 9.9f), ea = 38f, eb = 30.4f, tilt = 0f, a0 = 140f, a1 = -10f }),
        new Strike("GreatSide", 3, VfxId.MSmear1, new SmearAura.Shape { arc = true, c = new Vector2(-2.2f, 26.7f), ea = 37.4f, eb = 29.9f, tilt = 0f, a0 = -219.8f, a1 = -12.8f }),
        new Strike("GreatSideOne", 3, VfxId.MSmear2, new SmearAura.Shape { arc = true, c = new Vector2(-2.6f, 26.4f), ea = 36.1f, eb = 28.9f, tilt = 0f, a0 = -217.5f, a1 = -8.2f }),
        new Strike("DaggerCross", 2, VfxId.MSmear3, new SmearAura.Shape { arc = true, c = new Vector2(-2f, 21.5f), ea = 28.9f, eb = 23.1f, tilt = 0f, a0 = 183.7f, a1 = -40.5f }),
        new Strike("DaggerCross", 6, VfxId.MSmear4, new SmearAura.Shape { arc = true, c = new Vector2(2f, 18.5f), ea = 29.7f, eb = 23.8f, tilt = 0f, a0 = -150.1f, a1 = 45.4f }),
        new Strike("Spin", 4, VfxId.MSmear5, new SmearAura.Shape { arc = true, c = new Vector2(-2.4f, 24.7f), ea = 35.4f, eb = 28.3f, tilt = 0f, a0 = -215.4f, a1 = -7.3f }),
        new Strike("Spin", 6, VfxId.MSmear6, new SmearAura.Shape { arc = true, c = new Vector2(-0.2f, 18.5f), ea = 32.2f, eb = 25.7f, tilt = 0f, a0 = -27.7f, a1 = 177f }),
        new Strike("LeapSlam", 5, VfxId.MSmear7, new SmearAura.Shape { arc = true, c = new Vector2(-0.5f, 19f), ea = 30f, eb = 24f, tilt = 0f, a0 = 190f, a1 = -41.1f }),
        new Strike("Iaido", 4, VfxId.MSmear8, new SmearAura.Shape { arc = true, c = new Vector2(4.5f, 17f), ea = 27.8f, eb = 22.3f, tilt = 0f, a0 = -194.7f, a1 = 1.9f }),
        new Strike("AirSlash", 2, VfxId.MSmear9, new SmearAura.Shape { arc = true, c = new Vector2(-0.5f, 20f), ea = 29.2f, eb = 23.4f, tilt = 0f, a0 = 186.5f, a1 = -43.5f }),
        new Strike("Slam", 5, VfxId.MSmear10, new SmearAura.Shape { arc = true, c = new Vector2(0f, 19f), ea = 29.4f, eb = 23.5f, tilt = 0f, a0 = 148.4f, a1 = -58.6f }),
        new Strike("Lunge", 3, VfxId.MSmear11, new SmearAura.Shape { arc = false, p0 = new Vector2(4f, 14.9f), p1 = new Vector2(58.3f, 10.1f) }),
        new Strike("DaggerFlurry", 2, VfxId.MSmear12, new SmearAura.Shape { arc = false, p0 = new Vector2(0f, 21f), p1 = new Vector2(54.5f, 21f) }),
        new Strike("DaggerFlurry", 5, VfxId.MSmear13, new SmearAura.Shape { arc = false, p0 = new Vector2(6f, 18.7f), p1 = new Vector2(60.5f, 20.6f) }),
        new Strike("Punch", 2, VfxId.MSmear14, new SmearAura.Shape { arc = false, p0 = new Vector2(3f, 21f), p1 = new Vector2(57.5f, 21f) }),
    };

    public static readonly Impact[] Impacts =
    {
        new Impact("GreatDown", 5, VfxId.GroundShock, 22f, 0.8f),
        new Impact("LeapSlam", 5, VfxId.GroundShock, 20f, 1f),
        new Impact("LeapSlam", 5, VfxId.DustBrown, 6f, 0.9f),
        new Impact("Slam", 5, VfxId.GroundShock, 18f, 0.9f),
        new Impact("ShieldBash", 3, VfxId.DustBrown, -4f, 0.7f),
        new Impact("Iaido", 4, VfxId.StepDust, -6f, 1f),
        new Impact("Lunge", 3, VfxId.StepDust, -6f, 0.9f),
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
