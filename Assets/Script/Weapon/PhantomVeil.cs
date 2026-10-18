using UnityEngine;

/// <summary>
/// 유령 계보(Phantom Scythe) 순간 이동 베기에 붙는 짧은 무적 + 반투명 잔상.
/// 판정 프리팹이 켜질 때 시간 만료 무적 토큰을 잡으므로 연속 시전·풀 재사용에도 반드시 풀린다.
/// </summary>
public class PhantomVeil : MonoBehaviour
{
    [SerializeField] private float _duration = 0.45f;

    private void OnEnable()
    {
        PlayerController pc = PlayerController.instance;
        if (pc == null)
        {
            return;
        }
        PlayerStatus.AcquireFor(StatusFlag.Invincible, _duration, "PhantomVeil");
        if (pc.mainCharacter != null)
        {
            DashAfterimage.Begin(pc.mainCharacter, _duration);
        }
    }
}
