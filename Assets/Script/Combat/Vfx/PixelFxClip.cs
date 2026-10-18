using UnityEngine;

/// <summary>
/// 도트 이펙트 프레임 묶음 (PixelVfxBaker가 3D 파티클을 렌더→도트화해서 만든다). Vfx가 SpriteRenderer로 재생한다.
/// </summary>
[CreateAssetMenu(menuName = "ProjectB/Pixel Fx Clip")]
public class PixelFxClip : ScriptableObject
{
    public Sprite[] frames;
    public float fps = 20f;
    [Tooltip("베이크 출처 (프리팹·테마) — 재베이크 추적용")]
    public string source;

    public float Duration => frames == null || fps <= 0f ? 0f : frames.Length / fps;
}
