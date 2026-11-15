"""playrec_fit 순수 함수 시험: python -m unittest Tools.tests.test_playrec_fit (저장소 루트에서)."""
import os
import sys
import unittest

sys.path.insert(0, os.path.join(os.path.dirname(os.path.abspath(__file__)), ".."))
import playrec_fit as pf  # noqa: E402


def inp(t, a, ph, v=1.0):
    return {"t": t, "k": "in", "a": a, "ph": ph, "dev": "kb", "v": v}


def tk(t, a=0, mx=0.0, en=None, s=1, ed=None, hp=None, dr=None, p=(0.0, 0.0)):
    r = {"t": t, "k": "tk", "a": a, "mx": mx, "p": [p[0], p[1], 0, 0], "s": s,
         "ed": ed or [8, 8, 0, -1], "en": en or []}
    if hp:
        r["hp"] = hp
    if dr:
        r["dr"] = dr
    return r


def foe(sp, dx, dist=None, hp=100, state=0):
    return [sp, dx, 0.0, 0, 0, abs(dx) if dist is None else dist, hp, state, 0, 0]


class DistTests(unittest.TestCase):
    def test_small_sample_is_null_with_n(self):
        d = pf.dist([0.1, 0.2, 0.3])
        self.assertEqual(d["n"], 3)
        self.assertIsNone(d["p50"])
        self.assertIsNone(d["p10"])

    def test_percentiles(self):
        d = pf.dist([1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11])
        self.assertEqual(d["n"], 11)
        self.assertAlmostEqual(d["p50"], 6.0)
        self.assertAlmostEqual(d["p10"], 2.0)
        self.assertAlmostEqual(d["p90"], 10.0)

    def test_ratio_null_below_min(self):
        self.assertIsNone(pf.ratio(1, 2)["value"])
        self.assertEqual(pf.ratio(3, 6)["value"], 0.5)

    def test_rate_null_when_short(self):
        self.assertIsNone(pf.rate("x", 10, 0.5)["value"])
        self.assertEqual(pf.rate("x", 10, 2.0)["value"], 5.0)


class InputTests(unittest.TestCase):
    def test_press_durations_pairs_s_c(self):
        rows = [inp(1.0, "Player/Attack", "S"), inp(1.1, "Player/Attack", "C"),
                inp(2.0, "Player/Attack", "S"), inp(2.0, "Player/Jump", "S"), inp(2.3, "Player/Attack", "C")]
        d = pf.press_durations(rows)
        self.assertEqual(len(d["Player/Attack"]), 2)
        self.assertAlmostEqual(d["Player/Attack"][1], 0.3)
        self.assertNotIn("Player/Jump", d)  # C 가 없으면 버린다

    def test_bursts_split_at_gap(self):
        within, lengths, between = pf.bursts([0.0, 0.2, 0.4, 1.5, 1.7, 20.0])
        self.assertEqual(lengths, [3, 2, 1])
        self.assertEqual(len(within), 3)
        self.assertEqual(len(between), 1)  # 1.7 -> 20.0 은 INTERVAL_CAP 초과

    def test_direction_change_intervals(self):
        rows = [inp(0, "Player/Move", "S", 1), inp(1, "Player/Move", "C", 0),
                inp(2, "Player/Move", "S", -1), inp(3, "Player/Move", "P", 1), inp(3.5, "Player/Move", "S", 1)]
        self.assertEqual(pf.direction_change_intervals(rows), [1.0])

    def test_engage_reaction(self):
        sts = [{"t": 0, "en": [{"dx": 9}]}, {"t": 1, "en": [{"dx": 5}]}, {"t": 2, "en": [{"dx": 4}]}]
        self.assertEqual(pf.engage_reactions(sts, [1.4]), [1.4 - 1])


class ContextTests(unittest.TestCase):
    def test_jump_context_priority(self):
        self.assertEqual(pf.jump_context(tk(0, en=[foe("A", 1.0)], mx=1)), "enemy_near")
        self.assertEqual(pf.jump_context(tk(0, s=1 | 4)), "rope")
        self.assertEqual(pf.jump_context(tk(0, ed=[8, 0.5, 0, -1])), "edge")       # 오른쪽 봄, 오른쪽 끝 0.5
        self.assertEqual(pf.jump_context(tk(0, s=1 | 8, ed=[3, 0.5, 0, -1], mx=-1)), "moving")
        self.assertEqual(pf.jump_context(tk(0)), "standing")
        self.assertEqual(pf.jump_context(None), "unknown")

    def test_dead_enemy_not_near(self):
        self.assertEqual(pf.jump_context(tk(0, en=[foe("A", 1.0, state=5)])), "standing")

    def test_idle_reaction_only_when_nothing_held(self):
        ti = pf.TickIndex([tk(0.9), tk(1.9, a=8)])
        avs = [{"t": 1.5, "t0": 1.0}, {"t": 2.5, "t0": 2.0}]
        ins = [inp(1.2, "Player/Jump", "S"), inp(2.1, "Player/Attack", "S")]
        out = pf.idle_reactions(avs, ti, ins)
        self.assertEqual(len(out), 1)
        self.assertAlmostEqual(out[0], 0.2)

    def test_segments(self):
        self.assertEqual(pf.segments([0, 5, 30, 31], 15), [[0, 5], [30, 31]])

    def test_target_choice_nearest_and_drop(self):
        tks = [tk(10.0, en=[foe("R", 1.0, hp=100), foe("R", 4.0, hp=50)], dr=[0.5, 0]),
               tk(11.0, en=[foe("R", 0.8)], p=(0.2, 0.0))]
        evs = [{"t": 10.0, "e": "kill", "a": "R"}, {"t": 10.5, "e": "pick", "a": "x"},
               {"t": 11.0, "e": "dmg", "a": "R"}]
        r = pf.target_choices(evs, pf.TickIndex(tks))
        self.assertEqual(r, (1, 1, 0, 1, 1, 1))  # 가까운 쪽(HP 100)을 골랐고 드롭을 먼저 주웠다

    def test_town_return_reads_cx_before(self):
        evs = [{"t": 1, "e": "map", "a": "Field"}, {"t": 10, "e": "map", "a": "Town"}]
        cxs = [{"t": 5, "bag": 3, "pot": 2}, {"t": 10.2, "bag": 9, "pot": 9}]
        self.assertEqual(pf.town_returns(evs, cxs), ([3], [2]))

    def test_potion_hp(self):
        ti = pf.TickIndex([tk(0.9, hp=[30, 100, 0, 0])])
        self.assertEqual(pf.potion_hp([{"t": 1.0, "e": "use", "a": "Small HP Potion"}], ti), [30.0])

    def test_focus_intervals_same_window_only(self):
        fos = [{"t": 0, "p": "inv/a"}, {"t": 0.3, "p": "inv/b"}, {"t": 0.5, "p": "shop/x"}, {"t": 0.7, "p": ""}]
        self.assertEqual(pf.focus_intervals(fos), [0.3])


if __name__ == "__main__":
    unittest.main()
