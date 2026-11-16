using System.Collections.Generic;
using System.Text;

/// <summary>
/// 학습용 행 직렬화(관측 수집은 PlayObservation): k:"tk"(20Hz 관측+행동) · k:"gd"(지형 격자 전체) · k:"cx"(진행 맥락) · k:"tkh"(행동 비트 이름표).
/// 형식표는 PlayBotDesign.md 3.2절. 필드를 바꾸면 그 표와 Tools/playrec_summary.py 를 같이 고친다.
/// 호출자는 PlayRecWriter.Gate 를 잡고 PlayRecWriter.Begin 으로 머리를 쓴 뒤 이 함수로 몸통을 붙인다.
/// </summary>
public static class PlayRecTick
{
    /// <summary>tk 몸통. n = 틱 번호, u = 실제 시간, mask = 누르고 있는 액션 비트, mx·my = 이동 축</summary>
    public static void WriteTick(StringBuilder sb, long n, float u, long mask, float mx, float my, PlayObservation o, bool uiOpen)
    {
        PlayRecJson.KeyInt(sb, "n", n);
        PlayRecJson.KeyNum(sb, "u", u, 3);
        PlayRecJson.KeyInt(sb, "a", mask);
        PlayRecJson.KeyNum(sb, "mx", mx, 2);
        PlayRecJson.KeyNum(sb, "my", my, 2);
        PlayRecJson.KeyInt(sb, "ui", uiOpen ? 1 : 0);
        if (!o.HasPlayer)
        {
            PlayRecJson.KeyInt(sb, "pl", 0);
            return;
        }
        sb.Append(",\"p\":[");
        AppendNums(sb, o.X, o.Y, o.Vx, o.Vy);
        sb.Append(']');
        PlayRecJson.KeyInt(sb, "s", StateBits(o));
        PlayRecJson.KeyStr(sb, "fsm", o.Fsm);
        PlayRecJson.KeyStr(sb, "fl", o.Flags);
        if (o.HasHealth)
        {
            sb.Append(",\"hp\":[");
            PlayRecJson.AppendLong(sb, o.Hp);
            sb.Append(',');
            PlayRecJson.AppendLong(sb, o.HpMax);
            sb.Append(',');
            PlayRecJson.AppendLong(sb, o.Mp);
            sb.Append(',');
            PlayRecJson.AppendLong(sb, o.MpMax);
            sb.Append(']');
        }
        sb.Append(",\"cd\":[");
        for (int i = 0; i < o.Cooldowns.Length; i++)
        {
            if (i > 0)
            {
                sb.Append(',');
            }
            PlayRecJson.AppendFixed(sb, o.Cooldowns[i], 1);
        }
        sb.Append(']');
        PlayRecJson.KeyInt(sb, "cb", o.Combo);
        PlayRecJson.KeyInt(sb, "hc", o.Hits);
        WriteTerrainRef(sb, o.Terrain);
        WriteFoes(sb, o.Foes);
        WriteProjs(sb, o.Foes);
        WritePoint(sb, "dr", o.Drop);
        WritePoint(sb, "pt", o.Portal);
        WritePoint(sb, "npc", o.Npc);
    }

    /// <summary>봇 전용 추가 필드(사람 기록에는 없다): be = 따라가는 간선 종류, bn = 목표 구간, bf = Focus(몹 Uid·-1 배회·-2 드롭·-3 창)</summary>
    public static void WriteBotTrace(StringBuilder sb)
    {
        PlayRecJson.KeyStr(sb, "be", BotTrace.Edge);
        PlayRecJson.KeyInt(sb, "bn", BotTrace.Node);
        PlayRecJson.KeyInt(sb, "bf", BotTrace.Focus);
    }

    /// <summary>s 비트: 1 접지, 2 로프 위, 4 로프/사다리 근처, 8 왼쪽 봄, 16 피해 가능(무적 아님)</summary>
    public static int StateBits(PlayObservation o)
    {
        return (o.Grounded ? 1 : 0) | (o.OnRope ? 2 : 0) | (o.NearLadder ? 4 : 0) | (o.Face < 0 ? 8 : 0) | (o.CanBeHit ? 16 : 0);
    }

    private static void WriteTerrainRef(StringBuilder sb, PlayObsTerrain t)
    {
        if (t.Hash == 0)
        {
            return;
        }
        sb.Append(",\"gh\":\"");
        PlayRecGrid.AppendHex(sb, t.Hash);
        sb.Append("\",\"ed\":[");
        AppendNums(sb, t.EdgeL, t.EdgeR, t.Fall, t.DropF);
        sb.Append(']');
    }

    /// <summary>en: [종, dx, dy, vx, vy, 거리, HP%, 상태, 판정, 신호] 가까운 순 최대 6</summary>
    private static void WriteFoes(StringBuilder sb, PlayObsEnemies f)
    {
        sb.Append(",\"en\":[");
        for (int i = 0; i < f.FoeCount; i++)
        {
            if (i > 0)
            {
                sb.Append(',');
            }
            PlayObsEnemies.Foe e = f.Foes[i];
            sb.Append('[');
            PlayRecJson.AppendString(sb, e.Id);
            sb.Append(',');
            AppendNums(sb, e.Dx, e.Dy, e.Vx, e.Vy);
            sb.Append(',');
            PlayRecJson.AppendFixed(sb, e.Dist, 2);
            sb.Append(',');
            PlayRecJson.AppendLong(sb, e.HpPercent);
            sb.Append(',');
            PlayRecJson.AppendLong(sb, e.State);
            sb.Append(',');
            sb.Append(e.Hitbox ? '1' : '0');
            sb.Append(',');
            PlayRecJson.AppendLong(sb, e.Sig);
            sb.Append(']');
        }
        sb.Append(']');
    }

    /// <summary>pj: [dx, dy, vx, vy] 가까운 순 최대 4. 없으면 생략</summary>
    private static void WriteProjs(StringBuilder sb, PlayObsEnemies f)
    {
        if (f.ProjCount == 0)
        {
            return;
        }
        sb.Append(",\"pj\":[");
        for (int i = 0; i < f.ProjCount; i++)
        {
            if (i > 0)
            {
                sb.Append(',');
            }
            PlayObsEnemies.Proj p = f.Projs[i];
            sb.Append('[');
            AppendNums(sb, p.Dx, p.Dy, p.Vx, p.Vy);
            sb.Append(']');
        }
        sb.Append(']');
    }

    private static void WritePoint(StringBuilder sb, string key, PlayObservation.NearPoint p)
    {
        if (!p.Found)
        {
            return;
        }
        sb.Append(',');
        PlayRecJson.AppendString(sb, key);
        sb.Append(":[");
        PlayRecJson.AppendFixed(sb, p.Dx, 2);
        sb.Append(',');
        PlayRecJson.AppendFixed(sb, p.Dy, 2);
        sb.Append(']');
    }

    /// <summary>gd 몸통: gh 해시, 원점, 크기, 칸 문자열(행 0 = 맨 위)</summary>
    public static void WriteGrid(StringBuilder sb, PlayObsTerrain t, string map)
    {
        sb.Append(",\"gh\":\"");
        PlayRecGrid.AppendHex(sb, t.Hash);
        sb.Append('"');
        PlayRecJson.KeyStr(sb, "map", map);
        PlayRecJson.KeyInt(sb, "ox", t.Ox);
        PlayRecJson.KeyInt(sb, "oy", t.Oy);
        PlayRecJson.KeyInt(sb, "w", PlayRecGrid.W);
        PlayRecJson.KeyInt(sb, "h", PlayRecGrid.H);
        sb.Append(",\"c\":\"");
        PlayRecGrid.AppendCells(sb, t.Cells);
        sb.Append('"');
    }

    /// <summary>cx 몸통: 진행 맥락</summary>
    public static void WriteContext(StringBuilder sb, PlayObsContext c)
    {
        PlayRecJson.KeyInt(sb, "lv", c.Level);
        PlayRecJson.KeyInt(sb, "expp", c.ExpPercent);
        PlayRecJson.KeyInt(sb, "gold", c.Gold);
        PlayRecJson.KeyInt(sb, "bag", c.BagFree);
        PlayRecJson.KeyInt(sb, "pot", c.Potions);
        PlayRecJson.KeyInt(sb, "night", c.Night);
        sb.Append(",\"q\":[");
        for (int i = 0; i < c.QuestCount; i++)
        {
            if (i > 0)
            {
                sb.Append(',');
            }
            sb.Append('[');
            PlayRecJson.AppendString(sb, c.QuestIds[i]);
            sb.Append(',');
            PlayRecJson.AppendLong(sb, c.QuestProg[i]);
            sb.Append(',');
            PlayRecJson.AppendLong(sb, c.QuestNeed[i]);
            sb.Append(']');
        }
        sb.Append("],\"eq\":[");
        for (int i = 0; i < c.Equip.Length; i++)
        {
            if (i > 0)
            {
                sb.Append(',');
            }
            if (c.Equip[i] == null)
            {
                sb.Append("null");
            }
            else
            {
                PlayRecJson.AppendString(sb, c.Equip[i]);
            }
        }
        sb.Append("],\"buff\":[");
        for (int i = 0; i < c.BuffCount; i++)
        {
            if (i > 0)
            {
                sb.Append(',');
            }
            PlayRecJson.AppendString(sb, c.Buffs[i]);
        }
        sb.Append(']');
    }

    /// <summary>tkh 몸통: 비트 i = names[i]</summary>
    public static void WriteHeader(StringBuilder sb, IList<string> names)
    {
        sb.Append(",\"bits\":[");
        for (int i = 0; i < names.Count; i++)
        {
            if (i > 0)
            {
                sb.Append(',');
            }
            PlayRecJson.AppendString(sb, names[i]);
        }
        sb.Append(']');
    }

    private static void AppendNums(StringBuilder sb, float a, float b, float c, float d)
    {
        PlayRecJson.AppendFixed(sb, a, 2);
        sb.Append(',');
        PlayRecJson.AppendFixed(sb, b, 2);
        sb.Append(',');
        PlayRecJson.AppendFixed(sb, c, 2);
        sb.Append(',');
        PlayRecJson.AppendFixed(sb, d, 2);
    }
}
