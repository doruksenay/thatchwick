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
  python tools/art/ui_mockup.py hud hud_v2 --yon ilmek --scene <sahne.png> --out <x2.png> [--scale 2]
      HUD parçalarını (Assets/Art/Iso/_aday/ui_hud2_<yon>_*.png) bir oyun sahnesinin üstüne düzene göre yerleştirir.
      Sahne yerel çözünürlüğe (düzenin size'ı) kırpılıp ölçeklenir; çıktı size x scale. Eksik parça mor çerçeveyle çizilir.

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


HUD_DIR = os.path.join("Assets", "Art", "Iso", "_aday")
NEED_FILL = ((60, "bar_fill_green"), (30, "bar_fill_amber"), (-1, "bar_fill_red"))


def _asset(name):
    for d in (HUD_DIR, os.path.join("Assets", "Art", "Iso"), os.path.join("Assets", "Art", "Iso", "_aday", "anim"),
              os.path.join("Assets", "Resources", "UI", "Icons")):
        p = os.path.join(ROOT, d, name + ".png")
        if os.path.exists(p):
            return p
    return None


def nine_slice(im, w, h, c):
    """9-slice: köşeler olduğu gibi, kenar ve orta döşenerek (germe yok; piksel ızgarası bozulmaz).
    c: tek sayı (dört kenar aynı) ya da [sol, sağ, üst, alt]; 0 o yönde kenar yok demektir."""
    W, H = im.size
    if isinstance(c, int):
        c = max(1, min(c, W // 2 - 1, H // 2 - 1))
        l = r = t = b = c
    else:
        l, r, t, b = c
    out = Image.new("RGBA", (w, h), (0, 0, 0, 0))

    def tile(src, box):
        bx, by, bw, bh = box
        if bw <= 0 or bh <= 0 or src.width == 0 or src.height == 0:
            return
        for yy in range(by, by + bh, src.height):
            for xx in range(bx, bx + bw, src.width):
                piece = src.crop((0, 0, min(src.width, bx + bw - xx), min(src.height, by + bh - yy)))
                out.alpha_composite(piece, (xx, yy))

    def put(box_src, box_dst):
        tile(im.crop(box_src), box_dst)

    mw, mh = w - l - r, h - t - b
    put((l, t, W - r, H - b), (l, t, mw, mh))
    put((l, 0, W - r, t), (l, 0, mw, t))
    put((l, H - b, W - r, H), (l, h - b, mw, b))
    put((0, t, l, H - b), (0, t, l, mh))
    put((W - r, t, W, H - b), (w - r, t, r, mh))
    put((0, 0, l, t), (0, 0, l, t))
    put((W - r, 0, W, t), (w - r, 0, r, t))
    put((0, H - b, l, H), (0, h - b, l, b))
    put((W - r, H - b, W, H), (w - r, h - b, r, b))
    return out


def _piece(base, yon, el, missing_pieces):
    # 'src': parçayı başka bir yönün çıktısından al (karma yön; ör. düğmeler ilmekten).
    name = f"ui_hud2_{el.get('src', yon)}_{el['piece']}"
    # Düzeltme turları var olan PNG'nin üzerine yazmaz, _v2, _v3 ekler; en yeni sürüm kullanılır.
    for v in range(9, 1, -1):
        if _asset(f"{name}_v{v}"):
            name = f"{name}_v{v}"
            break
    path = _asset(name)
    x, y, w, h = el["box"]
    if path is None:
        missing_pieces.add(name)
        d = ImageDraw.Draw(base)
        d.rectangle([x, y, x + w - 1, y + h - 1], outline=(255, 0, 255, 255))
        return
    im = Image.open(path).convert("RGBA")
    if "slice" in el:
        im = nine_slice(im, w, h, el["slice"])
    elif im.size != (w, h):
        # Native boyut beklenir; farklıysa ortalanır (ölçeklenmez) ve raporlanır.
        missing_pieces.add(f"{name} (boyut {im.width}x{im.height}, beklenen {w}x{h})")
    base.alpha_composite(im, (x + (w - im.width) // 2, y + (h - im.height) // 2))


def _portrait(base, sprite, box):
    path = _asset(sprite)
    x, y, w, h = box
    if path is None:
        placeholder_portrait(base, (x + 4, y + 4, w - 8, h - 8))
        return
    im = Image.open(path).convert("RGBA")
    bb = im.getbbox() or (0, 0, im.width, im.height)
    # Oyundaki Portraits.HeadCrop gibi: baş, dolu yüksekliğin yaklaşık %34'ü; kare, yatayda ortalı.
    side0 = max(1, round((bb[3] - bb[1]) * 0.34))
    cx = (bb[0] + bb[2]) // 2
    head = im.crop((cx - side0 // 2, bb[1], cx - side0 // 2 + side0, bb[1] + side0))
    side = min(w, h) - 8
    k = max(1, head.width // side + (1 if head.width % side else 0))
    head = head.resize((max(1, head.width // k), max(1, head.height // k)), Image.NEAREST)
    base.alpha_composite(head.crop((0, 0, min(head.width, side), min(head.height, side))),
                         (x + (w - min(head.width, side)) // 2, y + 4))


def cmd_hud(screen, yon, scene, out, scale):
    layout = load_layout(screen)
    W, H = layout["size"]
    elements = list(layout["elements"])
    over = {e["id"]: e for e in layout.get("yon", {}).get(yon, [])}
    elements = [over.get(e["id"], e) for e in elements]
    bg = Image.open(scene).convert("RGBA")
    k = max(1, min(bg.width // W, bg.height // H))   # sahne tam katıysa küçült, değilse kırp
    bg = bg.resize((bg.width // k, bg.height // k), Image.NEAREST)
    left, top = (bg.width - W) // 2, (bg.height - H) // 2
    base = bg.crop((left, top, left + W, top + H))
    draw = ImageDraw.Draw(base)
    missing_pieces = set()
    for el in elements:
        if "piece" in el:
            _piece(base, yon, el, missing_pieces)
        if "icon" in el:
            paste_fit(base, os.path.relpath(_asset(el["icon"]) or el["icon"], ROOT), el["box"])
        if "portrait" in el:
            _portrait(base, el["portrait"], el["box"])
        if "needs" in el:
            bx, by, bw, bh = el["box"]
            rx, ry, rw, rh = el["bar"]
            for i, (icon, _label, val) in enumerate(el["needs"]):
                yy = by + i * el["row"]
                isz = el.get("icon_size", 16)
                fx, fy = el.get("fill_inset", [2, 1])
                by0 = yy + el.get("bar_dy", rh // 2 + 2)
                _piece(base, yon, {"piece": icon, "box": [bx, yy, isz, isz]}, missing_pieces)
                _piece(base, yon, {"piece": "bar", "slice": el.get("bar_slice", 3), "box": [bx + rx, by0, rw, rh]}, missing_pieces)
                fill = next(n for t, n in NEED_FILL if val > t)
                inner = max(0, int((rw - 2 * fx) * val / 100))
                fp = _asset(f"ui_hud2_{yon}_{fill}")
                if fp and inner:
                    f_im = Image.open(fp).convert("RGBA")
                    f_im = f_im.resize((inner, el.get("fill_h", rh - 2)), Image.NEAREST)
                    base.alpha_composite(f_im, (bx + rx + fx, by0 + fy))
                elif not fp:
                    missing_pieces.add(f"ui_hud2_{yon}_{fill}")
        if "text" in el:
            text(draw, base, None, el["text"], el.get("size", 8), align=el.get("align", "left"),
                 box=el.get("text_box", el["box"]) if "piece" in el else el["box"])
    big = base.resize((W * scale, H * scale), Image.NEAREST)
    os.makedirs(os.path.dirname(os.path.abspath(out)), exist_ok=True)
    big.convert("RGB").save(out)
    print(f"{out}  ({W * scale}x{H * scale})")
    for m in sorted(missing_pieces):
        print("  eksik:", m)
    return sorted(missing_pieces)


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
    p3 = sub.add_parser("hud")
    p3.add_argument("screen")
    p3.add_argument("--yon", required=True)
    p3.add_argument("--scene", required=True)
    p3.add_argument("--out", required=True)
    p3.add_argument("--scale", type=int, default=2)
    a = ap.parse_args()
    if a.cmd == "hud":
        cmd_hud(a.screen, a.yon, a.scene, a.out, a.scale)
        return
    if a.cmd == "pieces":
        cmd_pieces(a.screen, a.scale)
    else:
        if not a.placeholder and not a.panel:
            sys.exit("--panel ya da --placeholder gerekli")
        cmd_mockup(a.screen, a.panel, a.out, a.scale, a.placeholder)


if __name__ == "__main__":
    main()
