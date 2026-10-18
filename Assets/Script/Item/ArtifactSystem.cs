using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>아티팩트 종류 (TowerWorld.md 5절 — Shift 액티브 아이템)</summary>
public enum ArtifactId { None, HealingDew, GuardianShell, Hourglass, StarBurst }

/// <summary>
/// 아티팩트: Shift 로 발동하는 액티브 아이템. 인벤토리 아이템이 아니라 '해금 목록'이며(ES3 artifact.unlocked),
/// 장착 1개(ES3 artifact.equipped)를 Shift 로 쓴다. Tab 으로 다음 해금 아티팩트로 교체.
/// 효과: 이슬(회복 35%) · 수호 껍질(무적 3초) · 모래시계(시간 20% 4초, 플레이어는 정상) · 별 폭발(주변 적 피해 + 넉백).
/// HUD: 탐험가 칩 아래 아이콘 + 쿨다운.
/// </summary>
public class ArtifactSystem : MonoBehaviour
{
    public class Def
    {
        public ArtifactId id;
        public string name, desc;
        public float cooldown;
        public Color color;
    }

    public static readonly List<Def> Defs = new List<Def>
    {
        new Def { id = ArtifactId.HealingDew, name = "Healing Dew", desc = "Restore 35% HP", cooldown = 25f, color = new Color(0.45f, 1f, 0.55f) },
        new Def { id = ArtifactId.GuardianShell, name = "Guardian Shell", desc = "Invincible for 3s", cooldown = 40f, color = new Color(0.5f, 0.8f, 1f) },
        new Def { id = ArtifactId.Hourglass, name = "Hourglass", desc = "Slow the world for 4s", cooldown = 50f, color = new Color(0.95f, 0.85f, 0.4f) },
        new Def { id = ArtifactId.StarBurst, name = "Star Burst", desc = "Blast nearby foes", cooldown = 30f, color = new Color(1f, 0.55f, 0.35f) },
    };

    const string UnlockKey = "artifact.unlocked", EquipKey = "artifact.equipped";
    const float BurstRadius = 4f;

    static ArtifactSystem instance;
    HashSet<ArtifactId> unlocked = new HashSet<ArtifactId>();
    ArtifactId equipped;
    float readyAt;
    Image icon, cdFill;
    TextMeshProUGUI label;
    Tween slowTween;

    public static ArtifactId Equipped => instance != null ? instance.equipped : ArtifactId.None;
    public static bool IsUnlocked(ArtifactId id) => instance != null && instance.unlocked.Contains(id);
    public static float CooldownLeft => instance != null ? Mathf.Max(0f, instance.readyAt - Time.time) : 0f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        if (instance != null)
        {
            return;
        }
        var go = new GameObject("[ArtifactSystem]");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<ArtifactSystem>();
    }

    void Awake()
    {
        if (ES3.KeyExists(UnlockKey))
        {
            unlocked = new HashSet<ArtifactId>(ES3.Load<List<ArtifactId>>(UnlockKey));
        }
        if (unlocked.Count == 0)
        {
            unlocked.Add(ArtifactId.HealingDew); // 시작 아티팩트 (브로치의 이슬)
            SaveUnlocked();
        }
        equipped = ES3.KeyExists(EquipKey) ? ES3.Load<ArtifactId>(EquipKey) : ArtifactId.HealingDew;
        if (!unlocked.Contains(equipped))
        {
            equipped = ArtifactId.HealingDew;
        }
        BuildHud();
        Refresh();
    }

    public static Def Get(ArtifactId id) => Defs.Find(d => d.id == id);

    public static bool Unlock(ArtifactId id)
    {
        if (instance == null || id == ArtifactId.None || instance.unlocked.Contains(id))
        {
            return false;
        }
        instance.unlocked.Add(id);
        instance.SaveUnlocked();
        Def d = Get(id);
        PickupLog.Report("Artifact: " + d.name, 1, "U");
        Sfx.Play(SfxId.RareDrop);
        GameLog.Info(GameLog.Cat.Item, "아티팩트 해금: " + d.name);
        instance.Refresh();
        return true;
    }

    void SaveUnlocked()
    {
        ES3.Save(UnlockKey, new List<ArtifactId>(unlocked));
    }

    void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null || DatabaseManager.isOpenUI)
        {
            return;
        }
        if (kb.tabKey.wasPressedThisFrame)
        {
            CycleEquipped();
        }
        if (kb.leftShiftKey.wasPressedThisFrame || kb.rightShiftKey.wasPressedThisFrame)
        {
            TryUse();
        }
        if (cdFill != null)
        {
            float left = CooldownLeft;
            Def d = Get(equipped);
            cdFill.fillAmount = d != null && d.cooldown > 0f ? Mathf.Clamp01(left / d.cooldown) : 0f;
            label.text = left > 0.05f ? Mathf.CeilToInt(left).ToString() : "SHIFT";
        }
    }

    void CycleEquipped()
    {
        var list = new List<ArtifactId>();
        foreach (Def d in Defs)
        {
            if (unlocked.Contains(d.id))
            {
                list.Add(d.id);
            }
        }
        if (list.Count <= 1)
        {
            return;
        }
        int i = list.IndexOf(equipped);
        equipped = list[(i + 1) % list.Count];
        ES3.Save(EquipKey, equipped);
        Sfx.Play(SfxId.UiMove, 0.6f);
        Refresh();
    }

    public static bool TryUseNow() => instance != null && instance.TryUse();

    bool TryUse()
    {
        Def d = Get(equipped);
        if (d == null || Time.time < readyAt || PlayerController.instance == null)
        {
            if (d != null)
            {
                Sfx.Play(SfxId.CraftFail, 0.4f);
            }
            return false;
        }
        readyAt = Time.time + d.cooldown;
        Vector3 pos = PlayerController.instance.transform.position;
        switch (equipped)
        {
            case ArtifactId.HealingDew:
                {
                    PlayerHealthManager phm = PlayerHealthManager.Instance;
                    int heal = Mathf.RoundToInt(phm.fullHP * 0.35f);
                    phm.HpUp(heal);
                    Vfx.Play(VfxId.BurstGreen, pos + Vector3.up);
                    PickupLog.Report("+" + heal + " HP", 1, "C");
                    break;
                }
            case ArtifactId.GuardianShell:
                PlayerStatus.AcquireFor(StatusFlag.Invincible, 3f, "Artifact:GuardianShell");
                Vfx.Play(VfxId.RuneBlue, pos + Vector3.up * 0.1f);
                break;
            case ArtifactId.Hourglass:
                slowTween?.Kill();
                Time.timeScale = 0.2f;
                slowTween = DOVirtual.DelayedCall(4f, () => Time.timeScale = 1f, true).SetLink(gameObject);
                Vfx.Play(VfxId.RuneGold, pos + Vector3.up * 0.1f);
                break;
            case ArtifactId.StarBurst:
                {
                    Vfx.Play(VfxId.BurstGold, pos + Vector3.up, false, 1.6f);
                    HitFeedback.Shake(HitFeedback.ShakeTier.Heavy);
                    int[] dmg = { 60, 90, 20, 50, 0, 50, 0, 0, 0, 0 };
                    foreach (EnemyHealth e in FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None))
                    {
                        if (e.nowHP > 0 && Vector2.Distance(e.transform.position, pos) <= BurstRadius)
                        {
                            e.damage2Enemy(dmg, 0.3f, 8f, Vector2.up, pos.x, true, true, 0.15f, 1f);
                        }
                    }
                    break;
                }
        }
        Sfx.Play(SfxId.Victory, 0.35f);
        GameLog.Info(GameLog.Cat.Player, "아티팩트 사용: " + d.name);
        Refresh();
        return true;
    }

    // ── HUD ──
    void BuildHud()
    {
        UiTheme theme = UiTheme.Get();
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        Image chip = theme.NewPanel("Chip", transform, false, 0.85f);
        var cr = chip.rectTransform;
        cr.anchorMin = cr.anchorMax = cr.pivot = new Vector2(0f, 1f);
        cr.anchoredPosition = new Vector2(16f, -298f);
        cr.sizeDelta = new Vector2(84f, 84f);

        icon = new GameObject("Icon", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
        icon.transform.SetParent(chip.transform, false);
        icon.rectTransform.anchorMin = Vector2.zero;
        icon.rectTransform.anchorMax = Vector2.one;
        icon.rectTransform.offsetMin = new Vector2(12f, 12f);
        icon.rectTransform.offsetMax = new Vector2(-12f, -12f);
        icon.preserveAspect = true;
        icon.raycastTarget = false;

        cdFill = new GameObject("Cooldown", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
        cdFill.transform.SetParent(chip.transform, false);
        cdFill.rectTransform.anchorMin = Vector2.zero;
        cdFill.rectTransform.anchorMax = Vector2.one;
        cdFill.rectTransform.offsetMin = new Vector2(6f, 6f);
        cdFill.rectTransform.offsetMax = new Vector2(-6f, -6f);
        cdFill.color = new Color(0f, 0f, 0f, 0.65f);
        cdFill.type = Image.Type.Filled;
        cdFill.fillMethod = Image.FillMethod.Radial360;
        cdFill.fillOrigin = (int)Image.Origin360.Top;
        cdFill.fillClockwise = false;
        cdFill.raycastTarget = false;
        cdFill.sprite = theme.frame; // 단색 채움용 (스프라이트 없으면 사각)

        label = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
        label.transform.SetParent(chip.transform, false);
        if (theme.pixelFont != null)
        {
            label.font = theme.pixelFont;
        }
        label.fontSize = 18f;
        label.alignment = TextAlignmentOptions.Center;
        label.color = new Color(1f, 0.95f, 0.8f);
        label.outlineWidth = 0.25f;
        label.outlineColor = new Color32(0, 0, 0, 220);
        label.raycastTarget = false;
        var lr = label.rectTransform;
        lr.anchorMin = new Vector2(0f, 0f);
        lr.anchorMax = new Vector2(1f, 0f);
        lr.pivot = new Vector2(0.5f, 0f);
        lr.anchoredPosition = new Vector2(0f, -22f);
        lr.sizeDelta = new Vector2(0f, 24f);
    }

    void Refresh()
    {
        if (icon == null)
        {
            return;
        }
        Def d = Get(equipped);
        icon.sprite = Resources.Load<Sprite>("Artifacts/" + equipped);
        icon.color = icon.sprite != null ? Color.white : (d != null ? d.color : Color.gray);
        icon.transform.DOKill(true);
        icon.transform.localScale = Vector3.one;
        icon.transform.DOPunchScale(Vector3.one * 0.2f, 0.25f, 5, 0.5f).SetLink(icon.gameObject);
    }
}
