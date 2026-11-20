---
id: "468a"
title: "Karakter yaratma önizleme sahnesi: ayna köşesi (gece ve gündüz kodla)"
status: pending
approved_by: pending     # açık karar (belge 43 §7.6): ayna köşesi mi, kapı önü mü; kullanıcı seçince user yap
family: arayuz
style_gate: false
cost_est: 12
cost_max: 20
depends_on: []
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/ui_creator_scene_mirror.png
  - art/results/468a-yaratma-onizleme-sahnesi.md
created: 2026-11-20
---
# 468a Karakter yaratma önizleme sahnesi

## Amaç
Belge 43 §5.2: yeni karakter yaratma ekranının sol yarısı, karakterin üstünde durduğu küçük sahne. Boş stüdyo yerine evin içinde ayna köşesi. Aynı sahne oyun içi Ayna ekranında da kullanılır. Karakter (84 px, tam sayı büyütme) sahnenin önünde, ayrı çizilir.

## Tarif
- `create_image_pixflux` (ya da yapı ailesi stil eşlemesi), 192x192, saydam değil.
- Açıklama: `cosy medieval cottage interior corner, isometric 2:1, a tall wooden-framed mirror leaning on a plastered wall, a small rug on wooden floorboards in front of it, a stool, a candle on a shelf, warm soft light, empty space in the centre foreground for a standing person, pixel art, no people`.
- Palet: `art/style` yapı ailesi; kenar konturu koyu ve renkli (gorsel-yonerge).
- Gece ve gündüz ışığı kodla verilir (`DayLight`); ayrı görsel yok.

## Doğrulama
- `style.py check --family yapi`.
- Orta ön planda 84 px'lik karakter için en az 40x100 px boş alan; ayna karakterin arkasında, başının üstüne taşacak boyda.
- Göz kontrolü: ayna camı düz bir yansıma göstermiyor; içinde karakter yok.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
