"""keys json 의 동작을 PixelLab animate_with_skeleton_v3 로 바로 보낸다(MCP 엔드포인트를 JSON-RPC 로 직접 호출 — 긴 키포인트를 대화로 옮기지 않아 Claude 사용량 절약).
인증 헤더는 ~/.claude.json 의 이 프로젝트 pixellab MCP 설정에서 읽고 출력하지 않는다.
사용: python Tools/pixelfx/m41_submit.py <keys json> <동작 이름> <action 문구(100자 이내)> <seed>  → job_id 출력
"""
import json
import os
import sys
import urllib.request

ROOT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..")
FIRST = os.path.join(ROOT, "Docs", "ArtCompare", "Motion", "cel", "m41", "first_frame.json")


def server():
    d = json.load(open(os.path.expanduser("~/.claude.json"), encoding="utf-8"))
    for k, v in d.get("projects", {}).items():
        s = v.get("mcpServers", {}).get("pixellab")
        if s and "ProjectB" in k:
            return s["url"], s.get("headers", {})
    raise SystemExit("pixellab MCP 설정 없음")


def rpc(url, headers, body, sid=None):
    h = dict(headers)
    h.update({"Content-Type": "application/json", "Accept": "application/json, text/event-stream"})
    if sid:
        h["Mcp-Session-Id"] = sid
    req = urllib.request.Request(url, json.dumps(body).encode(), h, method="POST")
    with urllib.request.urlopen(req, timeout=120) as r:
        sid = r.headers.get("Mcp-Session-Id") or sid
        raw = r.read().decode("utf-8")
    if raw.lstrip().startswith("{"):
        return json.loads(raw), sid
    data = [ln[5:].strip() for ln in raw.splitlines() if ln.startswith("data:")]
    return (json.loads(data[-1]) if data else None), sid


def main():
    keys, name, action, seed = sys.argv[1], sys.argv[2], sys.argv[3], int(sys.argv[4])
    ff = json.load(open(FIRST, encoding="utf-8"))
    kp = json.load(open(keys, encoding="utf-8"))[name]["kp"]
    kp = [[{"label": p["label"], "x": round(p["x"], 3), "y": round(p["y"], 3)} for p in f] for f in kp]
    url, headers = server()
    _, sid = rpc(url, headers, {"jsonrpc": "2.0", "id": 1, "method": "initialize", "params": {
        "protocolVersion": "2025-03-26", "capabilities": {}, "clientInfo": {"name": "m41_submit", "version": "1"}}})
    rpc(url, headers, {"jsonrpc": "2.0", "method": "notifications/initialized"}, sid)
    args = {"description": ff["description"], "action": action[:100], "direction": "east", "view": "side", "seed": seed,
            "first_frame_url": ff["first_frame_url"], "first_frame_keypoints": ff["first_frame_keypoints"], "keypoints": kp}
    res, _ = rpc(url, headers, {"jsonrpc": "2.0", "id": 2, "method": "tools/call",
                                "params": {"name": "animate_with_skeleton_v3", "arguments": args}}, sid)
    text = "".join(c.get("text", "") for c in (res or {}).get("result", {}).get("content", []))
    job = next((ln.split(":", 1)[1].strip() for ln in text.splitlines() if ln.startswith("job_id:")), None)
    print(name, len(kp), job or text[:300])


if __name__ == "__main__":
    main()
