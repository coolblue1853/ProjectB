using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 파괴 가능 오브젝트 (TowerWorld.md 4절 — 1층계 나무통·상자). 플레이어 공격에 맞으면 체력이 줄고 금이 가며,
/// 부서지면 파편 프레임 + 흔들림 + 드롭(DropManager 공유 풀)을 낸 뒤 비활성. 리스폰 시간이 지나면 다시 선다.
/// 스프라이트 시트 3프레임: 0 온전 / 1 금 / 2 파편.
/// </summary>
[RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
public class BreakableProp : MonoBehaviour
{
    [System.Serializable]
    public struct Drop
    {
        public string itemName;
        [Range(0f, 1f)] public float chance;
    }

    [SerializeField] private Sprite[] _frames;       // 0 온전, 1 금, 2 파편
    [SerializeField] private int _hits = 2;          // 부서지기까지 타격 수
    [SerializeField] private float _respawnSeconds = 90f;
    [SerializeField] private Drop[] _drops;
    [SerializeField] private DropManager _dropSource; // 드롭 풀 (없으면 가장 가까운 스포너의 것)

    SpriteRenderer sr;
    BoxCollider2D col;
    int left;
    bool broken;
    float respawnAt;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<BoxCollider2D>();
        col.isTrigger = true;
        gameObject.tag = "Enemy"; // 플레이어 DamageObject 가 Enemy 태그만 판정
        left = _hits;
    }

    void Update()
    {
        if (broken && Time.time >= respawnAt)
        {
            Restore();
        }
    }

    /// <summary>DamageObject 에서 호출. 적이 아니므로 피해량은 무시하고 타격 수로만 센다</summary>
    public void Hit(Vector3 from)
    {
        if (broken)
        {
            return;
        }
        left--;
        Sfx.Play(SfxId.Hit, 0.5f);
        transform.DOKill(true);
        transform.DOPunchPosition(new Vector3(0.08f * Mathf.Sign(transform.position.x - from.x), 0.04f, 0f), 0.15f, 8, 0.5f).SetLink(gameObject);
        if (left > 0)
        {
            if (_frames != null && _frames.Length > 1)
            {
                sr.sprite = _frames[1];
            }
            return;
        }
        Break();
    }

    void Break()
    {
        broken = true;
        respawnAt = Time.time + _respawnSeconds;
        col.enabled = false;
        if (_frames != null && _frames.Length > 2)
        {
            sr.sprite = _frames[2];
        }
        HitSparks.Emit(transform.position + Vector3.up * 0.5f, false, true);
        HitFeedback.Shake(HitFeedback.ShakeTier.Light);
        Sfx.Play(SfxId.Kill, 0.4f);
        DropManager dm = _dropSource != null ? _dropSource : Nearest();
        if (dm != null && _drops != null)
        {
            var picked = new List<string>();
            foreach (Drop d in _drops)
            {
                if (!string.IsNullOrEmpty(d.itemName) && Random.value < d.chance)
                {
                    picked.Add(d.itemName);
                }
            }
            if (picked.Count > 0)
            {
                dm.DropGuaranteed(picked, transform.position + Vector3.up * 0.3f);
            }
        }
        GameLog.Info(GameLog.Cat.Map, "파괴: " + name);
        sr.DOFade(0f, 1.2f).SetDelay(1.5f).SetLink(gameObject);
    }

    void Restore()
    {
        broken = false;
        left = _hits;
        col.enabled = true;
        sr.DOKill();
        sr.color = Color.white;
        if (_frames != null && _frames.Length > 0)
        {
            sr.sprite = _frames[0];
        }
        transform.localScale = Vector3.one * 0.01f;
        transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack).SetLink(gameObject);
    }

    DropManager Nearest()
    {
        DropManager best = null;
        float bestD = float.MaxValue;
        foreach (DropManager dm in FindObjectsByType<DropManager>(FindObjectsSortMode.None))
        {
            float d = (dm.transform.position - transform.position).sqrMagnitude;
            if (d < bestD)
            {
                bestD = d;
                best = dm;
            }
        }
        return best;
    }
}
