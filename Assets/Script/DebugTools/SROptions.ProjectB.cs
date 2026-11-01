using System.ComponentModel;
using UnityEngine;

/// <summary>
/// 1시간 플레이 테스트용 치트(SR Debugger 옵션, 11-01 사용자 "치트를 상시 볼 수 있게").
/// 시작하면 "Cheat" 묶음을 화면 왼쪽 가운데에 고정한다. 전체 창 = 오른쪽 위 구석 세 번 탭 또는 Ctrl+Shift+F1.
/// 라벨은 SR Debugger 기본 글꼴이 한글을 못 그릴 수 있어 영어로 쓴다.
/// </summary>
public partial class SROptions
{
    private const string Cheat = "Cheat";
    private const string Move = "Move";
    private const string Quest = "Quest";
    private static System.IDisposable s_godMode;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void PinCheats()
    {
        if (SRDebug.Instance != null)
        {
            SRDebug.Instance.PinAllOptions(Cheat);
        }
    }

    private static PlayerController Pc => PlayerController.instance;

    [Category(Cheat), DisplayName("Level +1")]
    public void LevelUp()
    {
        LevelSystem.GainExp(Mathf.Max(1, LevelRules.ExpToNext(LevelSystem.Level) - LevelSystem.Exp), "cheat");
    }

    [Category(Cheat), DisplayName("EXP +500")]
    public void Exp500()
    {
        LevelSystem.GainExp(500, "cheat");
    }

    [Category(Cheat), DisplayName("Gold +1000")]
    public void Gold1000()
    {
        DatabaseManager.money += 1000;
        PickupLog.ReportGold(1000);
        if (InventoryManager.instance != null)
        {
            InventoryManager.instance.SaveInventory();
        }
    }

    [Category(Cheat), DisplayName("Heal")]
    public void Heal()
    {
        PlayerHealthManager phm = Object.FindFirstObjectByType<PlayerHealthManager>();
        if (phm != null)
        {
            phm.HpHeal(99999);
            phm.SteminaHeal(99999);
        }
    }

    /// <summary>조작이 막혔을 때: 상태를 Logs/stuck_state.txt 에 남기고 잠금 토큰·플래그를 모두 푼다(11-01 "벽에 닿으면 아무 조작이 안 됨")</summary>
    [Category(Cheat), DisplayName("Unstuck")]
    public void Unstuck()
    {
        if (Pc == null)
        {
            return;
        }
        string line = $"[{System.DateTime.Now:HH:mm:ss}] map={UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}\n" +
                      Pc.DebugStateLine() + "\n" + PlayerStatus.Dump() + "\n\n";
        Debug.LogWarning("[PB][Unstuck] " + line);
        try
        {
            System.IO.File.AppendAllText(System.IO.Path.Combine(Application.dataPath, "../Logs/stuck_state.txt"), line);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("[PB][Unstuck] 기록 실패: " + e.Message);
        }
        PlayerStatus.ForceClear("cheat unstuck");
        Pc.DebugUnstuck();
    }

    [Category(Cheat), DisplayName("God Mode")]
    public bool GodMode
    {
        get { return s_godMode != null; }
        set
        {
            if (value == (s_godMode != null))
            {
                return;
            }
            if (value)
            {
                s_godMode = PlayerStatus.Acquire(StatusFlag.Invincible, "Cheat");
            }
            else
            {
                s_godMode.Dispose();
                s_godMode = null;
            }
            OnPropertyChanged(nameof(GodMode));
        }
    }

    [Category(Cheat), DisplayName("Day")]
    public void Day()
    {
        if (TimeChange.instance != null)
        {
            TimeChange.instance.LockDaytime = true;
        }
    }

    [Category(Cheat), DisplayName("Night")]
    public void Night()
    {
        TimeChange tc = TimeChange.instance;
        if (tc == null)
        {
            return;
        }
        tc.LockDaytime = false; // 낮 고정을 풀어야 밤으로 넘어간다
        float target = (tc.startNighttime + 0.05f) * tc.totalTime;
        float add = target - tc.value * tc.totalTime;
        if (add < 0f)
        {
            add += tc.totalTime;
        }
        tc.AddTime(add);
    }

    [Category(Cheat), DisplayName("Day-Night Cycle")]
    public void Cycle()
    {
        if (TimeChange.instance != null)
        {
            TimeChange.instance.LockDaytime = false;
        }
    }

    [Category(Cheat), DisplayName("Fill Quests")]
    public void FillQuests()
    {
        NpcQuestLog.DebugFillActive();
    }

    [Category(Cheat), DisplayName("Save")]
    public void SaveNow()
    {
        if (SaveManager.instance != null)
        {
            SaveManager.instance.SaveProgress();
        }
    }

    [Category(Move), DisplayName("Town")]
    public void GoTown() { Go("Town"); }

    [Category(Move), DisplayName("Side Road")]
    public void GoSideRoad() { Go("Lv1_NearForest_SideRoad"); }

    [Category(Move), DisplayName("Near Forest")]
    public void GoNearForest() { Go("Lv1_NearForest"); }

    [Category(Move), DisplayName("Normal Forest")]
    public void GoNormalForest() { Go("Lv1_NormalForest"); }

    [Category(Move), DisplayName("Deep Forest")]
    public void GoDeepForest() { Go("Lv1_DeepForest"); }

    [Category(Move), DisplayName("Swamp")]
    public void GoSwamp() { Go("Lv1_Swamp"); }

    [Category(Move), DisplayName("Bandit Fortress")]
    public void GoBandit() { Go("Lv1_BanditFortress"); }

    [Category(Move), DisplayName("Tower Outskirts")]
    public void GoTower() { Go("Lv1_TowerOutskirts"); }

    [Category(Quest), DisplayName("Item Key")]
    public string ItemKey { get; set; } = "LeafAdae";

    [Category(Quest), DisplayName("Give Item")]
    public void GiveItem()
    {
        if (InventoryManager.instance != null && !string.IsNullOrEmpty(ItemKey))
        {
            InventoryManager.instance.CreatItem(ItemKey);
        }
    }

    [Category(Quest), DisplayName("Give LeafAdae+FangDagger")]
    public void GiveWeapons()
    {
        if (InventoryManager.instance != null)
        {
            InventoryManager.instance.CreatItem("LeafAdae");
            InventoryManager.instance.CreatItem("FangDagger");
        }
    }

    private static void Go(string map)
    {
        if (Pc == null)
        {
            return;
        }
        if (map == "Town")
        {
            MapInfo.GoToTown(Pc.gameObject);
            return;
        }
        if (!MapInfo.GoToMap(map, Pc.gameObject))
        {
            PickupLog.Warn("map not found: " + map);
        }
    }
}
