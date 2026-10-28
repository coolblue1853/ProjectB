using UnityEngine;

/// <summary>
/// 칼 스미어(도트) 받침 레이어 — 10-28 사용자 "도트로 큰 형태를 잡고 세세한 건 셰이더·파티클로 감싸서 강화".
/// ① 글로우: 스미어와 같은 프레임을 SmearGlow2D(텍셀 고정 블러·4단 계단·가산)로 한 칸 뒤에 깐다. 호 안쪽(머리·몸)은 끈다.
/// ② 바람 호: 스미어 바깥 +4·+7px, 안쪽 -4·-8px 에 1px 선 4줄, 진행 끝쪽 구간이 진행 방향으로 미끄러지며 꼬리부터 줄어든다(0.10초).
/// ③ 칼끝 불꽃: 칼끝이 지나간 호 위에서 접선 방향으로 튀는 1~2px 가산 점(월드 공간, 스미어가 꺼져도 남는다).
/// 수치·모양은 Tools/pixelfx/smear_preview.py 와 같다(미리보기 → 인게임). Vfx.PlayThemed 가 셀 스미어를 crisp 로 띄울 때 붙인다.
/// </summary>
public class SmearAura : MonoBehaviour
{
    // 호·선 모양(셀 px, 스미어 피벗 = 셀 원점 기준, y 위 +). gen_smear.fit_tip_circle 결과 — 스미어를 다시 뽑으면 같이 고친다
    struct Shape
    {
        public bool arc;
        public Vector2 c;
        public float r, a0, a1;
        public Vector2 p0, p1;
    }

    const float Ppu = 36f; // 셀·스미어 PPU(1 텍셀 = 셀 1px)
    static readonly float[] GlowEnv = { 1f, 0.8f, 0.6f, 0.45f };
    const float GlowAlpha = 0.75f;
    const float GlowInner = 8f;
    // 바깥 2줄 + 안쪽 2줄(할로우 나이트식 결 — 호 안에 평행한 줄무늬). 시작 구간(lo)이 줄마다 달라 길이가 엇갈린다
    static readonly float[] WindOffs = { 4f, 7f, -4f, -8f };
    static readonly float[] WindLos = { 0.45f, 0.6f, 0.3f, 0.55f };
    const float WindHi = 1f, WindSlide = 18f, WindLife = 0.10f;
    const int WindPoints = 14;
    const int SparkCount = 14;
    const float SparkDrag = 6f, SparkGravity = 40f; // px/s²

    static Material glowMaterial, addMaterial;
    static readonly int GlowColorId = Shader.PropertyToID("_GlowColor");
    static readonly int UVRectId = Shader.PropertyToID("_UVRect");
    static readonly int ArcId = Shader.PropertyToID("_Arc");

    SpriteRenderer _src, _glow;
    MaterialPropertyBlock _block;
    LineRenderer[] _wind;
    ParticleSystem _sparks;
    Shape _shape;
    Color[] _ramp;
    float _started, _fps, _speed, _scale;
    bool _left, _active;

    static Shape ShapeFor(VfxId id)
    {
        switch (id)
        {
            case VfxId.WSmearDown:
                return new Shape { arc = true, c = new Vector2(-1f, 11.4f), r = 32.7f, a0 = 137f, a1 = -15.5f };
            case VfxId.WSmearUp:
                return new Shape { arc = true, c = new Vector2(1.9f, 22.3f), r = 24.2f, a0 = -49.7f, a1 = 148.9f };
            default:
                return new Shape { arc = false, p0 = new Vector2(14f, 18f), p1 = new Vector2(46f, 19f) };
        }
    }

    /// <summary>스미어 Live 의 오브젝트에 받침 레이어를 붙이고 시작한다(풀 재사용 시 다시 부른다)</summary>
    public static void Begin(GameObject go, SpriteRenderer src, VfxId id, PixelRampId ramp, float fps, float speed)
    {
        if (!go.TryGetComponent(out SmearAura aura))
        {
            aura = go.AddComponent<SmearAura>();
            aura.Build(src);
        }
        aura.Play(id, ramp, fps, speed);
    }

    void Build(SpriteRenderer src)
    {
        _src = src;
        if (glowMaterial == null)
        {
            glowMaterial = Resources.Load<Material>("FX/M_SmearGlow");
            addMaterial = Resources.Load<Material>("FX/M_SmearAdd");
        }
        var g = new GameObject("Glow");
        g.transform.SetParent(transform, false);
        _glow = g.AddComponent<SpriteRenderer>();
        _glow.sharedMaterial = glowMaterial;
        _glow.sortingLayerID = src.sortingLayerID;
        _glow.sortingOrder = src.sortingOrder - 1; // 스미어 뒤에서 받친다
        _block = new MaterialPropertyBlock();

        _wind = new LineRenderer[WindOffs.Length];
        for (int i = 0; i < _wind.Length; i++)
        {
            var w = new GameObject("Wind" + i);
            w.transform.SetParent(transform, false);
            LineRenderer lr = w.AddComponent<LineRenderer>();
            lr.sharedMaterial = addMaterial;
            lr.useWorldSpace = true;
            lr.positionCount = WindPoints;
            lr.numCapVertices = 0;
            lr.numCornerVertices = 0;
            lr.alignment = LineAlignment.View;
            lr.sortingLayerID = src.sortingLayerID;
            lr.sortingOrder = src.sortingOrder + 1;
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.receiveShadows = false;
            _wind[i] = lr;
        }

        // 불꽃은 스미어(0.11초)보다 오래 살아 Vfx 루트에 둔다 — 스미어 오브젝트가 꺼져도 끝까지 날아간다
        var s = new GameObject("SmearSparks");
        s.transform.SetParent(transform.parent, false);
        _sparks = s.AddComponent<ParticleSystem>();
        _sparks.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ParticleSystem.MainModule main = _sparks.main;
        main.playOnAwake = false;
        main.loop = false;
        main.duration = 0.5f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startSpeed = 0f;
        main.maxParticles = 128;
        ParticleSystem.EmissionModule em = _sparks.emission;
        em.enabled = false;
        ParticleSystem.ShapeModule sh = _sparks.shape;
        sh.enabled = false;
        ParticleSystem.LimitVelocityOverLifetimeModule lim = _sparks.limitVelocityOverLifetime;
        lim.enabled = true;
        lim.limit = 1000f;
        lim.drag = SparkDrag;
        lim.multiplyDragByParticleSize = false;
        lim.multiplyDragByParticleVelocity = false;
        var pr = s.GetComponent<ParticleSystemRenderer>();
        pr.sharedMaterial = addMaterial;
        pr.renderMode = ParticleSystemRenderMode.Billboard;
        pr.sortingLayerID = src.sortingLayerID;
        pr.sortingOrder = src.sortingOrder + 1;
    }

    void Play(VfxId id, PixelRampId ramp, float fps, float speed)
    {
        _shape = ShapeFor(id);
        _ramp = PixelRamp.Get(ramp);
        _fps = fps;
        _speed = speed > 0f ? speed : 1f;
        _started = Time.time;
        _left = _src.flipX;
        _scale = Mathf.Abs(transform.localScale.y);
        _active = true;
        _glow.enabled = true;
        foreach (LineRenderer lr in _wind)
        {
            lr.enabled = true;
            lr.widthMultiplier = _scale / Ppu; // 1px
        }
        EmitSparks();
        LateUpdate();
    }

    void OnDisable()
    {
        _active = false;
        if (_glow != null)
        {
            _glow.enabled = false;
        }
    }

    void OnDestroy()
    {
        if (_sparks != null)
        {
            Destroy(_sparks.gameObject);
        }
    }

    /// <summary>셀 px(피벗 기준) → 월드</summary>
    Vector3 World(Vector2 p)
    {
        Vector3 o = transform.position;
        return new Vector3(o.x + (_left ? -p.x : p.x) * _scale / Ppu, o.y + p.y * _scale / Ppu, 0f);
    }

    Vector3 Dir(Vector2 v)
    {
        return new Vector3((_left ? -v.x : v.x) * _scale / Ppu, v.y * _scale / Ppu, 0f);
    }

    void LateUpdate()
    {
        if (!_active || _src == null)
        {
            return;
        }
        float t = (Time.time - _started) * _speed;
        UpdateGlow(t);
        UpdateWind(t);
    }

    void UpdateGlow(float t)
    {
        Sprite sp = _src.sprite;
        _glow.sprite = sp;
        _glow.flipX = _src.flipX;
        _glow.flipY = _src.flipY;
        if (sp == null)
        {
            return;
        }
        int f = Mathf.Clamp((int)(t * _fps), 0, GlowEnv.Length - 1);
        Color c = _ramp[2];
        c.a = GlowAlpha * GlowEnv[f];
        Texture tex = sp.texture;
        Rect r = sp.textureRect;
        var uv = new Vector4(r.xMin / tex.width, r.yMin / tex.height, r.xMax / tex.width, r.yMax / tex.height);
        Vector2 hs = new Vector2(0.5f / tex.width, 0.5f / tex.height); // 반 텍셀 안쪽(옆 프레임 경계 번짐 방지)
        uv += new Vector4(hs.x, hs.y, -hs.x, -hs.y);
        Vector4 arc = _shape.arc ? new Vector4(sp.pivot.x + _shape.c.x, sp.pivot.y + _shape.c.y, _shape.r - GlowInner, 1f) : Vector4.zero;
        _block.SetColor(GlowColorId, c);
        _block.SetVector(UVRectId, uv);
        _block.SetVector(ArcId, arc);
        _glow.SetPropertyBlock(_block);
    }

    void UpdateWind(float t)
    {
        float u = t / WindLife;
        bool on = u >= 0f && u < 1f;
        Color head = _ramp[3];
        head.a = 0.7f * (1f - u);
        Color tail = head;
        tail.a = 0f;
        for (int n = 0; n < _wind.Length; n++)
        {
            LineRenderer lr = _wind[n];
            lr.enabled = on;
            if (!on)
            {
                continue;
            }
            lr.startColor = tail;
            lr.endColor = head;
            float lo = WindLos[n] + (WindHi - WindLos[n]) * u; // 꼬리부터 줄어든다
            for (int j = 0; j < WindPoints; j++)
            {
                float k = lo + (WindHi - lo) * j / (WindPoints - 1);
                Vector2 p;
                if (_shape.arc)
                {
                    float sgn = _shape.a1 > _shape.a0 ? 1f : -1f;
                    float a = (_shape.a0 + (_shape.a1 - _shape.a0) * k + sgn * WindSlide * u) * Mathf.Deg2Rad;
                    float rr = _shape.r + WindOffs[n];
                    p = _shape.c + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * rr;
                }
                else
                {
                    p = Vector2.Lerp(_shape.p0, _shape.p1, k) + new Vector2(14f * u, n == 0 ? WindOffs[n] : -WindOffs[n] * 0.8f);
                }
                lr.SetPosition(j, World(p));
            }
        }
    }

    void EmitSparks()
    {
        ParticleSystem.MainModule main = _sparks.main;
        main.gravityModifier = SparkGravity * _scale / Ppu / 9.81f;
        ParticleSystem.ColorOverLifetimeModule col = _sparks.colorOverLifetime;
        col.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(_ramp[4], 0f), new GradientColorKey(_ramp[3], 0.5f), new GradientColorKey(_ramp[2], 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.8f, 0.5f), new GradientAlphaKey(0f, 1f) });
        col.color = grad;
        _sparks.Play();
        var ep = new ParticleSystem.EmitParams { startColor = Color.white, applyShapeToPosition = false };
        for (int i = 0; i < SparkCount; i++)
        {
            Vector2 p, tang, radial;
            if (_shape.arc)
            {
                float k = Mathf.Pow(Random.value, 0.6f); // 진행 끝쪽에 많이
                float a = (_shape.a0 + (_shape.a1 - _shape.a0) * k) * Mathf.Deg2Rad;
                float rad = _shape.r + Random.Range(-2f, 3f);
                radial = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                p = _shape.c + radial * rad;
                float sgn = _shape.a1 < _shape.a0 ? -1f : 1f;
                tang = new Vector2(-radial.y, radial.x) * sgn;
            }
            else
            {
                float k = Mathf.Sqrt(Random.value);
                p = Vector2.Lerp(_shape.p0, _shape.p1, k) + new Vector2(0f, Random.Range(-2f, 2f));
                tang = Vector2.right;
                radial = Random.value < 0.5f ? Vector2.up : Vector2.down;
            }
            Vector2 v = tang * Random.Range(70f, 160f) + radial * Random.Range(10f, 45f);
            ep.position = World(p);
            ep.velocity = Dir(v) * _speed;
            ep.startLifetime = Random.Range(0.12f, 0.28f) / _speed;
            ep.startSize = (Random.value < 0.3f ? 2f : 1f) * _scale / Ppu;
            _sparks.Emit(ep, 1);
        }
    }
}
