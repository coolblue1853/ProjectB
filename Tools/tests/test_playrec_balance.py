"""playrec_balance 순수 함수 시험: python -m unittest Tools.tests.test_playrec_balance (저장소 루트에서)."""
import os
import sys
import tempfile
import unittest

sys.path.insert(0, os.path.join(os.path.dirname(os.path.abspath(__file__)), ".."))
import playrec_balance as pb  # noqa: E402

BS = chr(92)
ARROW = "��"  # 기록에서 깨진 화살표


class ParseTests(unittest.TestCase):
    def test_exp_only_mob_kill(self):
        self.assertEqual(pb.parse_exp("EXP +6 (Rabbit_BD(Clone)) � Lv2 206/650"), (6, "Rabbit_BD"))
        self.assertIsNone(pb.parse_exp("EXP +700 (Q101) � Lv2 0/650"))

    def test_drop_with_probability(self):
        self.assertEqual(pb.parse_drop("��� Rabbit Meat (�� 15.00%)"), ("Rabbit Meat", 15.0))
        self.assertIsNone(pb.parse_drop("+6 G"))

    def test_gold_drop_with_profile(self):
        self.assertEqual(pb.parse_gold("x 17G x2 (ForestSpore_BD, Lv5)"), (34, "ForestSpore_BD", 5))
        self.assertIsNone(pb.parse_gold("Rabbit Meat x1"))

    def test_sure_drop_excludes_pickup_log(self):
        self.assertEqual(pb.parse_sure("�� �� Enhance Stone"), "Enhance Stone")
        self.assertIsNone(pb.parse_sure("�� ��: Wood x1"))

    def test_hit_subtracts_extra_damage(self):
        msg = "Rabbit_BD(Clone) � 4 +� 1 HP 20" + ARROW + "16"
        self.assertEqual(pb.parse_hit(msg), ("Rabbit_BD", 20, 15))
        self.assertEqual(pb.parse_hit("Rabbit_BD(Clone) � 4 HP 2" + ARROW + "-2"), ("Rabbit_BD", 2, -2))
        self.assertIsNone(pb.parse_hit("Player hit"))


class ChainTests(unittest.TestCase):
    def test_two_rabbits_interleaved(self):
        hits = [(1.0, "R", 20, 15), (1.5, "R", 20, 14), (2.0, "R", 15, 8), (2.5, "R", 14, 0), (3.0, "R", 8, -1)]
        done = pb.track_hits(hits)
        self.assertEqual(len(done), 2)
        self.assertEqual([round(c["t1"] - c["t0"], 1) for c in done], [1.0, 2.0])
        self.assertTrue(all(c["full"] for c in done))

    def test_partial_start_is_not_full(self):
        hits = [(1.0, "R", 20, 15), (2.0, "R", 20, 12), (3.0, "R", 8, 0)]  # 3번째는 어느 사슬과도 안 이어짐(HP 8)
        done = pb.track_hits(hits)
        self.assertEqual(len(done), 1)
        self.assertFalse(done[0]["full"])

    def test_idle_chain_dropped(self):
        hits = [(1.0, "R", 20, 15), (100.0, "R", 15, 0)]
        done = pb.track_hits(hits)
        self.assertEqual(done[0]["t0"], 100.0)


class SessionTests(unittest.TestCase):
    def rows(self):
        st = [{"t": i * 0.25, "k": "st", "map": "Lv1_X", "lv": 1, "ui": []} for i in range(0, 241)]
        return st + [
            {"t": 0.0, "k": "cx", "night": 0},
            {"t": 30.0, "k": "cx", "night": 1},
            {"t": 10.0, "k": "log", "cat": "Combat", "msg": "Rabbit_BD(Clone) � 4 HP 20" + ARROW + "16"},
            {"t": 11.0, "k": "log", "cat": "Combat", "msg": "Rabbit_BD(Clone) � 16 HP 16" + ARROW + "0"},
            {"t": 11.0, "k": "log", "cat": "Player", "msg": "EXP +6 (Rabbit_BD(Clone)) x"},
            {"t": 11.0, "k": "log", "cat": "Item", "msg": "x Rabbit Meat (� 15.00%)"},
            {"t": 11.0, "k": "log", "cat": "Item", "msg": "x x 7G x1 (Rabbit_BD, Lv1)"},
            {"t": 40.0, "k": "log", "cat": "Player", "msg": "EXP +24 (Rabbit_BD(Clone)) x"},
            {"t": 40.0, "k": "log", "cat": "Item", "msg": "�� �� Enhance Stone"},
            {"t": 12.0, "k": "hitat", "a": "Rabbit_BD", "v": 6, "r": "hit"},
            {"t": 12.1, "k": "hitat", "a": "Rabbit_BD", "v": 6, "r": "iframe"},
        ]

    def test_kill_attribution_and_day_night(self):
        s = pb.build_session(self.rows())
        k = s["kills"]
        self.assertEqual([x["night"] for x in k], [0, 1])
        self.assertEqual(k[0]["gold"], 7)
        self.assertEqual(k[0]["items"], [("Rabbit Meat", 15.0)])
        self.assertEqual(k[1]["sure"], ["Enhance Stone"])
        self.assertAlmostEqual(k[0]["ttk"][0], 1.0)
        self.assertEqual(k[0]["ttk"][1], 2)

    def test_dwell_by_night(self):
        s = pb.build_session(self.rows())
        d = s["dwell"]
        self.assertAlmostEqual(sum(v for (m, n, b), v in d.items() if n == 0), 30.0, places=3)

    def test_hurts_kept_with_result(self):
        s = pb.build_session(self.rows())
        self.assertEqual([h["res"] for h in s["hurts"]], ["hit", "iframe"])


class MiscTests(unittest.TestCase):
    def test_elite_flag_needs_double_mode_exp(self):
        ks = [{"mob": "R", "exp": 6}, {"mob": "R", "exp": 6}, {"mob": "R", "exp": 24}]
        pb.mark_elite(ks)
        self.assertEqual([k["elite"] for k in ks], [False, False, True])

    def test_pct_and_per_min(self):
        self.assertEqual(pb.pct([1, 2, 3, 4, 5], 50), 3)
        self.assertIsNone(pb.pct([], 50))
        self.assertEqual(pb.per_min(30, 2.0), 15.0)
        self.assertIsNone(pb.per_min(3, 0))

    def test_lv_bin_and_flag(self):
        self.assertEqual(pb.lv_bin(1), "Lv1-2")
        self.assertEqual(pb.lv_bin(2), "Lv1-2")
        self.assertEqual(pb.lv_bin(7), "Lv7-8")
        self.assertEqual(pb.flag(2.5), "▲")
        self.assertEqual(pb.flag(0.4), "▼")
        self.assertEqual(pb.flag(1.0), "")

    def test_expected_gold_and_level_spans(self):
        self.assertAlmostEqual(pb.expected_gold(5), 8.8)
        lv = [(10.0, 2), (30.0, 3), (50.0, 4), (90.0, 6)]
        kills = [{"t": 15.0}, {"t": 25.0}, {"t": 35.0}]
        self.assertEqual(pb.level_spans(lv, kills), [(2, 2, 20.0), (3, 1, 20.0)])

    def test_timeline_carry_forward(self):
        tl = pb.Timeline([(0.0, 0), (30.0, 1)])
        self.assertEqual((tl.at(5.0), tl.at(30.0), tl.at(99.0)), (0, 1, 1))

    def test_keep_analysis_preserves_tail(self):
        with tempfile.TemporaryDirectory() as d:
            p = os.path.join(d, "a.md")
            with open(p, "w", encoding="utf-8") as f:
                f.write("자동\n" + pb.MARKER + "\n손글씨\n")
            self.assertEqual(pb.keep_analysis(p), [pb.MARKER, "손글씨"])
            self.assertEqual(pb.keep_analysis(os.path.join(d, "none.md")), [])

    def test_read_csv_strips_stray_cr(self):
        with tempfile.TemporaryDirectory() as d:
            p = os.path.join(d, "i.csv")
            with open(p, "wb") as f:
                f.write(b"int,string\r\nindex,name,cat\r\n1,Rock,Junk\r\n2,A,\rB\r\n")
            rows = pb.read_csv(p, key="name")
            self.assertEqual(rows[0], {"index": "1", "name": "Rock", "cat": "Junk"})
            self.assertEqual(len(rows), 2)


if __name__ == "__main__":
    unittest.main()
