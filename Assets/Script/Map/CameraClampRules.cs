/// <summary>
/// 카메라 중심을 경계 안으로 즉시 넣는 순수 계산(ProCamera2DNumericBoundaries 의 하드 클램프와 같은 규칙: 아래/왼쪽 먼저, 위/오른쪽이 이김).
/// 포털 도착 때 CenterOnTargets 가 경계 밖에 카메라를 놓으면 부드러운 경계가 몇 프레임에 걸쳐 끌어들여 "미끄러지는" 모습이 보였다(Q-PT01).
/// </summary>
public static class CameraClampRules
{
    public static float Clamp(float center, float half, bool useMin, float min, bool useMax, float max)
    {
        if (useMin && center - half < min)
        {
            center = min + half;
        }
        if (useMax && center + half > max)
        {
            center = max - half;
        }
        return center;
    }
}
