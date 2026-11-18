import os, shutil
from PIL import Image

T = "/home/runner/.claude/projects/-home-runner-work-thatchwick-thatchwick/654028c9-07c5-454b-92d7-01ad664f49e2/tool-results/mcp-pixellab-blob-"
D = os.path.dirname(os.path.abspath(__file__))
OUT = os.path.join(D, "out")
os.makedirs(OUT, exist_ok=True)
# reduce_colors sonuçları ham dosyanın yerine geçer
RED = {"braid_single": "1794990301714-8dse7g", "braids_two": "1794990302112-0vunws",
       "topknot": "1794990302603-o0a4rl", "receding": "1794990303062-r6x26e"}
NAMES = ["long_curly", "braid_single", "braids_two", "bun_high", "bun_low", "ponytail", "topknot", "receding", "bald_fringe", "coif_hair"]
for n in NAMES:
    if n in RED:
        im = Image.open(T + RED[n] + ".png").convert("RGBA")
    else:
        im = Image.open(os.path.join(D, f"portrait_layer_hair_{n}_64.png")).convert("RGBA")
    px = im.load()
    removed = 0
    for y in range(64):
        for x in range(64):
            # yüz bölgesi: x 20..43, y >= 22 (alın çizgisinin altı) saydam
            if 20 <= x <= 43 and y >= 22 and px[x, y][3] > 0:
                px[x, y] = (0, 0, 0, 0)
                removed += 1
    cols = len({px[x, y] for x in range(64) for y in range(64) if px[x, y][3] > 0})
    opaque = sum(1 for x in range(64) for y in range(64) if px[x, y][3] > 0)
    print(n, "silinen", removed, "opak", opaque, "renk", cols)
    im.save(os.path.join(OUT, f"portrait_layer_hair_{n}_64.png"))
