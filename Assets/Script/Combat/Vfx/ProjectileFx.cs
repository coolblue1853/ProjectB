using UnityEngine;

/// <summary>
/// 투사체·소환수·스킬 판정 프리팹에 붙이는 시각 효과 묶음 (1층계 폴리싱, GameFeelPlan 8절):
/// 꼬리 트레일(TrailRenderer, 공유 머티리얼), 생성 직후(첫 LateUpdate)·꺼질 때 도트 이펙트(Vfx 풀 재사용).
/// 판정 로직(HomingOrb·StraightArrow·SummonRunner·DamageObject)은 건드리지 않는다.
/// </summary>
[DisallowMultipleComponent]
public class ProjectileFx : MonoBehaviour
{
    [SerializeField] private VfxId _spawnFx = VfxId.None;
    [SerializeField] private VfxId _endFx = VfxId.None;
    [SerializeField] private float _fxScale = 1f;
    [Tooltip("흙먼지·충격파처럼 지면에 붙일 이펙트면 아래 지면을 찾아 그 높이에서 재생")]
    [SerializeField] private bool _snapToGround;
    [Tooltip("0 이면 트레일 없음")]
    [SerializeField] private float _trailTime;
    [SerializeField] private float _trailWidth = 0.2f;
    [SerializeField] private Color _trailColor = new Color(1f, 1f, 1f, 0.7f);
    [SerializeField] private int _trailSortingOrder = 205;

    private static Material s_trailMaterial;
    private static bool s_quitting;

    private TrailRenderer _trail;
    private bool _spawned;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        s_quitting = false;
        Application.quitting -= OnQuit;
        Application.quitting += OnQuit;
    }

    private static void OnQuit()
    {
        s_quitting = true;
    }

    private void Awake()
    {
        if (_trailTime > 0f)
        {
            BuildTrail();
        }
    }

    // 판정 오브젝트는 Instantiate 로만 생긴다 → 등장 연출은 첫 LateUpdate 에서 한 번
    // (같은 오브젝트의 StraightArrow.Start 가 시작 위치를 흩뿌린 뒤가 되도록 — Start 끼리는 순서 보장이 없다)
    private void LateUpdate()
    {
        if (_spawned)
        {
            return;
        }
        _spawned = true;
        if (_trail != null)
        {
            _trail.Clear();
        }
        Play(_spawnFx);
    }

    private void OnDisable()
    {
        // 씬 정리·플레이 종료 중, 그리고 부모(무기·플레이어)가 꺼지면서 함께 꺼질 때는 소멸 연출을 내지 않는다
        if (s_quitting || !gameObject.scene.isLoaded || (transform.parent != null && !transform.parent.gameObject.activeInHierarchy))
        {
            return;
        }
        Play(_endFx);
    }

    private void Play(VfxId id)
    {
        if (id == VfxId.None)
        {
            return;
        }
        Vector3 pos = transform.position;
        if (_snapToGround || VfxLibrary.IsGroundFx(id))
        {
            pos.y = GroundProbe.Y(pos);
        }
        bool left = transform.lossyScale.x < 0f;
        Vfx.Play(id, pos, left, _fxScale);
    }

    private void BuildTrail()
    {
        _trail = gameObject.AddComponent<TrailRenderer>();
        if (s_trailMaterial == null)
        {
            s_trailMaterial = Resources.Load<Material>("FX/M_Trail");
            if (s_trailMaterial == null)
            {
                s_trailMaterial = Resources.Load<Material>("FX/M_PixelFx");
            }
        }
        _trail.sharedMaterial = s_trailMaterial;
        _trail.time = _trailTime;
        _trail.minVertexDistance = 0.06f;
        _trail.numCapVertices = 0;
        _trail.numCornerVertices = 0;
        _trail.widthMultiplier = _trailWidth;
        _trail.widthCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0f));
        var grad = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(_trailColor, 0f), new GradientColorKey(_trailColor, 1f) },
            new[] { new GradientAlphaKey(_trailColor.a, 0f), new GradientAlphaKey(0f, 1f) });
        _trail.colorGradient = grad;
        _trail.sortingOrder = _trailSortingOrder;
        _trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        _trail.receiveShadows = false;
    }
}
