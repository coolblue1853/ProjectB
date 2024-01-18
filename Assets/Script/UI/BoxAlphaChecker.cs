using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class BoxAlphaChecker : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        ActivateChildrenBasedOnImageAlpha();
    }
    void ActivateChildrenBasedOnImageAlpha()
    {
        // �ڽ��� Image ������Ʈ ��������
        Image selfImage = GetComponent<Image>();

        if (selfImage != null)
        {
            // �ڽ��� �̹��� ���İ� ��������
            float selfAlpha = selfImage.color.a;

            // ��� �ڽ� ������Ʈ ��������
            Transform[] childTransforms = GetComponentsInChildren<Transform>(true);

            // �ڽ� ������Ʈ�� Ȱ��ȭ ���� ����
            foreach (Transform childTransform in childTransforms)
            {
                if (childTransform != transform) // �θ� �ڽ��� ����
                {
                    GameObject childObject = childTransform.gameObject;
                    childObject.SetActive(selfAlpha > 0f);
                }
            }
        }
        else
        {
            Debug.LogError("This object does not have an Image component.");
        }
    }
}

