using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 새 주인공용 픽셀 리그 (PixelRigDesign.md). 뼈 = Transform 계층, 부위 = SpriteRenderer, 애니 = AnimationClip을 직접 샘플링.
/// 프리팹은 에디터 스크립트 PixelRigBuilder가 만든다. 기존 애니 이름(평타·스킬 수십 종)은 <see cref="MapName"/>으로 기본 클립에 묶는다.
/// </summary>
public class PixelRig : CharacterRig
{
    [Serializable]
    public class Clip
    {
        public string name;
        public AnimationClip clip;
        public bool loop;
    }

    [SerializeField] private Clip[] _clips = new Clip[0];
    [SerializeField] private SpriteRenderer[] _renderers = new SpriteRenderer[0];
    [SerializeField] private Transform[] _bones = new Transform[0];
    [SerializeField] private Transform _socketRight;
    [SerializeField] private Transform _socketLeft;
    [Tooltip("스프라이트 PPU — 뼈 위치를 이 격자(아트 1px)에 맞춘다")]
    [SerializeField] private float _ppu = 36f;
    [Tooltip("무기 스프라이트 정렬 순서(앞손 / 뒷손)")]
    [SerializeField] private int _weaponOrderFront = 48;
    [SerializeField] private int _weaponOrderBack = 37;
    // 공중 공격 다리: 꼭대기 포즈와 같은 접은 다리 그림(빌더가 넣는다). 없으면 다리를 2px 통째로 올린다
    [SerializeField] private Sprite _airLegF;
    [SerializeField] private Sprite _airLegB;
    // 셀 이동·점프(Q-423, PixelRigDesign 3.7): 이 이름의 클립이 재생되면 부위 그림 대신 셀 프레임을 보여 주고 무기는 프레임별 손에 붙인다
    [SerializeField] private SpriteRenderer _cel;
    [SerializeField] private CelClip[] _celClips = new CelClip[0];

    private const string WeaponDir = "PixelRig/Weapons/";
    private const float WeaponWorldScale = 1f; // 도트 무기 24 PPU × 1 = 아트 1px 1/24유닛(몹·캐릭터와 같은 밀도)
    private static readonly Dictionary<string, PixelWeapon> PixelWeapons = new Dictionary<string, PixelWeapon>();
    private readonly List<Transform> _upright = new List<Transform>();
    private readonly Dictionary<Transform, SpriteRenderer> _uprightRenderers = new Dictionary<Transform, SpriteRenderer>();
    private bool _ranged;
    // 뒷손 무기(방패 등): 막기 동작 중에는 몸통 앞에 그린다(평소엔 몸 뒤 — 앞으로 내민 방패가 몸통에 가려졌다)
    private readonly List<SpriteRenderer> _backWeapon = new List<SpriteRenderer>();
    private const int GuardBackWeaponOrder = 47;
    // 로프·사다리에 매달리는 동안 무기는 숨긴다(칼날이 머리 위로 솟았다 — 상업 플랫포머 관례)
    private readonly List<SpriteRenderer> _weaponRenderers = new List<SpriteRenderer>();
    private bool _weaponsHidden;
    private readonly Dictionary<string, Clip> _byName = new Dictionary<string, Clip>();
    private readonly Dictionary<string, Clip> _resolved = new Dictionary<string, Clip>();
    private Clip _current;
    private string _requested;
    private float _time;
    private float _globalSpeed = 1f;
    private float _clipSpeed = 1f;
    // 점프 세분(상승·꼭대기·낙하)과 착지 눌림 — 플레이어 코드는 Play("Jump") 하나만 부른다
    private Rigidbody2D _body;
    private float _playedAt;
    private float _landUntil;
    private Clip _launch, _up, _apex, _fall, _land, _hurt, _kneel, _fidget;
    // 대기 습관: 대기로 6초 넘게 서 있으면 배낭 추켜올리기를 한 번, 이후 다시 6초
    private const float FidgetAfter = 6f;
    private float _fidgetAt = -1f;
    // 달리기 발걸음 먼지: 발이 땅에 닿는 포즈(빌더 Run 6포즈 중 앞발 0·뒷발 3)에 들어설 때 발 뒤에 한 번
    private const int RunPoses = 6;
    private static readonly int[] RunContactPoses = { 0, 3 };
    private const float StepDustBehind = 0.12f;
    private Clip _run;
    private int _runPose = -1;
    // 걷기 발소리: 8포즈 중 발이 닿는 0·4(먼지 없이 소리만)
    private const int WalkPoses = 8;
    private static readonly int[] WalkContactPoses = { 0, 4 };
    private Clip _walk;
    private int _walkPose = -1;
    // 달리다 방향 전환: 좌우가 바뀌는 순간 새 방향 발 뒤로 차고 나가는 먼지(1.2배)
    private const float TurnDustScale = 1.2f;
    private float _lastFacing;
    // 제동: 달리기 → 대기로 바뀌면 잠깐 버티는 포즈와 발 앞 먼지(클립 길이 0.14보다 짧게 — 끝 시점은 첫 포즈로 돌아간다)
    private const float BrakeTime = 0.13f;
    private const float BrakeDustAhead = 0.2f;
    // 도약 먼지: 땅 동작에서 점프로 바뀌는 순간 양발 바깥으로 작게(착지 먼지보다 작게 — 차고 오르는 힘)
    private const float JumpDustSpread = 0.16f;
    private const float JumpDustScale = 0.75f;
    private Clip _brake;
    private float _brakeUntil;
    // 피격 흰 번쩍임: 맞는 순간 2프레임 남짓 모든 부위를 흰 단색으로(몹 피격 플래시와 같은 All In 1 HITEFFECT)
    private const float FlashTime = 0.07f;
    private static Material _flashMaterial;
    private Material[] _baseMaterials;
    private float _flashUntil;
    private bool _flashing;
    // 사망: 무릎 꿇음 → 0.3초 뒤 몸 전체를 90° 눕힘(도트 90° 회전은 지글거림 없음). 부활하면 원래대로
    private float _diedAt = -1f;
    private const float KneelTime = 0.3f;
    private const float LieLiftPx = 11f; // 눕힌 몸이 땅에 묻히지 않게 들어 올리는 아트 픽셀(몸 반폭)
    private Vector3 _restLocalPos;
    private float _hurtUntil;
    private const int HurtEyeParam = 4;
    private const float HurtTime = 0.22f;
    private const float ApexBand = 1.5f;   // |세로 속도| 이하면 꼭대기 포즈
    private const float LaunchTime = 0.06f;
    private const float LandTime = 0.1f;
    // 착지 무게감: 공중 최대 낙하 속도로 세기(0~1) — 높이 떨어질수록 눌림이 길고 먼지가 크다(보통 점프 ≈ 0.6)
    private const float HardLandVy = 14f;
    private const float HardLandExtra = 0.6f; // 세기 1이면 눌림 시간 +60%
    private float _minVy;
    // 걸어서 떨어짐: 컨트롤러는 점프 없이 발판에서 떨어지면 걷기·대기를 그대로 둬서 허공에서 걸었다 → 리그가 낙하 포즈·착지 연출
    private const float LedgeFallVy = 3f;
    private const float LedgeLandVy = 0.5f;
    private bool _ledgeFall;
    private readonly Dictionary<string, CelClip> _celByName = new Dictionary<string, CelClip>();
    private bool _celShown;
    private float _landStart;
    // 점프 v2(PixelRigDesign 3.6): 입력 뒤 뜨기 전엔 웅크림, 뜬 뒤엔 세로 속도로 연속 자세(JumpArc) 시점을 고른다
    private const float TakeoffVy = 0.5f;
    private const float CrouchMax = 0.2f;
    private const float ArcSpan = 16f; // 세로 속도 +8 → 0, 0 → 0.5, -8 → 1
    private Clip _crouch, _arc;
    private bool _tookOff;
    private float _arcAt = -1f;

    /// <summary>마지막 착지 세기 0~1 (PlayerController 가 착지 먼지 크기에 쓴다)</summary>
    public float LastLandStrength { get; private set; } = 0.6f;
    private const float AirAttackVy = 0.6f;
    private const float UprightForwardPx = 2f; // 세워 드는 무기를 손 앞으로 내미는 아트 픽셀 // 세로 속도가 이보다 크면 공중으로 본다
    private Transform _legF, _legB;
    private SpriteRenderer _legFRenderer, _legBRenderer;

    private void Awake()
    {
        foreach (Clip c in _clips)
        {
            if (c.clip != null)
            {
                _byName[c.name] = c;
            }
        }
        _body = GetComponentInParent<Rigidbody2D>();
        _legF = transform.Find("LegF");
        _legB = transform.Find("LegB");
        _legFRenderer = _legF != null ? _legF.GetComponent<SpriteRenderer>() : null;
        _legBRenderer = _legB != null ? _legB.GetComponent<SpriteRenderer>() : null;
        _byName.TryGetValue("JumpLaunch", out _launch);
        _byName.TryGetValue("Jump", out _up);
        _byName.TryGetValue("JumpApex", out _apex);
        _byName.TryGetValue("JumpFall", out _fall);
        _byName.TryGetValue("Land", out _land);
        _byName.TryGetValue("Hurt", out _hurt);
        _byName.TryGetValue("DeathKneel", out _kneel);
        _byName.TryGetValue("IdleFidget", out _fidget);
        _byName.TryGetValue("JumpCrouch", out _crouch);
        foreach (CelClip cc in _celClips)
        {
            if (cc != null && cc.sprites.Length > 0)
            {
                _celByName[cc.name] = cc;
            }
        }
        _byName.TryGetValue("JumpArc", out _arc);
        _byName.TryGetValue("Run", out _run);
        _byName.TryGetValue("Walk", out _walk);
        _byName.TryGetValue("Brake", out _brake);
        _restLocalPos = transform.localPosition;
        Play("Idle");
    }

    public override void Play(string anim)
    {
        // 점프 중에 땅 동작으로 바뀌면 착지 눌림을 잠깐 보여 준다
        if (_requested == "Jump" && _land != null && (anim == "Idle" || anim == "Walk" || anim == "Run"))
        {
            LastLandStrength = Mathf.Clamp01(-_minVy / HardLandVy);
            _landStart = Time.time;
            _landUntil = Time.time + LandTime * (1f + HardLandExtra * LastLandStrength);
            Footsteps.Land(LastLandStrength);
        }
        if (_requested == "Run" && anim == "Idle" && _brake != null)
        {
            _brakeUntil = Time.time + BrakeTime;
            bool left = transform.lossyScale.x < 0f;
            // 먼지는 앞쪽으로 흩어진다(발걸음 먼지 그림을 반대로 뒤집어 앞으로 굴린다)
            Vfx.Play(VfxId.StepDust, transform.position + Vector3.right * (left ? -BrakeDustAhead : BrakeDustAhead), !left);
        }
        else if (anim != "Idle")
        {
            _brakeUntil = 0f;
        }
        if (anim == "Jump" && (_requested == "Idle" || _requested == "Walk" || _requested == "Run"))
        {
            Vector3 feet = transform.position;
            Vfx.Play(VfxId.StepDust, feet + Vector3.left * JumpDustSpread, false, JumpDustScale, true);
            Vfx.Play(VfxId.StepDust, feet + Vector3.right * JumpDustSpread, true, JumpDustScale, true);
        }
        if (anim == "Jump" && _requested != "Jump")
        {
            _minVy = 0f;
            _tookOff = false;
        }
        _playedAt = Time.time;
        _current = Resolve(anim);
        _requested = anim;
        _time = 0f;
        _clipSpeed = 1f;
    }

    public override void CrossFade(string anim, float fade)
    {
        if (_requested == anim)
        {
            return;
        }
        Play(anim);
    }

    public override bool IsPlaying(string anim)
    {
        return _requested == anim;
    }

    public override void SetAnimationSpeed(float speed)
    {
        _globalSpeed = speed;
    }

    public override void SetAnimationSpeed(string anim, float speed)
    {
        if (_requested == anim)
        {
            _clipSpeed = speed;
        }
    }

    public override void SetMeshAlphaAll(float alpha)
    {
        foreach (SpriteRenderer r in _renderers)
        {
            Color c = r.color;
            c.a = alpha;
            r.color = c;
        }
    }

    public override void SetControlParamInt(string param, int value)
    {
        // 예전 AnyPortrait 의 "Eye Param" 4 = 피격 표정 신호(PlayerHealthManager). 새 주인공은 눈을 앞머리로 가리므로
        // 대신 몸 전체 피격 포즈를 잠깐 보여 준다
        if (param == "Eye Param" && value == HurtEyeParam)
        {
            _hurtUntil = Time.time + HurtTime;
            _flashUntil = Time.time + FlashTime;
        }
    }

    public override Transform GetBoneSocket(string socket)
    {
        return socket == "LeftWeapon" ? _socketLeft : _socketRight;
    }

    public override void SetMeshImage(string part, Texture2D texture)
    {
        // 방어구 외형 교체는 레이어 설계(CharacterPixelRules 6절) 뒤에 붙인다
    }

    public override void AdoptWeapon(Transform weapon, bool isLeft)
    {
        if (!isLeft)
        {
            _ranged = false; // 오른손(주무기)이 활이면 아래에서 다시 켠다
            _resolved.Clear();
        }
        int order = isLeft ? _weaponOrderBack : _weaponOrderFront;
        if (isLeft)
        {
            _backWeapon.Clear();
        }
        _weaponRenderers.RemoveAll(x => x == null);
        foreach (SpriteRenderer r in weapon.GetComponentsInChildren<SpriteRenderer>(true))
        {
            r.sortingLayerID = _renderers.Length > 0 ? _renderers[0].sortingLayerID : r.sortingLayerID;
            r.sortingOrder = order;
            UsePixelWeapon(r);
            if (!_weaponRenderers.Contains(r))
            {
                _weaponRenderers.Add(r);
            }
            r.enabled = !_weaponsHidden; // 매달린 채 무기를 바꿔도 숨김 유지
            if (isLeft)
            {
                _backWeapon.Add(r);
            }
        }
    }

    /// <summary>
    /// 같은 이름의 도트 무기 그림(Resources/PixelRig/Weapons, 칼날 위·손잡이 피벗·24 PPU)이 있으면 바꿔 끼운다.
    /// 기존 무기 그림은 AnyPortrait 용 고해상도라 도트 격자에서 가는 막대로 뭉개졌다(PixelRigDesign 3.5절 무기 쥐기).
    /// 크기는 캐릭터 도트와 같게(아트 1px = 1/24유닛) — 부모 배율을 상쇄한다.
    /// </summary>
    private void UsePixelWeapon(SpriteRenderer r)
    {
        if (r.sprite == null)
        {
            return;
        }
        PixelWeapon pw = LoadPixelWeapon(r.sprite.name);
        if (pw == null)
        {
            return;
        }
        r.sprite = pw.sprite;
        if (pw.sprite.name.IndexOf("bow", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            _ranged = true; // 활: 시전(Cast) 동작 대신 쏘기(Shoot)
        }
        Transform t = r.transform;
        t.localPosition = new Vector3(0f, 0f, t.localPosition.z);
        // 그림 속 칼날 방향(angle)을 위(90°)로 맞춘다 — 손목 회전이 칼날 각도를 정한다
        t.localRotation = Quaternion.Euler(0f, 0f, 90f - pw.angle);
        float parent = t.parent != null ? Mathf.Abs(t.parent.lossyScale.y) : 1f;
        float s = parent > 0.0001f ? WeaponWorldScale / parent : 1f;
        t.localScale = new Vector3(s, s, 1f);
        if (pw.upright && !_upright.Contains(t))
        {
            _upright.Add(t); // 방패·너클·활·지팡이: 손목 각도와 상관없이 똑바로
            _uprightRenderers[t] = r;
        }
        // 세워 드는 무기는 그림의 큰 덩어리가 손잡이보다 뒤(얼굴 쪽)에 있으면 좌우를 뒤집어 앞으로 — 해골 지팡이 머리가 얼굴을 가렸다
        r.flipX = pw.upright && pw.sprite.pivot.x > pw.sprite.rect.width * 0.5f;
    }

    private class PixelWeapon
    {
        public Sprite sprite;
        public float angle = 90f;
        public bool upright;
    }

    [Serializable]
    private class PixelWeaponMeta
    {
        public float angle = 90f;
        public bool upright;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        PixelWeapons.Clear(); // 도메인 리로드 없이 플레이할 때 지난 무기 정보가 남지 않게
        _flashMaterial = null;
    }

    private static PixelWeapon LoadPixelWeapon(string name)
    {
        PixelWeapon pw;
        if (PixelWeapons.TryGetValue(name, out pw))
        {
            return pw;
        }
        Sprite sp = Resources.Load<Sprite>(WeaponDir + name);
        if (sp != null)
        {
            pw = new PixelWeapon { sprite = sp };
            TextAsset meta = Resources.Load<TextAsset>(WeaponDir + name);
            if (meta != null)
            {
                PixelWeaponMeta m = JsonUtility.FromJson<PixelWeaponMeta>(meta.text);
                pw.angle = m.angle;
                pw.upright = m.upright;
            }
        }
        PixelWeapons[name] = pw;
        return pw;
    }

    private void LateUpdate()
    {
        if (_current == null || _current.clip == null)
        {
            return;
        }
        // 걷기·달리기는 이동 속도 배율만큼 빨리(버프로 빨라져도 발이 미끄러지지 않게)
        float loco = _requested == "Walk" || _requested == "Run" ? Mathf.Clamp(StatMath.MoveSpeedMul, 0.5f, 2f) : 1f;
        _time += Time.deltaTime * _globalSpeed * _clipSpeed * loco;
        Clip c = PickVariant();
        float len = Mathf.Max(c.clip.length, 0.0001f);
        float t = c == _arc && _arcAt >= 0f ? _arcAt * len : c == _fidget && _fidgetAt >= 0f ? Mathf.Min(_fidgetAt, len) : c.loop ? Mathf.Repeat(_time, len) : Mathf.Min(_time, len);
        c.clip.SampleAnimation(gameObject, t);
        if (_requested == "Jump" && _body != null)
        {
            _minVy = Mathf.Min(_minVy, _body.linearVelocity.y);
        }
        StepDust(c, t, len);
        ApplyCel(c);
        UpdateFlash();
        TuckLegsInAir(c);
        ApplyDeathLie();
        SortBackWeapon(c);
        HideWeaponsOnRope();
        SnapBones();
        KeepUpright();
    }

    /// <summary>
    /// 셀 클립이 있는 동작이면 부위 그림을 숨기고 셀 프레임을 보여 준다. 무기 소켓(앞손·뒷손 뼈)은 그 프레임의 손 좌표로 옮기고 칼날은 앞 위 45°.
    /// 프레임 시점: 걷기·달리기 = 클립 경과(_time, 이동 속도 배율 반영), 공중 = 세로 속도(_arcAt), 착지 = 착지 시작부터 눌림 구간 비율.
    /// </summary>
    private void ApplyCel(Clip c)
    {
        CelClip cel = null;
        if (_cel != null && c != null && _diedAt < 0f)
        {
            _celByName.TryGetValue(c.name, out cel);
        }
        if (cel == null)
        {
            ShowCel(false);
            return;
        }
        int i;
        if (c == _arc)
        {
            i = cel.IndexAtNormalized(_arcAt);
        }
        else if (c == _land)
        {
            float span = Mathf.Max(0.0001f, _landUntil - _landStart);
            i = cel.IndexAtNormalized((Time.time - _landStart) / span);
        }
        else
        {
            i = cel.IndexAt(_time);
        }
        ShowCel(true);
        _cel.sprite = cel.sprites[i];
        Transform shF = _socketRight != null ? _socketRight.parent.parent : null;
        if (shF != null && i < cel.hand.Length)
        {
            shF.localRotation = Quaternion.identity;
            _socketRight.parent.position = transform.TransformPoint(cel.hand[i]);
            // 손이 몸 뒤로 갈수록 칼을 세운다(45° → 100°) — 팔을 뒤로 흔들 때 앞으로 눕힌 칼날이 몸·얼굴을 가로질렀다
            float back = Mathf.InverseLerp(CelHandFrontPx, CelHandBackPx, cel.hand[i].x * _ppu);
            _socketRight.parent.localRotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(CelBladeAngle, CelBladeBack, back) - 90f);
        }
        if (_socketLeft != null && i < cel.handB.Length)
        {
            _socketLeft.parent.position = transform.TransformPoint(cel.handB[i]);
        }
    }

    private const float CelBladeAngle = 45f;
    private const float CelBladeBack = 100f;
    private const float CelHandFrontPx = 6f;  // 손 x(엉덩이 기준 아트 px)가 이만큼 앞이면 45°
    private const float CelHandBackPx = -6f;  // 이만큼 뒤면 100°
    private const float CelCrouchMin = 0.06f; // 물리 점프가 입력 즉시 떠서, 뜬 뒤에도 처음 이만큼은 웅크림을 보여 준다

    private void ShowCel(bool on)
    {
        if (on == _celShown || _cel == null)
        {
            return;
        }
        _celShown = on;
        _cel.enabled = on;
        foreach (SpriteRenderer r in _renderers)
        {
            if (r != null && r != _cel)
            {
                r.enabled = !on;
            }
        }
    }

    /// <summary>땅 동작 중 하강하면 낙하 포즈, 멈추면 착지 눌림·먼지(점프로 떨어질 때와 같게)</summary>
    private Clip LedgeFall(float now)
    {
        if (_body == null || _fall == null || !(_requested == "Idle" || _requested == "Walk" || _requested == "Run"))
        {
            _ledgeFall = false;
            return null;
        }
        float vy = _body.linearVelocity.y;
        if (vy < -LedgeFallVy)
        {
            if (!_ledgeFall)
            {
                _ledgeFall = true;
                _minVy = 0f;
            }
            _minVy = Mathf.Min(_minVy, vy);
            return LedgeFallPose(vy);
        }
        if (_ledgeFall && vy > -LedgeLandVy)
        {
            _ledgeFall = false;
            LastLandStrength = Mathf.Clamp01(-_minVy / HardLandVy);
            _landStart = now;
            _landUntil = now + LandTime * (1f + HardLandExtra * LastLandStrength);
            DustPuffs.Land(transform.position, LastLandStrength);
            Footsteps.Land(LastLandStrength);
            return _land;
        }
        return _ledgeFall ? LedgeFallPose(vy) : null;
    }

    private Clip LedgeFallPose(float vy)
    {
        if (_arc == null)
        {
            return _fall;
        }
        _arcAt = Mathf.Clamp01(0.5f - vy / ArcSpan);
        return _arc;
    }

    /// <summary>피격 직후 FlashTime 동안 부위 머티리얼을 흰 단색으로, 끝나면 원래 머티리얼로</summary>
    private void UpdateFlash()
    {
        bool want = Time.time < _flashUntil;
        if (want == _flashing)
        {
            return;
        }
        if (want && !EnsureFlashMaterial())
        {
            return;
        }
        _flashing = want;
        if (_baseMaterials == null || _baseMaterials.Length != _renderers.Length)
        {
            _baseMaterials = new Material[_renderers.Length];
            for (int i = 0; i < _renderers.Length; i++)
            {
                _baseMaterials[i] = _renderers[i] != null ? _renderers[i].sharedMaterial : null;
            }
        }
        for (int i = 0; i < _renderers.Length; i++)
        {
            if (_renderers[i] != null)
            {
                _renderers[i].sharedMaterial = want ? _flashMaterial : _baseMaterials[i];
            }
        }
    }

    private static bool EnsureFlashMaterial()
    {
        if (_flashMaterial != null)
        {
            return true;
        }
        var template = Resources.Load<Material>("FX/M_EnemySprite");
        if (template == null || !template.HasProperty("_HitEffectBlend"))
        {
            return false;
        }
        _flashMaterial = new Material(template) { name = "HeroFlash" };
        _flashMaterial.SetFloat("_HitEffectBlend", 1f);
        _flashMaterial.SetColor("_HitEffectColor", Color.white);
        _flashMaterial.SetFloat("_HitEffectGlow", 1f);
        return true;
    }

    /// <summary>달리기에서 발 닿는 포즈로 넘어가는 순간 발 뒤에 도트 먼지(Px_StepDust). 다른 클립이면 포즈 기록을 지운다</summary>
    private void StepDust(Clip c, float t, float len)
    {
        float facing = Mathf.Sign(transform.lossyScale.x);
        if (_lastFacing != 0f && facing != _lastFacing && c == _run)
        {
            bool turnLeft = facing < 0f;
            Vfx.Play(VfxId.StepDust, transform.position + Vector3.right * (turnLeft ? StepDustBehind : -StepDustBehind), turnLeft, TurnDustScale, true);
        }
        _lastFacing = facing;
        WalkStep(c, t, len);
        if (c != _run)
        {
            _runPose = -1;
            return;
        }
        int pose = Mathf.Min(RunPoses - 1, (int)(t / len * RunPoses));
        if (pose == _runPose)
        {
            return;
        }
        _runPose = pose;
        if (System.Array.IndexOf(RunContactPoses, pose) < 0)
        {
            return;
        }
        bool left = transform.lossyScale.x < 0f;
        Vector3 feet = transform.position + Vector3.right * (left ? StepDustBehind : -StepDustBehind);
        Vfx.Play(VfxId.StepDust, feet, left);
        Footsteps.Contact(true);
    }

    /// <summary>걷기 발 닿는 포즈로 넘어가는 순간 발소리</summary>
    private void WalkStep(Clip c, float t, float len)
    {
        if (c != _walk)
        {
            _walkPose = -1;
            return;
        }
        int pose = Mathf.Min(WalkPoses - 1, (int)(t / len * WalkPoses));
        if (pose == _walkPose)
        {
            return;
        }
        _walkPose = pose;
        if (System.Array.IndexOf(WalkContactPoses, pose) >= 0)
        {
            Footsteps.Contact(false);
        }
    }

    /// <summary>점프는 세로 속도로 상승·꼭대기·낙하를 고르고, 막 착지했으면 눌림 포즈</summary>
    private void OnEnable()
    {
        PlayerHealthManager.PlayerDied += OnDied;
        PlayerHealthManager.PlayerRespawned += OnRespawned;
    }

    private void OnDisable()
    {
        PlayerHealthManager.PlayerDied -= OnDied;
        PlayerHealthManager.PlayerRespawned -= OnRespawned;
    }

    private void OnDied()
    {
        _diedAt = Time.time;
        _restLocalPos = transform.localPosition;
    }

    private void OnRespawned()
    {
        _diedAt = -1f;
        transform.localRotation = Quaternion.identity;
        transform.localPosition = _restLocalPos;
    }

    /// <summary>사망 중이면 무릎 꿇음 클립, 0.3초 뒤부터는 몸 전체를 뒤로 90° 눕힌다(발 원점 기준 → 몸 반폭만큼 들어 올림)</summary>
    private void ApplyDeathLie()
    {
        if (_diedAt < 0f)
        {
            return;
        }
        bool lie = Time.time - _diedAt >= KneelTime;
        transform.localRotation = lie ? Quaternion.Euler(0f, 0f, 90f) : Quaternion.identity;
        transform.localPosition = _restLocalPos + (lie ? new Vector3(0f, LieLiftPx / _ppu, 0f) : Vector3.zero);
    }

    private Clip PickVariant()
    {
        float now = Time.time;
        if (_diedAt >= 0f && _kneel != null)
        {
            return _kneel;
        }
        if (now < _hurtUntil && _hurt != null)
        {
            return _hurt;
        }
        if (now < _landUntil && (_requested == "Idle" || _requested == "Walk" || _requested == "Run"))
        {
            return _land;
        }
        if (now < _brakeUntil && _requested == "Idle" && _brake != null)
        {
            return _brake;
        }
        Clip ledge = LedgeFall(now);
        if (ledge != null)
        {
            return ledge;
        }
        if (_requested == "Idle" && _fidget != null)
        {
            float idleFor = now - _playedAt;
            float cycle = FidgetAfter + _fidget.clip.length;
            float inCycle = idleFor % cycle;
            if (idleFor >= FidgetAfter && inCycle >= FidgetAfter)
            {
                _fidgetAt = inCycle - FidgetAfter; // 습관 클립 안의 시각
                return _fidget;
            }
            _fidgetAt = -1f;
        }
        if (_requested != "Jump" || _body == null || _up == null)
        {
            return _current;
        }
        float vy = _body.linearVelocity.y;
        if (_arc != null)
        {
            if (!_tookOff && vy > TakeoffVy)
            {
                _tookOff = true;
            }
            if (_crouch != null && ((!_tookOff && now - _playedAt < CrouchMax) || now - _playedAt < CelCrouchMin))
            {
                return _crouch;
            }
            _arcAt = Mathf.Clamp01(0.5f - vy / ArcSpan);
            return _arc;
        }
        if (_launch != null && now - _playedAt < LaunchTime)
        {
            return _launch;
        }
        if (vy > ApexBand)
        {
            return _up;
        }
        if (vy < -ApexBand && _fall != null)
        {
            return _fall;
        }
        return _apex != null ? _apex : _up;
    }

    /// <summary>
    /// 공중 공격: 공격 클립은 다리를 선 자세로 덮어써서 공중에서 똑바로 서 휘둘렀다 → 떠 있으면 다리를 접는다(꼭대기 포즈와 같은 2px)
    /// </summary>
    private void TuckLegsInAir(Clip c)
    {
        if (_body == null || _legF == null || c == _up || c == _apex || c == _fall || c == _launch || c == _land || c == _hurt || c == _arc || c == _crouch)
        {
            return;
        }
        if (_requested == "Jump" || _requested == "Idle" || _requested == "Walk" || _requested == "Run" || _requested == "Dash" || (_requested != null && _requested.StartsWith("Rope")))
        {
            return;
        }
        if (Mathf.Abs(_body.linearVelocity.y) < AirAttackVy)
        {
            return;
        }
        float px = 1f / _ppu;
        if (_airLegF != null && _airLegB != null && _legFRenderer != null && _legBRenderer != null)
        {
            // 통째로 올리면 다리가 몸통 뒤로 숨어 사라져 보였다 → 접은 다리 그림 + 1px(꼭대기 포즈와 같게)
            _legFRenderer.sprite = _airLegF;
            _legBRenderer.sprite = _airLegB;
            _legF.localPosition += new Vector3(0f, px, 0f);
            _legB.localPosition += new Vector3(0f, px, 0f);
            return;
        }
        _legF.localPosition += new Vector3(px, 2f * px, 0f);
        _legB.localPosition += new Vector3(0f, 2f * px, 0f);
    }

    private void HideWeaponsOnRope()
    {
        bool hide = _requested != null && _requested.StartsWith("Rope");
        if (hide == _weaponsHidden)
        {
            return;
        }
        _weaponsHidden = hide;
        for (int i = _weaponRenderers.Count - 1; i >= 0; i--)
        {
            if (_weaponRenderers[i] == null)
            {
                _weaponRenderers.RemoveAt(i);
                continue;
            }
            _weaponRenderers[i].enabled = !hide;
        }
    }

    private void SortBackWeapon(Clip c)
    {
        int order = c != null && c.name == "Guard" ? GuardBackWeaponOrder : _weaponOrderBack;
        for (int i = _backWeapon.Count - 1; i >= 0; i--)
        {
            if (_backWeapon[i] == null)
            {
                _backWeapon.RemoveAt(i);
                continue;
            }
            _backWeapon[i].sortingOrder = order;
        }
    }

    /// <summary>손에 끼우는 무기(방패·너클·활)는 손목이 돌아도 화면에서 똑바로 — 좌우 반전은 부모 배율이 처리한다</summary>
    private void KeepUpright()
    {
        for (int i = _upright.Count - 1; i >= 0; i--)
        {
            Transform t = _upright[i];
            if (t == null)
            {
                _upright.RemoveAt(i);
                continue;
            }
            t.rotation = Quaternion.identity;
            // 손 앞으로 2px 내밀어 쥔다(넓은 지팡이·깃발이 얼굴에 걸치지 않게). 좌우 반전은 루트 배율 부호로
            float forward = Mathf.Sign(transform.lossyScale.x) * UprightForwardPx / 24f;
            Vector3 hold = t.parent != null ? t.parent.position : t.position;
            t.position = new Vector3(hold.x + forward, hold.y, t.position.z);
            // 바닥(리그 원점 = 발바닥)보다 내려가면 그만큼 1px 단위로 들어 올린다 — 방패·지팡이·활 아래쪽이 땅에 묻혔다
            if (_diedAt < 0f && _uprightRenderers.TryGetValue(t, out SpriteRenderer sr) && sr != null)
            {
                float below = transform.position.y - sr.bounds.min.y;
                if (below > 0f)
                {
                    float px = 1f / 24f;
                    t.position += new Vector3(0f, Mathf.Ceil(below / px) * px, 0f);
                }
            }
        }
    }

    /// <summary>뼈 위치를 아트 픽셀 격자에 맞춘다 — 부위가 반 픽셀에 걸려 번지지 않게</summary>
    private void SnapBones()
    {
        float inv = 1f / _ppu;
        foreach (Transform b in _bones)
        {
            Vector3 p = b.localPosition;
            p.x = Mathf.Round(p.x * _ppu) * inv;
            p.y = Mathf.Round(p.y * _ppu) * inv;
            b.localPosition = p;
        }
    }

    private Clip Resolve(string anim)
    {
        if (string.IsNullOrEmpty(anim))
        {
            return _current;
        }
        Clip c;
        if (_resolved.TryGetValue(anim, out c))
        {
            return c;
        }
        string mapped = MapName(anim);
        if (_ranged && mapped == "Cast")
        {
            mapped = "Shoot";
        }
        if (!_byName.TryGetValue(anim, out c) && !_byName.TryGetValue(mapped, out c))
        {
            _byName.TryGetValue("Idle", out c);
        }
        _resolved[anim] = c;
        return c;
    }

    /// <summary>기존 AnyPortrait 애니 이름 → 기본 클립 이름 (PixelRigDesign.md 2절)</summary>
    public static string MapName(string anim)
    {
        string a = anim.ToLowerInvariant();
        if (a.StartsWith("rope"))
        {
            return a.Contains("stay") ? "RopeStay" : "RopeUp";
        }
        if (a.Contains("shield"))
        {
            return "Guard";
        }
        if (a.Contains("spin"))
        {
            return "Spin";
        }
        if (a.Contains("staff") || a.Contains("buff") || a.Contains("blow") || a.Contains("spead") || a.Contains("cast"))
        {
            return "Cast";
        }
        if (a.Contains("stab") || a.Contains("straight") || a.Contains("punch"))
        {
            return "Stab";
        }
        if (a.Contains("up"))
        {
            return "SwingUp";
        }
        if (a.Contains("side") || a.Contains("wide") || a.Contains("vesh") || a.Contains("spin") || a.Contains("cross"))
        {
            return "SwingSide";
        }
        return "SwingDown";
    }
}
