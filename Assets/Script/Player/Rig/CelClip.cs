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
    public float[] angles = new float[0]; // 칼날 각도(도, 0 = 앞, 90 = 위). NoAngle 이면 리그가 손 위치로 정한다(공격 셀은 그림 속 칼에서 읽은 값)
    public const float NoAngle = -999f;

    /// <summary>경과 시간으로 프레임 번호(반복이면 돌고, 아니면 마지막에서 멈춤)</summary>
    public int IndexAt(float time)
    {
        int n = sprites.Length;
        if (n == 0)
        {
            return 0;
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
