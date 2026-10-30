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
    private bool _mainBehind;
    [SerializeField] private int _weaponOrderBack = 37;
    // 공중 공격 다리: 꼭대기 포즈와 같은 접은 다리 그림(빌더가 넣는다). 없으면 다리를 2px 통째로 올린다
    [SerializeField] private Sprite _airLegF;
    [SerializeField] private Sprite _airLegB;
    // 셀 이동·점프(Q-423, PixelRigDesign 3.7): 이 이름의 클립이 재생되면 부위 그림 대신 셀 프레임을 보여 주고 무기는 프레임별 손에 붙인다
    [SerializeField] private SpriteRenderer _cel;
    [SerializeField] private SpriteRenderer _celFist; // 무기 위 주먹(정렬 = 앞 무기 + 1)
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
    private CelClip _fxCel; // 마스터 모션 이펙트(MotionFxTable): 지금 셀과 이미 지난 프레임
    private int _fxFrame = -1;
    // 무기군 타이밍 프로파일(MotionSpec39 §3, 10-30 GPT "같은 몸이라도 무기군별 준비·회복 비율로 전투 맛을 달리"): 타격 프레임 앞 = 준비, 뒤 = 회복
    private float _windupMul = 1f;
    private float _recoverMul = 1f;
    private int _celPhase;
    private readonly Dictionary<string, int> _strikeFrame = new Dictionary<string, int>();
    private MotionVariants.Variant _variant; // 재생 프로파일(구간·속도·정지·반복, MotionVariants)
    private string _requestOverride; // 요청 이름으로 정해지는 대체 셀(단검·대검 분기, RequestOverride), 없으면 null
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
    private static readonly int[] CelWalkContacts = { 1, 5 }; // 셀 걷기에서 보폭이 가장 큰(발이 닿는) 프레임 — 발 벌림 13·20px
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
    private CelClip _endCel;   // 공격이 끝나 대기·걷기로 돌아갈 때 잠깐 보여 줄 회복 셀("<셀>End") — 콤보가 이어지면 안 쓰인다
    private float _endStart;
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
    // 던파식 장비 레이어 합성 결과(클립 이름 → 프레임 스프라이트). 장비가 바뀔 때만 SetEquipmentLayers 가 다시 만든다
    private readonly Dictionary<string, Sprite[]> _celComposite = new Dictionary<string, Sprite[]>();
    private readonly List<Sprite> _composedSprites = new List<Sprite>();
    private bool _celShown;
    private float _landStart;
    // 점프 v2(PixelRigDesign 3.6): 입력 뒤 뜨기 전엔 웅크림, 뜬 뒤엔 세로 속도로 연속 자세(JumpArc) 시점을 고른다
    private const float TakeoffVy = 0.5f;
    private const float CrouchMax = 0.2f;
    private const float ArcSpan = 16f; // 세로 속도 +8 → 0, 0 → 0.5, -8 → 1
    private Clip _crouch, _arc;
    private bool _tookOff;
    private float _arcAt = -1f;
    private float _bladeAt = 375f; // 표시 중인 칼 각도(앞 15° = 375° 로 펴서 앞↔뒤 전환이 늘 아래쪽으로 돈다)

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
        bool toGround = anim == "Idle" || anim == "Walk" || anim == "Run";
        _endCel = null;
        if (toGround && _current != null && _diedAt < 0f && _celByName != null
            && _celByName.TryGetValue(CelKey(_current.name, _requested, _variant) + "End", out CelClip end))
        {
            _endCel = end;
            _endStart = Time.time;
        }
        _playedAt = Time.time;
        _variant = MotionVariants.Find(anim);
        _current = Resolve(anim);
        _requested = anim;
        _requestOverride = RequestOverride(anim); // 매 프레임 문자열 검사 대신 요청이 바뀔 때 한 번
        _time = 0f;
        _clipSpeed = 1f;
        _fxCel = null;
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

    public override Transform CelRoot => _cel != null ? _cel.transform : transform;

    /// <summary>
    /// frame = MapName 기본 셀(Stab·SwingDown·SwingUp)의 프레임 번호(WeaponFx.StrikeFrame). 실제로 재생될 셀이 기본 셀과 다르면
    /// (GreatUpper·GreatDown·Lunge·Punch·단검 등) 그 셀의 타격 프레임(StrikeFrameOf — MotionFxTable) 시작 시각을, 표에 없으면 예전처럼 기본 셀 시각을 돌려준다.
    /// 무기군 준비 배율(SetTimingProfile)·변형 구간(MotionVariants from·speed)도 재생과 같은 규칙으로 반영한다.
    /// 예전엔 MapName 셀·속도 1 기준이라 대검 내려찍기 판정·궤적이 칼을 드는 중(0.22초)에 먼저 나왔다(10-30 모션 리뷰)
    /// </summary>
    public override float FrameStartTime(string anim, int frame)
    {
        if (string.IsNullOrEmpty(anim))
        {
            return -1f;
        }
        string baseKey = MapName(anim);
        _celByName.TryGetValue(baseKey, out CelClip baseCel);
        Clip clip = Resolve(anim);
        MotionVariants.Variant variant = MotionVariants.Find(anim);
        string key = RequestOverride(anim) ?? CelKey(clip != null ? clip.name : baseKey, anim, variant);
        if (!_celByName.TryGetValue(key, out CelClip cel))
        {
            return baseCel != null ? baseCel.StartTimeOf(frame) : -1f;
        }
        int strike = StrikeFrameOf(key);
        if (key != baseKey && strike < 0)
        {
            // 타격 프레임을 모르는 셀(예: 단검 연속 찌르기)은 예전처럼 기본 셀 시각 — 길이 비율로 옮기면 짧은 무기 공격 주기(0.3초)를 넘었다
            return baseCel != null ? baseCel.StartTimeOf(frame) : cel.StartTimeOf(frame);
        }
        int index = key == baseKey ? frame : Mathf.Min(strike, cel.sprites.Length - 1);
        float t = cel.StartTimeOf(index);
        if (variant != null && variant.baseCel == key && variant.speed > 0f)
        {
            t = Mathf.Max(0f, t - cel.StartTimeOf(variant.from)) / variant.speed; // 변형은 from 프레임부터 speed 배로(MotionVariants.IndexIn 과 같은 규칙)
        }
        if (strike >= 0 && !cel.loop && index <= strike)
        {
            t *= _windupMul; // 타격 프레임 앞(준비 구간)은 재생이 _windupMul 배 느리다(Update 의 phase)
        }
        return t;
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
        if (_socketRight != null)
        {
            _socketRight.localScale = Vector3.one; // 납도(weaponLen) 도중 무기를 바꾸면 줄어든 소켓 배율로 새 무기 크기를 보정해 거대해졌다(10-30 GPT 캡처)
        }
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
        float phase = _celPhase < 0 ? 1f / _windupMul : _celPhase > 0 ? 1f / _recoverMul : 1f;
        _time += Time.deltaTime * _globalSpeed * _clipSpeed * loco * phase;
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
    /// <summary>
    /// 마스터 모션 이펙트(10-29): 셀 이름으로 직접 재생한 동작(스킬)만 타격 프레임에 스미어·지면 충격을 띄운다.
    /// 무기 평타(Weapon)가 다른 이름으로 같은 셀을 빌려 쓸 때는 Weapon 이 이미 궤적을 띄우므로 건너뛴다(기본 3타 보존).
    /// </summary>
    /// <summary>모션 이펙트 켜기(무기만 보는 확인 캡처에서 끈다)</summary>
    public static bool MotionFxEnabled = true;
    /// <summary>동작 스미어 색(직업 테마 — 10-29 밤 직업 최종본: 사무라이 흰색·해적 금색·심해 파랑·마도학자 보라). 무기 데이터 연결 전 임시 창구</summary>
    public static PixelRampId MotionFxRamp = PixelRampId.Steel;

    private void MotionFx(CelClip cel, int i)
    {
        if (!MotionFxEnabled)
        {
            return;
        }
        if (cel != _fxCel || i < _fxFrame)
        {
            _fxCel = cel;
            _fxFrame = -1;
        }
        bool own = _requested == cel.name || (_variant != null && _variant.baseCel == cel.name);
        if (i <= _fxFrame || _diedAt >= 0f || !own)
        {
            _fxFrame = Mathf.Max(_fxFrame, i);
            return;
        }
        Transform root = CelRoot;
        string fxName = FxName(cel.name);
        bool left = root.lossyScale.x < 0f;
        float scale = Mathf.Abs(root.lossyScale.y);
        foreach (MotionFxTable.Strike s in MotionFxTable.Strikes)
        {
            if (s.anim == fxName && s.frame > _fxFrame && s.frame <= i)
            {
                Vfx.PlayThemed(s.id, root.position, left, scale, MotionFxRamp, false, false, 1f, true);
            }
        }
        if (_variant != null && WeaponFx.TryBasicCelStrike(cel.name, out VfxId basic, out int bf) && bf > _fxFrame && bf <= i)
        {
            Vfx.PlayThemed(basic, root.position, left, scale, MotionFxRamp, false, false, 1f, true);
        }
        foreach (MotionFxTable.Impact m in MotionFxTable.Impacts)
        {
            if (m.anim == fxName && m.frame > _fxFrame && m.frame <= i)
            {
                Vfx.Play(m.id, transform.TransformPoint(new Vector3(m.ahead / _ppu, 0f, 0f)), left, m.scale);
            }
        }
        _fxFrame = i;
    }

    private void ApplyCel(Clip c)
    {
        CelClip cel = null;
        if (_cel != null && c != null)
        {
            // 제동은 셀 대기 그림으로(리그 제동 포즈는 3/4 정면이라 달리기 셀에서 멈출 때 몸이 돌아 보였다), 사망은 셀 사망(무릎 → 눕기)
            // 셀이 없는 공격·스킬은 비슷한 측면 셀로(3/4 리그로 돌아가면 몸이 확 돌아 보였다): 옆베기·회전 → 내려베기, 시전 → 올려베기, 활 → 찌르기, 막기 → 대기
            string key = _diedAt >= 0f ? "Death" : c == _brake ? "Idle" : c == _current ? CelKey(c.name, _requested, _variant) : CelKey(c.name, null, null);
            // 단검·대검 분기(요청 이름으로 정해지는 대체 셀)는 Play 때 RequestOverride 로 한 번 계산해 둔다
            if (_diedAt < 0f && _requestOverride != null)
            {
                key = _requestOverride;
            }
            _celByName.TryGetValue(key, out cel);
        }
        bool ending = _endCel != null && _diedAt < 0f && c != _hurt && Time.time - _endStart < _endCel.Length;
        if (ending)
        {
            cel = _endCel;
        }
        if (cel == null)
        {
            ShowCel(false);
            return;
        }
        int i;
        if (ending)
        {
            i = cel.IndexAt(Time.time - _endStart);
        }
        else if (_diedAt >= 0f)
        {
            i = cel.IndexAt(Time.time - _diedAt);
        }
        else if (c == _hurt)
        {
            i = cel.IndexAt(HurtTime - (_hurtUntil - Time.time));
        }
        else if (c == _arc)
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
            // 걷기는 대기 그림과 같은 3번(두 발 모임)부터 — 0번부터면 출발 순간 한 번 튀었다(대기·걷기0 차이 634px, 걷기3 = 0px)
            i = _variant != null && cel.name == _variant.baseCel
                ? _variant.IndexIn(cel, _time)
                : cel.IndexAt(_time + (c.name == "Walk" ? WalkStartFrame * cel.frame : 0f));
            int strike = StrikeFrameOf(cel.name);
            _celPhase = strike < 0 || cel.loop ? 0 : i < strike ? -1 : i > strike ? 1 : 0;
        }
        ShowCel(true);
        // 전신 회전 구간: 리그를 좌우 반전(몸·무기·주먹이 함께 뒤집혀 뒤돈 것처럼), 끝나면 원래대로
        bool flip = _variant != null && _diedAt < 0f && cel.name == _variant.baseCel && _variant.FlipAt(i);
        Vector3 ls = transform.localScale;
        if ((ls.x < 0f) != flip)
        {
            ls.x = -ls.x;
            transform.localScale = ls;
        }
        MotionFx(cel, i);
        _cel.sprite = _celComposite.TryGetValue(cel.name, out Sprite[] comp) && i < comp.Length && comp[i] != null ? comp[i] : cel.sprites[i];
        if (_celFist != null)
        {
            _celFist.sprite = i < cel.fists.Length ? cel.fists[i] : null;
        }
        Transform shF = _socketRight != null ? _socketRight.parent.parent : null;
        if (shF != null && i < cel.hand.Length)
        {
            shF.localRotation = Quaternion.identity;
            _socketRight.parent.position = transform.TransformPoint(cel.hand[i]);
            // 손이 몸 뒤로 갈수록 칼을 세운다(45° → 100°) — 팔을 뒤로 흔들 때 앞으로 눕힌 칼날이 몸·얼굴을 가로질렀다
            // 두 자세로 끊는다: 손이 엉덩이보다 많이 뒤면 칼을 뒤로 눕혀 끌고(170°), 아니면 앞으로 낮게(15°).
            // 중간 각도로 보간하면 손이 뒤로 갈 때 칼이 서서(≈60°) 얼굴을 가로질렀다(달리기 2~4번, 10-26)
            // 땅 위 반복 동작(대기·걷기·달리기·제동)은 칼을 늘 앞에 — 달리다 손이 뒤로 갈 때마다 칼이 뒤로 뒤집혀 이상했다(10-26 사용자)
            bool groundLoop = cel.name == "Idle" || cel.name == "Walk" || cel.name == "Run";
            float back = !groundLoop && cel.hand[i].x * _ppu < CelHandBackPx ? 1f : 0f;
            float blade = i < cel.angles.Length && cel.angles[i] > CelClip.NoAngle + 1f
                ? cel.angles[i] // 공격 셀: 그림 속 칼에서 읽은 각도
                : Mathf.Lerp(CelBladeAngle, CelBladeBack, back);
            if (c == _arc && back > 0f)
            {
                // 공중에서 뒤로 끄는 칼은 관성으로 몸보다 늦게 따라온다: 오를 땐 칼끝이 처지고 떨어질 땐 들린다
                blade = Mathf.Lerp(CelBladeRise, CelBladeFall, _arcAt);
            }
            else if (c == _crouch && back > 0f)
            {
                blade = CelBladeRise; // 도약 웅크림도 공중과 같은 높이(땅 위 210° 보다 위)
            }
            if (groundLoop && blade == CelBladeAngle)
            {
                // 팔 흔들기를 칼끝이 따라간다: 손이 앞이면 칼끝이 들리고 뒤면 처진다(고정 15°는 막대처럼 굳어 보였다)
                blade += Mathf.Clamp((cel.hand[i].x * _ppu - CelHandSwingRef) * CelBladeSwing, -CelBladeSwingMax, CelBladeSwingMax);
            }
            bool drawn = i < cel.angles.Length && cel.angles[i] > CelClip.NoAngle + 1f;
            _socketRight.parent.localRotation = Quaternion.Euler(0f, 0f, SmoothBlade(blade, drawn) - 90f);
            KeepBladeAboveGround(drawn);
            float len = i < cel.weaponLen.Length && cel.weaponLen[i] > 0f ? cel.weaponLen[i] : 1f;
            _socketRight.localScale = new Vector3(1f, len, 1f); // 손잡이 피벗 기준으로 칼날만 짧게(칼집에 들어감)
            SetMainWeaponBehind(i < cel.behind.Length && cel.behind[i]);
        }
        if (_socketLeft != null && i < cel.handB.Length)
        {
            _socketLeft.parent.position = transform.TransformPoint(cel.handB[i]);
            // 앞손 무기(권총·두 번째 단검)는 앞을 향한다 — 손목 각도 채널이 없어 위를 향해 세워져 있었다(10-29 사용자 "무기 방향 위치 잘 맞춰라")
            float angleB = i < cel.anglesB.Length && cel.anglesB[i] > CelClip.NoAngle + 1f ? cel.anglesB[i] : 0f;
            _socketLeft.parent.localRotation = Quaternion.Euler(0f, 0f, angleB - 90f);
        }
    }

    /// <summary>
    /// 장비 외형 레이어 적용(던파식, CelEquipmentPlan): 슬롯 순서로 맨몸 셀에 지울 마스크·덮어쓰기를 합성해 캐시한다. 빈 목록이면 맨몸.
    /// </summary>
    public void SetEquipmentLayers(IEnumerable<CelEquipLayer> layers)
    {
        foreach (Sprite sp in _composedSprites)
        {
            if (sp != null)
            {
                Destroy(sp.texture);
                Destroy(sp);
            }
        }
        _composedSprites.Clear();
        _celComposite.Clear();
        List<CelEquipLayer> ordered = CelComposer.Ordered(layers ?? new CelEquipLayer[0]);
        if (ordered.Count == 0)
        {
            return;
        }
        foreach (CelClip clip in _celClips)
        {
            if (clip == null || clip.sprites.Length == 0)
            {
                continue;
            }
            Sprite[] comp = CelComposer.Compose(clip, ordered);
            _celComposite[clip.name] = comp;
            for (int i = 0; i < comp.Length; i++)
            {
                if (comp[i] != null && comp[i] != clip.sprites[i])
                {
                    _composedSprites.Add(comp[i]);
                }
            }
        }
    }

    /// <summary>
    /// 이동 자세 사이 칼 각도는 한 프레임에 뒤집지 않고 아래로 휘둘러 돈다(착지 때 170° → 15° 가 한 번에 바뀌어 튀었다). 공격 셀 각도는 그대로.
    /// </summary>
    /// <summary>
    /// 긴 무기 끝이 발바닥(리그 원점)보다 내려가면 그만큼 칼끝을 들어 올린다(10-29 사용자 "무기 움직임이 부자연스럽다":
    /// 대검·창이 짧은 칼 기준 각도(-25~-35°)로 땅을 뚫고 내려갔다). 앞 아래면 수평 쪽으로, 뒤 아래면 뒤 수평 쪽으로 6°씩.
    /// </summary>
    private void KeepBladeAboveGround(bool drawn)
    {
        if (!drawn || _weaponsHidden)
        {
            return;
        }
        Transform hand = _socketRight.parent;
        float ground = transform.position.y - 1f / 24f;
        for (int step = 0; step < 12; step++)
        {
            float low = float.MaxValue;
            foreach (SpriteRenderer r in _weaponRenderers)
            {
                if (r != null && r.enabled && !_backWeapon.Contains(r) && !_upright.Contains(r.transform) && r.transform.IsChildOf(hand))
                {
                    low = Mathf.Min(low, r.bounds.min.y);
                }
            }
            if (low >= ground || low == float.MaxValue)
            {
                return;
            }
            float z = Mathf.Repeat(hand.localEulerAngles.z + 90f, 360f); // 칼 방향(0 = 앞, 270 = 아래)
            float dir = z > 270f || z < 90f ? 6f : -6f;                  // 앞 아래 → 위로, 뒤 아래 → 아래쪽 반대로(뒤 수평 쪽)
            // 수평 근처까지만 든다 — 넓은 무기(닻 갈고리)는 수평이어도 아래 끝이 발바닥 아래라 계속 들려 위로 섰다(10-30 GPT 리뷰 "올려치기 장전에서 무기가 위로 선다")
            if ((dir > 0f && z < 90f && z + dir > 15f) || (dir < 0f && z + dir < 165f))
            {
                return;
            }
            hand.localRotation = Quaternion.Euler(0f, 0f, z + dir - 90f);
            _bladeAt = Mathf.Repeat(z + dir - 90f, 360f) + 90f;
        }
    }

    private float SmoothBlade(float target, bool exact)
    {
        target = Mathf.Repeat(target - 90f, 360f) + 90f; // 90~450: 위(90)를 끝으로 두어 아래(270)를 지나게
        if (exact)
        {
            _bladeAt = target;
        }
        else if (Mathf.Abs(target - _bladeAt) < CelBladeLagRange)
        {
            // 작은 흔들림은 손보다 살짝 늦게 따라온다(칼 무게) — 손과 똑같이 움직이면 손에 붙은 막대처럼 보였다
            _bladeAt = Mathf.Lerp(_bladeAt, target, 1f - Mathf.Exp(-CelBladeLag * Time.deltaTime));
        }
        else
        {
            // 앞↔뒤처럼 크게 바뀌면 한 번에 — 돌려서 바꾸면 중간(아래 270°)에서 칼이 땅을 찌르고 서 있었다(10-26 밤 사용자 점프 착지 스크린샷)
            _bladeAt = target;
        }
        return _bladeAt;
    }

    /// <summary>
    /// 요청 이름으로 정해지는 대체 셀(없으면 null): 단검 왼손 찌르기 → 연속 찌르기, 교차 베기 → X 베기(10-26).
    /// 대검·둔기(Great…)인데 같은 이름 셀이 없으면 양손 셀(옆 → GreatSide, 그 밖 → GreatDown) — 한손 베기 셀로 대체하면 묵직한 무기가 가볍게 보였다(10-26).
    /// 마스터 모션 셀 이름으로 바로 요청한 경우(GreatUpper·GreatSideOne 등)는 그 셀을 그대로 쓴다(10-30 품질 루프)
    /// </summary>
    private string RequestOverride(string anim)
    {
        if (string.IsNullOrEmpty(anim))
        {
            return null;
        }
        if (anim == "LeftHandStab" && _celByName.ContainsKey("DaggerFlurry"))
        {
            return "DaggerFlurry";
        }
        if (anim == "CrossAttack" && _celByName.ContainsKey("DaggerCross"))
        {
            return "DaggerCross";
        }
        if (!anim.StartsWith("Great", StringComparison.Ordinal) || _celByName.ContainsKey(anim))
        {
            return null;
        }
        string great = anim.IndexOf("Side", StringComparison.Ordinal) >= 0 && _celByName.ContainsKey("GreatSide") ? "GreatSide" : "GreatDown"; // 옆 휘두르기 셀은 생성기가 몸을 돌려 그려 기각 → 내려찍기로
        return _celByName.ContainsKey(great) ? great : null;
    }

    /// <summary>
    /// 셀 고르기(마스터 모션 38개, MotionSet.md): 요청한 애니 이름과 같은 셀(예 "Kick", "Knockdown") → 리그 클립과 같은 이름의 셀(회전·시전·막기)
    /// → 예전 대체 셀(CelAlias). 새 스킬은 셀 이름을 애니 이름으로 바로 요청하면 된다.
    /// </summary>
    /// requested·variant 는 요청 클립일 때만(아니면 null) — 재생 중 요청이든 FrameStartTime 의 미리 보기든 같은 셀을 고른다.
    private string CelKey(string clip, string requested, MotionVariants.Variant variant)
    {
        // 피격·착지·대기 몸짓처럼 요청과 다른 변형 클립이 골라졌으면 그 클립 셀을 쓴다(10-27: 대기 중 피격이 대기 셀로 보이던 문제)
        if (variant != null && _celByName.ContainsKey(variant.baseCel))
        {
            return variant.baseCel;
        }
        if (!string.IsNullOrEmpty(requested) && _celByName.ContainsKey(requested))
        {
            return requested;
        }
        string skill = SkillCel(requested);
        if (skill != null && _celByName.ContainsKey(skill))
        {
            return skill;
        }
        string own = clip == "Shoot" ? "Bow" : clip;
        return _celByName.ContainsKey(own) ? own : CelAlias(clip);
    }

    // 무기 스킬(예전 AnyPortrait 애니 이름) → 마스터 모션 셀(10-30): MapName 의 공용 5종(내려·올려·수평·찌르기·시전)보다 무기에 맞는 동작으로.
    // 무기별 사용처는 Resources/Weapon 프리팹 skillAnim(대검·둔기 GreatVashDown, 도끼·낫 GreatSideAttack·DeathSideGround, 너클 upperCut, 창·깃발 StrongStab, 단검 StabLR·SpeadBuff …)
    private static readonly Dictionary<string, string> SkillCels = new Dictionary<string, string>
    {
        { "GreatVashDown", "GreatDown" },       // 대검·둔기 양손 내려찍기
        { "GreatSideAttack", "GreatSide" },     // 도끼·대낫 양손 수평
        { "DeathSideGround", "LowSweep" },      // 대낫·도끼 하단 쓸기
        { "upperCut", "Punch" },                // 너클 — 한 손 올려베기보다 원투
        { "StrongStab", "Lunge" },              // 창·할버드·깃발 몸째 돌진 찌르기
        { "StabLR", "DaggerCross" },            // 쌍단검 좌우 → 교차 베기
        { "SpeadBuff", "Charge" },              // 버프 → 모으기(정면)
        { "AdeaAttSpeadBuff", "Charge" },
        { "WideVesh", "GreatSideOne" },         // 한손검 수평(3/4 뒤 감기·3/4 앞 팔로스루)
        { "StaffMiddle", "Charge" },
    };

    /// <summary>무기 스킬 애니 이름에 맞는 마스터 모션 셀 이름(없으면 null — 공용 MapName 셀)</summary>
    public static string SkillCel(string anim)
    {
        return !string.IsNullOrEmpty(anim) && SkillCels.TryGetValue(anim, out string cel) ? cel : null;
    }

    private static string CelAlias(string clip)
    {
        switch (clip)
        {
            case "SwingSide":
            case "Spin":
                return "SwingDown";
            case "Cast":
                return "SwingUp";
            case "Shoot":
                return "Stab";
            case "Guard":
                return "Idle";
            default:
                return clip;
        }
    }

    // 이동 중 칼은 앞으로 낮게(가슴 높이) — 무기를 맨 위에 그리면서 세운 칼(70~100°)이 얼굴을 덮었다(10-26). 손이 등 뒤로 크게 갈 때(점프 등)만 세운다
    private const float CelBladeAngle = -20f; // 칼을 앞·아래로 늘어뜨려 든다(10-26 저녁 사용자: 15° 로 앞을 겨눈 채 걸으면 부자연스럽다)
    private const float CelBladeBack = 210f; // 손이 등 뒤면 칼을 뒤·아래로 늘어뜨려 끈다 — 170°(수평 뒤)는 꼬리처럼 튀어나오고 앞(-20°)과의 전환이 뒤집혀 보였다(10-26 밤)
    private const float CelBladeLagRange = 40f; // 이보다 작은 각도 변화는 늦게 따라오기
    private const float CelBladeLag = 14f;      // 따라오기 빠르기(1/초)
    private const float CelBladeRise = 200f; // 10-26 밤 사용자: 점프 칼을 하늘 쪽으로 20° 올려 달라(220→200, 185→165)
    private const float CelBladeFall = 165f;
    private const float CelHandSwingRef = -5f;  // 걷기·달리기 손 x 가운데(엉덩이 기준 아트 px)
    private const float CelBladeSwing = 2.5f;   // 손 1px 당 칼 각도
    private const float CelBladeSwingMax = 10f;
    private const int WalkStartFrame = 3;
    private const float CelHandFrontPx = -6f; // 손 x(엉덩이 기준 아트 px)가 이보다 앞이면 15°
    private const float CelHandBackPx = -7f;  // 이보다 뒤면 뒤로 끌기(170°)
    private const float CelCrouchMin = 0.06f; // 물리 점프가 입력 즉시 떠서, 뜬 뒤에도 처음 이만큼은 웅크림을 보여 준다

    private void ShowCel(bool on)
    {
        if (on == _celShown || _cel == null)
        {
            return;
        }
        _celShown = on;
        _cel.enabled = on;
        if (_celFist != null)
        {
            _celFist.enabled = on;
        }
        foreach (SpriteRenderer r in _renderers)
        {
            if (r != null && r != _cel && r != _celFist)
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
        // 셀 걷기면 셀 프레임 번호로(발이 닿는 = 보폭 최대 프레임 1·5), 아니면 리그 8포즈
        bool cel = _celByName.TryGetValue("Walk", out CelClip wc) && wc.sprites.Length > 0;
        int pose = cel ? wc.IndexAt(_time + WalkStartFrame * wc.frame) : Mathf.Min(WalkPoses - 1, (int)(t / len * WalkPoses));
        if (pose == _walkPose)
        {
            return;
        }
        _walkPose = pose;
        if (System.Array.IndexOf(cel ? CelWalkContacts : WalkContactPoses, pose) >= 0)
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
        if (_diedAt < 0f || _celByName.ContainsKey("Death"))
        {
            return; // 셀 사망(무릎 꿇고 눕는 그림)이 있으면 몸 전체를 90° 눕히지 않는다
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
        // 셀 대기(측면)가 있으면 리그 습관 동작(3/4 정면)은 건너뛴다 — 시점이 확 돌아 보였다(Q-424)
        if (_requested == "Idle" && _fidget != null && !_celByName.ContainsKey("Idle"))
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
        // 로프에 매달렸거나 셀로 쓰러져 누웠으면 무기를 숨긴다(누운 몸 위에 칼이 서 있었다)
        bool hide = (_requested != null && _requested.StartsWith("Rope")) || (_diedAt >= 0f && _celByName.ContainsKey("Death"));
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

    /// <summary>뒷모습 프레임은 주무기를 몸 뒤에(몸 앞 = 화면 안쪽). 바뀔 때만 순서를 다시 매긴다</summary>
    private void SetMainWeaponBehind(bool behind)
    {
        if (behind == _mainBehind)
        {
            return;
        }
        _mainBehind = behind;
        for (int i = _weaponRenderers.Count - 1; i >= 0; i--)
        {
            SpriteRenderer r = _weaponRenderers[i];
            if (r != null && !_backWeapon.Contains(r))
            {
                r.sortingOrder = behind ? _weaponOrderBack : _weaponOrderFront;
            }
        }
    }

    /// <summary>
    /// 무기군 타이밍 프로파일: 준비(타격 프레임 앞)·회복(뒤) 구간 길이 배율. 1 = 셀 그대로, 1.3 = 30% 길게(무거운 무기), 0.8 = 빠르게(단검).
    /// 셀을 다시 그리지 않고 같은 몸으로 무기군마다 다른 완급을 낸다. 타격 프레임은 MotionFxTable·기본 3타 표에서 찾는다.
    /// </summary>
    public void SetTimingProfile(float windup, float recover)
    {
        _windupMul = Mathf.Clamp(windup, 0.3f, 3f);
        _recoverMul = Mathf.Clamp(recover, 0.3f, 3f);
    }

    /// <summary>직업 외형 셀 "J<직업>_<셀>" 은 같은 이름 공용 셀의 이펙트·타격 프레임을 쓴다(10-30 — 키가 같은 측면 원본이라 프레임 구조가 같거나 비슷)</summary>
    private static string FxName(string cel)
    {
        return cel.Length > 4 && cel[0] == 'J' && cel[3] == '_' ? cel.Substring(4) : cel;
    }

    private int StrikeFrameOf(string cel)
    {
        cel = FxName(cel);
        if (_strikeFrame.TryGetValue(cel, out int f))
        {
            return f;
        }
        f = -1;
        foreach (MotionFxTable.Strike s in MotionFxTable.Strikes)
        {
            if (s.anim == cel)
            {
                f = s.frame;
                break;
            }
        }
        if (f < 0 && WeaponFx.TryBasicCelStrike(cel, out VfxId _, out int bf))
        {
            f = bf;
        }
        _strikeFrame[cel] = f;
        return f;
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
            // 앞손(반대 손)에 세워 드는 방패·너클·활은 몸 앞에 — 몸 뒤로 그려져 방패가 어디 있는지 안 보였다(10-29 사용자 A14·R8)
            _backWeapon[i].sortingOrder = _upright.Contains(_backWeapon[i].transform) ? GuardBackWeaponOrder : order;
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
