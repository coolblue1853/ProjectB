using AnyPortrait;
using UnityEngine;

/// <summary>기존 AnyPortrait 캐릭터를 <see cref="CharacterRig"/> API로 감싼다 — 동작은 예전과 같다.</summary>
public class PortraitRig : CharacterRig
{
    private apPortrait _portrait;

    public static PortraitRig For(apPortrait portrait)
    {
        if (portrait == null)
        {
            return null;
        }
        PortraitRig rig = portrait.GetComponent<PortraitRig>();
        if (rig == null)
        {
            rig = portrait.gameObject.AddComponent<PortraitRig>();
        }
        rig._portrait = portrait;
        return rig;
    }

    public override void Play(string anim)
    {
        _portrait.Play(anim);
    }

    public override void CrossFade(string anim, float fade)
    {
        _portrait.CrossFade(anim, fade);
    }

    public override bool IsPlaying(string anim)
    {
        return _portrait.IsPlaying(anim);
    }

    public override void SetAnimationSpeed(float speed)
    {
        _portrait.SetAnimationSpeed(speed);
    }

    public override void SetAnimationSpeed(string anim, float speed)
    {
        _portrait.SetAnimationSpeed(anim, speed);
    }

    public override void SetMeshAlphaAll(float alpha)
    {
        _portrait.SetMeshAlphaAll(alpha);
    }

    public override void SetControlParamInt(string param, int value)
    {
        _portrait.SetControlParamInt(param, value);
    }

    public override Transform GetBoneSocket(string socket)
    {
        return _portrait.GetBoneSocket(socket);
    }

    public override void SetMeshImage(string part, Texture2D texture)
    {
        _portrait.SetMeshImage(part, texture);
    }
}
