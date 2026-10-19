using Com.LuisPedroFonseca.ProCamera2D;
using UnityEngine;
using UrpPixelPerfect = UnityEngine.Rendering.Universal.PixelPerfectCamera;

/// <summary>
/// 화면 전체 픽셀 밀도 통일 (ArtStyle.md 머리말 "픽셀 느낌도 전반적으로 맞추고").
/// 메인 카메라에 URP Pixel Perfect Camera 를 붙여 24 PPU · 480×270 렌더 텍스처로 그린 뒤 정수 배율로 키운다.
/// 소스마다 다른 텍셀 밀도(배경 14~143, 몹 24, AnyPortrait 메시)가 같은 픽셀 격자로 정리된다.
/// 씬을 저장하지 않으려고 런타임에 붙인다. 보스 연출 줌아웃은 기준 해상도를 키워 정수 배율로 처리한다.
/// </summary>
public static class PixelLook
{
    public const bool Enabled = true;
    // 격자 = 아트 24 PPU 의 2배(48). 480×270(화면 4px 단위)으로 그리면 카메라 추적이 4px 계단으로 움직여 화면이 덜덜 떨렸다(사용자 멀미 피드백, 2026-10-19).
    // 960×540 이면 스텝이 2px 로 줄고, 아트 도트 1칸 = 격자 2칸(정수)이라 밀도 통일은 유지된다. 직교 크기(5.625)는 그대로.
    const int AssetsPpu = 48;
    static readonly Vector2Int NormalRes = new Vector2Int(960, 540);
    static readonly Vector2Int ZoomOutRes = new Vector2Int(1280, 720);

    static UrpPixelPerfect _ppc;

    public static bool Active => _ppc != null && _ppc.isActiveAndEnabled;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Setup()
    {
        _ppc = null;
        Camera cam = Camera.main;
        if (!Enabled || cam == null)
        {
            return;
        }
        _ppc = cam.GetComponent<UrpPixelPerfect>();
        if (_ppc == null)
        {
            _ppc = cam.gameObject.AddComponent<UrpPixelPerfect>();
        }
        _ppc.assetsPPU = AssetsPpu;
        _ppc.gridSnapping = UrpPixelPerfect.GridSnapping.UpscaleRenderTexture;
        _ppc.cropFrame = UrpPixelPerfect.CropFrame.None;
        ApplyRes(NormalRes);
        // ProCamera2D 픽셀 퍼펙트는 카메라 위치만 1/184 격자에 맞춘다. URP 쪽과 겹치면 떨리므로 끈다
        ProCamera2DPixelPerfect legacy = cam.GetComponent<ProCamera2DPixelPerfect>();
        if (legacy != null)
        {
            legacy.enabled = false;
        }
    }

    /// <summary>보스 연출용 줌아웃. Pixel Perfect 가 직교 크기를 잠그므로 ProCamera2D.Zoom 대신 쓴다.</summary>
    public static void SetZoomOut(bool zoomOut)
    {
        if (!Active)
        {
            return;
        }
        ApplyRes(zoomOut ? ZoomOutRes : NormalRes);
    }

    static void ApplyRes(Vector2Int res)
    {
        _ppc.refResolutionX = res.x;
        _ppc.refResolutionY = res.y;
    }
}
