using DG.Tweening;
using TMPro;
using UnityEngine;

/// <summary>
/// 공격 예고(텔레그래프, QualityRoadmap C2): Animator "isAttack"이 켜지는 순간(BT의 준비 대기 0.5초 시작)
/// 머리 위에 노란 '!'를 띄우고 몸체를 붉게 두 번 깜빡인다. 공격 타이밍은 바꾸지 않고 읽을 수 있게만 한다.
/// isAttack 파라미터가 있는 몬스터에만 EnemyHealth가 자동 부착한다.
/// </summary>
public class EnemyTelegraph : MonoBehaviour
{
    static readonly int AttackId = Animator.StringToHash("isAttack");
    static readonly Color WarnRed = new Color(1f, 0.25f, 0.15f);
    const float MarkTime = 0.45f, FlashGap = 0.18f;

    Animator animator;
    SpriteHitFx hitFx;
    Transform holder;
    TextMeshPro mark;
    bool last;
    float hideAt, secondFlashAt = -1f;

    public static void AttachIfAttacker(EnemyHealth enemy)
    {
        var anim = enemy.GetComponent<Animator>();
        if (anim == null || anim.runtimeAnimatorController == null || enemy.GetComponent<EnemyTelegraph>() != null)
        {
            return;
        }
        foreach (AnimatorControllerParameter p in anim.parameters)
        {
            if (p.nameHash == AttackId && p.type == AnimatorControllerParameterType.Bool)
            {
                var t = enemy.gameObject.AddComponent<EnemyTelegraph>();
                t.animator = anim;
                t.hitFx = enemy.GetComponent<SpriteHitFx>();
                t.Build(enemy);
                return;
            }
        }
    }

    void Build(EnemyHealth enemy)
    {
        var sr = enemy.GetComponent<SpriteRenderer>();
        Bounds local = sr != null && sr.sprite != null ? sr.sprite.bounds : new Bounds(Vector3.zero, Vector3.one * 2f);
        Vector3 ls = enemy.transform.lossyScale;
        var go = new GameObject("Telegraph");
        holder = go.transform;
        holder.SetParent(enemy.transform, false);
        holder.localScale = new Vector3(1f / Mathf.Abs(ls.x), 1f / Mathf.Abs(ls.y), 1f); // 월드 단위로 그리기
        float top = local.max.y * Mathf.Abs(ls.y);

        var markGo = new GameObject("Mark", typeof(TextMeshPro));
        markGo.transform.SetParent(holder, false);
        markGo.transform.localPosition = new Vector3(0.45f, top + 0.5f, 0f); // 정예 ★(가운데)와 겹치지 않게 옆으로
        mark = markGo.GetComponent<TextMeshPro>();
        UiTheme theme = UiTheme.Get();
        if (theme.pixelFont != null)
        {
            mark.font = theme.pixelFont;
        }
        mark.text = "!";
        mark.fontSize = 22f;
        mark.alignment = TextAlignmentOptions.Center;
        mark.color = new Color(1f, 0.9f, 0.2f);
        mark.outlineWidth = 0.3f;
        mark.outlineColor = new Color32(120, 0, 0, 255);
        mark.sortingOrder = 121;
        mark.rectTransform.sizeDelta = new Vector2(1f, 1f);
        markGo.SetActive(false);
    }

    void Update()
    {
        bool now = animator.GetBool(AttackId);
        if (now && !last)
        {
            Trigger();
        }
        last = now;

        if (mark.gameObject.activeSelf && Time.time >= hideAt)
        {
            mark.gameObject.SetActive(false);
        }
        if (secondFlashAt > 0f && Time.time >= secondFlashAt)
        {
            secondFlashAt = -1f;
            Flash();
        }
        // 부모가 좌우 반전해도 '!'가 뒤집히지 않게
        float sign = Mathf.Sign(transform.lossyScale.x);
        Vector3 hs = holder.localScale;
        if (Mathf.Sign(hs.x) != sign)
        {
            holder.localScale = new Vector3(Mathf.Abs(hs.x) * sign, hs.y, hs.z);
        }
    }

    void Trigger()
    {
        mark.gameObject.SetActive(true);
        hideAt = Time.time + MarkTime;
        Transform mt = mark.transform;
        mt.DOKill(true);
        mt.localScale = Vector3.one * 0.4f;
        mt.DOScale(1f, 0.12f).SetEase(Ease.OutBack).SetLink(mark.gameObject);
        Flash();
        secondFlashAt = Time.time + FlashGap;
    }

    void Flash()
    {
        if (hitFx != null)
        {
            hitFx.Flash(0.07f, 0.55f, 1.4f, WarnRed);
        }
    }

    void OnDisable()
    {
        last = false;
        secondFlashAt = -1f;
        if (mark != null)
        {
            mark.gameObject.SetActive(false);
        }
    }
}
