#!/usr/bin/env bash
# 구역 회귀 소크: 맵마다 낮·밤으로 MapInfo.GoToMap → 게임 시간 N초 대기 → 몬스터 수·콘솔 에러/경고(아틀라스 제외)·스크린샷
# 사용: bash Tools/zone_soak.sh [대기초=15]   결과: Logs/zone_soak/result.tsv, 스크린샷 Logs/zone_soak/*.png. 세이브는 백업·복원
set -u
cd "$(dirname "$0")/.."
WAIT=${1:-15}
U="bash Tools/u.sh"
OUT=Logs/zone_soak; mkdir -p $OUT
SAVE="/c/Users/user/AppData/LocalLow/DefaultCompany/ProjectB/SaveFile.es3"
cp "$SAVE" $OUT/SaveFile.backup.es3 || { echo "save backup failed"; exit 1; }
trap 'cp $OUT/SaveFile.backup.es3 "$SAVE"; echo "save restored"' EXIT
MAPS="Town Lv1_NearForest_SideRoad Lv1_NormalForest Lv1_DeepForest Lv1_BanditFortress Lv1_Swamp Lv1_TowerOutskirts Lv2_MagicCliffs"
printf 'return "ready";\n' > $OUT/rdy.cs
printf 'return Time.time.ToString("F1", System.Globalization.CultureInfo.InvariantCulture);\n' > $OUT/time.cs
$U call read_console '{"action":"clear"}' >/dev/null
$U call manage_editor '{"action":"play"}' | head -c 60; echo
sleep 12
for i in $(seq 1 60); do $U cs $OUT/rdy.cs 2>/dev/null | grep -q ready && break; sleep 2; done
sleep 3
sed 's/__SEC__/1500/g' Tools/probes/keep_hp.cs > $OUT/keep.cs
$U cs $OUT/keep.cs
printf 'map\tdn\tat\tstat\terrors\twarnings\n' > $OUT/result.tsv
for DN in day night; do
  for M in $MAPS; do
    $U call read_console '{"action":"clear"}' >/dev/null
    sed "s/__MAP__/$M/g;s/__DN__/$DN/g" Tools/probes/zone_soak_go.cs > $OUT/go.cs
    GO=$($U cs $OUT/go.cs); T0=${GO%% *}; AT=${GO##*at=}
    for i in $(seq 1 60); do  # 최대 약 3분(실패한 응답이면 그냥 넘어간다)
      sleep 3
      T=$($U cs $OUT/time.cs)
      python -c "import sys; sys.exit(0 if float('$T') - float('$T0') >= $WAIT else 1)" 2>/dev/null && break
    done
    STAT=$($U cs Tools/probes/zone_soak_stat.cs)
    PREV=$(ls -t Assets/Screenshots/*.png 2>/dev/null | head -1)
    $U call manage_camera '{"action":"screenshot"}' >/dev/null
    for i in $(seq 1 10); do sleep 1; NEW=$(ls -t Assets/Screenshots/*.png 2>/dev/null | head -1); [ "$NEW" != "$PREV" ] && { cp "$NEW" "$OUT/${M}_${DN}.png"; break; }; done
    $U call read_console '{"action":"get","types":["error"],"count":"100","format":"plain"}' > "$OUT/${M}_${DN}.err.json"
    $U call read_console '{"action":"get","types":["warning"],"count":"200","format":"plain"}' > "$OUT/${M}_${DN}.warn.json"
    CNT=$(python - "$OUT/${M}_${DN}.err.json" "$OUT/${M}_${DN}.warn.json" <<'PY'
import json, sys
def load(f):
    try:
        return [i for i in (json.load(open(f, encoding="utf-8")).get("data") or []) if isinstance(i, str)]
    except Exception:
        return []
e = load(sys.argv[1])
w = [i for i in load(sys.argv[2]) if "atlas" not in i.lower()]
print(f"{len(e)}\t{len(w)}")
PY
)
    printf '%s\t%s\t%s\t%s\t%s\n' "$M" "$DN" "$AT" "$STAT" "$CNT" | tee -a $OUT/result.tsv
  done
done
$U call manage_editor '{"action":"stop"}' | head -c 60; echo
sleep 3
