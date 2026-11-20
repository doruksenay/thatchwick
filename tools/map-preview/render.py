#!/usr/bin/env python3
"""Harita önizlemesi (belge 64 §3, Katman 0): tools/map-preview dökümünü gerçek görsellerle izometrik PNG'ye çizer.

Kullanım:
  python tools/map-preview/render.py <döküm klasörü> <çıktı klasörü> [--regions road,town] [--mode cutaway|up|both]
                                     [--sites] [--debug] [--half]

- cutaway: zemin kat, ön duvarlar alçak, çatı yok (oyunun Kesit kipi). İç döşemeye bakmak için.
- up: bütün duvarlar tam boy ve çatılar (dış görünüş). Dükkân dışarıdan ayırt ediliyor mu diye bakmak için.
- --sites: her binanın her katı ayrı görüntü (bina-<id>-kat<z>.png).
- Görseli olmayan eşya parlak mor kutu ve adıyla çizilir; görsel LFS işaretçisiyse betik durur (kutuyla "önizleme" üretmez).

Izgara +x ekranda sağ-yukarı, +y sol-yukarı gider (Game/Iso.cs); 1 karo 64x32 piksel, bir kat 96 piksel.
"""
import argparse
import json
import os
import sys

from PIL import Image, ImageDraw, ImageFont

TILE_W, TILE_H, LEVEL_PX = 64, 32, 96
BG = (22, 31, 24, 255)   # WorldView.OutsideColor
MAGENTA = (255, 0, 255, 255)


class Art:
    def __init__(self, root, index):
        self.root, self.index, self.cache = root, index, {}

    def get(self, name, flip=False, scale=1.0, tint=None):
        key = (name, flip, round(scale, 3), tuple(round(t, 3) for t in tint) if tint else None)
        if key in self.cache:
            return self.cache[key]
        path = self.index.get(name)
        if path is None:
            self.cache[key] = None
            return None
        full = os.path.join(self.root, path)
        with open(full, 'rb') as fh:
            if fh.read(7) == b'version':
                sys.exit(f"Görsel bir LFS işaretçisi, gerçek dosya değil: {path}\n"
                         f"Önce: git lfs pull --include \"Assets/Art/Iso/**\"")
        im = Image.open(full).convert('RGBA')
        if flip:
            im = im.transpose(Image.FLIP_LEFT_RIGHT)
        if abs(scale - 1.0) > 1e-3:
            im = im.resize((max(1, round(im.width * scale)), max(1, round(im.height * scale))), Image.NEAREST)
        if tint:
            r, g, b, a = im.split()
            r = r.point(lambda v, k=tint[0]: min(255, int(v * k)))
            g = g.point(lambda v, k=tint[1]: min(255, int(v * k)))
            b = b.point(lambda v, k=tint[2]: min(255, int(v * k)))
            im = Image.merge('RGBA', (r, g, b, a))
        self.cache[key] = im
        return im


def screen(gx, gy, lift=0.0):
    return (gx - gy) * (TILE_W / 2), -((gx + gy) * (TILE_H / 2) + lift * 32)


def diamond(draw, cx, cy, color, scale=1.0):
    hw, hh = TILE_W / 2 * scale, TILE_H / 2 * scale
    draw.polygon([(cx - hw, cy), (cx, cy - hh), (cx + hw, cy), (cx, cy + hh)], fill=tuple(color))


def bounds(x0, y0, w, h, top=160, bottom=24):
    xs, ys = [], []
    for gx, gy in ((x0, y0), (x0 + w, y0), (x0, y0 + h), (x0 + w, y0 + h)):
        sx, sy = screen(gx, gy)
        xs.append(sx)
        ys.append(sy)
    return min(xs) - 16, min(ys) - top, max(xs) + 16, max(ys) + bottom


def render(doc, art, mode, rect=None, level=0, debug=False, font=None, flat=False):
    """rect: (x, y, w, h) dünya karosu; None ise bütün bölge. level: gösterilen kat (cutaway'de yalnızca o kat)."""
    x0, y0, w, h = rect if rect else (doc['x'], doc['y'], doc['w'], doc['h'])
    left, top, right, bottom = bounds(x0, y0, w, h, top=260 if mode == 'up' else 150)
    W, H = int(right - left), int(bottom - top)
    img = Image.new('RGBA', (W, H), BG)
    dr = ImageDraw.Draw(img)
    missing = []

    def visible(d):
        if rect is not None:
            m = 2 if d['k'] in ('ground', 'path') else 1
            if not (x0 - m <= d['gx'] <= x0 + w + m and y0 - m <= d['gy'] <= y0 + h + m):
                return False
        z = d.get('z', 0)
        if mode == 'up':
            return z >= 0
        if d['k'] == 'roof':
            return False
        return z == level

    items = [d for d in doc['draws'] if visible(d)]
    items.sort(key=lambda d: d['order'] - (0 if mode == 'up' else d.get('z', 0) * 14000))
    for d in items:
        z = d.get('z', 0)
        lift = d.get('lift', 0.0) + (z * 3.0 if mode == 'up' else 0.0)
        sx, sy = screen(d['gx'], d['gy'], lift)
        cx, cy = sx - left, sy - top
        name = d.get('s')
        if d['k'] == 'wall' and mode == 'cutaway' and d.get('front') and d.get('low'):
            name = d['low']
        im = art.get(name, d.get('flip', False), d.get('scale', 1.0), d.get('tint')) if name else None
        if im is not None:
            px = d.get('px', -1)
            if px < 0:
                ox, oy = im.width / 2, im.height / 2
            else:
                s = d.get('scale', 1.0)
                pxs = (art.get(name).width - px) if d.get('flip') else px
                ox, oy = pxs * s, im.height - d.get('py', 0) * s
            img.alpha_composite(im, (int(round(cx - ox)), int(round(cy - oy)))) if 0 <= cx - ox < W and 0 <= cy - oy < H and cx - ox + im.width <= W and cy - oy + im.height <= H \
                else paste_clipped(img, im, int(round(cx - ox)), int(round(cy - oy)))
            continue
        color = d.get('color') or MAGENTA
        if d['k'] in ('ground', 'roof'):
            c = color
            if d.get('tint') and d['k'] == 'roof':
                c = color
            diamond(dr, cx, cy, c)
        elif d['k'] == 'rug':
            if d.get('missing'):
                diamond(dr, cx, cy, MAGENTA, 0.9)
                missing.append(d.get('label', '?'))
        elif d['k'] == 'wall':
            # yer tutucu duvar: kenar boyunca paralelkenar
            hgt = 12 if (mode == 'cutaway' and d.get('front')) else 56
            if d.get('px', 32) == 32:   # yatay kenar: +x yönünde (sağ-yukarı)
                pts = [(cx, cy), (cx + 32, cy - 16), (cx + 32, cy - 16 - hgt), (cx, cy - hgt)]
            else:                       # dikey kenar: +y yönünde (sol-yukarı)
                pts = [(cx, cy), (cx - 32, cy - 16), (cx - 32, cy - 16 - hgt), (cx, cy - hgt)]
            dr.polygon(pts, fill=tuple(color), outline=(60, 40, 30, 255))
            if d.get('missing'):
                missing.append(d.get('label', '?'))
        else:
            # görseli olmayan eşya: mor kutu ve adı
            s = d.get('scale', 1.0)
            bw = 22 * max(1, min(d.get('w', 1), d.get('h', 1))) * (s if s < 1 else 1)
            bh = 30 * (s if s < 1 else 1)
            dr.polygon([(cx - bw, cy), (cx, cy - bw / 2), (cx + bw, cy), (cx, cy + bw / 2)], fill=MAGENTA)
            dr.rectangle([cx - bw * 0.7, cy - bh, cx + bw * 0.7, cy], fill=MAGENTA, outline=(90, 0, 90, 255))
            label = d.get('label', '?')
            if font is not None and s >= 1:
                dr.text((cx - bw * 0.7 + 2, cy - bh + 2), label[:14], fill=(255, 255, 255, 255), font=font)
            missing.append(label)
    if debug:
        ov = Image.new('RGBA', (W, H), (0, 0, 0, 0))
        od = ImageDraw.Draw(ov)
        for gy in range(y0, y0 + h + 1):
            a = screen(x0, gy)
            b = screen(x0 + w, gy)
            od.line([(a[0] - left, a[1] - top), (b[0] - left, b[1] - top)], fill=(255, 255, 255, 40))
        for gx in range(x0, x0 + w + 1):
            a = screen(gx, y0)
            b = screen(gx, y0 + h)
            od.line([(a[0] - left, a[1] - top), (b[0] - left, b[1] - top)], fill=(255, 255, 255, 40))
        for gy in range(y0, y0 + h, 4):
            for gx in range(x0, x0 + w, 4):
                sx, sy = screen(gx + 0.5, gy + 0.5)
                if font is not None:
                    od.text((sx - left - 10, sy - top - 4), f"{gx - doc['x']},{gy - doc['y']}", fill=(255, 255, 0, 200), font=font)
        img = Image.alpha_composite(img, ov)
    return img, missing


def paste_clipped(img, im, x, y):
    W, H = img.size
    l, t = max(0, -x), max(0, -y)
    r, b = min(im.width, W - x), min(im.height, H - y)
    if r <= l or b <= t:
        return
    img.alpha_composite(im.crop((l, t, r, b)), (x + l, y + t))


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument('dump')
    ap.add_argument('out')
    ap.add_argument('--regions')
    ap.add_argument('--mode', default='both', choices=['cutaway', 'up', 'both'])
    ap.add_argument('--sites', action='store_true')
    ap.add_argument('--debug', action='store_true')
    ap.add_argument('--half', action='store_true', help='yarı boyutlu kopya da yaz (gözden geçirme için)')
    a = ap.parse_args()
    root = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..'))
    with open(os.path.join(a.dump, 'art-index.json'), encoding='utf-8') as fh:
        art = Art(root, json.load(fh))
    try:
        font = ImageFont.load_default()
    except Exception:
        font = None
    only = a.regions.split(',') if a.regions else None
    report = []
    for fn in sorted(os.listdir(a.dump)):
        if not fn.endswith('.json') or fn == 'art-index.json':
            continue
        with open(os.path.join(a.dump, fn), encoding='utf-8') as fh:
            doc = json.load(fh)
        if only and doc['id'] not in only:
            continue
        out = os.path.join(a.out, doc['id'])
        os.makedirs(out, exist_ok=True)
        modes = ['cutaway', 'up'] if a.mode == 'both' else [a.mode]
        miss_all = []
        for mode in modes:
            img, missing = render(doc, art, mode, debug=a.debug, font=font)
            name = 'harita.png' if mode == 'cutaway' else 'harita-dis.png'
            if a.debug:
                name = name.replace('.png', '-debug.png')
            img.convert('RGB').save(os.path.join(out, name))
            if a.half:
                img.resize((img.width // 2, img.height // 2), Image.LANCZOS).convert('RGB').save(os.path.join(out, name.replace('.png', '-yari.png')))
            if mode == 'cutaway':
                miss_all = missing
        if a.sites:
            for s in doc['sites']:
                for z in range(s['minLevel'], s['maxLevel'] + 1):
                    rect = (s['x'] - 2, s['y'] - 2, s['w'] + 4, s['h'] + 4)
                    img, _ = render(doc, art, 'cutaway', rect=rect, level=z, font=font)
                    sid = s['id'].replace(':', '-')
                    img = img.resize((img.width * 2, img.height * 2), Image.NEAREST)
                    img.convert('RGB').save(os.path.join(out, f"bina-{sid}-kat{z}.png"))
        uniq = sorted(set(miss_all))
        report.append(f"{doc['id']}: görseli olmayan {len(miss_all)} çizim, {len(uniq)} tür: {', '.join(uniq)}")
    with open(os.path.join(a.out, 'eksik-gorseller.txt'), 'w', encoding='utf-8') as fh:
        fh.write('\n'.join(report) + '\n')
    print('\n'.join(r[:200] for r in report))


if __name__ == '__main__':
    main()
