import glob, os, shutil, subprocess
from PIL import Image
R = "/home/runner/work/thatchwick/thatchwick"
fs = sorted(glob.glob(R + "/art/results/021g-tmp/out/*.png"))
for f in fs:
    d = R + "/Assets/Art/Iso/_aday/" + os.path.basename(f)
    if os.path.exists(d):
        print("VAR, atlandı", d)
    else:
        shutil.copy(f, d)
subprocess.run(["python3", R + "/tools/art/style.py", "sheet", "--family", "karakter", "--out", R + "/art/results/021g-yanyana.png"] + fs, cwd=R)
# birleşim önizlemesi x4: taban + katman
base = Image.open(R + "/Assets/Art/Iso/_aday/portrait_base_oval_adult_64.png").convert("RGBA")
sheet = Image.new("RGBA", (5 * 192, 2 * 192), (200, 200, 200, 255))
for i, f in enumerate(fs):
    c = base.copy(); c.alpha_composite(Image.open(f).convert("RGBA"))
    sheet.paste(c.resize((192, 192), 0), ((i % 5) * 192, (i // 5) * 192))
    # döşeme yok (portre); validate
    r = subprocess.run(["python3", R + "/tools/art/validate_iso.py", f], capture_output=True, text=True, cwd=R)
    print(os.path.basename(f), r.stdout.strip()[-80:], r.returncode)
sheet.save(R + "/art/results/021g-onizleme-x3.png")
