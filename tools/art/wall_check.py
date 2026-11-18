"""325 doğrulama: validate_iso, yan yana önizleme, döşeme-dışı kontrolü."""
import glob
import subprocess
import sys

from PIL import Image

fs = sorted(glob.glob("art/tmp325/out/*.png"))
for f in fs:
    r = subprocess.run([sys.executable, "tools/art/validate_iso.py", "--kind", "object",
                        "--max-colors", "48", f], capture_output=True, text=True)
    print(f, r.returncode, r.stdout.strip()[-200:], r.stderr.strip()[-200:])
    im = Image.open(f).convert("RGBA")
    px = im.load()
    cols = {px[x, y][:3] for x in range(im.width) for y in range(im.height) if px[x, y][3]}
    semi = sum(1 for x in range(im.width) for y in range(im.height) if 0 < px[x, y][3] < 255)
    print("  size", im.size, "colors", len(cols), "semi", semi)
    n = "sw_ne" if "sw_ne" in f else "se_nw"
    src = Image.open("Assets/Art/Iso/iso_wall_wall_%s.png" % n).convert("RGBA").load()
    ch = sum(1 for x in range(102) for y in range(115) if src[x, y] != px[x, y])
    print("  değişen piksel", ch)
w = Image.new("RGBA", (102 * len(fs), 115), (255, 0, 255, 255))
for i, f in enumerate(fs):
    w.alpha_composite(Image.open(f).convert("RGBA"), (i * 102, 0))
w.resize((w.width * 2, 230), Image.NEAREST).save("art/tmp325/prev.png")
