#!/usr/bin/env python3
"""Harita taslağı çizici (belge 64a, Aşama C): bölgenin terrain.png'sini sabit tohumla, doğal görünen hatlarla boyar.

Oyun gürültü üretmez; dosyada ne varsa onu okur. Bu betik yalnızca ilk taslağı çizer. Kullanıcı dosyayı herhangi bir
çizim programında açıp palette'teki renklerle düzeltebilir (maps/README.md). Betik yeniden çalıştırılırsa elle yapılan
değişikliğin üstüne yazar: önce dosyayı yedekle ya da --only ile yalnızca başka bölgeyi çiz.

Kullanım: python tools/maps/draw.py [--only road] [--preview]
Yardımcılar: eğri yol (Catmull-Rom; genişlik ±1 oynar, düz koşu kısa), gürültülü dere, kenar bandı (iç sınırı girintili),
kaya sırtı.
"""
import argparse
import json
import math
import os
import random

from PIL import Image

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..'))
MAPS = os.path.join(ROOT, 'Assets', 'Resources', 'Content', 'maps')

PALETTE = {
    'grass': (96, 140, 72), 'dirt': (139, 110, 80), 'sand': (222, 200, 150), 'stone': (130, 130, 138),
    'water': (64, 120, 176), 'deep_water': (36, 72, 132), 'path': (186, 140, 90), 'bridge': (150, 96, 56),
    'band': (24, 64, 34), 'rock': (92, 88, 84),
}


# ---------------------------------------------------------------- gürültü
def lattice(x, y, salt):
    h = (x * 374761393 + y * 668265263 + salt * 1442695041) & 0xFFFFFFFF
    h = ((h ^ (h >> 13)) * 1274126177) & 0xFFFFFFFF
    h ^= h >> 16
    return (h & 0xFFFFFF) / 0xFFFFFF


def noise(x, y, salt):
    x0, y0 = math.floor(x), math.floor(y)
    fx, fy = x - x0, y - y0
    fx, fy = fx * fx * (3 - 2 * fx), fy * fy * (3 - 2 * fy)
    a, b = lattice(x0, y0, salt), lattice(x0 + 1, y0, salt)
    c, d = lattice(x0, y0 + 1, salt), lattice(x0 + 1, y0 + 1, salt)
    return (a + (b - a) * fx) * (1 - fy) + (c + (d - c) * fx) * fy


# ---------------------------------------------------------------- tuval
class Canvas:
    def __init__(self, w, h, base='grass'):
        self.w, self.h = w, h
        self.t = [[base] * w for _ in range(h)]

    def get(self, x, y):
        return self.t[y][x] if 0 <= x < self.w and 0 <= y < self.h else None

    def set(self, x, y, kind, over=None):
        if 0 <= x < self.w and 0 <= y < self.h and (over is None or self.t[y][x] in over):
            self.t[y][x] = kind

    def save(self, path):
        im = Image.new('RGB', (self.w, self.h))
        for y in range(self.h):
            for x in range(self.w):
                im.putpixel((x, y), PALETTE[self.t[y][x]])
        os.makedirs(os.path.dirname(path), exist_ok=True)
        im.save(path)


def catmull(points, steps=12):
    """Kontrol noktalarından yumuşak eğri (uçlar dahil)."""
    pts = [points[0]] + list(points) + [points[-1]]
    out = []
    for i in range(1, len(pts) - 2):
        p0, p1, p2, p3 = pts[i - 1], pts[i], pts[i + 1], pts[i + 2]
        for s in range(steps):
            t = s / steps
            t2, t3 = t * t, t * t * t
            x = 0.5 * ((2 * p1[0]) + (-p0[0] + p2[0]) * t + (2 * p0[0] - 5 * p1[0] + 4 * p2[0] - p3[0]) * t2 + (-p0[0] + 3 * p1[0] - 3 * p2[0] + p3[0]) * t3)
            y = 0.5 * ((2 * p1[1]) + (-p0[1] + p2[1]) * t + (2 * p0[1] - 5 * p1[1] + 4 * p2[1] - p3[1]) * t2 + (-p0[1] + 3 * p1[1] - 3 * p2[1] + p3[1]) * t3)
            out.append((x, y))
    out.append(points[-1])
    return out


def stroke(cv, points, width, kind, salt, wobble=0.45, over=None, mark=None):
    """Eğri boyunca fırça: genişlik gürültüyle ±wobble oynar. mark: boyanan karoları toplar."""
    curve = catmull(points)
    dist = 0.0
    for i, (x, y) in enumerate(curve):
        if i:
            dist += math.hypot(x - curve[i - 1][0], y - curve[i - 1][1])
        r = width / 2 + (noise(dist / 3.0, 0, salt) - 0.5) * 2 * wobble
        for yy in range(math.floor(y - r - 1), math.ceil(y + r + 1)):
            for xx in range(math.floor(x - r - 1), math.ceil(x + r + 1)):
                if (xx + 0.5 - x) ** 2 + (yy + 0.5 - y) ** 2 <= r * r:
                    if cv.get(xx, yy) is not None and (over is None or cv.get(xx, yy) in over):
                        cv.set(xx, yy, kind)
                        if mark is not None:
                            mark.add((xx, yy))


def connect4(cv, kinds=('path', 'bridge')):
    """Yalnızca çaprazdan değen yol karolarını dik komşuyla birleştirir (oyunda yürüme 4 yönlü)."""
    changed = True
    while changed:
        changed = False
        for y in range(cv.h - 1):
            for x in range(cv.w):
                for dx in (-1, 1):
                    a, b = cv.get(x, y), cv.get(x + dx, y + 1)
                    if a in kinds and b in kinds and cv.get(x + dx, y) not in kinds and cv.get(x, y + 1) not in kinds:
                        fill = (x, y + 1) if cv.get(x, y + 1) not in ('water',) else (x + dx, y)
                        cv.set(fill[0], fill[1], 'path')
                        changed = True


def band(cv, depth, salt, sides='NSEW', keep=()):
    """Kenar bandı: bölge kenarından içeri, derinliği gürültüyle 'depth' aralığında oynayan sık orman ya da kaya şeridi.
    keep: banda girmeyecek karolar (yol, dere ağzı ve çevresi)."""
    lo, hi = depth
    keep = set(keep)
    for y in range(cv.h):
        for x in range(cv.w):
            if (x, y) in keep:
                continue
            d = []
            if 'N' in sides: d.append((y, x))
            if 'S' in sides: d.append((cv.h - 1 - y, x + 1000))
            if 'W' in sides: d.append((x, y + 2000))
            if 'E' in sides: d.append((cv.w - 1 - x, y + 3000))
            for dd, along in d:
                lim = lo + (hi - lo) * (0.65 * noise(along / 4.0, 0, salt) + 0.35 * noise(along / 1.6, 3, salt))
                if dd < lim:
                    cv.set(x, y, 'band', over=('grass',))
                    break


def near(tiles, r):
    out = set()
    for (x, y) in tiles:
        for dy in range(-r, r + 1):
            for dx in range(-r, r + 1):
                out.add((x + dx, y + dy))
    return out


# ---------------------------------------------------------------- bölgeler
def road():
    """Köy yolu (40x32 yerel). Geçitler (dünya koordinatı Regions.cs PortalPairs): batı (0,24-25) çiftlik, doğu (39,12-13)
    köy meydanı, kuzey (7,0) Güney Yamaçları. Ortasından kıvrılarak akan dere, yolun dereyi geçtiği yerde ahşap köprü."""
    W, H = 40, 32
    cv = Canvas(W, H)
    rnd = random.Random(64)
    roads = set()
    # Dere: kuzeyden güneye kıvrılır, genişliği 2 karo civarında oynar.
    stream_pts = [(20.5 + 2.2 * math.sin(y / 5.0) + (noise(y / 3.0, 9, 11) - 0.5) * 2.0, y) for y in range(-1, H + 2, 4)]
    water = set()
    stroke(cv, stream_pts, 2.3, 'water', salt=12, wobble=0.5, mark=water)
    # Ana yol: çiftlikten köprüye, köprüden köy meydanına (2 karo; kavşakta genişler).
    main = [(-0.5, 25.0), (4, 24.6), (8.5, 23.2), (12.5, 20.6), (16.5, 18.6), (20.5, 17.4), (24.5, 16.0), (29, 15.3), (33.5, 13.8), (37, 13.0), (40.5, 13.0)]
    stroke(cv, main, 2.4, 'path', salt=21, wobble=0.35, over=('grass', 'water'), mark=roads)
    # Köprü: yolun suyla kesiştiği karolar + bir karo öteye
    for (x, y) in list(roads):
        if (x, y) in water:
            cv.set(x, y, 'bridge')
    for (x, y) in list(roads):
        if cv.get(x, y) == 'path' and any(cv.get(x + dx, y) == 'bridge' for dx in (-1, 1)):
            cv.set(x, y, 'bridge')
    # Kuzeye ayrılan patika (Güney Yamaçları geçidi, 1 karo)
    branch = [(9.0, 23.0), (9.6, 18.5), (8.2, 13.5), (8.9, 8.5), (7.6, 4.0), (7.5, -0.5)]
    stroke(cv, branch, 1.25, 'path', salt=33, wobble=0.25, over=('grass',), mark=roads)
    connect4(cv)
    roads |= {(x, y) for y in range(H) for x in range(W) if cv.get(x, y) in ('path', 'bridge')}
    # Geçit karoları kesin yol olsun
    for (x, y) in [(0, 24), (0, 25), (1, 24), (1, 25), (39, 12), (39, 13), (38, 12), (38, 13), (7, 0), (7, 1)]:
        if cv.get(x, y) in ('grass', 'band', 'path'):
            cv.set(x, y, 'path')
            roads.add((x, y))
    # Kenar bandı: kuzey ve güneyde 3-5, doğu ve batıda 1-3 karo; yol, dere ağzı ve çevresi açık.
    keep = near(roads, 2) | near({p for p in water if p[1] < 3 or p[1] > H - 4}, 1) | near(water, 0)
    # Ulu ceviz (GreatTrees, yerel 30,21) ve çevresi açık
    keep |= {(x, y) for x in range(28, 34) for y in range(19, 25)}
    band(cv, (2.5, 5.5), salt=41, sides='NS', keep=keep)
    band(cv, (1.0, 2.8), salt=43, sides='EW', keep=keep)
    # Kaya sırtı: güneydoğu bandının önünde kısa bir sırt (iri kaya ve kütük)
    for (x, y) in [(31, 27), (32, 27), (33, 26), (34, 26), (35, 26), (12, 27), (13, 27)]:
        if (x, y) not in keep and cv.get(x, y) == 'grass':
            cv.set(x, y, 'rock')
    markers = {
        'ponds': [
            {'x': _water_x(cv, 0), 'y': 0, 'fish': [{'id': 'item.fish_trout', 'n': 5}, {'id': 'item.fish_crayfish', 'n': 5}, {'id': 'item.fish_bullhead', 'n': 4}]},
            {'x': _water_x(cv, H - 1), 'y': H - 1, 'fish': [{'id': 'item.fish_trout', 'n': 3}, {'id': 'item.fish_crayfish', 'n': 4}, {'id': 'item.mussel', 'n': 4}]},
        ],
        # Simge yapı: yol kavşağında eski sınır taşı (yazıtlı dikili taş), çevresinde birkaç iri kaya.
        'items': [
            {'id': 'item.boundary_stone', 'x': 12, 'y': 17, 'rot': 0, 'grown': False},
            {'id': 'item.boulder_round', 'x': 13, 'y': 16, 'rot': 0, 'grown': False},
            {'id': 'item.boulder', 'x': 11, 'y': 15, 'rot': 0, 'grown': False},
        ],
        'keep': [{'x0': 28, 'y0': 19, 'x1': 33, 'y1': 24}, {'x0': 10, 'y0': 14, 'x1': 14, 'y1': 18}],
    }
    return cv, markers


def _water_x(cv, y):
    for x in range(cv.w):
        if cv.get(x, y) == 'water':
            return x
    raise ValueError(f'satır {y}: su yok')


# ---------------------------------------------------------------- köy meydanı
def _town_buildings():
    """Regions.cs TownBuildings'ten bina dikdörtgenleri, kapılar ve ekler (yerel). Tek doğruluk kaynağı C# planı."""
    import re
    src = open(os.path.join(ROOT, 'Assets', 'Scripts', 'Core', 'World', 'Regions.cs'), encoding='utf-8').read()
    out = []
    pat = re.compile(r'new TownBuilding\("([^"]+)", "[^"]*", "\w+", (\d+), (\d+), (\d+), (\d+), \'(\w)\', (\d+),[^\n]*?(?:\.WithAnnex\((\d+), (\d+), (\d+), (\d+), (\d+), (\d+), "[^"]+"\))?[,;]?\s*(?://[^\n]*)?\n')
    for m in pat.finditer(src):
        bid = m.group(1)
        x, y, w, h = map(int, m.group(2, 3, 4, 5))
        door, at = m.group(6), int(m.group(7))
        annex = tuple(map(int, m.group(8, 9, 10, 11))) if m.group(8) else None
        out.append({'id': bid, 'x': x, 'y': y, 'w': w, 'h': h, 'door': door, 'at': at, 'annex': annex})
    assert len(out) >= 18, f'bina sayısı {len(out)}'
    return out


def _river_x(y):
    return 50 + round(2.0 * math.sin(y / 8.0))


def _in_plaza(x, y):
    dx, dy = abs(x - 30), abs(y - 26)
    return dx <= 7 and dy <= 5 and dx + dy <= 10


def town():
    """Köy meydanı (64x52 yerel). Bina planı ve geçitler C#'ta (Regions.TownBuildings, PortalPairs); taslak araziyi çizer:
    ırmak (kıyısında kum ve sazlık cepleri), iki köprü, kenarı düzensiz taş meydan, eğri sokaklar, kapılardan sokağa
    patikalar, atölye kapılarında ve sokak kenarlarında aşınmış toprak, ince kenar bandı."""
    W, H = 64, 52
    cv = Canvas(W, H)
    bs = _town_buildings()
    inside = set()
    for b in bs:
        for yy in range(b['y'], b['y'] + b['h']):
            for xx in range(b['x'], b['x'] + b['w']):
                inside.add((xx, yy))
        if b['annex']:
            ax, ay, aw, ah = b['annex']
            for yy in range(ay, ay + ah):
                for xx in range(ax, ax + aw):
                    inside.add((xx, yy))
    pen = {(x, y) for x in range(55, 63) for y in range(4, 13)}
    blocked = inside | pen

    # Irmak: 3 karo, yer yer 4 (dış kıvrımda), kıyıda kum cepleri
    water = set()
    bridges = {26, 27, 44, 45}
    for y in range(H):
        c = _river_x(y)
        cols = [c - 1, c, c + 1]
        if y not in bridges and noise(y / 3.0, 5, 71) > 0.62:
            cols.append(c + 2 if math.cos(y / 8.0) > 0 else c - 2)
        for x in cols:
            if (x, y) not in blocked:
                cv.set(x, y, 'water'); water.add((x, y))
    # Meydan: sekizgen + düzensiz kenar (kapıların önü açık kalır: tabela ve kapı önü yeri)
    door_front = set()
    for b in bs:
        d, at = b['door'], b['at']
        o = {'N': (at, b['y'] - 1), 'S': (at, b['y'] + b['h']), 'W': (b['x'] - 1, at), 'E': (b['x'] + b['w'], at)}[d]
        door_front |= {(o[0] + ddx, o[1] + ddy) for ddx in range(-2, 3) for ddy in range(-2, 3)}
    for y in range(H):
        for x in range(W):
            if (x, y) in blocked or (x, y) in water:
                continue
            dx, dy = abs(x - 30), abs(y - 26)
            # Taş meydan sekizgenden küçük (20.11: "çok boş"): kenarında çim halka, köşelerinde gölge ağaçları.
            if dx <= 5 and dy <= 4 and dx + dy <= 8:
                cv.set(x, y, 'stone')
            else:
                over = max(dx - 5, dy - 4, dx + dy - 8)
                if over <= 1 and (x, y) not in door_front and noise(x / 1.7, y / 1.7, 81) > 0.45:
                    cv.set(x, y, 'stone')
    roads = set()

    def road(pts, width, salt, wob=0.3):
        stroke(cv, pts, width, 'path', salt=salt, wobble=wob, over=('grass', 'dirt', 'water'), mark=roads)

    road([(-0.5, 19.0), (4.5, 19.0), (9.0, 20.2), (13.0, 22.0), (17.0, 23.8), (21.0, 25.2), (24.5, 25.6)], 2.3, 101)
    road([(31.0, 23.0), (31.3, 18.0), (31.6, 13.5), (30.4, 10.0), (29.2, 5.0), (29.0, -0.5)], 2.3, 103)
    road([(31.0, 30.0), (31.5, 34.0), (32.7, 38.5), (33.8, 43.5), (34.0, 51.5)], 2.3, 105)
    road([(36.0, 27.0), (42.0, 27.2), (47.0, 27.0), (53.0, 27.0), (58.0, 26.8), (64.5, 27.0)], 2.2, 107, wob=0.2)
    road([(13.0, 21.5), (13.2, 27.0), (12.7, 34.0), (13.0, 40.0), (12.6, 44.5)], 2.1, 109)
    road([(34.0, 46.6), (40.0, 46.8), (46.0, 46.4), (48.5, 45.5), (52.0, 45.2), (56.5, 45.5)], 1.3, 111, wob=0.2)
    # Köprüler: yolun suyu kestiği yerler
    for (x, y) in list(roads):
        if (x, y) in water:
            cv.set(x, y, 'bridge')
    for (x, y) in list(roads):
        if (x, y) in blocked:
            cv.set(x, y, 'grass'); roads.discard((x, y))
    # Geçit karoları kesin yol
    for (x, y) in [(0, 18), (0, 19), (1, 18), (1, 19), (28, 0), (29, 0), (28, 1), (29, 1), (33, 51), (34, 51), (33, 50), (34, 50), (63, 26), (63, 27), (62, 26), (62, 27)]:
        if cv.get(x, y) not in ('bridge', 'water'):
            cv.set(x, y, 'path'); roads.add((x, y))
    connect4(cv)
    roads = {(x, y) for y in range(H) for x in range(W) if cv.get(x, y) in ('path', 'bridge')}

    # Kapılardan sokağa patika (en kısa yol, binaların ve suyun içinden geçmez)
    from collections import deque
    doors = []
    for b in bs:
        d, at = b['door'], b['at']
        out = {'N': (at, b['y'] - 1), 'S': (at, b['y'] + b['h']), 'W': (b['x'] - 1, at), 'E': (b['x'] + b['w'], at)}[d]
        step = {'N': (0, -1), 'S': (0, 1), 'W': (-1, 0), 'E': (1, 0)}[d]
        doors.append((b['id'], out, step))
    for bid, start, step in doors:
        # Kapıdan iki karo dümdüz çıkış (tabela ve kapı önü yeri), sonra en yakın yola.
        stub = set()
        if start in roads:
            continue
        for k in (0, 1):
            t = (start[0] + step[0] * k, start[1] + step[1] * k)
            if t in blocked or t in water or cv.get(*t) is None:
                break
            if t in roads:
                break
            if cv.get(*t) not in ('stone', 'bridge'):
                cv.set(t[0], t[1], 'path')
            stub.add(t)
            start = t
        if any((start[0] + a, start[1] + c) in roads for a, c in ((1, 0), (-1, 0), (0, 1), (0, -1))):
            roads |= stub
            continue

        prev = {start: None}
        q = deque([start])
        goal = None
        while q:
            cur = q.popleft()
            if cur in roads and cur != start:
                goal = cur; break
            for ddx, ddy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
                n = (cur[0] + ddx, cur[1] + ddy)
                if n in prev or not (0 <= n[0] < W and 0 <= n[1] < H) or n in blocked or n in water:
                    continue
                if abs(n[0] - 30) <= 5 and abs(n[1] - 26) <= 4 and abs(n[0] - 30) + abs(n[1] - 26) <= 8:   # taş meydanın çekirdeği
                    prev[n] = cur; goal = n; q.clear(); break
                prev[n] = cur; q.append(n)
        assert goal is not None, f'{bid}: kapıdan yola ulaşılamıyor'
        cur = goal
        while cur is not None:
            if cv.get(*cur) not in ('stone', 'bridge'):
                cv.set(cur[0], cur[1], 'path')
            roads.add(cur)
            cur = prev[cur]
        roads |= stub
    # Aşınmış toprak: atölye kapıları önünde öbek, sokak kenarında yer yer
    work = {'smithy', 'mason', 'mill'}   # yalnızca ağır iş kapıları (sadeleştirme 20.11)
    for bid, (ox, oy), _ in doors:
        if bid not in work:
            continue
        for yy in range(oy - 3, oy + 4):
            for xx in range(ox - 3, ox + 4):
                r = math.hypot(xx - ox, yy - oy)
                if r <= 1.0 + 0.9 * noise(xx / 1.5, yy / 1.5, 91) and cv.get(xx, yy) == 'grass' and (xx, yy) not in blocked:
                    cv.set(xx, yy, 'dirt')
    # Kıyı: suya değen çimde kum cepleri ve sazlık yeri
    for (x, y) in list(water):
        for ddx, ddy in ((1, 0), (-1, 0)):
            n = (x + ddx, y)
            if cv.get(*n) == 'grass' and n not in blocked and noise(n[0] / 2.0, n[1] / 2.5, 95) > 0.58:
                cv.set(n[0], n[1], 'sand')
    # İnce kenar bandı (yol, ırmak, bina ve ağıl çevresi açık)
    keep = near(roads, 2) | near(water, 1) | near(blocked, 2)
    band(cv, (0.6, 2.4), salt=97, sides='NSEW', keep=keep)

    markers = {
        'ponds': [
            {'x': _river_x(0), 'y': 0, 'fish': [{'id': 'item.fish_trout', 'n': 6}, {'id': 'item.fish_pike', 'n': 2}, {'id': 'item.fish_carp', 'n': 3}, {'id': 'item.fish_perch', 'n': 4}]},
            {'x': _river_x(35), 'y': 35, 'fish': [{'id': 'item.fish_trout', 'n': 4}, {'id': 'item.fish_carp', 'n': 4}, {'id': 'item.fish_eel', 'n': 2}, {'id': 'item.fish_salmon', 'n': 2}, {'id': 'item.fish_perch', 'n': 2}]},
            {'x': _river_x(51), 'y': 51, 'fish': [{'id': 'item.fish_trout', 'n': 2}, {'id': 'item.fish_crayfish', 'n': 4}, {'id': 'item.mussel', 'n': 3}]},
        ],
        'items': [],
        'keep': [],
    }
    return cv, markers


# ---------------------------------------------------------------- yaban bölgeleri ve çiftlik
# Taban: kod çizimiyle kurulan arazi (dotnet run --project tools/map-preview -- out=art/preview/_base export). Taslak bunun
# üstüne: köşeleri yuvarlanmış patikalar (RegionsWild.cs Trail noktalarından), girintili su kıyısı ve kum, kenar bandı.
WILD_FN = {'forest': 'ForestMap', 'lake': 'LakeMap', 'marsh': 'MarshMap', 'hill': 'HillMap', 'slopes': 'SlopesMap', 'coast': 'CoastMap'}
BASE_DIR = os.path.join(ROOT, 'art', 'preview', '_base')


def _base(region):
    path = os.path.join(BASE_DIR, f'base-{region}.json')
    if not os.path.exists(path):
        raise SystemExit(f'{path} yok: önce `dotnet run --project tools/map-preview -- out=art/preview/_base export`')
    return json.load(open(path, encoding='utf-8'))


def _wild_src(fn):
    import re
    src = open(os.path.join(ROOT, 'Assets', 'Scripts', 'Core', 'World', 'RegionsWild.cs'), encoding='utf-8').read()
    consts = {'MineMouthX': 28, 'MineMouthY': 34}
    m = re.search(r'private static void ' + fn + r'\(Painter p\)\s*\{(.*?)\n        \}', src, re.S)
    body = m.group(1)
    if 'pierTop' in body:
        consts['pierTop'] = 19 + round(1.5 * math.sin(20 / 3.2) + 0.8 * math.sin(20 / 7.0)) - 1

    def ev(e):
        return int(eval(e, {}, consts))
    trails = []
    for t in re.finditer(r'Trail\(p, keep, (\d+), ([^;]*)\);', body):
        w, rest = int(t.group(1)), t.group(2)
        if rest.strip() == 'LakeToCoast':
            pts = [(24, 7), (5, 7), (5, 31)]
        else:
            pts = [(ev(a), ev(b)) for a, b in re.findall(r'\(([^(),]+(?:\([^)]*\))?[^(),]*), ([^(),]+)\)', rest)]
        trails.append((w, pts))
    ponds = []
    for a, b in re.findall(r'p\.PondAt\(([^,]+), ([^,]+),', body):
        try:
            ponds.append((ev(a), ev(b)))
        except NameError:
            pass   # döngüdeki göletler (sazlık): taban suyu zaten yerinde
    return trails, ponds


def _meander(pts, salt, amp=0.9, step=4.0):
    """Uzun düz parçaları gürültüyle kıvrar: her ~step karoda dik yönde ±amp sapma (uçlar ve köşeler yerinde)."""
    out = [pts[0]]
    for i in range(len(pts) - 1):
        (ax, ay), (bx, by) = pts[i], pts[i + 1]
        L = math.hypot(bx - ax, by - ay)
        n = int(L // step)
        nx, ny = (-(by - ay) / L, (bx - ax) / L) if L else (0, 0)
        for k in range(1, n):
            t = k / n
            off = (noise((ax + bx) * 0.37 + k * 1.3, (ay + by) * 0.21, salt) - 0.5) * 2 * amp * math.sin(math.pi * t)
            out.append((ax + (bx - ax) * t + nx * off, ay + (by - ay) * t + ny * off))
        out.append((bx, by))
    return out


def _rounded(pts, r=2.6):
    """Köşeleri yuvarlanmış çoklu çizgi (kontrol noktaları yoğun)."""
    out = [pts[0]]
    for i in range(1, len(pts) - 1):
        a, b, c = pts[i - 1], pts[i], pts[i + 1]
        la, lc = math.hypot(b[0] - a[0], b[1] - a[1]), math.hypot(c[0] - b[0], c[1] - b[1])
        rr = min(r, la / 2, lc / 2)
        p0 = (b[0] + (a[0] - b[0]) * rr / max(la, 1e-6), b[1] + (a[1] - b[1]) * rr / max(la, 1e-6))
        p2 = (b[0] + (c[0] - b[0]) * rr / max(lc, 1e-6), b[1] + (c[1] - b[1]) * rr / max(lc, 1e-6))
        out.append(p0)
        for k in range(1, 6):
            t = k / 6
            out.append(((1 - t) ** 2 * p0[0] + 2 * (1 - t) * t * b[0] + t * t * p2[0], (1 - t) ** 2 * p0[1] + 2 * (1 - t) * t * b[1] + t * t * p2[1]))
        out.append(p2)
    out.append(pts[-1])
    return out


def _brush(cv, pts, width, kind, salt, over, mark):
    """Yoğun noktalı çizgi boyunca fırça (catmull yok: yuvarlanmış köşe korunur)."""
    dist = 0.0
    dense = []
    for i in range(len(pts) - 1):
        (ax, ay), (bx, by) = pts[i], pts[i + 1]
        n = max(1, int(math.hypot(bx - ax, by - ay) * 3))
        for k in range(n):
            dense.append((ax + (bx - ax) * k / n, ay + (by - ay) * k / n))
    dense.append(pts[-1])
    for i, (x, y) in enumerate(dense):
        if i:
            dist += math.hypot(x - dense[i - 1][0], y - dense[i - 1][1])
        r = max(0.72, width / 2 + (noise(dist / 3.0, 0, salt) - 0.5) * 0.4)   # 0.72 > √0.5: çizginin geçtiği her karo boyanır (kopuk patika olmaz)
        for yy in range(math.floor(y - r - 1), math.ceil(y + r + 1)):
            for xx in range(math.floor(x - r - 1), math.ceil(x + r + 1)):
                if (xx + 0.5 - x) ** 2 + (yy + 0.5 - y) ** 2 <= r * r and cv.get(xx, yy) in over:
                    cv.set(xx, yy, kind)
                    mark.add((xx, yy))


WILD_OPTS = {
    # band: (derinlik aralığı, kenarlar) listesi; restroke: patikalar yeniden çizilir; shore: kum olasılığı eşiği (düşük = çok kum)
    'forest': {'band': [((2.5, 5.5), 'NEW'), ((1.5, 3.0), 'S')], 'restroke': True, 'shore': 0.62, 'grow': 0.72, 'salt': 211},
    'lake': {'band': [((1.5, 3.5), 'NEW'), ((1.0, 2.2), 'S')], 'beach': True, 'restroke': True, 'shore': 0.42, 'grow': 0.7, 'salt': 223},
    'marsh': {'band': [((1.0, 2.8), 'NSEW')], 'restroke': False, 'shore': 0.7, 'grow': 0.6, 'salt': 227},
    'hill': {'band': [((2.0, 4.5), 'NW'), ((1.0, 2.5), 'S')], 'rock': [((1.5, 3.5), 'E')], 'restroke': True, 'shore': 1.0, 'grow': 1.0, 'salt': 229},
    'slopes': {'band': [((1.0, 2.5), 'EW')], 'restroke': False, 'shore': 1.0, 'grow': 1.0, 'salt': 233},
    'coast': {'band': [((1.5, 3.5), 'NEW')], 'beach': True, 'restroke': True, 'shore': 1.0, 'grow': 1.0, 'salt': 239},
}
BASE_KIND = {'.': 'grass', '=': 'path', '~': 'water', 'b': 'bridge', 'S': 'stone', 'd': 'dirt', 's': 'sand', '#': 'grass'}


def wild(region):
    b = _base(region)
    W, H = b['w'], b['h']
    o = WILD_OPTS[region]
    salt = o['salt']
    cv = Canvas(W, H)
    for y, row in enumerate(b['rows']):
        for x, ch in enumerate(row):
            cv.set(x, y, BASE_KIND[ch])
    if o.get('beach'):
        for y in range(H):
            for x in range(W):
                if cv.get(x, y) == 'dirt':
                    cv.set(x, y, 'sand')   # kumsal: eski kod çiziminde toprak (FloorType.Sand sonradan geldi)
    if region == 'slopes':
        # Teras setleri taş döşeme şeridi değil, alçak taş duvar (RegionsWild.SlopesMap kenar olarak çizer).
        for y in range(H):
            for x in range(W):
                if cv.get(x, y) == 'stone':
                    cv.set(x, y, 'grass')
    if region == 'hill':
        # Taş ocağı düzlüğü döşeme gibi duruyordu (Unity 20.11): ocak ve maden ağzı çevresi taş kalır, gerisi çakıllı toprak,
        # aralarda gürültüyle kaya çıkıntıları (EdgeBand iri kaya koyar).
        for y in range(H):
            for x in range(W):
                if cv.get(x, y) != 'stone':
                    continue
                near_q = 27 <= x <= 35 and 21 <= y <= 29
                near_m = 24 <= x <= 32 and 31 <= y <= 39
                if near_q or near_m:
                    continue
                cv.set(x, y, 'rock' if noise(x / 1.9, y / 1.9, 291) > 0.72 else 'dirt')
    trails, ponds = _wild_src(WILD_FN[region])
    roads = {(x, y) for y in range(H) for x in range(W) if cv.get(x, y) in ('path', 'bridge')}
    edge_paths = {(x, y) for (x, y) in roads if x <= 2 or y <= 2 or x >= W - 3 or y >= H - 3}
    if o['restroke'] and trails:
        # Eski L patikaları sil (köprü ve kenardaki geçit ağızları kalır), köşeleri yuvarlanmış hâliyle yeniden çiz.
        for (x, y) in list(roads):
            if cv.get(x, y) == 'path' and (x, y) not in edge_paths:
                cv.set(x, y, 'grass')
        roads = {p for p in roads if cv.get(*p) in ('path', 'bridge')}
        for i, (w, pts) in enumerate(trails):
            cpts = [(px + w / 2, py + w / 2) for (px, py) in pts]
            _brush(cv, _rounded(_meander(cpts, salt + 70 + i, amp=1.7 if w >= 2 else 1.3), 3.2), w + 0.25, 'path', salt + i, over=('grass', 'dirt', 'sand'), mark=roads)
        connect4(cv)
        roads = {(x, y) for y in range(H) for x in range(W) if cv.get(x, y) in ('path', 'bridge')}
    water = {(x, y) for y in range(H) for x in range(W) if cv.get(x, y) == 'water'}
    fixed = near(roads, 1) | {(x, y) for y in range(H) for x in range(W) if cv.get(x, y) in ('stone', 'bridge')}
    # Su kıyısı: yer yer bir karo içeri taşar (girintili kıyı), kıyı çiminde kum
    for (x, y) in list(water):
        for ddx, ddy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
            n = (x + ddx, y + ddy)
            if cv.get(*n) in ('grass', 'dirt') and n not in fixed and noise(n[0] / 1.8, n[1] / 1.8, salt + 50) > o['grow']:
                cv.set(n[0], n[1], 'water')
    water = {(x, y) for y in range(H) for x in range(W) if cv.get(x, y) == 'water'}
    for (x, y) in water:
        for ddx in (-1, 0, 1):
            for ddy in (-1, 0, 1):
                n = (x + ddx, y + ddy)
                if cv.get(*n) == 'grass' and n not in fixed and noise(n[0] / 2.0, n[1] / 2.0, salt + 60) > o['shore']:
                    cv.set(n[0], n[1], 'sand')
    for (px, py) in ponds:
        if cv.get(px, py) not in ('water',):
            cv.set(px, py, 'water')
    # Kenar bandı ve kaya sırtı (patika, su, taş ve geçit çevresi açık)
    open_ = near(roads, 2) | near(water, 1) | near({p for p in fixed if cv.get(*p) == 'stone'}, 1) | near(edge_paths, 3)
    for k, (depth, sides) in enumerate(o['band']):
        band(cv, depth, salt=salt + 10 + k, sides=sides, keep=open_)
    for k, (depth, sides) in enumerate(o.get('rock', [])):
        before = {(x, y) for y in range(H) for x in range(W) if cv.get(x, y) == 'band'}
        band(cv, depth, salt=salt + 30 + k, sides=sides, keep=open_)
        for y in range(H):
            for x in range(W):
                if cv.get(x, y) == 'band' and (x, y) not in before:
                    cv.set(x, y, 'rock')
    markers = {'ponds': [], 'items': [], 'keep': []}
    lm = LANDMARKS.get(region)
    if lm:
        place_landmark(cv, roads, markers, *lm)
    # Kaya sırtı patikaya değmez (2x1 devrik kütük yola taşmasın).
    for (x, y) in near(roads, 2):
        if cv.get(x, y) == 'rock':
            cv.set(x, y, 'dirt' if region == 'hill' else 'grass')
    return cv, markers


# Simge yapılar (belge 64 §3, iş 753): bölge -> (eşya, ayak izi w, h, tercih edilen yer, kural).
# kural: 'open' çimde patikadan 2-4 karo uzakta görünür yer; 'shore' suya değen kara; 'water_edge' bir ucu suda.
LANDMARKS = {
    'forest': ('item.landmark_lightning_oak', 2, 2, (8, 22), 'open'),
    'lake': ('item.landmark_sunken_boat', 2, 1, (14, 26), 'shore'),
    'marsh': ('item.landmark_broken_boardwalk', 2, 1, (26, 8), 'shore'),
    'hill': ('item.landmark_watchtower_ruin', 2, 2, (8, 8), 'open'),
    'slopes': ('item.landmark_vineyard_house_ruin', 2, 2, (4, 2), 'open'),
    'coast': ('item.landmark_beacon_fire_place', 1, 1, (40, 10), 'open'),
}


def place_landmark(cv, roads, markers, item_id, w, h, pref, rule):
    """Tercih edilen yere en yakın uygun yeri arar: ayak izi ve bir karoluk çevresi çim ya da kum, patikaya 2-5 karo,
    banttan uzak. Bulursa markers'a eşyayı ve 2 karoluk açık alanı (keep) yazar."""
    best = None
    for y in range(1, cv.h - h - 1):
        for x in range(1, cv.w - w - 1):
            cells = [(x + i, y + j) for i in range(w) for j in range(h)]
            ring = [(x + i, y + j) for i in range(-1, w + 1) for j in range(-1, h + 1)]
            if any(cv.get(*c) not in ('grass', 'sand') for c in cells):
                continue
            if any(cv.get(*c) in ('band', 'rock', 'path', 'bridge', 'stone') for c in ring):
                continue
            dr = min((abs(cx - rx) + abs(cy - ry) for (cx, cy) in cells for (rx, ry) in roads), default=99)
            if rule == 'open' and not (2 <= dr <= 6):
                continue
            wet = sum(1 for c in ring if cv.get(*c) == 'water')
            if rule == 'shore' and wet == 0:
                continue
            score = abs(x - pref[0]) + abs(y - pref[1])
            if best is None or score < best[0]:
                best = (score, x, y)
    if best is None:
        print(f'  simge yapı yeri bulunamadı: {item_id}')
        return
    _, x, y = best
    markers['items'].append({'id': item_id, 'x': x, 'y': y, 'rot': 0, 'grown': False})
    markers['keep'].append({'x0': x - 2, 'y0': y - 2, 'x1': x + w + 1, 'y1': y + h + 1})


def farm():
    """Çiftlik (96x72 yerel): oyuncunun arazisi; yalnızca kuzey, batı ve güney kenarda ince bant ve doğu geçidine kıvrılan
    çiftlik yolu. Ev, gölet ve tarla kod çizimiyle kalır (StarterWorld)."""
    b = _base('farm')
    W, H = b['w'], b['h']
    cv = Canvas(W, H)
    for y, row in enumerate(b['rows']):
        for x, ch in enumerate(row):
            cv.set(x, y, BASE_KIND[ch])
    roads = set()
    for (x, y) in [(x, y) for y in range(H) for x in range(W) if cv.get(x, y) == 'path' and x >= 70]:
        cv.set(x, y, 'grass')
    _brush(cv, _rounded([(68.5, 40.5), (73.5, 40.5), (78.0, 40.8), (83.0, 39.6), (88.0, 39.9), (92.5, 41.0), (96.0, 41.0)], 3.0), 2.2, 'path', 251, over=('grass', 'dirt'), mark=roads)
    for (x, y) in [(94, 40), (95, 40), (94, 41), (95, 41)]:
        cv.set(x, y, 'path'); roads.add((x, y))
    connect4(cv)
    # Gölet: dikdörtgen yerine aynı yerde, kaynağı (8,44) içinde kalan girintili gölet; kıyısında kum ve sazlık cebi.
    for (x, y) in [(x, y) for y in range(H) for x in range(W) if cv.get(x, y) == 'water']:
        cv.set(x, y, 'grass')
    cxp, cyp = 13.5, 47.5
    for y in range(40, 56):
        for x in range(4, 24):
            dx, dy = (x + 0.5 - cxp) / 7.2, (y + 0.5 - cyp) / 4.6
            r = dx * dx + dy * dy + (noise(x / 2.2, y / 2.2, 261) - 0.5) * 0.55
            if r <= 1.0 or (x, y) in ((8, 44), (9, 44), (8, 45)):
                cv.set(x, y, 'water')
    water = {(x, y) for y in range(H) for x in range(W) if cv.get(x, y) == 'water'}
    for (x, y) in water:
        for ddx, ddy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
            n = (x + ddx, y + ddy)
            if cv.get(*n) == 'grass' and noise(n[0] / 1.7, n[1] / 1.7, 263) > 0.55:
                cv.set(n[0], n[1], 'sand')
    built = {(x, y) for y in range(H) for x in range(W) if cv.get(x, y) in ('stone', 'bridge', 'dirt')}
    keep = near(roads, 2) | near(water, 2) | near(built, 3)
    band(cv, (1.0, 3.0), salt=257, sides='NWS', keep=keep)
    # Çiftlik doğası: köşelerde iki koru (bant öbeği), batıda kaya çıkıntısı; evden ve yoldan uzak.
    def blob(cx, cy, rx, ry, kind, salt):
        for y in range(int(cy - ry - 2), int(cy + ry + 3)):
            for x in range(int(cx - rx - 2), int(cx + rx + 3)):
                if (x, y) in keep or cv.get(x, y) != 'grass':
                    continue
                dx, dy = (x + 0.5 - cx) / rx, (y + 0.5 - cy) / ry
                if dx * dx + dy * dy + (noise(x / 1.6, y / 1.6, salt) - 0.5) * 0.7 <= 1.0:
                    cv.set(x, y, kind)
    blob(84, 9, 6.5, 4.0, 'band', 271)     # kuzeydoğu korusu
    blob(78, 62, 5.0, 3.5, 'band', 273)    # güneydoğu korusu
    blob(6, 22, 2.2, 3.2, 'rock', 277)     # batıda kaya çıkıntısı
    blob(52, 60, 2.5, 1.6, 'rock', 279)    # güneyde taşlık
    return cv, {'ponds': [], 'items': [], 'keep': []}


def despeckle(cv):
    """Tek karoluk benek (dört komşusu aynı başka tür): komşunun türüne çevrilir. Yol, köprü ve su dokunulmaz
    (geçit, iskele ve balık noktası olabilir)."""
    for y in range(1, cv.h - 1):
        for x in range(1, cv.w - 1):
            k = cv.get(x, y)
            if k in ('path', 'bridge', 'water', 'deep_water'):
                continue
            nb = {cv.get(x - 1, y), cv.get(x + 1, y), cv.get(x, y - 1), cv.get(x, y + 1)}
            if len(nb) == 1:
                n = nb.pop()
                if n != k and n not in ('path', 'bridge'):
                    cv.set(x, y, n)


REGIONS = {'road': road, 'town': town, 'farm': farm, **{k: (lambda k=k: wild(k)) for k in WILD_FN}}


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument('--only')
    ap.add_argument('--preview', action='store_true', help='8 kat büyütülmüş kopya yaz (art/preview/taslak-<bölge>.png)')
    a = ap.parse_args()
    for name, fn in REGIONS.items():
        if a.only and name != a.only:
            continue
        cv, markers = fn()
        despeckle(cv)
        out = os.path.join(MAPS, name)
        cv.save(os.path.join(out, 'terrain.png'))
        with open(os.path.join(out, 'markers.json'), 'w', encoding='utf-8') as fh:
            json.dump(markers, fh, ensure_ascii=False, indent=2)
            fh.write('\n')
        if a.preview:
            im = Image.open(os.path.join(out, 'terrain.png')).resize((cv.w * 8, cv.h * 8), Image.NEAREST)
            os.makedirs(os.path.join(ROOT, 'art', 'preview'), exist_ok=True)
            im.save(os.path.join(ROOT, 'art', 'preview', f'taslak-{name}.png'))
        print(f'{name}: {cv.w}x{cv.h} yazıldı')
    with open(os.path.join(MAPS, 'palette.json'), 'w', encoding='utf-8') as fh:
        json.dump({'entries': [{'terrain': k, 'color': '#%02X%02X%02X' % v} for k, v in PALETTE.items()]}, fh, ensure_ascii=False, indent=2)
        fh.write('\n')


if __name__ == '__main__':
    main()
