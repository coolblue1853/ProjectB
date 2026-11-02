"""pixellab MCP 켜고 끄기(토큰 절약). 사용: python Tools/mcp_toggle.py off|on  — 적용은 새 세션부터.
끈 항목은 ~/.claude/mcp_stash.json 에 보관(인증 헤더 포함, 저장소 밖)."""
import json, os, sys
home = os.path.expanduser('~')
cfg, stash = os.path.join(home, '.claude.json'), os.path.join(home, '.claude', 'mcp_stash.json')
proj = 'C:/Users/user/ProjectB'
d = json.load(open(cfg, encoding='utf-8'))
s = d['projects'][proj]['mcpServers']
st = json.load(open(stash, encoding='utf-8')) if os.path.exists(stash) else {}
mode = sys.argv[1] if len(sys.argv) > 1 else ''
if mode == 'off' and 'pixellab' in s:
    st['pixellab'] = s.pop('pixellab')
elif mode == 'on' and 'pixellab' in st:
    s['pixellab'] = st.pop('pixellab')
else:
    sys.exit('상태: ' + ('켜짐' if 'pixellab' in s else '꺼짐'))
json.dump(d, open(cfg, 'w', encoding='utf-8'), ensure_ascii=False, indent=2)
json.dump(st, open(stash, 'w', encoding='utf-8'), ensure_ascii=False, indent=2)
print('pixellab', mode)
