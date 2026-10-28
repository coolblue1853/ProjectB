using System.Text;
using UnityEngine;

/// <summary>
/// 디버그 프로브용: 4초 동안 매 프레임 메인 카메라·플레이어 위치를 기록해 프레임 간 변화(떨림)를 Logs/playtest/idlecam.txt 로 남긴다(10-29 "가만히 있어도 화면이 떨린다").
/// </summary>
public class IdleCamRecorder : MonoBehaviour
{
    private const float Duration = 4f;
    private readonly StringBuilder _sb = new StringBuilder();
    private float _start;
    private Vector3 _lastCam, _lastPlayer;
    private int _frames, _camJumps, _playerJumps;
    private float _maxCam, _maxPlayer;

    private void Start()
    {
        _start = Time.unscaledTime;
        _lastCam = Camera.main.transform.position;
        _lastPlayer = PlayerController.instance.transform.position;
    }

    private void LateUpdate()
    {
        Vector3 c = Camera.main.transform.position;
        Vector3 p = PlayerController.instance.transform.position;
        float dc = (c - _lastCam).magnitude, dp = (p - _lastPlayer).magnitude;
        _maxCam = Mathf.Max(_maxCam, dc);
        _maxPlayer = Mathf.Max(_maxPlayer, dp);
        if (dc > 0.001f)
        {
            _camJumps++;
        }
        if (dp > 0.001f)
        {
            _playerJumps++;
        }
        if (_frames < 240)
        {
            _sb.Append($"{Time.unscaledTime - _start:0.000}\tcam {c.x:0.0000},{c.y:0.0000}\tplayer {p.x:0.0000},{p.y:0.0000}\n");
        }
        _frames++;
        _lastCam = c;
        _lastPlayer = p;
        if (Time.unscaledTime - _start >= Duration)
        {
            System.IO.Directory.CreateDirectory("Logs/playtest");
            System.IO.File.WriteAllText("Logs/playtest/idlecam.txt",
                $"frames {_frames} camMoved {_camJumps} maxCamStep {_maxCam:0.0000} playerMoved {_playerJumps} maxPlayerStep {_maxPlayer:0.0000}\n" + _sb);
            Destroy(gameObject);
        }
    }
}
