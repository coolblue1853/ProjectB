using UnityEngine;

/// <summary>
/// 층계 보스를 처음 쓰러뜨려야 열리는 포털 (⑥구역 탑 근교 → 2층계, TowerWorld 16절).
/// 맵의 Portal 묶음(항상 켜짐)에 붙이고 _portal 에 잠긴 포털을 건다. 맵이 켜질 때 보스 첫 처치 기록(BossLoot 의 ES3 "bossFirstKill.보스")을 보고
/// 포털을 켜거나 끄며, 맵에 있는 동안 보스가 쓰러지면 바로 열고 "탑 입구 개방" 배너를 띄운다. 새 세이브 키는 만들지 않는다.
/// </summary>
public class TowerGatePortal : MonoBehaviour
{
    [SerializeField] private GameObject _portal;
    [SerializeField] private string _bossName = "Gatekeeper_BD";
    [SerializeField] private string _bannerTitle = "Tower Gate";
    [SerializeField] private string _bannerSubtitle = "The way to the 2nd floor is open";

    public bool IsOpen => ES3.KeyExists("bossFirstKill." + _bossName);

    private void OnEnable()
    {
        BossLoot.Defeated += OnBossDefeated;
        if (_portal != null)
        {
            _portal.SetActive(IsOpen);
        }
    }

    private void OnDisable()
    {
        BossLoot.Defeated -= OnBossDefeated;
    }

    private void OnBossDefeated(string bossName)
    {
        if (bossName != _bossName || _portal == null || _portal.activeSelf)
        {
            return;
        }
        _portal.SetActive(true);
        BossBanner.Show(_bannerTitle, _bannerSubtitle, true);
        Sfx.Play(SfxId.BossSlam, 0.5f);
        GameLog.Info(GameLog.Cat.Map, "탑 입구 개방: " + _portal.name);
    }
}
