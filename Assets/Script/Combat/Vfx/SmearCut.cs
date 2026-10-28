using UnityEngine;

/// <summary>
/// 칼선 — 칼끝이 지나간 자리에 남는 1px 예리한 선(10-28 사용자 "칼날 지나간 자리에 예리한 칼선").
/// 스미어(0.11초)보다 오래(0.24초) 남고, 꼬리부터 줄어들며 앞쪽(진행 끝)이 가장 밝다. 스미어 오브젝트가 꺼져도 남도록 Vfx 루트 쪽 오브젝트에 붙는다.
/// 모양·수치는 Tools/pixelfx/smear_preview.py cut_line 과 같다. SmearAura 가 만들고 시작시킨다.
/// </summary>
public class SmearCut : MonoBehaviour
{
    const float Life = 0.24f;
    const float Lo = 0.05f;
    const int Points = 24;

    LineRenderer _line;
    System.Func<float, Vector3> _path; // 진행도 0~1 → 월드 위치
    Color _head, _tail;
    float _started, _speed;

    public void Build(Material mat, int sortingLayerId, int sortingOrder)
    {
        _line = gameObject.AddComponent<LineRenderer>();
        _line.sharedMaterial = mat;
        _line.useWorldSpace = true;
        _line.positionCount = Points;
        _line.numCapVertices = 0;
        _line.numCornerVertices = 0;
        _line.alignment = LineAlignment.View;
        _line.sortingLayerID = sortingLayerId;
        _line.sortingOrder = sortingOrder;
        _line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        _line.receiveShadows = false;
        _line.enabled = false;
    }

    public void Play(System.Func<float, Vector3> path, float width, Color head, Color tail, float speed)
    {
        _path = path;
        _head = head;
        _tail = tail;
        _speed = speed > 0f ? speed : 1f;
        _started = Time.time;
        _line.widthMultiplier = width;
        _line.enabled = true;
        enabled = true;
        LateUpdate();
    }

    void LateUpdate()
    {
        if (_path == null)
        {
            return;
        }
        float u = (Time.time - _started) * _speed / Life;
        if (u >= 1f)
        {
            _line.enabled = false;
            enabled = false;
            return;
        }
        float lo = Lo + (1f - Lo) * Mathf.Pow(u, 1.4f); // 꼬리부터 줄어든다
        for (int j = 0; j < Points; j++)
        {
            _line.SetPosition(j, _path(lo + (1f - lo) * j / (Points - 1)));
        }
        float fade = 1f - u * 0.5f;
        Color h = _head;
        h.a *= fade;
        Color t = _tail;
        t.a *= fade;
        _line.startColor = t;
        _line.endColor = h;
    }
}
