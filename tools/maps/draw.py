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
            if _in_plaza(x, y):
                cv.set(x, y, 'stone')
            else:
                dx, dy = abs(x - 30), abs(y - 26)
                over = max(dx - 7, dy - 5, dx + dy - 10)
                if over <= 2 and (x, y) not in door_front and noise(x / 1.7, y / 1.7, 81) > 0.35 + 0.25 * over:
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
                if _in_plaza(*n):
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
    work = {'smithy', 'carpenter', 'mason', 'potter', 'saddler', 'mill', 'animals_hut'}
    for bid, (ox, oy), _ in doors:
        if bid not in work:
            continue
        for yy in range(oy - 3, oy + 4):
            for xx in range(ox - 3, ox + 4):
                r = math.hypot(xx - ox, yy - oy)
                if r <= 1.4 + 1.6 * noise(xx / 1.5, yy / 1.5, 91) and cv.get(xx, yy) == 'grass' and (xx, yy) not in blocked:
                    cv.set(xx, yy, 'dirt')
    for (x, y) in list(roads):
        for ddx, ddy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
            n = (x + ddx, y + ddy)
            if cv.get(*n) == 'grass' and n not in blocked and noise(n[0] / 2.2, n[1] / 2.2, 93) > 0.66:
                cv.set(n[0], n[1], 'dirt')
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


REGIONS = {'road': road, 'town': town}


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument('--only')
    ap.add_argument('--preview', action='store_true', help='8 kat büyütülmüş kopya yaz (art/preview/taslak-<bölge>.png)')
    a = ap.parse_args()
    for name, fn in REGIONS.items():
        if a.only and name != a.only:
            continue
        cv, markers = fn()
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
