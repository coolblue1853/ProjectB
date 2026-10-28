using UnityEngine;

/// <summary>
/// 플레이어 캐릭터 그림·애니의 공통 창구 (PixelRigDesign.md 3절).
/// 플레이어 코드는 apPortrait 대신 이 API만 쓴다 — AnyPortrait 래퍼(<see cref="PortraitRig"/>)와 새 픽셀 리그(<see cref="PixelRig"/>)가 구현한다.
/// </summary>
public abstract class CharacterRig : MonoBehaviour
{
    public abstract void Play(string anim);
    public abstract void CrossFade(string anim, float fade);
    public abstract bool IsPlaying(string anim);
    /// <summary>전체 재생 속도(히트스톱 0, 복원 1)</summary>
    public abstract void SetAnimationSpeed(float speed);
    /// <summary>특정 클립이 재생 중일 때만 그 클립의 속도(공속 배율)</summary>
    public abstract void SetAnimationSpeed(string anim, float speed);
    public abstract void SetMeshAlphaAll(float alpha);
    public abstract void SetControlParamInt(string param, int value);
    public abstract Transform GetBoneSocket(string socket);
    /// <summary>방어구 외형 교체(부위 이름 = AnyPortrait 메시 이름)</summary>
    public abstract void SetMeshImage(string part, Texture2D texture);

    /// <summary>셀 그림의 원점(발 가운데). 셀과 같은 좌표로 그린 스미어를 여기에 띄운다. 없으면 null</summary>
    public virtual Transform CelRoot => null;

    /// <summary>공격 클립의 frame 번째 프레임 시작 시각(초, 속도 1). 모르면 음수</summary>
    public virtual float FrameStartTime(string anim, int frame)
    {
        return -1f;
    }

    /// <summary>소켓에 붙인 무기 스프라이트를 리그에 맞게 정리한다(정렬 순서·크기). 기본은 아무것도 안 함.</summary>
    public virtual void AdoptWeapon(Transform weapon, bool isLeft)
    {
    }
}
