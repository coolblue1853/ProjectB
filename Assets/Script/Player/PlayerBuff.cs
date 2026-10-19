﻿using UnityEngine;

/// <summary>
/// 시간제 버프(스킬 버프 오브젝트·음식 방어 버프). 스탯은 이 인스턴스 Id 의 모디파이어로 넣는다(PlayerStateDesign Phase 10c).
/// 예전에는 += / -= 라 만료 전에 파괴되거나 두 번 켜지면 수치가 남았다 → 파괴(OnDestroy)에서도 RemoveById 로 반드시 해제.
/// 같은 인스턴스를 다시 켜면 교체가 아니라 중첩(현행 유지, PlayerStateDesign 2.5 임시 가정)이고, 해제는 한 번에 전부.
/// </summary>
public class PlayerBuff : MonoBehaviour
{
    public string buffType;
    public int buffTime;
    public int buffPower;
    public bool isStartIm = true; // 즉시 버프를 시작하는가

    string BuffId => "Buff:" + buffType + ":" + GetInstanceID();

    void Start()
    {
        if (isStartIm)
        {
            ActiveBuff();
        }
    }

    public void ActiveBuff()
    {
        if (TryStat(buffType, out StatId id))
        {
            PlayerStats.Add(new StatModifier(id, ModType.Add, buffPower) { Id = BuffId, Owner = name });
        }
        Invoke(nameof(DestoryBuff), buffTime);
    }

    public void DestoryBuff()
    {
        PlayerStats.RemoveById(BuffId);
        if (gameObject != null)
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        PlayerStats.RemoveById(BuffId); // 만료 전에 파괴돼도 수치가 남지 않게
    }

    /// <summary>"AttSpeed" / "Speed" / "HitCount" / "def"(대소문자 무시)</summary>
    static bool TryStat(string type, out StatId id)
    {
        switch ((type ?? "").ToLowerInvariant())
        {
            case "attspeed": id = StatId.AttackSpeed; return true;
            case "speed": id = StatId.MoveSpeed; return true;
            case "hitcount": id = StatId.HitCount; return true;
            case "def": id = StatId.Def; return true;
            default: id = StatId.Def; return false;
        }
    }
}
