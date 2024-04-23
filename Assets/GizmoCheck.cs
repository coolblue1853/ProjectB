using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GizmoCheck : MonoBehaviour
{
    public Vector2 boxSize; // 확인할 직사각형 영역의 크기
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        // 현재 오브젝트의 위치에서 boxSize를 기준으로 직사각형 그리기
        Gizmos.DrawWireCube(transform.position, new Vector3(boxSize.x, boxSize.y, 1));
    }
}
