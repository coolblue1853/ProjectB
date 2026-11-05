using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 마도서 기믹 — TomePage 래퍼(WeaponTypes.md 2.9). C 키로 불/냉기/번개 장 전환(쿨 3초).
/// 장별 평타 프리팹·부가효과 적용은 후속(그래픽 보류, 임시 텍스트).
/// </summary>
public class TomePageGimmick : WeaponGimmick
{
    private static readonly string[] PageNames = { "불", "냉기", "번개" };
    private readonly TomePage _tome = new TomePage();

    public TomePage Tome => _tome;
    public string HudText => $"장: {PageNames[_tome.Page]}" + (_tome.CanSwitch ? "" : $" ({_tome.CooldownRemaining:0.0}s)");

    private void Update()
    {
        _tome.Tick(Time.deltaTime);
        Keyboard kb = Keyboard.current;
        if (kb != null && kb.cKey.wasPressedThisFrame)
        {
            _tome.TrySwitch();
        }
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(20, 120, 220, 24), HudText); // 임시 HUD(그래픽 보류)
    }
}
