using System;
using UnityEngine;

/// <summary>
/// 셀(프레임별 그림) 클립 한 개 — 기본 이동·점프용(Q-423, PixelRigDesign 3.7). PixelRigBuilder 가 Assets/Art/Player/PixelRig/Cel/cel.json 으로 채운다.
/// 손 좌표는 리그 원점(발바닥·엉덩이 x) 기준 로컬 유닛 — 무기 소켓을 프레임마다 그 손에 붙인다.
/// </summary>
[Serializable]
public class CelClip
{
    public string name;
    public float frame = 0.1f;
    public bool loop = true;
    public Sprite[] sprites = new Sprite[0];
    public Sprite[] fists = new Sprite[0]; // 무기 위에 다시 그릴 주먹(무기는 몸·옷·투구 위, 주먹 아래)
    public Vector2[] hand = new Vector2[0];
    public Vector2[] handB = new Vector2[0];
    public float[] times = new float[0];  // 프레임별 시간(초). 비어 있거나 0 이면 frame 을 쓴다 — 공격 완급(준비 길게·타격 짧게, 10-27 MotionRootCause 6번)
    /// <summary>프레임별: 주무기를 몸 뒤에 그린다(뒷모습 프레임 — 회전 베기·돌려차기, 10-29 밤). 비어 있으면 앞</summary>
    public bool[] behind = new bool[0];
    /// <summary>프레임별 앞손(반대 손) 무기 각도 — 쌍검 X 교차·쌍권총(10-30). 비어 있거나 NoAngle 이면 앞(0°) 고정</summary>
    public float[] anglesB = new float[0];
    /// <summary>프레임별 주무기 보이는 길이 비율(1 = 전부) — 납도처럼 칼이 칼집에 들어가며 짧아지는 프레임(10-30). 비면 1</summary>
    public float[] weaponLen = new float[0];
    public float[] angles = new float[0]; // 칼날 각도(도, 0 = 앞, 90 = 위). NoAngle 이면 리그가 손 위치로 정한다(공격 셀은 그림 속 칼에서 읽은 값)
    public const float NoAngle = -999f;

    /// <summary>클립 전체 길이(초) — 프레임별 시간이 있으면 그 합</summary>
    public float Length
    {
        get
        {
            int n = sprites.Length;
            if (times.Length != n)
            {
                return n * frame;
            }
            float total = 0f;
            for (int k = 0; k < n; k++)
            {
                total += times[k] > 0f ? times[k] : frame;
            }
            return total;
        }
    }

    /// <summary>프레임 시작 시각(초, 속도 1 기준) — 칼 궤적 이펙트를 베는 프레임에 맞출 때 쓴다(10-28)</summary>
    public float StartTimeOf(int index)
    {
        float total = 0f;
        int n = Mathf.Min(index, sprites.Length);
        for (int k = 0; k < n; k++)
        {
            total += times.Length == sprites.Length && times[k] > 0f ? times[k] : frame;
        }
        return total;
    }

    /// <summary>경과 시간으로 프레임 번호(반복이면 돌고, 아니면 마지막에서 멈춤)</summary>
    public int IndexAt(float time)
    {
        int n = sprites.Length;
        if (n == 0)
        {
            return 0;
        }
        if (times.Length == n)
        {
            float total = 0f;
            for (int k = 0; k < n; k++)
            {
                total += times[k] > 0f ? times[k] : frame;
            }
            float t = Mathf.Max(0f, time);
            if (loop)
            {
                t = Mathf.Repeat(t, Mathf.Max(total, 0.0001f));
            }
            for (int k = 0; k < n; k++)
            {
                t -= times[k] > 0f ? times[k] : frame;
                if (t < 0f)
                {
                    return k;
                }
            }
            return n - 1;
        }
        int i = Mathf.FloorToInt(Mathf.Max(0f, time) / Mathf.Max(frame, 0.0001f));
        return loop ? i % n : Mathf.Min(i, n - 1);
    }

    /// <summary>0~1 진행도로 프레임 번호(점프 공중 자세처럼 리그가 시점을 고를 때)</summary>
    public int IndexAtNormalized(float t)
    {
        int n = sprites.Length;
        return n == 0 ? 0 : Mathf.Clamp(Mathf.FloorToInt(t * n), 0, n - 1);
    }
}
