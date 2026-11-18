import subprocess
import sys
from PIL import Image

D = "/home/runner/.claude/projects/-home-runner-work-thatchwick-thatchwick/1e7fc06d-a8ca-47b0-bf75-1da774ac7c99/tool-results/"
SRC = {
    ("plank", "sw_ne"): "mcp-pixellab-blob-1794995509892-imj0pd.png",
    ("plank", "se_nw"): "mcp-pixellab-blob-1794995511473-5th9uj.png",
    ("stone", "sw_ne"): "mcp-pixellab-blob-1794995511963-sk2x5v.png",
    ("stone", "se_nw"): "mcp-pixellab-blob-1794995512356-zgzc9r.png",
}
OFF = {"sw_ne": 0, "se_nw": 6}
for (mat, ax), f in SRC.items():
    raw_path = "art/tmp325/raw_%s_%s.png" % (mat, ax)
    subprocess.run([sys.executable, "tools/art/save_png.py", D + f, raw_path], check=False)
    ox = OFF[ax]
    base = Image.open("Assets/Art/Iso/iso_wall_wall_%s.png" % ax).convert("RGBA")
    raw = Image.open(raw_path).convert("RGBA")
    m = Image.open("art/tmp325/mask_%s.png" % ax).convert("L")
    out = base.copy()
    ch = 0
    for y in range(96):
        for x in range(96):
            if m.getpixel((x, y)) > 128:
                out.putpixel((x + ox, y + 4), raw.getpixel((x, y)))
            elif raw.getpixel((x, y)) != base.getpixel((x + ox, y + 4)):
                ch += 1
    out.save("art/tmp325/iso_wall_wall_%s_%s.png" % (mat, ax))
    print(mat, ax, "maske dışı fark", ch)
