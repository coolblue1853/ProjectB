using UnityEngine;

/// <summary>
/// 셀 재생 프로파일(10-29 사용자: "속도를 바꾸거나 중간에 멈추는 것으로도 쓸 수 있다") — 그림 없이 기존 셀의 구간·속도·정지·반복으로 만드는 동작.
/// MotionSpec39 §7 에서 합친 동작(모으기·연속 찌르기·메치기·도약 내려찍기·잡기)을 여기서 정의한다. 이름으로 Play 하면 기본 셀을 이 규칙으로 재생한다.
/// </summary>
public static class MotionVariants
{
    public sealed class Variant
    {
        public readonly string name;
        public readonly string baseCel;
        public readonly float speed;
        public readonly int from;
        /// <summary>마지막 프레임(포함). -1 = 셀 끝</summary>
        public readonly int to;
        /// <summary>이 프레임에 도착하면 멈춰 있는다(-1 = 멈추지 않음)</summary>
        public readonly int hold;
        /// <summary>from~to 를 반복</summary>
        public readonly bool loop;
        /// <summary>to → from 으로 거꾸로(납도 = 발도 역재생)</summary>
        public readonly bool reverse;
        /// <summary>이 프레임 구간(포함)은 리그를 좌우 반전 — 몸이 한 바퀴 도는 것처럼(전신 회전베기). -1 = 없음</summary>
        public readonly int flipFrom, flipTo;

        public Variant(string name, string baseCel, float speed, int from = 0, int to = -1, int hold = -1, bool loop = false, bool reverse = false, int flipFrom = -1, int flipTo = -1)
        {
            this.name = name;
            this.baseCel = baseCel;
            this.speed = speed;
            this.from = from;
            this.to = to;
            this.hold = hold;
            this.loop = loop;
            this.reverse = reverse;
            this.flipFrom = flipFrom;
            this.flipTo = flipTo;
        }

        public bool FlipAt(int frame)
        {
            return flipFrom >= 0 && frame >= flipFrom && frame <= flipTo;
        }

        /// <summary>재생 경과 시간 → 기본 셀 프레임 번호</summary>
        public int IndexIn(CelClip cel, float time)
        {
            int n = cel.sprites.Length;
            if (n == 0)
            {
                return 0;
            }
            int last = to < 0 || to >= n ? n - 1 : to;
            float t0 = cel.StartTimeOf(from);
            float t1 = cel.StartTimeOf(last + 1);
            float t = time * speed;
            float local;
            if (loop)
            {
                local = t0 + Mathf.Repeat(t, Mathf.Max(0.0001f, t1 - t0));
            }
            else
            {
                local = Mathf.Min(t0 + t, t1 - 0.0001f);
                if (hold >= 0)
                {
                    local = Mathf.Min(local, cel.StartTimeOf(hold));
                }
            }
            if (reverse)
            {
                local = t1 - 0.0001f - (local - t0);
            }
            return cel.IndexAt(local);
        }
    }

    // 프레임 번호는 Assets/Art/Player/PixelRig/Cel/cel.json 기준(셀을 다시 만들면 함께 확인 — 10-29 보간 다듬기 번호)
    private static readonly Variant[] All =
    {
        // 모으기(Charge)는 10-29 빠진 동작 조사로 합치기 취소 — 원래 셀(가슴 앞 교차 후 활짝 펼치기)을 쓴다
        new Variant("DaggerFlurry", "Stab", 0.9f, 1, 4, loop: true),     // 연속 찌르기 = 찌르기(당김 1 → 뻗음 4) 구간 반복. 10-29 사용자 "너무 빨라서 모르겠다" 1.6 → 0.9
        new Variant("LeapSlam", "AirSlash", 0.9f),                       // 도약 내려찍기 = 점프 + 공중 베기
        new Variant("DownThrust", "AirSlash", 0.8f, 5, 8, hold: 8),       // 10-29 사용자 "멈춤이라 모르겠다" → 벤 뒤(5) 칼을 아래로 돌려(8) 유지
        new Variant("GreatUpper", "GreatUpper", 1f, 0, 12, hold: 12),     // 올려친 끝(8)에서 멈춤 — 회복에서 칼을 내리는 게 내려치기로 보였다(10-29 사용자)
        new Variant("Whirl", "Spin", 0.9f),                              // A23 전신 회전베기 = 회전 베기 셀(10-29 밤 방향 그림 프레임으로 다시 — 좌우 반전 방식 폐기, 사용자 "이 방향 아니다")
        new Variant("UnderThrow", "Place", 1.4f),                         // N7 아래로 던지기(볼링·씨앗·포션 굴리기) = 웅크려 놓기를 빠르게
    };

    public static Variant Find(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }
        foreach (Variant v in All)
        {
            if (v.name == name)
            {
                return v;
            }
        }
        return null;
    }
}
