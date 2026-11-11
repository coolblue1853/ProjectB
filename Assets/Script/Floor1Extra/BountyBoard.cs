using System.Collections.Generic;
using PixelCrushers;
using UnityEngine;

/// <summary>
/// 마을 사냥 의뢰 게시대의 상태·저장·처치 집계 (화면은 퀘스트 창 입구 모드(QuestEntrance), 푯말 배치는 BountyKioskPlacer).
/// 하루 = TimeChange 밤→낮 전환 1회. 하루에 3건이 걸리고, 한 번에 하나만 받는다. 켜고 끄기: Floor1Content.csv 의 bounty.
/// </summary>
public class BountyBoard : MonoBehaviour, IMessageHandler
{
    public const string SaveKey = "f1.bounty";

    private static BountyBoard s_instance;

    private List<BountyDef> _defs = new List<BountyDef>();
    private BountyState _state = new BountyState();
    private bool _wasDay = true;
    private bool _dirty;

    public static BountyBoard Instance => s_instance;
    public IReadOnlyList<BountyDef> Defs => _defs;
    public BountyState State => _state;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        s_instance = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Boot()
    {
        if (s_instance != null || !Floor1Data.On("bounty"))
        {
            return;
        }
        TextAsset csv = Resources.Load<TextAsset>("Floor1Bounties");
        if (csv == null)
        {
            return;
        }
        var go = new GameObject("[BountyBoard]");
        DontDestroyOnLoad(go);
        s_instance = go.AddComponent<BountyBoard>();
        s_instance._defs = BountyRules.Parse(csv.text);
        if (ES3.KeyExists(SaveKey))
        {
            s_instance._state = ES3.Load<BountyState>(SaveKey);
            s_instance.DropUnknownIds();
        }
        BountyKioskPlacer.Place();
    }

    /// <summary>CSV 에서 지워진 의뢰 id 가 저장에 남아 있으면 걸러낸다 (패널 NRE 방지)</summary>
    private void DropUnknownIds()
    {
        _state.Offered.RemoveAll(id => BountyRules.Find(_defs, id) == null);
        _state.Done.RemoveAll(id => BountyRules.Find(_defs, id) == null);
        if (_state.Active.Length > 0 && BountyRules.Find(_defs, _state.Active) == null)
        {
            _state.Active = "";
            _state.Progress = 0;
        }
    }

    private void OnEnable()
    {
        MessageSystem.AddListener(this, "Kill", string.Empty);
    }

    private void OnDisable()
    {
        MessageSystem.RemoveListener(this);
        Save();
    }

    private void Update()
    {
        TimeChange tc = TimeChange.instance;
        if (tc != null)
        {
            if (!_wasDay && tc.isDaytime)
            {
                if (_state.Active.Length > 0)
                {
                    PickupLog.Notice("하루가 지나 진행 중이던 사냥 의뢰가 사라졌다");
                }
                StartNewDay(_state.Day + 1);
            }
            _wasDay = tc.isDaytime;
        }
        if (_dirty)
        {
            Save();
        }
    }

    /// <summary>패널을 열 때: 목록이 비어 있으면(첫 방문) 지금 레벨로 오늘 목록을 만든다</summary>
    public void EnsureOffers()
    {
        if (_state.Offered.Count == 0)
        {
            StartNewDay(_state.Day);
        }
    }

    public void StartNewDay(int day)
    {
        BountyRules.NewDay(_state, _defs, day, LevelSystem.Level);
        _dirty = true;
    }

    /// <summary>우측 퀘스트 추적 HUD 에 붙일 사냥 의뢰 두 줄(리치 텍스트). 진행 중 의뢰가 없으면 빈 문자열</summary>
    public string TrackerText()
    {
        BountyDef d = _state.Active.Length > 0 ? BountyRules.Find(_defs, _state.Active) : null;
        return BountyRules.TrackerLines(_state, d);
    }

    public bool Accept(string id)
    {
        bool ok = BountyRules.Accept(_state, id);
        _dirty |= ok;
        return ok;
    }

    public bool Abandon(string id)
    {
        bool ok = BountyRules.Abandon(_state, id);
        _dirty |= ok;
        return ok;
    }

    /// <summary>보고: 조건을 채웠고 가방에 보상이 들어가면 지급하고 true</summary>
    public bool TryReport(string id)
    {
        BountyDef d = BountyRules.Find(_defs, id);
        if (!BountyRules.IsReady(_state, d))
        {
            return false;
        }
        if (!d.Reward.CanReceive())
        {
            PickupLog.Warn("보상을 받을 가방 공간이 부족합니다");
            return false;
        }
        BountyRules.Complete(_state, id);
        d.Reward.Apply();
        _dirty = true;
        PickupLog.Notice("사냥 의뢰 완료 - " + d.Title + ": " + d.Reward.Describe());
        return true;
    }

    public void OnMessage(MessageArgs args)
    {
        if (BountyRules.OnKill(_state, _defs, args.parameter))
        {
            _dirty = true;
            BountyDef d = BountyRules.Find(_defs, _state.Active);
            if (BountyRules.IsReady(_state, d))
            {
                PickupLog.Notice("사냥 의뢰 목표 달성 - 마을 게시대에서 보고: " + d.Title);
            }
        }
    }

    private void Save()
    {
        _dirty = false;
        ES3.Save(SaveKey, _state);
    }
}
