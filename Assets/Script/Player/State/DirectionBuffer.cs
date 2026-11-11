using UnityEngine;

/// <summary>
/// 방향 입력 선행 버퍼(CombatInputRedesign §4.2, CI-P0). 패드 스틱은 손을 뗄 때 중립으로 바로 돌아가므로,
/// 공격 키 직전 window 초 안의 마지막 좌우 입력을 "현재 입력"으로 취급한다. 시간은 인자로 받아 테스트 가능.
/// </summary>
public sealed class DirectionBuffer
{
    /// <summary>임시 5f(60fps) = 0.08초 (Q-CI8)</summary>
    public const float DefaultWindow = 0.08f;

    readonly float _window;
    float _lastDir;
    float _lastTime = float.NegativeInfinity;

    public DirectionBuffer(float window = DefaultWindow)
    {
        _window = window;
    }

    /// <summary>매 프레임 호출. 비영 방향이면 방향(부호)과 시각을 저장한다</summary>
    public void Record(float now, float h)
    {
        if (h > 0f || h < 0f)
        {
            _lastDir = h > 0f ? 1f : -1f;
            _lastTime = now;
        }
    }

    /// <summary>currentH 가 비영이면 그대로, 아니면 window 안의 마지막 방향(없으면 0)</summary>
    public float Resolve(float now, float currentH)
    {
        if (currentH != 0f)
        {
            return currentH;
        }
        return now - _lastTime <= _window ? _lastDir : 0f;
    }

    public void Clear()
    {
        _lastDir = 0f;
        _lastTime = float.NegativeInfinity;
    }
}

/// <summary>
/// 스틱 아날로그 → 디지털 좌우(-1/0/+1) 히스테리시스(§4.3, CI-P0). 진입 0.5·해제 0.35, 데드존 0.25.
/// Lunge·방향 전환·캔슬 판정에만 쓰고 걷기 속도는 아날로그 그대로다. 임시값(Q-CI7).
/// </summary>
public struct StickDigital
{
    public const float DeadZone = 0.25f, Enter = 0.5f, Exit = 0.35f;

    int _state;

    /// <summary>현재 디지털 상태(-1·0·1)</summary>
    public int State => _state;

    /// <summary>값(-1~1)을 먹이고 디지털 상태를 돌려준다. 반대 방향으로 진입 임계를 넘으면 즉시 전환</summary>
    public int Update(float v)
    {
        float a = Mathf.Abs(v);
        if (a < DeadZone)
        {
            _state = 0;
            return _state;
        }
        int dir = v > 0f ? 1 : -1;
        if (_state == 0)
        {
            if (a >= Enter)
            {
                _state = dir;
            }
        }
        else if (dir != _state)
        {
            if (a >= Enter)
            {
                _state = dir;
            }
            else
            {
                _state = 0;
            }
        }
        else if (a < Exit)
        {
            _state = 0;
        }
        return _state;
    }

    /// <summary>상태 없는 판정: 0.25 미만 0, 0.5 이상 ±1, 사이는 0(약한 기울임 보호)</summary>
    public static int Digital(float v)
    {
        float a = Mathf.Abs(v);
        return a >= Enter ? (v > 0f ? 1 : -1) : 0;
    }
}
