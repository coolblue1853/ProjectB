// 로프 트리거 안에서 잠금(LadderLock)이 풀린 뒤 isLadder 복구 검증(SideRoad 로프0 x≈-393.6). 플레이 모드, 봇 미실행. 사용: u.sh cs
var pc = PlayerController.instance;
if (pc == null) return "no player";
if (PlayBot.IsRunning) return "bot running";
if (BgmDirector.CurrentMap != "Lv1_NearForest_SideRoad") return "map " + BgmDirector.CurrentMap;
var sb = new System.Text.StringBuilder();
GameObject best = null; float bd = 1e9f;
foreach (var g in GameObject.FindGameObjectsWithTag("Ladder")) { float d = Mathf.Abs(g.transform.position.x + 393.6f); if (d < bd) { bd = d; best = g; } }
if (best == null) return "no ladder";
var col = best.GetComponent<Collider2D>();
sb.AppendLine("ladder " + best.name + " x=" + best.transform.position.x.ToString("F2") + " bounds y " + col.bounds.min.y.ToString("F2") + "~" + col.bounds.max.y.ToString("F2"));
// 잠금을 잡은 채 트리거 안으로 이동 → Enter 가 잠금 때문에 무시된다
var tok = PlayerStatus.AcquireFor(StatusFlag.LadderLock, 0.8f, "probe");
pc.transform.position = new Vector3(best.transform.position.x, col.bounds.min.y + 0.5f, 0f);
pc.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
return sb.ToString() + "locked; " + (tok != null ? "tok" : "tok=null") + " (다음 호출 rope_stay_read 에서 잠금 해제 후 isLadder 확인)";
