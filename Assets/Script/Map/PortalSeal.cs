using UnityEngine;

/// <summary>
/// 보스 첫 처치 기록이 있어야 통과할 수 있는 포털 봉인 (Q-414: 외곽 샛길 → 2층계 지름길 `Portal_ToLv2`).
/// 같은 오브젝트의 <see cref="Portal"/> 가 위 키를 받으면 봉인 여부를 묻고, 막혀 있으면 이동 대신 안내 토스트를 띄운다.
/// 포털은 보이는 채로 두고(길이 있다는 걸 알린다) 이동만 막는다. 새 세이브 키는 만들지 않는다(BossLoot 첫 처치 키).
/// </summary>
public class PortalSeal : MonoBehaviour
{
    private const float MessageCooldown = 2f;

    [SerializeField] private string _bossName = "Gatekeeper_BD";
    [SerializeField] private string _sealedMessage = "Path sealed by the Tower.";

    private float _lastMessageTime = -99f;

    public bool IsOpen => BossLoot.HasFirstKill(_bossName);

    /// <summary>막힌 포털을 쓰려 할 때 안내한다(연타 시 2초에 한 번)</summary>
    public void NotifySealed()
    {
        if (Time.time - _lastMessageTime < MessageCooldown)
        {
            return;
        }
        _lastMessageTime = Time.time;
        PickupLog.Report(Loc.T(_sealedMessage), 1, "R");
        Sfx.Play(SfxId.CraftFail, 0.5f);
        GameLog.Info(GameLog.Cat.Map, "봉인된 포털: " + name + " (" + _bossName + " 첫 처치 필요)");
    }
}
