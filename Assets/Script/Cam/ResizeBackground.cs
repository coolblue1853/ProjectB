using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResizeBackground : MonoBehaviour
{
    public Camera mainCamera;
    public float scaleMultiplier = 1.0f;
    public Vector3 defaultSkyScale = new Vector3(0.8561203f, 0.4349922f, 0.4896549f);

    private Vector3 initialScale;

    private void Start()
    {
        // 초기 스케일 설정
        initialScale = defaultSkyScale;
        transform.localScale = initialScale;
    }

    private void Update()
    {
        // 카메라의 투사 크기에 따라 스케일 조정
        float orthographicSize = mainCamera.orthographicSize;
        float aspectRatio = mainCamera.aspect;

        // 새로운 스케일 계산
        float newScaleX = orthographicSize * 2 * aspectRatio * scaleMultiplier;
        float newScaleY = orthographicSize * 2 * scaleMultiplier;

        // 현재 스케일과 비교하여 변경 여부 결정
        Vector3 newScale = new Vector3(newScaleX, newScaleY, transform.localScale.z);
        if (newScale != transform.localScale)
        {
            // 변경이 필요한 경우에만 새로운 스케일 적용
            transform.localScale = newScale;
        }
    }
}
