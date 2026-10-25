#!/usr/bin/env bash
# 픽셀 리그 기본 이동 점검(PixelRigDesign 4절): 플레이 → 낮 → 칼 장착 → 걷기·달리기·점프 연속 촬영 → 정지 → 합본
# 결과: Logs/playtest/rig_check.png (행: 걷기·달리기·점프, 각 8컷), 클립 기록은 표준 출력
# 사용: bash Tools/rig_check.sh
cd "$(dirname "$0")/.."
U="bash Tools/u.sh"
$U call read_console '{"action":"clear"}' >/dev/null 2>&1
$U call manage_editor '{"action":"play"}' >/dev/null 2>&1
sleep 12
$U cs Tools/probes/set_day.cs >/dev/null 2>&1
$U cs Tools/probes/input_bg_on.cs >/dev/null 2>&1
cat > Logs/equip.cs <<'EOF'
var inv = InventoryManager.instance;
var box = inv.equipBox[7].GetComponent<EquipBoxCheck>();
var d = DatabaseManager.instance.LoadItemData(DatabaseManager.instance.FindItemDataIndex("RustedSword"));
box.LoadPrefab("RustedSword", "Weapon", d.tfName);
var amg = UnityEngine.Object.FindFirstObjectByType<AttackManager>();
DG.Tweening.DOVirtual.DelayedCall(0.1f, () => { if (amg.equipWeapon == null && Weapon.Equipped != null) { amg.equipWeapon = Weapon.Equipped; amg.EquipMainWeaopon(); } }, true);
return "equip scheduled";
EOF
$U cs Logs/equip.cs >/dev/null 2>&1
sleep 3
echo 'var l = System.AppDomain.CurrentDomain.GetData("pb_rigact"); return l == null ? "-" : l.ToString();' > Logs/rl.cs
for a in walk run jump; do
  sed "s/__ACTION__/$a/g" Tools/probes/rig_action_strip.cs > Logs/ra.cs
  $U cs Logs/ra.cs >/dev/null 2>&1
  sleep 5
  echo "$a: $($U cs Logs/rl.cs 2>&1 | head -c 220)"
  sleep 2
done
$U cs Tools/probes/input_bg_off.cs >/dev/null 2>&1
echo "errors: $($U call read_console '{"action":"get","types":["error"],"count":"5","format":"plain"}' 2>&1 | head -c 300)"
$U call manage_editor '{"action":"stop"}' >/dev/null 2>&1
python - <<'EOF'
from PIL import Image
ims = [Image.open(f"Logs/playtest/rig_act_{a}.png") for a in ["walk", "run", "jump"]]
w = max(im.size[0] for im in ims); h = ims[0].size[1]
out = Image.new("RGB", (w, h * 3))
for i, im in enumerate(ims):
    out.paste(im, (0, i * h))
out.save("Logs/playtest/rig_check.png")
print("sheet", out.size)
EOF
