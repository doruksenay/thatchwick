"""Palet kilidi (style.py) testleri: donmuş paletler, palet seçimi, kabul listesi okuma.
Çalıştırma: python -m unittest discover tools/art/tests"""
import json
import os
import sys
import tempfile
import unittest
from types import SimpleNamespace

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, os.path.dirname(HERE))
import style  # noqa: E402


class DonmusPaletler(unittest.TestCase):
    def test_iki_palet_donmus_ve_boyutu_dogru(self):
        for name, conf in style.PALETTE_SETS.items():
            with open(conf["json"], encoding="utf-8") as f:
                d = json.load(f)
            self.assertTrue(d.get("donduruldu"), name)
            self.assertTrue(d.get("karar"), f"{name}: karar notu boş")
            self.assertEqual(d.get("takim"), name)
            self.assertLessEqual(len(d["renkler"]), conf["size"])
            self.assertGreaterEqual(len(d["renkler"]), conf["size"] * 0.9, name)
            self.assertEqual(len(set(d["renkler"])), len(d["renkler"]), f"{name}: tekrar eden renk")

    def test_dunya_paleti_temel_renk_ailelerini_tasir(self):
        """Eski 64 renklik palette mavi yoktu, yalnızca 3 yeşil vardı; düz median-cut da maviyi eritiyordu (64c §1)."""
        import colorsys
        hues = []
        for r, g, b in style.load_palette("dunya"):
            h, s, v = colorsys.rgb_to_hsv(r / 255, g / 255, b / 255)
            if s > 0.2 and v > 0.25:
                hues.append(h * 360)
        self.assertGreaterEqual(sum(1 for h in hues if 70 <= h <= 160), 6, "yeşil")
        self.assertGreaterEqual(sum(1 for h in hues if 165 <= h <= 255), 2, "su mavisi")

    def test_donmus_palet_yeniden_uretilmez(self):
        args = SimpleNamespace(set="dunya", yeniden="", png=False)
        with self.assertRaises(SystemExit) as cm:
            style.build_palette(args)
        self.assertIn("donduruldu", str(cm.exception))


class PaletSecimi(unittest.TestCase):
    def test_ikon_ve_arayuz_arayuz_paletine(self):
        self.assertEqual(style.palette_set_for("Assets/Art/Iso/icon_item_pear.png"), "arayuz")
        self.assertEqual(style.palette_set_for("x/ui_icon_need_energy.png", "esya"), "arayuz")
        self.assertEqual(style.palette_set_for("x/iso_item_table.png", "arayuz"), "arayuz")

    def test_dunya_gorseli_dunya_paletine(self):
        self.assertEqual(style.palette_set_for("x/iso_item_table_wood_2x1_se_nw.png", "esya"), "dunya")
        self.assertEqual(style.palette_set_for("x/iso_ground_grass_a.png"), "dunya")


class KabulListesi(unittest.TestCase):
    def test_suslu_parantez_acilir(self):
        self.assertEqual(style._expand("a_{x,y}_{1,2}.png"), ["a_x_1.png", "a_x_2.png", "a_y_1.png", "a_y_2.png"])
        self.assertEqual(style._expand("a.png"), ["a.png"])

    def test_yalnizca_accepted_isler_okunur(self):
        with tempfile.TemporaryDirectory() as tmp:
            q = os.path.join(tmp, "art", "queue")
            os.makedirs(q)
            open(os.path.join(tmp, "a.png"), "wb").close()
            open(os.path.join(tmp, "b.png"), "wb").close()
            for jid, st, out in (("1", "accepted", "a.png"), ("2", "review", "b.png")):
                with open(os.path.join(q, f"{jid}.md"), "w", encoding="utf-8") as f:
                    f.write(f'---\nid: "{jid}"\nstatus: {st}\nfamily: esya\noutputs:\n  - {out}\n---\n# {jid}\n')
            old_root, old_q = style.ROOT, style.QUEUE
            try:
                style.ROOT, style.QUEUE = tmp, q
                got = style.accepted_job_files()
            finally:
                style.ROOT, style.QUEUE = old_root, old_q
            self.assertEqual(list(got), ["esya"])
            self.assertEqual([os.path.basename(p) for p in got["esya"]], ["a.png"])


if __name__ == "__main__":
    unittest.main()
