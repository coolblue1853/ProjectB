using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillCooldown : MonoBehaviour
{

    public Image[] backImage = new Image[4];
    public Image[] cooldownImage = new Image[4]; // 쿨타임 이미지
    public float[] cooldownTime = new float[4]; // 쿨타임 시간 (초)
    public bool[] isCooldown = new bool[4]; // 쿨타임 중인지 여부 확인을 위한 변수
    private float[] cooldownTimer = new float[4]; // 쿨타임 타이머
    static public SkillCooldown instance;
    public void ResetCoolTime()
    {
        for(int i =0; i < backImage.Length; i++)
        {
            cooldownImage[i].sprite = null;
            cooldownTime[i] = 0;
            isCooldown[i] = false;
            cooldownTimer[i] = 0;
        }
    }
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this.gameObject);
        }
        else
        {
            DontDestroyOnLoad(this.gameObject);
            instance = this;
        }
    }
    // ── 남은 쿨다운 숫자·스태미나 부족 표시 (던파식 HUD, CombatSpec 9절). 큰 창이 열리면(HudPolicy.Hidden) 숨긴다 ──
    const float ShortFlashTime = 0.6f;
    TextMeshProUGUI[] _labels;
    readonly float[] _shortUntil = new float[8];
    static readonly Color LockedTint = new Color(0.32f, 0.32f, 0.36f, 1f);
    AttackManager _attackManager;
    readonly int[] _shownKey = { -2, -2, -2, -2, -2, -2, -2, -2 }; // 라벨 문자열을 0.1초 단위로 바뀔 때만 다시 만든다(프레임당 할당 방지)

    /// <summary>슬롯 i 의 남은 쿨다운 초(쿨다운이 아니면 0)</summary>
    public float Remaining(int i)
    {
        return i >= 0 && i < isCooldown.Length && isCooldown[i] ? Mathf.Max(0f, cooldownTimer[i]) : 0f;
    }

    /// <summary>쿨다운 숫자 문자열: 10초 이상 정수, 미만 소수 한 자리, 아니면 빈 문자열</summary>
    public static string FormatRemaining(float seconds)
    {
        if (seconds <= 0f)
        {
            return string.Empty;
        }
        return seconds >= 10f ? Mathf.CeilToInt(seconds).ToString() : seconds.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);
    }

    /// <summary>스태미나 부족으로 스킬이 안 나갔다 — 그 슬롯에 잠깐 붉은 표시</summary>
    public void FlashShort(int slot)
    {
        if (slot >= 0 && slot < _shortUntil.Length)
        {
            _shortUntil[slot] = Time.time + ShortFlashTime;
        }
    }

    /// <summary>프로브·테스트용: 슬롯 라벨의 현재 문자열(없으면 null)</summary>
    public string LabelText(int slot)
    {
        return _labels != null && slot >= 0 && slot < _labels.Length && _labels[slot] != null ? _labels[slot].text : null;
    }

    void EnsureLabels()
    {
        if (_labels != null)
        {
            return;
        }
        _labels = new TextMeshProUGUI[cooldownImage.Length];
        UiTheme theme = UiTheme.Get();
        for (int i = 0; i < cooldownImage.Length; i++)
        {
            if (cooldownImage[i] == null)
            {
                continue;
            }
            var go = new GameObject("CdText", typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(cooldownImage[i].transform, false);
            var t = go.GetComponent<TextMeshProUGUI>();
            if (theme.pixelFont != null)
            {
                t.font = theme.pixelFont;
            }
            t.enableAutoSizing = true;
            t.fontSizeMin = 10f;
            t.fontSizeMax = 30f;
            t.alignment = TextAlignmentOptions.Center;
            t.enableWordWrapping = false;
            t.raycastTarget = false;
            t.outlineWidth = 0.3f;
            t.outlineColor = new Color32(0, 0, 0, 255);
            var r = t.rectTransform;
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.offsetMin = r.offsetMax = Vector2.zero;
            _labels[i] = t;
        }
    }

    /// <summary>슬롯이 무기 숙련 레벨 미달이면 해금 레벨, 아니면 0</summary>
    int LockedLevel(int slot)
    {
        if (_attackManager == null)
        {
            _attackManager = FindFirstObjectByType<AttackManager>();
        }
        Weapon w = _attackManager != null ? _attackManager.equipWeapon : null;
        if (w == null || WeaponMasteryService.IsSlotUnlocked(w, slot))
        {
            return 0;
        }
        return WeaponMasteryService.UnlockLevelFor(w, slot);
    }

    void UpdateLabels()
    {
        EnsureLabels();
        bool hidden = HudPolicy.Hidden;
        for (int i = 0; i < _labels.Length; i++)
        {
            TextMeshProUGUI t = _labels[i];
            if (t == null)
            {
                continue;
            }
            bool slotOn = cooldownImage[i] != null && cooldownImage[i].sprite != null && cooldownImage[i].gameObject.activeInHierarchy;
            bool shortage = slotOn && !hidden && Time.time < _shortUntil[i];
            int lockLv = slotOn ? LockedLevel(i) : 0;
            // 키: -1 = 비움, -3 = 부족 표시, -100-Lv = 숙련 잠금, 그 외 = 남은 시간(0.1초 단위 올림)
            int key = !slotOn || hidden ? -1 : lockLv > 0 ? -100 - lockLv : shortage ? -3 : Mathf.CeilToInt(Remaining(i) * 10f) - (Remaining(i) > 0f ? 0 : 1);
            Color tint = lockLv > 0 ? LockedTint : Color.white;
            if (cooldownImage[i] != null && cooldownImage[i].color != tint)
            {
                cooldownImage[i].color = tint;
                if (backImage[i] != null)
                {
                    backImage[i].color = tint;
                }
            }
            if (key == _shownKey[i])
            {
                continue;
            }
            _shownKey[i] = key;
            t.text = key <= -100 ? Loc.F("Lv {0}", -100 - key) : key == -3 ? "STA" : key < 0 ? string.Empty : FormatRemaining(key * 0.1f);
            t.color = key <= -100 ? new Color(1f, 0.85f, 0.4f) : shortage ? new Color(1f, 0.4f, 0.35f) : Color.white;
        }
    }

    void Update()
    {
        CheckCoolTime();
        UpdateLabels();
    }
    void CheckCoolTime()
    {
        for(int i =0; i < cooldownImage.Length; i++)
        {
            if (isCooldown[i] && cooldownImage[i] != null)
            {
                cooldownTimer[i] -= Time.deltaTime; // 쿨타임 타이머를 감소시킵니다.
                cooldownImage[i].fillAmount = 1 - (cooldownTimer[i] / cooldownTime[i]); // 이미지를 업데이트하여 쿨타임을 표시합니다.
                if (cooldownTimer[i] <= 0)                // 쿨타임이 완료되었을 때
                {
                    isCooldown[i] = false; // 쿨타임 상태를 해제합니다.
                    cooldownImage[i].fillAmount = 1; // 이미지를 가득 채웁니다.
                }
            }
        }
    }

    public void DeletSkillSprite()
    {
        for (int i = 0; i < cooldownImage.Length; i++)
        {
            cooldownImage[i].sprite = null;
        }
    }
    // 스킬을 사용할 때 호출되는 함수
    public void UseSkill(int skillNum,string skillName) 
    {
        if (!isCooldown[skillNum]) // 쿨타임 중이 아니라면
        {
            // 쿨감은 SkillEffectService 가 레거시 Dictionary·장비 효과를 합산·상한(60%) 처리해 돌려준다 (EquipEffectDesign §2.7)
            float cooldownPower = SkillEffectService.Query(skillName, WeaponKind.None).cooldownPct;
            isCooldown[skillNum] = true; // 쿨타임을 시작합니다.
            cooldownTimer[skillNum] = CooldownMath.Apply(cooldownTime[skillNum], cooldownPower); // 쿨타임 타이머를 초기화합니다.
            cooldownImage[skillNum].fillAmount = 0; // 이미지를 초기화합니다.
        }
    }
}
