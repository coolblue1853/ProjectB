using PixelCrushers;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// NPC 퀘스트 런타임 구동: MessageSystem Kill/Craft 수신, 세이브 슬롯이 (다시) 만들어지면 상태 읽기, 퀘스트 창 키(J, 패드 오른쪽 스틱 누름).
/// 씬을 건드리지 않고 부트 때 만든다 (LevelSystem 과 같은 방식).
/// </summary>
public class NpcQuestDriver : MonoBehaviour, IMessageHandler
{
    private static NpcQuestDriver s_instance;
    private Datas _boundDatas;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        s_instance = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        if (s_instance != null)
        {
            return;
        }
        var go = new GameObject("[NpcQuest]");
        DontDestroyOnLoad(go);
        s_instance = go.AddComponent<NpcQuestDriver>();
        go.AddComponent<NpcQuestWindow>();
        go.AddComponent<NpcQuestTracker>();
    }

    private void OnEnable()
    {
        MessageSystem.AddListener(this, "Kill", string.Empty);
        MessageSystem.AddListener(this, "Craft", string.Empty);
    }

    private void OnDisable()
    {
        MessageSystem.RemoveListener(this);
    }

    public void OnMessage(MessageArgs messageArgs)
    {
        if (messageArgs.message == "Kill")
        {
            NpcQuestLog.OnKill(messageArgs.parameter);
        }
        else if (messageArgs.message == "Craft")
        {
            NpcQuestLog.OnCraft(messageArgs.parameter);
        }
    }

    private void Update()
    {
        // 세이브가 (다시) 만들어지면 퀘스트 상태를 읽는다 (슬롯 전환·타이틀 후 로드)
        SaveManager sm = SaveManager.instance;
        if (sm != null && !ReferenceEquals(sm.datas, _boundDatas))
        {
            _boundDatas = sm.datas;
            NpcQuestLog.LoadFromDisk();
        }
    }
}
