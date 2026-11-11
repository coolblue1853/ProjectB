using UnityEngine;

/// <summary>
/// 낙사·끼임 복귀(REWORK-0 U34). 마지막으로 지면에 서 있던 위치를 기억하고,
/// 맵 카메라 하한(MapInfo.CurrentBottom)보다 한참 아래로 떨어지면 그 위치로 되돌린다. 피해는 주지 않는다(임시 가정, OpenQuestions).
/// 포털 이동·맵 전환 직후에는 기록을 비워 이전 맵 좌표로 돌아가지 않게 한다.
/// </summary>
public sealed class FallRecovery : MonoBehaviour
{
    /// <summary>카메라 하한보다 이만큼 더 내려가면 낙사로 본다</summary>
    public const float KillMargin = 8f;
    private const float RecordInterval = 0.25f;
    private const float MapJumpDistance = 25f;

    private Rigidbody2D _rb;
    private PlayerController _pc;
    private Vector2 _safe;
    private bool _hasSafe;
    private float _nextRecord;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _pc = GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (_rb == null || _pc == null || DatabaseManager.isUsePortal)
        {
            return;
        }
        Vector2 pos = _rb.position;
        if (_hasSafe && (pos - _safe).magnitude > MapJumpDistance && pos.y > MapInfo.CurrentBottom - KillMargin)
        {
            _hasSafe = false; // 맵이 바뀌어 멀리 이동했다 → 이전 맵 좌표 폐기
        }
        if (_pc.isGround && Mathf.Abs(_rb.linearVelocity.y) < 0.05f && Time.time >= _nextRecord)
        {
            _nextRecord = Time.time + RecordInterval;
            _safe = pos;
            _hasSafe = true;
        }
        if (_hasSafe && pos.y < MapInfo.CurrentBottom - KillMargin)
        {
            Recover();
        }
    }

    private void Recover()
    {
        _rb.linearVelocity = Vector2.zero;
        _rb.position = _safe + Vector2.up * 0.3f;
        transform.position = _rb.position;
        PickupLog.Notice("낙사해서 마지막 발판으로 돌아왔어요.");
        GameLog.Info(GameLog.Cat.Map, "낙사 복귀 → " + _safe);
    }
}
