import os, shutil
from PIL import Image

B = "/home/runner/.claude/projects/-home-runner-work-thatchwick-thatchwick/654028c9-07c5-454b-92d7-01ad664f49e2/tool-results/mcp-pixellab-blob-"
D = os.path.dirname(os.path.abspath(__file__))
M = [("long_curly", "1794990272586-yym6k6", "long"), ("braid_single", "1794990274443-8b9qb0", "long"),
     ("braids_two", "1794990274939-0du7lw", "long"), ("bun_high", "1794990275470-66jsl2", "short"),
     ("bun_low", "1794990276004-jo790p", "long"), ("ponytail", "1794990276424-2d88ks", "long"),
     ("topknot", "1794990276819-gsj22n", "short"), ("receding", "1794990277249-t6rj9g", "short"),
     ("bald_fringe", "1794990277671-6ih0t3", "short"), ("coif_hair", "1794990278138-3c8msf", "short")]
for n, b, k in M:
    p = B + b + ".png"
    im = Image.open(p).convert("RGBA")
    mk = Image.open(os.path.join(D, f"mask_{k}.png")).convert("L")
    a = im.split()[3]
    semi = sum(1 for v in a.getdata() if 0 < v < 255)
    outside = sum(1 for (x, y) in ((i % 64, i // 64) for i in range(4096)) if mk.getpixel((x, y)) < 128 and a.getpixel((x, y)) > 0)
    opaque = sum(1 for v in a.getdata() if v > 0)
    cols = len({px for px in im.getdata() if px[3] > 0})
    print(n, im.size, "opaque", opaque, "semi", semi, "outside", outside, "colors", cols)
    shutil.copy(p, os.path.join(D, f"portrait_layer_hair_{n}_64.png"))
