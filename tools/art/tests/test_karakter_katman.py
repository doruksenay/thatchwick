"""Karakter katman araçları (ramps.py, manken.py) testleri. Çalıştırma: python -m unittest discover tools/art/tests"""
import json
import os
import sys
import tempfile
import unittest

from PIL import Image, ImageDraw

HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, os.path.dirname(HERE))
import manken  # noqa: E402
import ramps  # noqa: E402


def keyed_mannequin():
    """24x40 basit anahtar renkli manken: kontur, ten baş, kırmızı üst, mavi kenar, yeşil alt, iki iris."""
    im = Image.new("RGBA", (24, 40), (0, 0, 0, 0))
    d = ImageDraw.Draw(im)
    k = {n: ramps.hex_to_rgb(h) + (255,) for n, (h, _c) in ramps.KEYS.items()}
    d.rectangle([8, 2, 15, 11], fill=k["outline"])          # baş konturu
    d.rectangle([9, 3, 14, 10], fill=k["skin"])             # ten
    d.point([(10, 6), (13, 6)], fill=k["iris"])             # gözler
    d.rectangle([6, 12, 17, 24], fill=k["outline"])
    d.rectangle([7, 13, 16, 23], fill=k["main"])
    d.rectangle([7, 23, 16, 23], fill=k["trim"])
    d.rectangle([8, 25, 15, 36], fill=k["bottom"])
    d.rectangle([8, 37, 15, 38], fill=k["shoes"])
    return im


class RampaTestleri(unittest.TestCase):
    def test_taban_tonu_aynen_kalir_ve_tonlar_siralidir(self):
        for hx in ["#D9A877", "#1C1A19", "#E8D28A", "#8C8C8C", "#3F5F8A"]:
            r = ramps.make_ramp(hx)
            self.assertEqual(r[2], ramps.hex_to_rgb(hx))
            v = [ramps.to_hsv(c)[2] for c in r]
            self.assertLessEqual(v[0], v[1])
            self.assertLessEqual(v[1], v[2])
            self.assertLessEqual(v[2], v[3])

    def test_gri_tonu_kaymaz(self):
        r = ramps.make_ramp("#8C8C8C")
        for c in r:
            self.assertTrue(max(c) - min(c) <= 1, c)

    def test_anahtar_renkler_birbirinden_uzak(self):
        sep, _pair = ramps.key_separation()
        self.assertGreaterEqual(sep, ramps.KEY_MIN_SEPARATION)

    def test_json_formulle_uyumlu(self):
        with open(ramps.RAMPS_JSON, encoding="utf-8") as f:
            data = json.load(f)
        for group in ("skin", "hair", "eye", "cloth"):
            for entry in data[group]:
                self.assertEqual([ramps.rgb_to_hex(c) for c in ramps.make_ramp(entry["base"])], entry["ramp"])
        for name, (hx, code) in ramps.KEYS.items():
            self.assertEqual(data["keys"][name]["region"], code)


class MankenTestleri(unittest.TestCase):
    def test_bolge_haritasi_anahtarlari_kodlara_cevirir(self):
        out, unk = manken.to_regions(keyed_mannequin())
        self.assertEqual(unk, 0.0)
        px = out.load()
        self.assertEqual(px[10, 4][0], manken.REGION["skin"])
        self.assertEqual(px[10, 4][1], 2)
        self.assertEqual(px[10, 15][0], manken.REGION["main"])
        self.assertEqual(px[10, 30][0], manken.REGION["bottom"])
        self.assertEqual(px[10, 6][0], manken.REGION["iris"])
        self.assertEqual(px[8, 2][0], manken.REGION["outline"])
        self.assertEqual(px[0, 0][3], 0)

    def test_golge_tonu_dogru_siraya_duser(self):
        im = keyed_mannequin()
        dark = ramps.make_ramp(ramps.KEYS["main"][0])[0]
        ImageDraw.Draw(im).point([(12, 20)], fill=dark + (255,))
        out, _ = manken.to_regions(im)
        self.assertEqual(out.load()[12, 20][:2], (manken.REGION["main"], 0))

    def test_anahtara_oturmayan_piksel_sayilir(self):
        im = keyed_mannequin()
        ImageDraw.Draw(im).rectangle([7, 13, 16, 18], fill=(255, 140, 0, 255))   # anahtar dışı turuncu
        _, unk = manken.to_regions(im)
        self.assertGreater(unk, manken.UNKNOWN_FAIL)

    def test_capa_basin_tepesi_ve_gozler(self):
        out, _ = manken.to_regions(keyed_mannequin())
        with tempfile.TemporaryDirectory() as tmp:
            p = os.path.join(tmp, "lay_body_m_orta_idle_s_00.png")
            out.save(p)
            grid, w, h = manken.read_regions(p)
        a = manken.anchor_of(grid, w, h)
        self.assertEqual(a["hy"], 2)
        self.assertIn(a["hx"], (11, 12))
        self.assertEqual(a["eyes"], [10, 6, 13, 6])
        # Çapalar görselin içinde (B kanalı): oyun ayrı dosya okumaz.
        self.assertEqual(manken.read_markers_im(out), a)
        self.assertEqual(out.load()[a["hx"], a["hy"]][2], 1)
        self.assertEqual(out.load()[10, 6][2], 2)

    def test_betikle_beden_govdeyi_genisletir_ve_daraltir(self):
        out, _ = manken.to_regions(keyed_mannequin())
        with tempfile.TemporaryDirectory() as tmp:
            p = os.path.join(tmp, "lay_body_m_orta_idle_s_00.png")
            out.save(p)
            grid, w, h = manken.read_regions(p)

        def width(g, y):
            xs = [x for x in range(w) if g[y][x][0]]
            return xs[-1] - xs[0] + 1

        y = 18   # gövde ortası
        base = width(grid, y)
        self.assertEqual(width(manken.derive_build(grid, w, h, "dolgun"), y), base + 4)
        self.assertEqual(width(manken.derive_build(grid, w, h, "ince"), y), base - 2)
        g = manken.derive_build(grid, w, h, "dolgun")
        xs = [x for x in range(w) if g[y][x][0]]
        self.assertEqual(g[y][xs[0]][0], manken.REGION["outline"])   # kontur uçta kalır
        self.assertEqual(g[y][xs[-1]][0], manken.REGION["outline"])
        self.assertEqual(width(g, 5), width(grid, 5))                 # baş değişmez (saç çapası bedenle çarpılmaz)

    def test_beden_turetilince_capalar_yeniden_yazilir(self):
        out, _ = manken.to_regions(keyed_mannequin())
        with tempfile.TemporaryDirectory() as tmp:
            p = os.path.join(tmp, "lay_body_m_orta_idle_s_00.png")
            out.save(p)
            grid, w, h = manken.read_regions(p)
            g = manken.derive_build(grid, w, h, "dolgun")
            dst = os.path.join(tmp, "lay_body_m_dolgun_idle_s_00.png")
            manken.write_regions(g, w, h, dst, force=False)
            self.assertEqual(manken.read_markers(dst), manken.read_markers_im(out))   # baş aynı: çapa aynı

    def test_parca_cikarma_capaya_gore_tuvale_oturur(self):
        src = keyed_mannequin()
        base, _ = manken.to_regions(src)
        a = manken.read_markers_im(base)
        edited = src.copy()
        hair = ramps.make_ramp(ramps.REFERENCE["hair"])
        d = ImageDraw.Draw(edited)
        d.rectangle([8, 0, 15, 1], fill=hair[2] + (255,))    # başın üstünde saç (gövde dışı, çene üstü) -> ön
        d.rectangle([9, 3, 14, 3], fill=hair[1] + (255,))    # alnın üstüne binen kâkül (gövde üstü) -> ön
        d.rectangle([18, 20, 19, 26], fill=hair[2] + (255,)) # gövdenin dışında, çenenin altında -> arka
        front, back = manken.extract_part(base, edited, src)
        fx, fy = 8 - a["hx"] + manken.PART_AX, 0 - a["hy"] + manken.PART_AY
        self.assertEqual(front.load()[fx, fy][:3], hair[2])
        self.assertEqual(front.load()[9 - a["hx"] + manken.PART_AX, 3 - a["hy"] + manken.PART_AY][:3], hair[1])
        self.assertEqual(back.load()[18 - a["hx"] + manken.PART_AX, 22 - a["hy"] + manken.PART_AY][:3], hair[2])
        self.assertEqual(front.load()[10 - a["hx"] + manken.PART_AX, 15 - a["hy"] + manken.PART_AY][3], 0)  # gövde parça değil

    def test_hedef_varsa_uzerine_yazilmaz(self):
        with tempfile.TemporaryDirectory() as tmp:
            p = os.path.join(tmp, "x.png")
            Image.new("RGBA", (2, 2)).save(p)
            with self.assertRaises(SystemExit):
                manken.save_new(Image.new("RGBA", (2, 2)), p, force=False)


if __name__ == "__main__":
    unittest.main()
