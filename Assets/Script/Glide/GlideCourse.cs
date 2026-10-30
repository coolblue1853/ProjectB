using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// 활공 코스 1개(FloorCoreContent.md 2절): 시작 표지 → 기류들 → 도착 표지. 시작 표지에 서면 기록이 시작되고, 지면에 닿으면 추락(직전 체크포인트 재시작),
/// 도착하면 평가(C~S)와 보상(층계 결정). 위치 폴링으로 판정한다(콜라이더 없음). 보상 반복 규칙은 세션 한정(저장 안 함).
/// </summary>
public class GlideCourse : MonoBehaviour
{
    public struct Spec
    {
        public string Id;
        public string Title;
        public float BaseSeconds;
        public int BaseReward;
        public float StartX;
        public float FinishX;
        public float YHint;
        public Zone[] Zones;
    }

    public struct Zone
    {
        public float X;
        public AirflowKind Kind;
        public float Width;
        public float Height;
    }

    private const float SnapUp = 6f;
    private const float SnapDown = 40f;
    private const float StartHalfWidth = 2.5f;
    private const float FallGroundTime = 0.35f;
    private const float RespawnGrace = 1.5f;
    private const float MaxRun = 300f;
    private const float SentenceRadius = 3.2f;
    private const float FinishHalfWidth = 2.5f;
    private const float FinishHeight = 14f;

    private static readonly Dictionary<string, int> _lastFullDay = new Dictionary<string, int>();
    private static Sprite _pole;
    private static Sprite _diamond;

    private Spec _spec;
    private bool _snapped;
    private bool _running;
    private float _time;
    private int _falls;
    private int _sentences;
    private float _groundTime;
    private float _grace;
    private float _cooldown;
    private Vector2 _checkpoint;
    private float _startY;
    private float _finishY;
    private readonly List<AirflowZone> _zones = new List<AirflowZone>();
    private readonly List<float> _zoneGround = new List<float>();
    private readonly List<Transform> _marks = new List<Transform>();
    private readonly List<bool> _markTaken = new List<bool>();
    private Transform _startMark;
    private Transform _finishMark;
    private TextMeshPro _label;

    public bool Running => _running;
    public float Elapsed => _time;
    public int Falls => _falls;
    public int Sentences => _sentences;
    public Spec Info => _spec;
    public IReadOnlyList<AirflowZone> Zones => _zones;
    public Vector2 StartPoint => new Vector2(_spec.StartX, _startY);
    public Vector2 FinishPoint => new Vector2(_spec.FinishX, _finishY);

    /// <summary>마지막 결과(검증용)</summary>
    public GlideGrade LastGrade { get; private set; }
    public int LastReward { get; private set; }
    public bool LastFirstOfDay { get; private set; }
    public bool HasResult { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        _lastFullDay.Clear();
        _pole = null;
        _diamond = null;
    }

    public void Init(Spec spec)
    {
        _spec = spec;
        BuildMarks();
    }

    private void OnDisable()
    {
        _running = false;
        if (_label != null)
        {
            _label.gameObject.SetActive(false);
        }
    }

    private void BuildMarks()
    {
        _startMark = MakeMark("Start", new Color(0.7f, 0.95f, 1f), false);
        _finishMark = MakeMark("Finish", new Color(1f, 0.85f, 0.45f), false);
        for (int i = 0; i < _spec.Zones.Length; i++)
        {
            Zone z = _spec.Zones[i];
            _zones.Add(AirflowZone.Create(transform, new Vector3(z.X, _spec.YHint, transform.position.z), z.Kind, z.Width, z.Height));
            _zoneGround.Add(_spec.YHint);
            _marks.Add(MakeMark("WindSentence_" + i, new Color(0.85f, 1f, 1f), true));
            _markTaken.Add(false);
        }
        var go = new GameObject("CourseLabel");
        go.transform.SetParent(transform, false);
        _label = go.AddComponent<TextMeshPro>();
        UiTheme theme = UiTheme.Get();
        if (theme.pixelFont != null)
        {
            _label.font = theme.pixelFont;
        }
        _label.fontSize = 3f;
        _label.alignment = TextAlignmentOptions.Center;
        _label.color = theme.unique;
        _label.rectTransform.sizeDelta = new Vector2(10f, 1.5f);
        _label.sortingOrder = 500;
        _label.text = _spec.Title;
    }

    private Transform MakeMark(string n, Color color, bool diamond)
    {
        var go = new GameObject(n);
        go.transform.SetParent(transform, false);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = diamond ? DiamondSprite() : PoleSprite();
        sr.color = color;
        sr.sortingOrder = 20;
        return go.transform;
    }

    private static Sprite PoleSprite()
    {
        if (_pole == null)
        {
            const int w = 8;
            const int h = 40;
            Color32[] px = MiniGameSprites.Blank(w * h);
            for (int y = 0; y < h; y++)
            {
                px[y * w + 3] = new Color32(235, 235, 245, 255);
                px[y * w + 4] = new Color32(200, 200, 215, 255);
            }
            for (int y = 26; y < 38; y++)
            {
                int len = 6 - Mathf.Abs(y - 32) / 2;
                for (int x = 5; x < 5 + len && x < w; x++)
                {
                    px[y * w + x] = new Color32(255, 255, 255, 255);
                }
            }
            _pole = MiniGameSprites.MakeSprite(px, w, h);
        }
        return _pole;
    }

    private static Sprite DiamondSprite()
    {
        if (_diamond == null)
        {
            const int s = 9;
            Color32[] px = MiniGameSprites.Blank(s * s);
            for (int y = 0; y < s; y++)
            {
                for (int x = 0; x < s; x++)
                {
                    int d = Mathf.Abs(x - 4) + Mathf.Abs(y - 4);
                    if (d <= 4)
                    {
                        px[y * s + x] = d == 4 ? new Color32(255, 255, 255, 200) : new Color32(255, 255, 255, 255);
                    }
                }
            }
            _diamond = MiniGameSprites.MakeSprite(px, s, s);
        }
        return _diamond;
    }

    private bool SnapAll()
    {
        float sy = Ground(_spec.StartX, _spec.YHint);
        float fy = Ground(_spec.FinishX, _spec.YHint);
        if (float.IsNaN(sy) || float.IsNaN(fy))
        {
            return false;
        }
        _startY = sy;
        _finishY = fy;
        float z = transform.position.z;
        _startMark.position = new Vector3(_spec.StartX, sy, z);
        _finishMark.position = new Vector3(_spec.FinishX, fy, z);
        _label.transform.position = new Vector3(_spec.StartX, sy + 4.2f, z);
        for (int i = 0; i < _zones.Count; i++)
        {
            float gy = Ground(_spec.Zones[i].X, _spec.YHint);
            if (float.IsNaN(gy))
            {
                gy = sy;
            }
            _zoneGround[i] = gy;
            _zones[i].transform.position = new Vector3(_spec.Zones[i].X, gy, z);
            _marks[i].position = new Vector3(_spec.Zones[i].X, gy + _spec.Zones[i].Height * 0.7f, z);
        }
        return true;
    }

    private static float Ground(float x, float yHint)
    {
        var filter = new ContactFilter2D();
        filter.SetLayerMask(LayerMask.GetMask("Ground"));
        filter.useTriggers = false;
        var hits = new RaycastHit2D[4];
        int n = Physics2D.Raycast(new Vector2(x, yHint + SnapUp), Vector2.down, filter, hits, SnapUp + SnapDown);
        return n > 0 ? hits[0].point.y : float.NaN;
    }

    private void Update()
    {
        GlideMotor m = GlideMotor.Instance;
        if (m == null)
        {
            return;
        }
        if (!_snapped)
        {
            _snapped = true;
            if (!SnapAll())
            {
                Debug.LogWarning("[PB][Glide] 코스 지면 못 찾음: " + _spec.Id);
                enabled = false;
                return;
            }
        }
        Vector2 p = m.Center;
        if (_cooldown > 0f)
        {
            _cooldown -= Time.deltaTime;
        }
        if (!_running)
        {
            if (_cooldown <= 0f && InStart(p))
            {
                Begin();
            }
            return;
        }
        Tick(m, p, Time.deltaTime);
    }

    private bool InStart(Vector2 p)
    {
        return Mathf.Abs(p.x - _spec.StartX) <= StartHalfWidth && p.y >= _startY - 1f && p.y <= _startY + 5f;
    }

    private bool InFinish(Vector2 p)
    {
        return Mathf.Abs(p.x - _spec.FinishX) <= FinishHalfWidth && p.y >= _finishY - 1f && p.y <= _finishY + FinishHeight;
    }

    private void Begin()
    {
        _running = true;
        _time = 0f;
        _falls = 0;
        _sentences = 0;
        _groundTime = 0f;
        _grace = 0.5f;
        HasResult = false;
        _checkpoint = new Vector2(_spec.StartX, _startY + 1.5f);
        for (int i = 0; i < _markTaken.Count; i++)
        {
            _markTaken[i] = false;
            _marks[i].gameObject.SetActive(true);
        }
        _label.gameObject.SetActive(true);
        GameLog.Info(GameLog.Cat.Player, "활공 코스 시작: " + _spec.Id);
    }

    private void Tick(GlideMotor m, Vector2 p, float dt)
    {
        _time += dt;
        _grace -= dt;
        _label.transform.position = new Vector3(p.x, p.y + 3.8f, transform.position.z);
        _label.text = _time.ToString("0.0") + "s  FALL " + _falls + "  " + _sentences + "/" + _marks.Count;

        for (int i = 0; i < _zones.Count; i++)
        {
            if (_zones[i].Contains(p))
            {
                _checkpoint = new Vector2(_spec.Zones[i].X, _zoneGround[i] + 1.5f);
            }
            if (!_markTaken[i] && ((Vector2)_marks[i].position - p).sqrMagnitude <= SentenceRadius * SentenceRadius)
            {
                _markTaken[i] = true;
                _marks[i].gameObject.SetActive(false);
                _sentences++;
            }
        }
        if (_time > 3f && InFinish(p))
        {
            Finish();
            return;
        }
        if (_time > MaxRun)
        {
            _running = false;
            _label.gameObject.SetActive(false);
            return;
        }
        bool safe = InStart(p) || InFinish(p) || InAnyZoneColumn(p);
        if (m.Grounded && !safe && _grace <= 0f)
        {
            _groundTime += dt;
            if (_groundTime >= FallGroundTime)
            {
                Respawn(m);
            }
        }
        else
        {
            _groundTime = 0f;
        }
    }

    private bool InAnyZoneColumn(Vector2 p)
    {
        for (int i = 0; i < _zones.Count; i++)
        {
            if (Mathf.Abs(p.x - _spec.Zones[i].X) <= _spec.Zones[i].Width * 0.5f)
            {
                return true;
            }
        }
        return false;
    }

    private void Respawn(GlideMotor m)
    {
        _falls++;
        _groundTime = 0f;
        _grace = RespawnGrace;
        Transform t = m.transform;
        t.position = new Vector3(_checkpoint.x, _checkpoint.y, t.position.z);
        var rb = m.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
        GameLog.Info(GameLog.Cat.Player, "활공 코스 추락 " + _falls);
    }

    private void Finish()
    {
        _running = false;
        _label.gameObject.SetActive(false);
        _cooldown = 2f;
        float day = TimeChange.instance != null && TimeChange.instance.totalTime > 1f ? TimeChange.instance.totalTime : GlideCourseRules.DefaultDayLength;
        int today = GlideCourseRules.DayIndex(Time.time, day);
        int last;
        bool first = !_lastFullDay.TryGetValue(_spec.Id, out last) || last != today;
        GlideGrade grade = GlideCourseRules.Grade(_time, _spec.BaseSeconds, _falls, _sentences, _marks.Count);
        int count = GlideCourseRules.Reward(_spec.BaseReward, grade, first);
        if (first)
        {
            _lastFullDay[_spec.Id] = today;
        }
        LastGrade = grade;
        LastReward = count;
        LastFirstOfDay = first;
        HasResult = true;
        bool given = false;
        InventoryManager inv = InventoryManager.instance;
        if (inv != null && inv.Service.Add(EnhanceRules.StoneName, count, AddSource.Other))
        {
            inv.SaveInventory();
            PickupLog.Report(ItemText.DisplayName(EnhanceRules.StoneName), count, ItemRarity.R.ToString());
            given = true;
        }
        else
        {
            PickupLog.ReportFull();
        }
        BossBanner.Show("Glide Course " + grade, _time.ToString("0.0") + "s  FALL " + _falls + "  " + (first ? "FIRST" : "REPEAT 50%") + (given ? "" : "  BAG FULL"), true);
        GameLog.Info(GameLog.Cat.Player, "활공 코스 완주 " + grade + " " + _time.ToString("0.0") + "s 추락 " + _falls + " 문장 " + _sentences + " 보상 " + count + (first ? " (첫 완주)" : " (반복)"));
    }
}
