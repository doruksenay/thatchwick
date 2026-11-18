import subprocess, sys, shutil, os
from PIL import Image
ok = True
for n in ("plank_sw_ne", "plank_se_nw", "stone_sw_ne", "stone_se_nw"):
    p = "art/tmp325/iso_wall_wall_%s.png" % n
    r = subprocess.run([sys.executable, "tools/art/validate_iso.py", "--kind", "object", "--max-colors", "48", p], capture_output=True, text=True)
    print(n, r.returncode, r.stdout.strip()[-200:], r.stderr.strip()[-200:])
    im = Image.open(p).convert("RGBA")
    cols = {q for q in im.getdata() if q[3]}
    semi = sum(1 for q in im.getdata() if 0 < q[3] < 255)
    print("  ", im.size, "renk", len(cols), "yarı saydam", semi)
    if r.returncode == 0:
        dst = "Assets/Art/Iso/iso_wall_wall_%s.png" % n
        if not os.path.exists(dst):
            shutil.copyfile(p, dst)
            print("  teslim", dst)
