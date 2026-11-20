"""Yerleşim dökümünü (Program.cs json=<id>) üstten PNG'ye çizer: zemin rengi, duvar/kapı/pencere, eşya kutusu ve kısaltma.
Kullanım: python tools/layout-report/render.py /tmp/layout_coast.json çıktı.png [hücre=20]"""
import json, sys, hashlib
from PIL import Image, ImageDraw, ImageFont
src, out = sys.argv[1], sys.argv[2]
C = int(sys.argv[3]) if len(sys.argv) > 3 else 20
FLOOR = {"Grass": (110, 150, 80), "Dirt": (150, 120, 85), "Stone": (140, 140, 148), "Wood": (170, 128, 80), "Water": (60, 110, 170), "Sand": (215, 195, 150)}
levels = json.load(open(src))
imgs = []
for lv in levels:
    W, H = lv["w"], lv["h"]
    im = Image.new("RGB", (W * C + 2, H * C + 20), (30, 30, 30))
    d = ImageDraw.Draw(im)
    d.text((4, H * C + 4), f"kat {lv['z']}", fill=(255, 255, 255))
    for t in lv["tiles"]:
        x, y = t["x"] * C, t["y"] * C
        col = (40, 40, 40) if t["b"] else FLOOR.get(t["f"], (120, 120, 120))
        if t["r"] != "None" and not t["b"]: col = (175, 150, 110)
        d.rectangle([x, y, x + C - 1, y + C - 1], fill=col)
    for t in lv["tiles"]:
        x, y = t["x"] * C, t["y"] * C
        for k, (x0, y0, x1, y1) in (("eh", (x, y, x + C, y)), ("ev", (x, y, x, y + C))):
            kind = t[k]
            if kind == "None": continue
            col = {"Wall": (20, 20, 20), "Door": (200, 60, 40), "Window": (120, 200, 255), "Fence": (90, 60, 30), "Gate": (200, 160, 60)}.get(kind, (255, 0, 255))
            d.line([x0, y0, x1, y1], fill=col, width=3)
    for it in lv["items"]:
        h = int(hashlib.md5(it["id"].encode()).hexdigest()[:6], 16)
        col = ((h >> 16) & 255, (h >> 8) & 255, h & 255)
        tiles = it["tiles"] or [[it["x"], it["y"]]]
        for tx, ty in tiles:
            if it["occ"]: d.rectangle([tx * C + 2, ty * C + 2, tx * C + C - 3, ty * C + C - 3], fill=col, outline=(0, 0, 0))
            else: d.ellipse([tx * C + C // 3, ty * C + C // 3, tx * C + 2 * C // 3, ty * C + 2 * C // 3], fill=col)
        short = it["id"].replace("item.", "")[:6]
        d.text((it["x"] * C + 2, it["y"] * C + 4), short, fill=(255, 255, 255))
    imgs.append(im)
W = sum(i.width for i in imgs) + 10 * (len(imgs) - 1)
out_im = Image.new("RGB", (W, max(i.height for i in imgs)), (0, 0, 0))
x = 0
for i in imgs: out_im.paste(i, (x, 0)); x += i.width + 10
out_im.save(out)
print(out, out_im.size)
