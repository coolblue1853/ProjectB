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
        // 자신의 Image 컴포넌트 가져오기
        Image selfImage = GetComponent<Image>();

        if (selfImage != null)
        {
            // 자신의 이미지 알파값 가져오기
            float selfAlpha = selfImage.color.a;

            // 모든 자식 오브젝트 가져오기
            Transform[] childTransforms = GetComponentsInChildren<Transform>(true);

            // 자식 오브젝트의 활성화 여부 설정
            foreach (Transform childTransform in childTransforms)
            {
                if (childTransform != transform) // 부모 자신은 제외
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

