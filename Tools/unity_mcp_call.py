"""unity-mcp(MCP for Unity) 서버를 stdio로 띄워 도구를 직접 호출하는 CLI.

Claude 세션에 MCP가 로드되지 않았을 때 Bash에서 에디터를 조작하기 위한 용도.

    uv run --with mcp python Tools/unity_mcp_call.py list
    uv run --with mcp python Tools/unity_mcp_call.py call read_console '{"action":"get","types":["error"]}'
"""
import asyncio
import json
import os
import sys

from mcp import ClientSession, StdioServerParameters
from mcp.client.stdio import stdio_client
from mcp.client.streamable_http import streamable_http_client

# 에디터가 HTTP 서버(기본 http://127.0.0.1:8080/mcp)에 붙는 모드면 HTTP로, 아니면 stdio로 서버를 띄운다.
HTTP_URL = os.environ.get("UNITY_MCP_URL", "http://127.0.0.1:8080/mcp")

UVX = os.environ.get("UVX_PATH", os.path.expandvars(r"%APPDATA%\Python\Python314\Scripts\uvx.exe"))


async def main() -> None:
    params = StdioServerParameters(
        command=UVX,
        args=["--from", "mcpforunityserver", "mcp-for-unity"],
        env={**os.environ, "DISABLE_TELEMETRY": "true", "UNITY_MCP_DISABLE_TELEMETRY": "true"},
    )
    use_http = os.environ.get("UNITY_MCP_STDIO") != "1"
    ctx = streamable_http_client(HTTP_URL) if use_http else stdio_client(params)
    async with ctx as streams:
        read, write = streams[0], streams[1]
        async with ClientSession(read, write) as session:
            await session.initialize()
            cmd = sys.argv[1] if len(sys.argv) > 1 else "list"
            if cmd == "list":
                tools = await session.list_tools()
                for t in tools.tools:
                    desc = (t.description or "").splitlines()[0][:110]
                    print(f"{t.name}: {desc}")
                    if "-v" in sys.argv:
                        print("   ", json.dumps(getattr(t, "input_schema", None) or getattr(t, "inputSchema", None), ensure_ascii=False)[:1500])
            elif cmd == "schema":
                tools = await session.list_tools()
                for t in tools.tools:
                    if t.name == sys.argv[2]:
                        print(t.description)
                        print(json.dumps(getattr(t, "input_schema", None) or getattr(t, "inputSchema", None), ensure_ascii=False, indent=1))
            elif cmd == "call":
                name = sys.argv[2]
                args = json.loads(sys.argv[3]) if len(sys.argv) > 3 else {}
                res = await session.call_tool(name, args)
                for c in res.content:
                    if getattr(c, "type", "") == "text":
                        print(c.text)
                    elif getattr(c, "type", "") == "image":
                        import base64
                        out = sys.argv[4] if len(sys.argv) > 4 else "Logs/mcp_image.png"
                        with open(out, "wb") as f:
                            f.write(base64.b64decode(c.data))
                        print(f"[image saved] {out}")
                    else:
                        print(c)
                if getattr(res, "is_error", getattr(res, "isError", False)):
                    sys.exit(1)


if __name__ == "__main__":
    sys.stdout.reconfigure(encoding="utf-8")
    asyncio.run(main())
