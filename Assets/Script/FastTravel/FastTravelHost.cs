using System;
using UnityEngine;

/// <summary>
/// 빠른 이동 상주 감시자 (NpcQuestDriver 와 같은 방식, 씬 수정 없이 부트 때 생성).
/// - 세이브가 (다시) 만들어지면 ft.stops 를 읽는다.
/// - 퀘스트 상태가 바뀔 때마다 해금 퀘스트 완료분을 반영한다(완료 훅 + 구세이브 자동 해금).
/// - 플레이어 피격 시각을 기록한다(전투 직후 이동 금지).
/// </summary>
public sealed class FastTravelHost : MonoBehaviour
{
    private static float s_lastHurt = -999f;
    private object _boundDatas;
    private PlayerHealthManager _hp;

    /// <summary>마지막 피격 뒤 지난 초 (기록 없음 = -1)</summary>
    public static float SecondsSinceHurt => s_lastHurt < 0f ? -1f : Time.time - s_lastHurt;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        s_lastHurt = -999f;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        if (FindAnyObjectByType<FastTravelHost>() != null)
        {
            return;
        }
        var go = new GameObject("[FastTravelHost]");
        DontDestroyOnLoad(go);
        go.AddComponent<FastTravelHost>();
    }

    private void OnEnable()
    {
        NpcQuestLog.Changed += OnQuestChanged;
    }

    private void OnDisable()
    {
        NpcQuestLog.Changed -= OnQuestChanged;
        Unbind();
    }

    private void OnQuestChanged()
    {
        if (_boundDatas == null)
        {
            return; // 이 슬롯의 ft.stops 를 읽기 전에 해금·저장하면 이전 슬롯 상태가 덮어쓴다
        }
        FastTravelState.AutoUnlock(NpcQuestLog.IsDone);
    }

    private void Update()
    {
        SaveManager sm = SaveManager.instance;
        if (sm != null && !ReferenceEquals(sm.datas, _boundDatas))
        {
            _boundDatas = sm.datas;
            FastTravelState.LoadFromDisk();
            FastTravelState.AutoUnlock(NpcQuestLog.IsDone);
        }
        PlayerHealthManager hp = PlayerHealthManager.Instance;
        if (hp != null && hp != _hp)
        {
            Unbind();
            _hp = hp;
            _hp.OnHpDamaged += OnHurt;
        }
    }

    private void Unbind()
    {
        if (_hp != null)
        {
            _hp.OnHpDamaged -= OnHurt;
            _hp = null;
        }
    }

    private void OnHurt(object sender, EventArgs e)
    {
        s_lastHurt = Time.time;
    }
}
