using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 방패(Bulwark) 홀드 방어 기믹 — G 키/패드 왼쪽 트리거를 누르고 있으면 방어. 피해를 막으면 CounterBuff 반격 버프 발동.
/// 키 바인딩은 임시(OpenQuestions Q-AUTO15). 방어 연출 그래픽은 보류.
/// </summary>
public class ShieldGuard : WeaponGimmick, IGuardSource
{
    private InputAction _guardAction;
    private CounterBuff _counter;

    /// <summary>지금 방어 중인가</summary>
    public bool Holding => _guardAction != null && _guardAction.IsPressed();

    private void OnEnable()
    {
        _counter = GetComponent<CounterBuff>();
        if (_guardAction == null)
        {
            _guardAction = new InputAction("Guard", InputActionType.Button);
            _guardAction.AddBinding("<Keyboard>/g");
            _guardAction.AddBinding("<Gamepad>/leftTrigger");
        }
        _guardAction.Enable();
        PlayerGuard.Source = this;
    }

    private void OnDisable()
    {
        if (_guardAction != null)
        {
            _guardAction.Disable();
        }
        if (ReferenceEquals(PlayerGuard.Source, this))
        {
            PlayerGuard.Source = null;
        }
    }

    public bool Intercept(ref int damage)
    {
        if (!Holding || damage <= 0)
        {
            return false;
        }
        damage = GuardRules.Resolve(damage, true);
        if (_counter != null)
        {
            _counter.NotifyBlocked();
        }
        return true;
    }
}
