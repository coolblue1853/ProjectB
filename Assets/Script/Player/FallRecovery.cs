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
    private const float LostRetry = 3f;

    private Rigidbody2D _rb;
    private PlayerController _pc;
    private Vector2 _safe;
    private bool _hasSafe;
    private float _nextRecord;
    private float _nextLost;

    /// <summary>
    /// 맵 카메라 하한보다 한참 아래인가(맵 밖). 안전 위치가 없어도(로드·텔레포트 직후 공중) 판단하는 기준이며,
    /// 이 위치는 저장하지 않는다(11-15 R5: y≈-74312 로 저장돼 다음 로드도 허공에서 시작하던 문제).
    /// </summary>
    public static bool IsLost(float y, float mapBottom)
    {
        return y < mapBottom - KillMargin;
    }

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
        if (_hasSafe && IsLost(pos.y, MapInfo.CurrentBottom))
        {
            Recover();
        }
        else if (!_hasSafe && IsLost(pos.y, MapInfo.CurrentBottom) && Time.time >= _nextLost && !ScreenFader.Busy)
        {
            // 안전 위치가 한 번도 없었다(로드·웨이포인트 직후 허공) → 영원히 떨어지므로 마을로 보낸다
            _nextLost = Time.time + LostRetry;
            GameLog.Info(GameLog.Cat.Map, "낙사 복귀: 안전 위치 없음 y=" + pos.y + " → 마을");
            PickupLog.Notice("길을 잃어 마을로 돌아왔어요.");
            MapInfo.GoToTown(_pc.gameObject);
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
