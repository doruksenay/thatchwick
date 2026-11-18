"""314 işi: PixelLab karakter rotasyonlarını art/tmp314/raw/<ad>/<yon>.png olarak indirir."""
import subprocess
import sys
from pathlib import Path

B = "https://backblaze.pixellab.ai/file/pixellab-characters/aac48078-9292-4106-bb52-5708e6df86c7"
CH = {
    "orhan": "3c778523-9f6b-46e1-adb8-b6302df43c0d",
    "hatice": "94e0b28e-f765-4862-93b8-4afe18d0780c",
    "zeynep": "3746a869-832d-4b9c-a612-3c6f820ce2df",
    "murat": "5c02872c-55fa-4c89-8c3f-89d6518f4638",
    "kerem": "c47130f1-4a74-452c-9500-76bd69f1d551",
    "ayse": "2ecddf1c-485e-43c3-8618-dcf68f981b37",
}
DIRS = {"south": "s", "south-east": "se", "east": "e", "north-east": "ne",
        "north": "n", "north-west": "nw", "west": "w", "south-west": "sw"}
root = Path("art/tmp314/raw")
for n, cid in CH.items():
    (root / n).mkdir(parents=True, exist_ok=True)
    for long, short in DIRS.items():
        out = root / n / f"{short}.png"
        r = subprocess.run([sys.executable, "tools/art/save_png.py",
                            f"{B}/{cid}/rotations/{long}.png", str(out)],
                           capture_output=True, text=True)
        if r.returncode != 0:
            print("HATA", n, short, r.stderr[-200:])
print("bitti")
