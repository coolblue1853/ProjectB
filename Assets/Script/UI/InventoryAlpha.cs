using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class InventoryAlpha : MonoBehaviour
{
    Transform parent;
    // Start is called before the first frame update
    void Start()
    {
         parent = transform.parent;

    }
    public void ChangeAlpha()
    {
  
        // A�� �θ��� ������ �ڽ����� Ȯ��
        bool isLastChild = transform.GetSiblingIndex() == parent.childCount - 1;
        Debug.Log(isLastChild);
        // A�� �ڽĵ��� �ݺ�
        foreach (Transform child in transform)
        {
            Image image = child.GetComponent<Image>();
            Collider2D collider = child.GetComponent<Collider2D>();
            if (image != null)
            {
                // A�� �θ��� ������ �ڽ��� �ƴϸ� ���İ��� 0���� ����
                // �׷��� ������ ���İ��� 1�� ����
                image.color = new Color(image.color.r, image.color.g, image.color.b, isLastChild ? 1f : 0f);
                collider.enabled  =isLastChild ? true : false;
            }
        }
    }
    public void A20()
    {
        foreach (Transform child in transform)
        {
            Image image = child.GetComponent<Image>();
            Collider2D collider = child.GetComponent<Collider2D>();
            if (image != null)
            {
                // A�� �θ��� ������ �ڽ��� �ƴϸ� ���İ��� 0���� ����
                // �׷��� ������ ���İ��� 1�� ����
                image.color = new Color(image.color.r, image.color.g, image.color.b, 0f);
                collider.enabled = false;
            }
        }
    }
    public void A21()
    {
        foreach (Transform child in transform)
        {
            Image image = child.GetComponent<Image>();
            Collider2D collider = child.GetComponent<Collider2D>();
            if (image != null)
            {
                // A�� �θ��� ������ �ڽ��� �ƴϸ� ���İ��� 0���� ����
                // �׷��� ������ ���İ��� 1�� ����
                image.color = new Color(image.color.r, image.color.g, image.color.b, 1f);
                collider.enabled = true;
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
