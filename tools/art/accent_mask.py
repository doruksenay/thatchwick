"""İkinci renk maskesi (Project belgesi 45 §10).

Bir eşya görselinden ikinci kaplamanın (korkuluk, minder, kulp, örtü) piksellerini ayırır ve
`<ad>_accent.png` olarak yazar: seçilen pikseller açık griye çevrilir (oyun kaplama rengiyle boyar),
öbürleri saydam olur. Ana görsele dokunulmaz.

Kullanım:
  python tools/art/accent_mask.py <görsel.png> --key "#8A6238" [--key "#7A5530"] [--tol 28] [--box x0,y0,x1,y1]
  --key  : ikinci renge ait renk(ler); her piksel en yakın anahtara RGB uzaklığı --tol altındaysa seçilir
  --box  : yalnızca bu dikdörtgende ara (piksel; sol üst 0,0); birden çok verilebilir
  --out  : çıktı (varsayılan: <görsel>_accent.png); var olan dosyanın üstüne yazmaz
Çıktıda en az 12 piksel yoksa hata verir (anahtar yanlış).
"""
import argparse
import os
import sys

from PIL import Image


def hex_rgb(h):
    h = h.lstrip('#')
    return tuple(int(h[i:i + 2], 16) for i in (0, 2, 4))


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument('image')
    ap.add_argument('--key', action='append', required=True)
    ap.add_argument('--tol', type=float, default=28.0)
    ap.add_argument('--box', action='append', default=[])
    ap.add_argument('--out')
    a = ap.parse_args()

    src = Image.open(a.image).convert('RGBA')
    keys = [hex_rgb(k) for k in a.key]
    boxes = [tuple(int(v) for v in b.split(',')) for b in a.box]
    out_path = a.out or os.path.splitext(a.image)[0] + '_accent.png'
    if os.path.exists(out_path):
        print(f'HATA: {out_path} zaten var (üstüne yazılmaz).')
        return 2

    w, h = src.size
    px = src.load()
    mask = Image.new('RGBA', (w, h), (0, 0, 0, 0))
    mp = mask.load()
    n = 0
    for y in range(h):
        for x in range(w):
            r, g, b, al = px[x, y]
            if al < 128:
                continue
            if boxes and not any(x0 <= x <= x1 and y0 <= y <= y1 for x0, y0, x1, y1 in boxes):
                continue
            d = min(((r - k[0]) ** 2 + (g - k[1]) ** 2 + (b - k[2]) ** 2) ** 0.5 for k in keys)
            if d > a.tol:
                continue
            lum = int(0.299 * r + 0.587 * g + 0.114 * b)
            v = max(150, min(255, 150 + lum * 105 // 255))   # açık gri: gölge farkı kalır, renk kaplamadan gelir
            mp[x, y] = (v, v, v, 255)
            n += 1
    if n < 12:
        print(f'HATA: yalnızca {n} piksel seçildi; anahtar rengi ya da --tol yanlış.')
        return 1
    mask.save(out_path)
    print(f'{out_path}: {n} piksel')
    return 0


if __name__ == '__main__':
    sys.exit(main())
