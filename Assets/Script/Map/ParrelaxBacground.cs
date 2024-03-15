using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParrelaxBacground : MonoBehaviour
{
    [SerializeField] private Vector2 parallaxEffectMultiplier;
    private Transform cameraTransform;
    private Vector3 lastCameraPosition;
    private float textureUnitSizeX;
    private float textureUnitSizeY;
    public bool isInfiniteHor;
    public bool isInfiniteVer;

    private void Start()
    {
        cameraTransform = Camera.main.transform;
        lastCameraPosition = cameraTransform.position;
        Sprite sprite = GetComponent<SpriteRenderer>().sprite;
        Texture2D texture = sprite.texture;

        // 원래의 텍스처 단위 크기 계산
        float originalTextureUnitSizeX = texture.width / sprite.pixelsPerUnit;
        float originalTextureUnitSizeY = texture.height / sprite.pixelsPerUnit;

        // 현재 스케일로 텍스처 단위 크기 조정
        textureUnitSizeX = originalTextureUnitSizeX * transform.localScale.x;
        textureUnitSizeY = originalTextureUnitSizeY * transform.localScale.y;
    }
    private void LateUpdate()
    {
        Vector3 deltaMovemnt = cameraTransform.position - lastCameraPosition;
        transform.position += new Vector3( deltaMovemnt.x * parallaxEffectMultiplier.x, deltaMovemnt.y * parallaxEffectMultiplier.y);
        lastCameraPosition = cameraTransform.position;

        if (isInfiniteHor)
        {
            if (Mathf.Abs(cameraTransform.position.x - transform.position.x) >= textureUnitSizeX)
            {
                float offsetPositionX = (cameraTransform.position.x - transform.position.x) % textureUnitSizeX;
                transform.position = new Vector3(cameraTransform.position.x + offsetPositionX, transform.position.y);
            }
        }
        if (isInfiniteVer)
        {
            if (Mathf.Abs(cameraTransform.position.y - transform.position.y) >= textureUnitSizeY)
            {
                float offsetPositionY = (cameraTransform.position.y - transform.position.y) % textureUnitSizeY;
                transform.position = new Vector3(cameraTransform.position.x, transform.position.y + offsetPositionY);
            }
        }

    }

}
