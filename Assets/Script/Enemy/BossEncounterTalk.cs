using BehaviorDesigner.Runtime;
using DG.Tweening;
using PixelCrushers.DialogueSystem;
using UnityEngine;

/// <summary>
/// 보스 첫 조우 대사 (⑥구역 탑의 문지기, TowerWorld 16절). 보스가 나온 뒤 플레이어가 가까이 오면 한 번만(ES3 키) 대화를 열고,
/// 대화 중에는 보스 BT 를 멈춘다(플레이어는 대사창 UiLock 으로 묶이므로 일방적으로 맞지 않게). 대화가 끝나면 BT 를 다시 켠다.
/// </summary>
public class BossEncounterTalk : MonoBehaviour
{
    [SerializeField] private string _conversation = StoryDialogues.Gatekeeper;
    [SerializeField] private string _seenKey = "story.gatekeeper.met";
    [SerializeField] private float _range = 9f;
    [SerializeField] private float _checkInterval = 0.25f;

    private BehaviorTree _bt;
    private bool _talking;
    private bool _seen;
    private float _nextCheck;

    private void Awake()
    {
        _bt = GetComponent<BehaviorTree>();
    }

    private void OnEnable()
    {
        _talking = false;
        _nextCheck = 0f;
        _seen = ES3.KeyExists(_seenKey); // 스폰 때 한 번만 읽는다(매 검사마다 세이브 파일을 읽지 않게)
    }

    private void OnDisable()
    {
        if (_talking && _bt != null)
        {
            _bt.enabled = true; // 대화 중 반환되면 다음 재사용 때 BT 가 꺼진 채로 나오지 않게
        }
        _talking = false;
    }

    private void Update()
    {
        if (_talking)
        {
            if (!DialogueManager.isConversationActive)
            {
                _talking = false;
                if (_bt != null)
                {
                    _bt.enabled = true;
                }
            }
            return;
        }
        if (Time.time < _nextCheck)
        {
            return;
        }
        _nextCheck = Time.time + _checkInterval;
        if (_seen)
        {
            enabled = false; // 이미 본 대사 — 이 개체는 더 검사하지 않는다(풀 재사용 때도 꺼진 채라 매 프레임 비용 없음)
            return;
        }
        PlayerController player = PlayerController.instance;
        if (player == null || TitleScreen.IsShowing || DatabaseManager.isOpenUI)
        {
            return;
        }
        if ((player.transform.position - transform.position).sqrMagnitude > _range * _range)
        {
            return;
        }
        if (!DialogueBoot.Talk(_conversation, transform))
        {
            return;
        }
        ES3.Save(_seenKey, true);
        _seen = true;
        _talking = true;
        if (_bt != null)
        {
            _bt.sequence?.Kill();
            _bt.enabled = false;
        }
        GameLog.Info(GameLog.Cat.AI, "보스 조우 대사: " + _conversation);
    }
}
