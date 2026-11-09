"""방향 돌려보기 HTML 만들기(11-06). 사용: python Tools/pixelfx/spin_viewer.py  — 방향 목록은 ITEMS 를 고친다.
← → 로 돌리고 Space 자동 회전, 숫자키 바로 이동. 출력: Logs/motion_review/dir5_new/spin_viewer.html (브라우저로 연다)"""
import base64
import json
import os

ROOT = os.path.join(os.path.dirname(os.path.abspath(__file__)), "..", "..")
D = "Logs/motion_review/dir5_new/"
ITEMS = [
    ("① 정면", D + "fs_0.png"),
    ("② 3/4 앞 (오른쪽)", D + "c2.png"),
    ("③ 완전 측면 (오른쪽)", D + "c3.png"),
    ("④ 3/4 뒤 (오른쪽)", D + "c4.png"),
    ("⑤ 뒷모습", D + "c5.png"),
    ("⑥ 3/4 뒤 (왼쪽, ④ 반전)", D + "c6.png"),
    ("⑦ 완전 측면 (왼쪽, ③ 반전)", D + "c7.png"),
    ("⑧ 3/4 앞 (왼쪽, ② 반전)", D + "c8.png"),
]
data = [{"t": t, "src": "data:image/png;base64," + base64.b64encode(open(os.path.join(ROOT, p), "rb").read()).decode()} for t, p in ITEMS]
html = '''<!doctype html><meta charset="utf-8"><title>방향 돌려보기</title>
<style>body{margin:0;background:#46506a;color:#fff;font-family:'Malgun Gothic',sans-serif;display:flex;flex-direction:column;align-items:center;height:100vh;justify-content:center}
img{width:512px;height:512px;image-rendering:pixelated;background:#46506a}
#t{font-size:26px;margin:12px}#h{opacity:.8;margin-top:10px}#d{display:flex;gap:8px;margin-top:10px}
#d i{width:14px;height:14px;border-radius:50%;background:#8a93a8;display:block}#d i.on{background:#ffd54a}</style>
<div id="t"></div><img id="im"><div id="d"></div>
<div id="h">← → 방향키 · Space 자동 회전 · 숫자키 바로 이동</div>
<script>
const D=@@DATA@@;let k=0,auto=null;
const d=document.getElementById('d');D.forEach(()=>d.appendChild(document.createElement('i')));
function show(){document.getElementById('t').textContent=D[k].t;document.getElementById('im').src=D[k].src;[...d.children].forEach((e,i)=>e.className=i==k?'on':'')}
addEventListener('keydown',e=>{if(e.key=='ArrowRight')k=(k+1)%D.length;else if(e.key=='ArrowLeft')k=(k+D.length-1)%D.length;
else if(e.key==' '){e.preventDefault();if(auto){clearInterval(auto);auto=null}else auto=setInterval(()=>{k=(k+1)%D.length;show()},350);return}
else if(e.key>='1'&&e.key<='9'&&+e.key<=D.length)k=+e.key-1;else return;show()});show();
</script>'''.replace("@@DATA@@", json.dumps(data, ensure_ascii=False))
open(os.path.join(ROOT, D + "spin_viewer.html"), "w", encoding="utf-8").write(html)
print("ok", len(data))
