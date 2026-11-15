using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 지형 관측(읽기 전용, 봇 재사용): 플레이어 주변 PlayRecGrid.W x H 칸 격자 + 발판 끝까지 거리 + 아래 낙하 높이.
/// 칸 값: 0 빈칸, 1 바닥(Ground·Ground Soft·Ground Hard), 2 발판(Platform 레이어 또는 PlatformEffector 단방향), 3 벽(Wall·Boundary),
/// 4 로프/사다리(태그 Ladder 트리거 — 게임이 둘을 구분하지 않는다), 5 위험(독포자 구름 SporeCloud 반경).
/// 칸 판정 = 그 칸(0.9 x 0.9 상자)에 닿는 콜라이더 중 우선순위(벽 > 바닥 > 발판 > 로프)가 가장 높은 것. 타일맵 합성 콜라이더가 외곽선 형식이라
/// 점 판정은 속이 비어 잡히지 않는다(11-15 실측) → 상자 판정. 바닥 칸은 "그 칸에 지면이 걸쳐 있다"는 뜻이다.
/// 격자는 원점(정수 칸)이 바뀔 때·1초마다·맵 전환 때만 다시 잰다(칸당 OverlapBox 1회, 할당 없음).
/// 거리 값: 발판 끝 EdgeL/EdgeR 는 접지일 때만(0.25 간격, 최대 MaxEdge, 끝이 없으면 MaxEdge), 공중이면 -1.
/// Fall = 발밑에서 아래 바닥까지(최대 MaxFall, 없으면 -1). DropF = 바라보는 쪽 발판 끝 너머의 낙하 높이(접지 아님·끝 없음·바닥 없음이면 -1).
/// </summary>
public sealed class PlayObsTerrain
{
    public const float MaxEdge = 8f;
    public const float MaxFall = 30f;
    private const float EdgeStep = 0.25f;
    private const float RescanSeconds = 1f;
    private const float HazardScanSeconds = 3f;
    private static readonly Vector2 CellBox = new Vector2(0.9f, 0.9f);

    public readonly byte[] Cells = new byte[PlayRecGrid.W * PlayRecGrid.H];
    public int Ox, Oy;
    public uint Hash;
    public float EdgeL = -1f, EdgeR = -1f, Fall = -1f, DropF = -1f;

    private readonly Collider2D[] _hits = new Collider2D[8];
    private readonly List<SporeCloud> _clouds = new List<SporeCloud>(8);
    private ContactFilter2D _filter;
    private int _groundMask;
    private int _layerGround, _layerSoft, _layerHard, _layerPlatform, _layerWall, _layerBoundary;
    private bool _ready;
    private float _nextRescan;
    private float _nextHazardScan;
    private Collider2D _feet;
    private PlayerController _feetOwner;

    public void ForceRescan()
    {
        _nextRescan = 0f;
        _nextHazardScan = 0f;
    }

    public void Clear()
    {
        Hash = 0;
        EdgeL = EdgeR = Fall = DropF = -1f;
    }

    /// <summary>격자가 새로 계산돼 해시가 바뀌면 true</summary>
    public bool Observe(PlayerController pc, bool grounded, int face, float real)
    {
        EnsureReady();
        Vector2 feet = FeetOf(pc);
        int ox = PlayRecGrid.OriginX(feet.x);
        int oy = PlayRecGrid.OriginY(feet.y + 0.05f);
        bool changed = false;
        if (ox != Ox || oy != Oy || real >= _nextRescan || Hash == 0)
        {
            _nextRescan = real + RescanSeconds;
            if (real >= _nextHazardScan)
            {
                _nextHazardScan = real + HazardScanSeconds;
                _clouds.Clear();
                _clouds.AddRange(Object.FindObjectsByType<SporeCloud>());
            }
            Ox = ox;
            Oy = oy;
            FillCells();
            uint h = PlayRecGrid.Hash(Cells, Ox, Oy);
            changed = h != Hash;
            Hash = h;
        }
        MeasureEdges(feet, grounded, face);
        return changed;
    }

    private void EnsureReady()
    {
        if (_ready)
        {
            return;
        }
        _ready = true;
        _layerGround = LayerMask.NameToLayer("Ground");
        _layerSoft = LayerMask.NameToLayer("Ground Soft");
        _layerHard = LayerMask.NameToLayer("Ground Hard");
        _layerPlatform = LayerMask.NameToLayer("Platform");
        _layerWall = LayerMask.NameToLayer("Wall");
        _layerBoundary = LayerMask.NameToLayer("Boundary");
        _groundMask = LayerMask.GetMask("Ground", "Platform", "Ground Soft", "Ground Hard");
        int all = LayerMask.GetMask("Default", "Ground", "Ground Soft", "Ground Hard", "Platform", "Wall", "Boundary");
        _filter = new ContactFilter2D();
        _filter.SetLayerMask(all);
        _filter.useTriggers = true;
    }

    private Vector2 FeetOf(PlayerController pc)
    {
        if (_feetOwner != pc)
        {
            _feetOwner = pc;
            _feet = pc.GetComponent<Collider2D>();
        }
        if (_feet != null && _feet.enabled)
        {
            Bounds b = _feet.bounds;
            return new Vector2(b.center.x, b.min.y);
        }
        Vector3 p = pc.transform.position;
        return new Vector2(p.x, p.y);
    }

    private void FillCells()
    {
        for (int row = 0; row < PlayRecGrid.H; row++)
        {
            float y = PlayRecGrid.CellCenterY(Oy, row);
            for (int col = 0; col < PlayRecGrid.W; col++)
            {
                float x = PlayRecGrid.CellCenterX(Ox, col);
                Cells[row * PlayRecGrid.W + col] = Classify(new Vector2(x, y));
            }
        }
    }

    private byte Classify(Vector2 point)
    {
        if (InHazard(point))
        {
            return PlayRecGrid.Hazard;
        }
        int n = Physics2D.OverlapBox(point, CellBox, 0f, _filter, _hits);
        byte best = PlayRecGrid.Empty;
        int bestRank = 0;
        for (int i = 0; i < n; i++)
        {
            Collider2D c = _hits[i];
            _hits[i] = null;
            if (c == null)
            {
                continue;
            }
            byte code = CodeOf(c);
            int rank = Rank(code);
            if (rank > bestRank)
            {
                bestRank = rank;
                best = code;
            }
        }
        return best;
    }

    private byte CodeOf(Collider2D c)
    {
        if (c.isTrigger)
        {
            return c.CompareTag("Ladder") ? PlayRecGrid.Ladder : PlayRecGrid.Empty;
        }
        int layer = c.gameObject.layer;
        if (layer == _layerWall || layer == _layerBoundary)
        {
            return PlayRecGrid.Wall;
        }
        if (layer == _layerPlatform || c.usedByEffector)
        {
            return PlayRecGrid.Platform;
        }
        if (layer == _layerGround || layer == _layerSoft || layer == _layerHard)
        {
            return PlayRecGrid.Ground;
        }
        return PlayRecGrid.Empty; // Default 레이어의 트리거 아닌 물체(소품 등)는 지형으로 보지 않는다
    }

    private static int Rank(byte code)
    {
        switch (code)
        {
            case PlayRecGrid.Wall: return 4;
            case PlayRecGrid.Ground: return 3;
            case PlayRecGrid.Platform: return 2;
            case PlayRecGrid.Ladder: return 1;
            default: return 0;
        }
    }

    private bool InHazard(Vector2 point)
    {
        for (int i = 0; i < _clouds.Count; i++)
        {
            SporeCloud c = _clouds[i];
            if (c == null || !c.ObsCloudActive)
            {
                continue;
            }
            Vector2 d = (Vector2)c.ObsCloudPos - point;
            if (d.sqrMagnitude <= SporeCloud.ObsRadius * SporeCloud.ObsRadius)
            {
                return true;
            }
        }
        return false;
    }

    private void MeasureEdges(Vector2 feet, bool grounded, int face)
    {
        RaycastHit2D down = Physics2D.Raycast(feet + Vector2.up * 0.1f, Vector2.down, MaxFall, _groundMask);
        Fall = down.collider != null ? Mathf.Max(0f, down.distance - 0.1f) : -1f;
        if (!grounded)
        {
            EdgeL = EdgeR = DropF = -1f;
            return;
        }
        EdgeL = EdgeDistance(feet, -1f);
        EdgeR = EdgeDistance(feet, 1f);
        float edge = face < 0 ? EdgeL : EdgeR;
        if (edge >= MaxEdge)
        {
            DropF = -1f;
            return;
        }
        Vector2 beyond = new Vector2(feet.x + face * (edge + EdgeStep), feet.y + 0.1f);
        RaycastHit2D drop = Physics2D.Raycast(beyond, Vector2.down, MaxFall, _groundMask);
        DropF = drop.collider != null ? Mathf.Max(0f, drop.distance - 0.1f) : -1f;
    }

    /// <summary>발밑 바닥이 이어지는 거리(0.25 간격). 끝을 못 찾으면 MaxEdge</summary>
    private float EdgeDistance(Vector2 feet, float dir)
    {
        for (float d = EdgeStep; d <= MaxEdge; d += EdgeStep)
        {
            Vector2 o = new Vector2(feet.x + dir * d, feet.y + 0.2f);
            if (Physics2D.Raycast(o, Vector2.down, 0.5f, _groundMask).collider == null)
            {
                return d - EdgeStep;
            }
        }
        return MaxEdge;
    }
}
