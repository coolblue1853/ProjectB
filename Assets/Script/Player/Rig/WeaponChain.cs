using UnityEngine;

/// <summary>
/// T1 줄·사슬 그리기(10-29 밤, MotionCoverage: 줄·사슬이 없어 대응 못 하던 직업 47종 — 채찍·사슬낫·유성추·사복검·올가미·낚싯줄).
/// 무기 손잡이에 붙여 쓰는 버렛 사슬: 손잡이에 고정된 마디들이 중력·관성으로 따라오고, 휘두르면 원심력으로 뻗는다.
/// 셀 동작마다 따로 그릴 필요가 없어 어떤 동작에도 붙는다. 도트 느낌으로 마디 위치를 픽셀 격자에 맞춘다.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class WeaponChain : MonoBehaviour
{
    [SerializeField] private int _links = 12;
    [SerializeField] private float _linkLength = 0.12f;
    [SerializeField] private float _gravity = 9f;
    [SerializeField, Range(0.8f, 1f)] private float _damping = 0.965f; // 10-30 GPT "손목 정지 뒤 끝이 늦게 튀어나가는 스냅이 약함" — 관성을 더 남긴다
    [SerializeField] private float _ppu = 36f;
    [Tooltip("손잡이에서 사슬이 시작하는 곳(손잡이 위쪽 방향 거리)")]
    [SerializeField] private float _startOffset = 0.25f;
    [Tooltip("끝 추(유성추·낫날)를 따로 그릴 때 붙일 트랜스폼(선택)")]
    [SerializeField] private Transform _tipObject;

    private LineRenderer _line;
    private Vector3[] _pos;
    private Vector3[] _prev;
    private Vector3[] _draw;

    public void Setup(int links, float linkLength, Color color, int sortingOrder)
    {
        _links = links;
        _linkLength = linkLength;
        _line = GetComponent<LineRenderer>();
        _line.startColor = _line.endColor = color;
        _line.sortingOrder = sortingOrder;
        Init();
    }

    private void Awake()
    {
        _line = GetComponent<LineRenderer>();
        if (_line.sharedMaterial == null)
        {
            _line.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
        }
        _line.useWorldSpace = true;
        _line.startWidth = _line.endWidth = 2f / _ppu;
        _line.numCapVertices = 0;
        Init();
    }

    private void Init()
    {
        _pos = new Vector3[_links + 1];
        _prev = new Vector3[_links + 1];
        _draw = new Vector3[_links + 1];
        Vector3 a = Anchor();
        for (int i = 0; i <= _links; i++)
        {
            _pos[i] = _prev[i] = a + Vector3.down * (_linkLength * i);
        }
        _line.positionCount = _links + 1;
    }

    private Vector3 Anchor()
    {
        return transform.position + transform.up * _startOffset;
    }

    private void LateUpdate()
    {
        float dt = Mathf.Min(Time.unscaledDeltaTime, 1f / 30f);
        Vector3 g = Vector3.down * (_gravity * dt * dt);
        _pos[0] = Anchor();
        for (int i = 1; i <= _links; i++)
        {
            Vector3 v = (_pos[i] - _prev[i]) * _damping;
            _prev[i] = _pos[i];
            _pos[i] += v + g;
        }
        // 첫 마디는 무기 방향으로 뻗는다 — 손목을 휘두르면 그 방향으로 사슬이 따라 나간다
        Vector3 dir = transform.up;
        for (int iter = 0; iter < 6; iter++)
        {
            _pos[0] = Anchor();
            _pos[1] = Vector3.Lerp(_pos[1], _pos[0] + dir * _linkLength, 0.85f);
            if (_links > 2)
            {
                _pos[2] = Vector3.Lerp(_pos[2], _pos[0] + dir * (_linkLength * 2f), 0.35f); // 손잡이 쪽 두 마디는 손목 방향을 따라 — 손목이 멈추면 끝만 늦게 날아간다
            }
            for (int i = 0; i < _links; i++)
            {
                Vector3 d = _pos[i + 1] - _pos[i];
                float len = d.magnitude;
                if (len < 1e-5f)
                {
                    continue;
                }
                Vector3 fix = d * ((len - _linkLength) / len);
                if (i == 0)
                {
                    _pos[i + 1] -= fix;
                }
                else
                {
                    _pos[i] += fix * 0.5f;
                    _pos[i + 1] -= fix * 0.5f;
                }
            }
        }
        for (int i = 0; i <= _links; i++)
        {
            _draw[i] = new Vector3(Mathf.Round(_pos[i].x * _ppu) / _ppu, Mathf.Round(_pos[i].y * _ppu) / _ppu, _pos[0].z);
        }
        _line.SetPositions(_draw);
        if (_tipObject != null)
        {
            _tipObject.position = _draw[_links];
        }
    }
}
