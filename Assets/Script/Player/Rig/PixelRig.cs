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

    private const string WeaponDir = "PixelRig/Weapons/";
    private const float WeaponWorldScale = 1f; // 도트 무기 24 PPU × 1 = 아트 1px 1/24유닛(몹·캐릭터와 같은 밀도)
    private static readonly Dictionary<string, PixelWeapon> PixelWeapons = new Dictionary<string, PixelWeapon>();
    private readonly List<Transform> _upright = new List<Transform>();
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
    private Clip _launch, _up, _apex, _fall, _land, _hurt;
    private float _hurtUntil;
    private const int HurtEyeParam = 4;
    private const float HurtTime = 0.22f;
    private const float ApexBand = 1.5f;   // |세로 속도| 이하면 꼭대기 포즈
    private const float LaunchTime = 0.06f;
    private const float LandTime = 0.1f;

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
        _byName.TryGetValue("JumpLaunch", out _launch);
        _byName.TryGetValue("Jump", out _up);
        _byName.TryGetValue("JumpApex", out _apex);
        _byName.TryGetValue("JumpFall", out _fall);
        _byName.TryGetValue("Land", out _land);
        _byName.TryGetValue("Hurt", out _hurt);
        Play("Idle");
    }

    public override void Play(string anim)
    {
        // 점프 중에 땅 동작으로 바뀌면 착지 눌림을 잠깐 보여 준다
        if (_requested == "Jump" && _land != null && (anim == "Idle" || anim == "Walk" || anim == "Run"))
        {
            _landUntil = Time.time + LandTime;
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
        int order = isLeft ? _weaponOrderBack : _weaponOrderFront;
        foreach (SpriteRenderer r in weapon.GetComponentsInChildren<SpriteRenderer>(true))
        {
            r.sortingLayerID = _renderers.Length > 0 ? _renderers[0].sortingLayerID : r.sortingLayerID;
            r.sortingOrder = order;
            UsePixelWeapon(r);
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
        Transform t = r.transform;
        t.localPosition = new Vector3(0f, 0f, t.localPosition.z);
        // 그림 속 칼날 방향(angle)을 위(90°)로 맞춘다 — 손목 회전이 칼날 각도를 정한다
        t.localRotation = Quaternion.Euler(0f, 0f, 90f - pw.angle);
        float parent = t.parent != null ? Mathf.Abs(t.parent.lossyScale.y) : 1f;
        float s = parent > 0.0001f ? WeaponWorldScale / parent : 1f;
        t.localScale = new Vector3(s, s, 1f);
        if (pw.upright && !_upright.Contains(t))
        {
            _upright.Add(t); // 방패·너클·활: 손목 각도와 상관없이 똑바로
        }
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
        _time += Time.deltaTime * _globalSpeed * _clipSpeed;
        Clip c = PickVariant();
        float len = Mathf.Max(c.clip.length, 0.0001f);
        float t = c.loop ? Mathf.Repeat(_time, len) : Mathf.Min(_time, len);
        c.clip.SampleAnimation(gameObject, t);
        SnapBones();
        KeepUpright();
    }

    /// <summary>점프는 세로 속도로 상승·꼭대기·낙하를 고르고, 막 착지했으면 눌림 포즈</summary>
    private Clip PickVariant()
    {
        float now = Time.time;
        if (now < _hurtUntil && _hurt != null)
        {
            return _hurt;
        }
        if (now < _landUntil && (_requested == "Idle" || _requested == "Walk" || _requested == "Run"))
        {
            return _land;
        }
        if (_requested != "Jump" || _body == null || _up == null)
        {
            return _current;
        }
        if (_launch != null && now - _playedAt < LaunchTime)
        {
            return _launch;
        }
        float vy = _body.linearVelocity.y;
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
        if (!_byName.TryGetValue(anim, out c) && !_byName.TryGetValue(MapName(anim), out c))
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
        if (a.Contains("staff") || a.Contains("buff") || a.Contains("shield") || a.Contains("blow") || a.Contains("spead") || a.Contains("cast"))
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
