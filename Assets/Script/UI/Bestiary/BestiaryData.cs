using System.Collections.Generic;
using UnityEngine;

/// <summary>몬스터 도감 정의 (Resources/UI/BestiaryData.asset, Tools/probes/build_bestiary.cs 로 생성). 순서 = 도감 표시 순서</summary>
[CreateAssetMenu(menuName = "ProjectB/Bestiary Data")]
public class BestiaryData : ScriptableObject
{
    [System.Serializable]
    public class Entry
    {
        public string id;          // 몬스터 프리팹 이름 (Kill 메시지 파라미터와 같음)
        public string displayName;
        public string region;
        public Sprite icon;        // RPG Monsters Icon
        public bool isBoss, isElite;
    }

    public List<Entry> entries = new List<Entry>();
}

/// <summary>도감 처치 기록 — MessageSystem "Kill"을 듣고 ES3("bestiary.kills")에 저장. 단계: 1=이름, 10=스탯, 50=드롭</summary>
public class BestiaryRecord : MonoBehaviour, PixelCrushers.IMessageHandler
{
    public const int RevealStats = 10, RevealDrops = 50;
    const string SaveKey = "bestiary.kills";

    static BestiaryRecord instance;
    Dictionary<string, int> kills = new Dictionary<string, int>();
    bool dirty;
    float saveAt;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        if (instance != null)
        {
            return;
        }
        var go = new GameObject("[BestiaryRecord]");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<BestiaryRecord>();
    }

    public static int Kills(string id)
    {
        return instance != null && instance.kills.TryGetValue(id, out int n) ? n : 0;
    }

    void Awake()
    {
        if (ES3.KeyExists(SaveKey))
        {
            kills = ES3.Load<Dictionary<string, int>>(SaveKey);
        }
    }

    void OnEnable()
    {
        PixelCrushers.MessageSystem.AddListener(this, "Kill", string.Empty);
    }

    void OnDisable()
    {
        PixelCrushers.MessageSystem.RemoveListener(this);
        Flush();
    }

    public void OnMessage(PixelCrushers.MessageArgs args)
    {
        string id = args.parameter;
        if (string.IsNullOrEmpty(id))
        {
            return;
        }
        kills.TryGetValue(id, out int n);
        kills[id] = n + 1;
        if (n + 1 == 1 || n + 1 == RevealStats || n + 1 == RevealDrops)
        {
            GameLog.Info(GameLog.Cat.Item, "도감 갱신: " + id + " " + (n + 1) + "마리");
        }
        dirty = true;
        saveAt = Time.unscaledTime + 2f; // 연속 처치는 2초 디바운스로 한 번에 저장
    }

    void Update()
    {
        if (dirty && Time.unscaledTime >= saveAt)
        {
            Flush();
        }
    }

    void Flush()
    {
        if (!dirty)
        {
            return;
        }
        dirty = false;
        ES3.Save(SaveKey, kills);
    }
}
