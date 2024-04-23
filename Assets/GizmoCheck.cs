using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GizmoCheck : MonoBehaviour
{
    public Vector2 boxSize; // Ȯ���� ���簢�� ������ ũ��
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        // ���� ������Ʈ�� ��ġ���� boxSize�� �������� ���簢�� �׸���
        Gizmos.DrawWireCube(transform.position, new Vector3(boxSize.x, boxSize.y, 1));
    }
}
