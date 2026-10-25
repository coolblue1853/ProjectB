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

    private readonly Dictionary<string, Clip> _byName = new Dictionary<string, Clip>();
    private readonly Dictionary<string, Clip> _resolved = new Dictionary<string, Clip>();
    private Clip _current;
    private string _requested;
    private float _time;
    private float _globalSpeed = 1f;
    private float _clipSpeed = 1f;

    private void Awake()
    {
        foreach (Clip c in _clips)
        {
            if (c.clip != null)
            {
                _byName[c.name] = c;
            }
        }
        Play("Idle");
    }

    public override void Play(string anim)
    {
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
        // 눈 표정 파라미터 — 새 주인공은 앞머리로 눈을 가려 아직 쓰지 않는다
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
        }
    }

    private void LateUpdate()
    {
        if (_current == null || _current.clip == null)
        {
            return;
        }
        _time += Time.deltaTime * _globalSpeed * _clipSpeed;
        float len = Mathf.Max(_current.clip.length, 0.0001f);
        float t = _current.loop ? Mathf.Repeat(_time, len) : Mathf.Min(_time, len);
        _current.clip.SampleAnimation(gameObject, t);
        SnapBones();
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
