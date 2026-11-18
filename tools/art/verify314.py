"""314: reduce_colors çıktısını hamla karşılaştırır, _aday'a kopyalar (var olanı ezmez), SE ölçümü yazar."""
import shutil
from pathlib import Path
from PIL import Image

D = ["s", "se", "e", "ne", "n", "nw", "w", "sw"]
A = Path("Assets/Art/Iso/_aday")
for n in ["orhan", "hatice", "zeynep", "murat", "kerem", "ayse"]:
    bad = 0
    cols = set()
    pivy = []
    semi = 0
    se = None
    for d in D:
        r = Image.open(f"art/tmp314/red/{n}/{d}.png").convert("RGBA")
        h = Image.open(A / f"_ham/iso_char_{n}_56_cand_{d}.png").convert("RGBA")
        if r.size != h.size or list(r.getchannel("A").getdata()) != list(h.getchannel("A").getdata()):
            bad += 1
        px = [p for p in r.getdata() if p[3]]
        cols |= set(p[:3] for p in px)
        semi += sum(1 for p in r.getdata() if 0 < p[3] < 255)
        bb = r.getbbox()
        pivy.append(r.size[1] - bb[3])
        dst = A / f"iso_char_{n}_56_cand_{d}.png"
        if not dst.exists():
            shutil.copyfile(f"art/tmp314/red/{n}/{d}.png", dst)
        if d == "se":
            se = (bb, len(set(p[:3] for p in px)))
    bb = se[0]
    print(n, "alfa_uyumsuz", bad, "renk8yon", len(cols), "semi", semi, "SE bbox", bb,
          "h", bb[3] - bb[1], "w", bb[2] - bb[0], "renkSE", se[1], "pivy", pivy)
