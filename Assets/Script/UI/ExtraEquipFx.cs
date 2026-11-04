using UnityEngine;
using TMPro;

/// <summary>오라 링 점 계산 (순수 — 테스트 대상). 발밑 납작한 타원</summary>
public static class AuraRingRules
{
    public const int Segments = 32;

    /// <summary>중심 기준 타원 둘레 점 n개. pulse(0~1)가 커지면 반지름이 최대 8% 커진다</summary>
    public static Vector3[] Points(int n, float rx, float ry, float pulse)
    {
        var pts = new Vector3[n];
        float k = 1f + 0.08f * Mathf.Clamp01(pulse);
        for (int i = 0; i < n; i++)
        {
            float a = Mathf.PI * 2f * i / n;
            pts[i] = new Vector3(Mathf.Cos(a) * rx * k, Mathf.Sin(a) * ry * k, 0f);
        }
        return pts;
    }
}

/// <summary>
/// 신규 슬롯 외형 (SLOT-4): 오라 = 발밑 코드 생성 링, 칭호 = 머리 위 이름표 텍스트. 몸 레이어는 건드리지 않는다.
/// 씬 로드 후 한 번 자동 생성되어 플레이어를 따라간다. 임시 코드 연출(아트 교체는 사용자 검토 후속).
/// </summary>
public sealed class ExtraEquipFx : MonoBehaviour
{
    const float RingRx = 0.7f, RingRy = 0.18f, TitleHeight = 2.85f; // 머리 위 한글 이름표(≈2.3) 위로 띄운다
    LineRenderer _ring;
    TextMeshPro _title;
    Material _mat;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        var go = new GameObject("ExtraEquipFx");
        DontDestroyOnLoad(go);
        go.AddComponent<ExtraEquipFx>();
    }

    void Awake()
    {
        _mat = new Material(Shader.Find("Sprites/Default"));
        var ringGo = new GameObject("AuraRing");
        ringGo.transform.SetParent(transform, false);
        _ring = ringGo.AddComponent<LineRenderer>();
        _ring.loop = true;
        _ring.useWorldSpace = false;
        _ring.positionCount = AuraRingRules.Segments;
        _ring.widthMultiplier = 0.05f;
        _ring.sharedMaterial = _mat;
        _ring.sortingOrder = -1;
        _ring.enabled = false;

        var tGo = new GameObject("TitleTag");
        tGo.transform.SetParent(transform, false);
        _title = tGo.AddComponent<TextMeshPro>();
        _title.fontSize = 2.2f;
        _title.alignment = TextAlignmentOptions.Center;
        _title.color = new Color(1f, 1f, 1f, 0.85f);
        _title.sortingOrder = 50;
        _title.enableWordWrapping = false;
        _title.enabled = false;
    }

    void OnDestroy()
    {
        if (_mat != null)
        {
            Destroy(_mat);
        }
    }

    void LateUpdate()
    {
        PlayerController pc = PlayerController.instance;
        if (pc == null)
        {
            _ring.enabled = false;
            _title.enabled = false;
            return;
        }
        Vector3 feet = pc.groundCheck != null ? pc.groundCheck.transform.position : pc.transform.position;

        ItemData aura = ExtraEquip.Get(EquipSlot.Aura);
        _ring.enabled = aura != null;
        if (aura != null)
        {
            float pulse = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 2.5f);
            _ring.transform.position = feet + new Vector3(0f, 0.05f, 0f);
            _ring.SetPositions(AuraRingRules.Points(AuraRingRules.Segments, RingRx, RingRy, pulse));
            var c = new Color(0.65f, 0.85f, 1f, 0.35f + 0.3f * pulse);
            _ring.startColor = _ring.endColor = c;
        }

        ItemData title = ExtraEquip.Get(EquipSlot.Title);
        _title.enabled = title != null;
        if (title != null)
        {
            string text = string.IsNullOrEmpty(title.itemNameT) ? title.name : title.itemNameT;
            if (_title.text != text)
            {
                _title.text = text;
            }
            _title.transform.position = pc.transform.position + new Vector3(0f, TitleHeight, 0f);
        }
    }
}
