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
        // �ʱ� ������ ����
        initialScale = defaultSkyScale;
        transform.localScale = initialScale;
    }

    private void Update()
    {
        // ī�޶��� ���� ũ�⿡ ���� ������ ����
        float orthographicSize = mainCamera.orthographicSize;
        float aspectRatio = mainCamera.aspect;

        // ���ο� ������ ���
        float newScaleX = orthographicSize * 2 * aspectRatio * scaleMultiplier;
        float newScaleY = orthographicSize * 2 * scaleMultiplier;

        // ���� �����ϰ� ���Ͽ� ���� ���� ����
        Vector3 newScale = new Vector3(newScaleX, newScaleY, transform.localScale.z);
        if (newScale != transform.localScale)
        {
            // ������ �ʿ��� ��쿡�� ���ο� ������ ����
            transform.localScale = newScale;
        }
    }
}
