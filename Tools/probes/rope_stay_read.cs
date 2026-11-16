// rope_stay_enter 뒤 1.5초 지나 실행: 잠금이 풀렸는데 로프 트리거 안에 서 있으면 isLadder 가 참이어야 한다
var pc = PlayerController.instance;
return "isLadder=" + pc.isLadder + " lock=" + DatabaseManager.checkAttackLadder + " pos=" + pc.transform.position.ToString("F2");
