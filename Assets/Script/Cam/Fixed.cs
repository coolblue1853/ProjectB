using UnityEngine;

public class Fixed : MonoBehaviour
{
    private int lastScreenWidth;
    private int lastScreenHeight;

    private void Start()
    {
        SetResolution(); // 초기에 게임 해상도 고정
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;
    }

    private void Update()
    {
        // 현재 해상도와 이전 해상도를 비교
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
        {
            SetResolution(); // 해상도가 변경된 경우 해상도 설정
            lastScreenWidth = Screen.width; // 이전 해상도 값을 갱신
            lastScreenHeight = Screen.height;
        }
    }

    /* 해상도 설정하는 함수 */
    public void SetResolution()
    {
        int setWidth = 1920; // 사용자 설정 너비
        int setHeight = 1080; // 사용자 설정 높이

        int deviceWidth = Screen.width; // 기기 너비 저장
        int deviceHeight = Screen.height; // 기기 높이 저장

        // 전체 화면 상태가 아닐 때만 SetResolution을 호출하여 창 모드를 유지
        if (!Screen.fullScreen)
        {
            // 현재 창 모드를 유지하면서 해상도 설정
            Screen.SetResolution(setWidth, (int)(((float)deviceHeight / deviceWidth) * setWidth), false);
        }

        if ((float)setWidth / setHeight < (float)deviceWidth / deviceHeight) // 기기의 해상도 비율이 더 큰 경우
        {
            float newWidth = ((float)setWidth / setHeight) / ((float)deviceWidth / deviceHeight); // 새로운 너비 계산
            Camera.main.rect = new Rect((1f - newWidth) / 2f, 0f, newWidth, 1f); // 새로운 Rect 적용
        }
        else // 게임의 해상도 비율이 더 큰 경우
        {
            float newHeight = ((float)deviceWidth / deviceHeight) / ((float)setWidth / setHeight); // 새로운 높이 계산
            Camera.main.rect = new Rect(0f, (1f - newHeight) / 2f, 1f, newHeight); // 새로운 Rect 적용
        }
    }
}
