"""314 işi: reduce_colors çıktılarını art/tmp314/red/<ad>/<yon>.png olarak indirir."""
import subprocess
import sys
from pathlib import Path

J = {
    "hatice": "6e983289-be9d-4550-ad05-206feb5557e7",
    "zeynep": "3d093160-f5d1-474c-93f1-3340bdc87733",
    "murat": "24fdda3b-2c18-4f86-abfc-54c5c5cded87",
    "kerem": "2377726f-c9f4-4f54-9a86-1dcdeb8c256d",
    "ayse": "c9672b92-4461-4f47-a94c-813ee4fb2a95",
    "orhan": "b92b8002-b58b-4008-b074-93b73ea1daf9",
}
D = ["s", "se", "e", "ne", "n", "nw", "w", "sw"]
for n, jid in J.items():
    (Path("art/tmp314/red") / n).mkdir(parents=True, exist_ok=True)
    for i, d in enumerate(D):
        r = subprocess.run([sys.executable, "tools/art/save_png.py",
                            f"https://api.pixellab.ai/mcp/images/{jid}/download?index={i}",
                            f"art/tmp314/red/{n}/{d}.png"], capture_output=True, text=True)
        if r.returncode:
            print("HATA", n, d, (r.stderr or r.stdout)[-200:])
print("bitti")
