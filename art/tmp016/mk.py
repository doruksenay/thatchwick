from PIL import Image, ImageFilter
import colorsys, sys
SRC = 'Assets/Art/Iso/_aday/iso_char_farmer_a2_56_cand_se.png'
im = Image.open(SRC).convert('RGBA')
W, H = im.size
p = im.load()


def cls(x, y):
    r, g, b, a = p[x, y]
    if a == 0:
        return None
    h, l, s = colorsys.rgb_to_hls(r / 255, g / 255, b / 255)
    return h * 360, l, s


def mk(f):
    m = Image.new('L', (W, H), 0)
    q = m.load()
    for y in range(H):
        for x in range(W):
            if cls(x, y) and f(x, y):
                q[x, y] = 255
    return m


def dil(m, n=1):
    return m.filter(ImageFilter.MaxFilter(2 * n + 1)).point(lambda v: 255 if v else 0)


def hair(x, y):
    return 1 <= y <= 7 and 24 <= x <= 39


def head(x, y):
    return hair(x, y) or (8 <= y <= 16 and 23 <= x <= 39 and cls(x, y)[1] < 0.2)


def beard(x, y):
    return 10 <= y <= 16 and 26 <= x <= 37


def front(x, y):
    h, l, s = cls(x, y)
    return 17 <= y <= 44 and 22 <= x <= 42 and 20 <= h <= 40 and s > 0.3


def headfront(x, y):
    return head(x, y) or beard(x, y) or front(x, y)


SETS = {'hair': head, 'beard': beard, 'front': front, 'bald': headfront}
if __name__ == '__main__':
    for k, f in SETS.items():
        m = dil(mk(f))
        m.save('art/tmp016/mask_' + k + '.png')
        print(k, sum(1 for v in m.getdata() if v))
        bg2 = Image.new('RGBA', (W, H), (200, 200, 200, 255))
        bg2.alpha_composite(im)
        ov = Image.new('RGBA', (W, H), (255, 0, 0, 120))
        bg2.paste(ov, (0, 0), m)
        bg2.resize((360, 360), Image.NEAREST).save('art/tmp016/ov_' + k + '.png')
