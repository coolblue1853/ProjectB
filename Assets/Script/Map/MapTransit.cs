using Com.LuisPedroFonseca.ProCamera2D;
using UnityEngine;

/// <summary>
/// 모든 맵·위치 이동의 공통 경로(Q-PT01, Floor1Completion 14-6).
/// 입력 잠금(ScreenFader) → 암전 → [이동 → 속도 0 → 카메라 즉시 스냅(댐핑 리셋)] → 밝아짐 → 입력 해제.
/// 포털, 웨이포인트, 귀환 주문서, 사망 부활, 낙사 복구가 모두 이 함수를 지난다.
/// </summary>
public static class MapTransit
{
    /// <summary>암전이 가장 어두울 때 move 를 부르고, 이어서 이동자 속도를 0 으로, 카메라를 즉시 맞춘다. done 은 다 밝아진 뒤</summary>
    public static void Run(GameObject traveler, System.Action move, System.Action done = null, ProCamera2D cam = null)
    {
        ScreenFader.Transition(() =>
        {
            try
            {
                move?.Invoke();
            }
            finally
            {
                Settle(traveler, cam); // 이동이 예외를 던져도 카메라는 맞춘다
            }
        }, done);
    }

    /// <summary>이동 직후: 남은 속도를 없애고 카메라를 목표 위치에 즉시 붙인다(이전 맵 좌표에서 미끄러지지 않게)</summary>
    public static void Settle(GameObject traveler, ProCamera2D cam = null)
    {
        if (traveler != null)
        {
            var rb = traveler.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }
        if (cam == null)
        {
            cam = Object.FindFirstObjectByType<ProCamera2D>();
        }
        if (cam != null)
        {
            cam.CenterOnTargets();
            ClampToBoundaries(cam);
        }
    }

    /// <summary>
    /// 스냅한 카메라가 숫자 경계 밖이면 바로 안으로 넣는다. 안 하면 부드러운 경계가 도착 후 몇 프레임 동안 끌어당겨 화면이 미끄러진다.
    /// </summary>
    private static void ClampToBoundaries(ProCamera2D cam)
    {
        ProCamera2DNumericBoundaries nb = cam.GetComponent<ProCamera2DNumericBoundaries>();
        if (nb == null)
        {
            nb = Object.FindFirstObjectByType<ProCamera2DNumericBoundaries>();
        }
        if (nb == null || !nb.enabled || !nb.UseNumericBoundaries)
        {
            return;
        }
        Vector3 p = cam.LocalPosition;
        float halfW = cam.ScreenSizeInWorldCoordinates.x * 0.5f, halfH = cam.ScreenSizeInWorldCoordinates.y * 0.5f;
        float x = CameraClampRules.Clamp(p.x, halfW, nb.UseLeftBoundary, nb.LeftBoundary, nb.UseRightBoundary, nb.RightBoundary);
        float y = CameraClampRules.Clamp(p.y, halfH, nb.UseBottomBoundary, nb.BottomBoundary, nb.UseTopBoundary, nb.TopBoundary);
        if (!Mathf.Approximately(x, p.x) || !Mathf.Approximately(y, p.y))
        {
            cam.MoveCameraInstantlyToPosition(new Vector2(x, y));
        }
    }
}
