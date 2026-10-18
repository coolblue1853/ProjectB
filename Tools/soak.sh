#!/usr/bin/env bash
# 자동 장시간 테스트: bash Tools/soak.sh [사이클=10] [무기=RustedSwordandShield]
# 매 사이클: 마을↔사냥터 포털, 무작위 스포너 이동·사냥(soak_step ×6), 가끔 밤 전환·보스 지역. 끝나면 에러/경고/모니터 요약 수집.
set -u
cd "$(dirname "$0")/.."
CYCLES=${1:-10}
WEAPON=${2:-RustedSwordandShield}
U="bash Tools/u.sh"
OUT=Logs/soak; mkdir -p $OUT
SAVE="/c/Users/user/AppData/LocalLow/DefaultCompany/ProjectB/SaveFile.es3"
cp "$SAVE" $OUT/SaveFile.backup.es3
sed 's/__KEY__/SideRoad/' Tools/probes/use_portal.cs > $OUT/to_field.cs
sed 's/__KEY__/Town/' Tools/probes/use_portal.cs > $OUT/to_town.cs
sed 's/__KEY__/Boss/' Tools/probes/tp_spawner.cs > $OUT/to_boss.cs
sed "s/__WEAPON__/$WEAPON/" Tools/probes/equip_weapon.cs > $OUT/equip.cs
$U call read_console '{"action":"clear"}' >/dev/null
$U call manage_editor '{"action":"play"}' | head -c 60; echo
sleep 10
$U cs $OUT/equip.cs
for c in $(seq 1 $CYCLES); do
  echo "== cycle $c"
  $U cs $OUT/to_field.cs | head -c 120; echo
  sleep 4
  if [ $((c % 3)) -eq 0 ]; then $U cs Tools/probes/set_night.cs | head -c 100; echo; fi
  if [ $((c % 4)) -eq 0 ]; then $U cs $OUT/to_boss.cs | head -c 100; echo; sleep 6; fi
  for s in 1 2 3 4 5 6; do $U cs Tools/probes/soak_step.cs | head -c 220; echo; sleep 2; done
  $U cs $OUT/to_town.cs | head -c 120; echo
  sleep 3
done
printf 'return RuntimeMonitor.LastSummary + " | anomalies=" + RuntimeMonitor.AnomalyCount;\n' > $OUT/mon.cs
$U cs $OUT/mon.cs
$U call read_console '{"action":"get","types":["error"],"count":"200","format":"plain"}' > $OUT/errors.json
$U call read_console '{"action":"get","types":["warning"],"count":"200","format":"plain","filter_text":"[PB]"}' > $OUT/pb_warnings.json
$U call read_console '{"action":"get","types":["log"],"count":"300","format":"plain","filter_text":"[PB][Monitor]"}' > $OUT/monitor.json
$U call manage_editor '{"action":"stop"}' | head -c 60; echo
sleep 2
cp $OUT/SaveFile.backup.es3 "$SAVE"
python - <<'PY'
import json, collections
for f in ["Logs/soak/errors.json", "Logs/soak/pb_warnings.json", "Logs/soak/monitor.json"]:
    try:
        d = json.load(open(f, encoding="utf-8"))
    except Exception as e:
        print(f, "parse fail", e); continue
    items = d.get("data") or []
    c = collections.Counter(i.strip().split("\n")[0][:240] for i in items if isinstance(i, str))
    print(f"--- {f}: {len(items)} entries, {len(c)} unique")
    for k, v in c.most_common(25):
        print(f"{v:4d}x {k}")
PY
