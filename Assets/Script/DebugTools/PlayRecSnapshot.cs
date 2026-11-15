using System.Collections.Generic;
using System.Text;

/// <summary>PlayObservation 을 상태 스냅샷 한 줄(k:"st")로 직렬화한다(관측 수집은 PlayObservation).</summary>
public static class PlayRecSnapshot
{
    public static void Write(StringBuilder sb, float t, string src, string map, PlayObservation o)
    {
        PlayRecJson.Begin(sb, t, "st", src);
        PlayRecJson.KeyStr(sb, "map", map);
        if (!o.HasPlayer)
        {
            PlayRecJson.KeyInt(sb, "pl", 0);
            WriteUi(sb, o.UiWindows);
            PlayRecJson.End(sb);
            return;
        }
        PlayRecJson.KeyNum(sb, "x", o.X, 2);
        PlayRecJson.KeyNum(sb, "y", o.Y, 2);
        PlayRecJson.KeyNum(sb, "vx", o.Vx, 2);
        PlayRecJson.KeyNum(sb, "vy", o.Vy, 2);
        PlayRecJson.KeyInt(sb, "gr", o.Grounded ? 1 : 0);
        PlayRecJson.KeyInt(sb, "rope", o.OnRope ? 1 : 0);
        PlayRecJson.KeyInt(sb, "lad", o.NearLadder ? 1 : 0);
        PlayRecJson.KeyInt(sb, "face", o.Face);
        PlayRecJson.KeyStr(sb, "fsm", o.Fsm);
        PlayRecJson.KeyStr(sb, "fl", o.Flags);
        if (o.HasHealth)
        {
            PlayRecJson.KeyInt(sb, "hp", o.Hp);
            PlayRecJson.KeyInt(sb, "hpm", o.HpMax);
            PlayRecJson.KeyInt(sb, "mp", o.Mp);
            PlayRecJson.KeyInt(sb, "mpm", o.MpMax);
        }
        PlayRecJson.KeyInt(sb, "lv", o.Level);
        PlayRecJson.KeyInt(sb, "exp", o.Exp);
        PlayRecJson.KeyInt(sb, "gold", o.Gold);
        WriteUi(sb, o.UiWindows);
        sb.Append(",\"en\":[");
        for (int i = 0; i < o.EnemyCount; i++)
        {
            if (i > 0)
            {
                sb.Append(',');
            }
            PlayObservation.NearEnemy e = o.Enemies[i];
            sb.Append("{\"id\":");
            PlayRecJson.AppendString(sb, e.Id);
            PlayRecJson.KeyNum(sb, "dx", e.Dx, 2);
            PlayRecJson.KeyNum(sb, "dy", e.Dy, 2);
            PlayRecJson.KeyInt(sb, "hp", e.HpPercent);
            sb.Append('}');
        }
        sb.Append(']');
        WritePoint(sb, "pt", o.Portal);
        WritePoint(sb, "npc", o.Npc);
        WritePoint(sb, "drop", o.Drop);
        PlayRecJson.End(sb);
    }

    private static void WriteUi(StringBuilder sb, IReadOnlyList<string> held)
    {
        sb.Append(",\"ui\":[");
        for (int i = 0; i < held.Count; i++)
        {
            if (i > 0)
            {
                sb.Append(',');
            }
            PlayRecJson.AppendString(sb, held[i]);
        }
        sb.Append(']');
    }

    /// <summary>가장 가까운 것의 [dx,dy]. 없으면 키 생략.</summary>
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
}
