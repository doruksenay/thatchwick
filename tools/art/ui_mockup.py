#!/usr/bin/env python3
"""Karakter arayüzü önizleyicisi.

PixelLab'in ürettiği boş panel PNG'sinin üstüne gerçek oyun içeriğini (metin, portre,
ihtiyaç çubukları, moodlet kartları) yerleştirir; kullanıcı ekranı oyundaki hâliyle görür.
Yerleşimin tek kaynağı `tools/art/ui_layouts.json` dosyasıdır.

Kullanım:
  python tools/art/ui_mockup.py pieces durum [--scale 2]
      create_ui_asset için 'pieces' JSON'unu yazar (koordinatlar x scale).
  python tools/art/ui_mockup.py mockup durum --panel <panel.png> --out <x2.png> [--scale 2]
      Önizleme üretir. Panel yerel boyutta (ör. 416x192) ya da tam katında olabilir.
  python tools/art/ui_mockup.py mockup durum --placeholder --out <x2.png>
      PixelLab çıktısı olmadan, kutuları düz çizerek yerleşimi dener (yalnızca test).

Pillow gerekir: `uv run --with pillow python tools/art/ui_mockup.py ...`
Eksik portre ya da ikon dosyası hata vermez: yerine yer tutucu çizilir ve raporlanır.
"""
import argparse
import json
import os
import sys

from PIL import Image, ImageDraw, ImageFont

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
LAYOUTS = os.path.join(ROOT, "tools", "art", "ui_layouts.json")
FONT_PATH = os.path.join(ROOT, "tools", "art", "fonts", "Tiny5-Regular.ttf")  # OFL; 8 px ızgara, Türkçe harfler var

INK_DARK = (42, 30, 20)
INK_LIGHT = (244, 239, 227)
GOOD_ON_LIGHT, GOOD_ON_DARK = (63, 107, 46), (167, 193, 122)
BAD_ON_LIGHT, BAD_ON_DARK = (154, 47, 34), (230, 140, 120)
BAR_GOOD, BAR_MID, BAR_LOW = (63, 107, 46), (199, 116, 42), (154, 47, 34)

missing = []


def load_layout(screen):
    with open(LAYOUTS, encoding="utf-8") as f:
        data = json.load(f)
    if screen not in data:
        sys.exit(f"Ekran yok: {screen}. Var olanlar: {[k for k in data if not k.startswith('_')]}")
    return data[screen]


def slots(layout):
    return {p["id"]: p["box"] for p in layout["pieces"]}


def cmd_pieces(screen, scale):
    layout = load_layout(screen)
    out = []
    for p in layout["pieces"]:
        x, y, w, h = p["box"]
        piece = {"id": p["id"], "kind": p["kind"], "x": x * scale, "y": y * scale, "w": w * scale, "h": h * scale, "label": p["label"]}
        if "radius" in p:
            piece["radius"] = p["radius"] * scale
        out.append(piece)
    print(json.dumps(out, ensure_ascii=False, indent=1))


def font(size, bold=False):
    # Piksel yazı yalnızca 8'in katlarında keskin: küçükler 8, büyükler 16.
    px = 8 if size <= 12 else 16
    if os.path.exists(FONT_PATH):
        return ImageFont.truetype(FONT_PATH, px)
    missing.append(FONT_PATH)
    return ImageFont.load_default()


def luminance(img, box):
    x, y, w, h = box
    region = img.crop((max(0, x), max(0, y), min(img.width, x + max(w, 1)), min(img.height, y + max(h, 1)))).convert("RGBA")
    px = [p for p in list(region.getdata()) if p[3] > 0]
    if not px:
        return 1.0
    r = sum(p[0] for p in px) / len(px)
    g = sum(p[1] for p in px) / len(px)
    b = sum(p[2] for p in px) / len(px)
    return (0.2126 * r + 0.7152 * g + 0.0722 * b) / 255.0


def text(draw, img, xy, s, size, bold=False, color=None, align="left", box=None):
    f = font(size, bold)
    l, t, r, b = draw.textbbox((0, 0), s, font=f)
    tw, th = r - l, b - t
    if box is not None:
        bx, by, bw, bh = box
        x = bx + (bw - tw) // 2 - l if align == "center" else bx + 3 - l
        if align == "right":
            x = bx + bw - tw - 3 - l
        y = by + (bh - th) // 2 - t
    else:
        x, y = xy
        if align == "right":
            x -= tw
        x -= l
        y -= t
    light_bg = luminance(img, (int(x + l), int(y + t), tw, th)) > 0.5
    if color == "good":
        fill = GOOD_ON_LIGHT if light_bg else GOOD_ON_DARK
    elif color == "bad":
        fill = BAD_ON_LIGHT if light_bg else BAD_ON_DARK
    elif color == "dim":
        fill = (90, 70, 50) if light_bg else (225, 210, 180)
    else:
        fill = INK_DARK if light_bg else INK_LIGHT
    draw.text((x, y), s, font=f, fill=fill)
    if bold and f.size >= 16:  # 8 px'te sahte kalın harfleri birbirine yapıştırıyor
        draw.text((x + 1, y), s, font=f, fill=fill)


def paste_fit(base, path, box, fallback_letter=None, draw=None):
    x, y, w, h = box
    if path and os.path.exists(os.path.join(ROOT, path)):
        im = Image.open(os.path.join(ROOT, path)).convert("RGBA")
        # Yalnızca tam sayı oranlı küçültme/büyütme: piksel ızgarası bozulmasın.
        if im.width > w:
            k = -(-im.width // w)
            im = im.resize((im.width // k, im.height // k), Image.NEAREST)
        elif im.width * 2 <= w:
            k = w // im.width
            im = im.resize((im.width * k, im.height * k), Image.NEAREST)
        base.alpha_composite(im, (x + (w - im.width) // 2, y + (h - im.height) // 2))
        return True
    if path:
        missing.append(path)
    if fallback_letter and draw is not None:
        text(draw, base, None, fallback_letter, 9, True, align="center", box=box)
    return False


PORTRAIT_MAP = [
    "....kkkkkkkk....", "...khhhhhhhhk...", "..khhhhhhhhhhk..", "..khhhhhhhhhhk..",
    "..khsssssssShk..", "..khsssssssShk..", "..khswessweShk..", "..khrssSssrShk..",
    "..khssssssSShk..", "...kbbbmmbbbk...", "....kbbbbbbk....", ".....kkkkkk.....",
    "......kssk......", "..kcccccccccck..", ".kcccccccccccck.", ".kcccccccccccck.",
]
PORTRAIT_PAL = {"k": (42, 30, 20), "h": (90, 58, 34), "s": (232, 195, 158), "S": (196, 160, 126), "r": (220, 150, 125),
                "w": (244, 239, 227), "e": (47, 93, 140), "m": (122, 59, 46), "c": (160, 110, 70), "b": (80, 50, 30)}


def placeholder_portrait(base, box):
    x, y, w, h = box
    k = max(1, min(w, h) // 16)
    ox, oy = x + (w - 16 * k) // 2, y + (h - 16 * k) // 2
    d = ImageDraw.Draw(base)
    for j, row in enumerate(PORTRAIT_MAP):
        for i, ch in enumerate(row):
            if ch in PORTRAIT_PAL:
                d.rectangle([ox + i * k, oy + j * k, ox + i * k + k - 1, oy + j * k + k - 1], fill=PORTRAIT_PAL[ch])


def draw_placeholder_panel(layout):
    w, h = layout["size"]
    img = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    for p in layout["pieces"]:
        x, y, bw, bh = p["box"]
        r = p.get("radius", 0)
        lab = p["label"]
        if p["id"] == "frame":
            fill, outline = (234, 219, 184, 255), (59, 42, 29, 255)
        elif p["id"] == "header":
            fill, outline = (90, 63, 42, 255), None
        elif "active tab" in lab and "inactive" not in lab:
            fill, outline = (234, 219, 184, 255), (59, 42, 29, 255)
        elif "inactive tab" in lab or "close" in lab:
            fill, outline = (122, 90, 62, 255), (59, 42, 29, 255)
        elif "name plaque" in lab:
            fill, outline = (72, 50, 33, 255), None
        else:
            fill, outline = (244, 239, 227, 255), (184, 159, 116, 255)
        d.rounded_rectangle([x, y, x + bw - 1, y + bh - 1], radius=r, fill=fill, outline=outline)
    return img


def cmd_mockup(screen, panel, out, scale, placeholder):
    layout = load_layout(screen)
    W, H = layout["size"]
    if placeholder:
        base = draw_placeholder_panel(layout)
    else:
        im = Image.open(panel).convert("RGBA")
        if im.size != (W, H):
            if im.width % W == 0 and im.height % H == 0 and im.width // W == im.height // H:
                im = im.resize((W, H), Image.NEAREST)
            else:
                print(f"⚠ panel boyutu {im.size}, beklenen {W}x{H} ya da tam katı; en yakın komşuyla sığdırıldı", file=sys.stderr)
                im = im.resize((W, H), Image.NEAREST)
        base = im
    canvas = Image.new("RGBA", (W, H), (62, 82, 48, 255))
    canvas.alpha_composite(base)
    base = canvas
    d = ImageDraw.Draw(base)
    d.fontmode = "1"  # kenar yumuşatma yok: piksel yazı
    S = slots(layout)
    c = layout.get("content", {})

    pc = c.get("portrait")
    if pc:
        if not paste_fit(base, pc.get("file"), S[pc["slot"]]):
            placeholder_portrait(base, S[pc["slot"]])

    g = c.get("gauge")
    if g:
        x, y, w, h = S[g["slot"]]
        n = len(g["segments"])
        inner_w = w - 4
        seg = (inner_w - (n - 1)) // n
        for i, col in enumerate(g["segments"]):
            sx = x + 2 + i * (seg + 1)
            d.rectangle([sx, y + 2, sx + seg - 1, y + h - 3], fill=col)
            if i == g.get("active"):
                d.rectangle([sx - 1, y + 1, sx + seg, y + h - 2], outline=INK_DARK)
                mx = sx + seg // 2
                d.polygon([(mx - 3, y - 5), (mx + 3, y - 5), (mx, y - 1)], fill=INK_DARK)

    for i, n in enumerate(c.get("needs", [])):
        paste_fit(base, n.get("icon"), S[f"need_icon_{i}"], n["label"][0], d)
        x, y, w, h = S[f"need_bar_{i}"]
        v = n["value"]
        col = BAR_LOW if v < 35 else BAR_MID if v < 55 else BAR_GOOD
        fw = max(1, int((w - 4) * v / 100))
        d.rectangle([x + 2, y + 2, x + 2 + fw - 1, y + h - 3], fill=col)

    plain = c.get("card_style") == "plain"
    for i, card in enumerate(c.get("cards", [])):
        box = S.get(f"card_{i}")
        if not box:
            break
        x, y, w, h = box
        tx = x + 4 if plain else x + 22
        if not plain:
            paste_fit(base, card.get("icon"), (x + 3, y + 3, 16, 16), card["name"][0], d)
        text(d, base, (tx, y + 3), card["name"], 9, True)
        val = card.get("value", 0)
        if val:
            text(d, base, (x + w - 4, y + 3), f"{val:+d}", 9, True, "good" if val > 0 else "bad", align="right")
        text(d, base, (tx, y + 13), card["time"], 8, color="dim")

    for pip in c.get("pips", []):
        x, y, w, h = S[pip["slot"]]
        ry = y + 16 + pip["row"] * 28
        text(d, base, (x + 6, ry), pip["label"], 8)
        pw = (w - 12 - 9) // 10
        for k in range(10):
            px0 = x + 6 + k * (pw + 1)
            col = BAR_GOOD if k < pip["level"] else (167, 193, 122) if k == pip["level"] else (205, 184, 146)
            d.rectangle([px0, ry + 11, px0 + pw - 1, ry + 15], fill=col)

    for t in c.get("texts", []):
        if "slot" in t:
            text(d, base, None, t["text"], t["size"], t.get("bold", False), t.get("color"), t.get("align", "left"), S[t["slot"]])
        else:
            text(d, base, tuple(t["at"]), t["text"], t["size"], t.get("bold", False), t.get("color"))

    out_img = base.resize((W * scale, H * scale), Image.NEAREST)
    os.makedirs(os.path.dirname(os.path.abspath(out)), exist_ok=True)
    out_img.save(out)
    print(f"yazıldı: {out} ({W * scale}x{H * scale})")
    if missing:
        print("eksik (yer tutucu çizildi):")
        for m in sorted(set(missing)):
            print("  -", m)


def main():
    ap = argparse.ArgumentParser()
    sub = ap.add_subparsers(dest="cmd", required=True)
    p1 = sub.add_parser("pieces")
    p1.add_argument("screen")
    p1.add_argument("--scale", type=int, default=2)
    p2 = sub.add_parser("mockup")
    p2.add_argument("screen")
    p2.add_argument("--panel")
    p2.add_argument("--out", required=True)
    p2.add_argument("--scale", type=int, default=2)
    p2.add_argument("--placeholder", action="store_true")
    a = ap.parse_args()
    if a.cmd == "pieces":
        cmd_pieces(a.screen, a.scale)
    else:
        if not a.placeholder and not a.panel:
            sys.exit("--panel ya da --placeholder gerekli")
        cmd_mockup(a.screen, a.panel, a.out, a.scale, a.placeholder)


if __name__ == "__main__":
    main()
