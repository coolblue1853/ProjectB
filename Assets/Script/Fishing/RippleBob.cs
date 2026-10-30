using UnityEngine;

/// <summary>물결 표시가 천천히 커졌다 작아지게(낚시터 위치 알림). 트윈 없이 sin</summary>
public class RippleBob : MonoBehaviour
{
    private Vector3 _base;

    private void Awake()
    {
        _base = transform.localScale;
    }

    private void Update()
    {
        float s = 1f + 0.08f * Mathf.Sin(Time.time * 2.2f);
        transform.localScale = new Vector3(_base.x * s, _base.y, _base.z);
    }
}
