using UnityEngine;

/// <summary>
/// 흡입 판정(AbsorbDark 계열) 복제본의 애니메이션 이벤트 수신자.
/// BeforeAbsorb 클립 끝의 GoNextAnim 이벤트를 받아 컨트롤러 트리거 Next 를 당겨 AfterAbsorb 로 넘긴다.
/// 원본 판정은 이 역할의 스크립트가 누락돼 "AnimationEvent has no receiver" 에러가 났다(B-142).
/// </summary>
[RequireComponent(typeof(Animator))]
public class AbsorbAnimRelay : MonoBehaviour
{
    private static readonly int NextHash = Animator.StringToHash("Next");

    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    // 애니메이션 이벤트에서 호출된다(BeforeAbsorb.anim).
    public void GoNextAnim()
    {
        if (_animator != null)
        {
            _animator.SetTrigger(NextHash);
        }
    }
}
