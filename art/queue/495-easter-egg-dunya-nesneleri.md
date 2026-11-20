---
id: "495"
title: "Easter egg dünya nesneleri ve ürün ikonları (12)"
status: pending
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 sohbet ("hepsi olsun", "yaz bunları ve sen ekle sıraya"), Project belgesi 55
priority: 50
family: esya
style_gate: false
cost_est: 12
cost_max: 24
depends_on: ["302", "420a"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_rusty_sword_ground_se.png
  - Assets/Art/Iso/_aday/iso_item_helmet_nest_se.png
  - Assets/Art/Iso/_aday/iso_item_broken_shield_ground_se.png
  - Assets/Art/Iso/_aday/iso_item_stone_tablet_se.png
  - Assets/Art/Iso/_aday/iso_item_sword_in_stone_se.png
  - Assets/Art/Iso/_aday/iso_wall_item_old_sword_sw_ne.png
  - Assets/Art/Iso/_aday/iso_fx_tiny_crown.png
  - Assets/Art/Iso/_aday/icon_item_message_bottle.png
  - Assets/Art/Iso/_aday/icon_item_old_boot.png
  - Assets/Art/Iso/_aday/icon_item_sword_ploughshare.png
  - Assets/Art/Iso/_aday/icon_item_shiny_trinket.png
  - Assets/Art/Iso/_aday/icon_item_old_sword.png
created: 2026-11-20
---
# 495 Easter egg dünya nesneleri ve ürün ikonları (12)

## Amaç
Project belgesi `55-easter-eggler.md`. Nadir görülen, tek seferlik sahnelerin nesneleri:
- **Bulunan silah** (§4): köy yolunda yarı gömülü paslı kılıç; sonraki kuşaklarda miğferde kuş yuvası ve kırık kalkan. Kılıç duvara asılabilir ("Eski kılıç" dekoru) ya da demircide "Kılıçtan Saban"a dönüşür.
- **Maden** (§1): en derin kattaki taş tablet ve kimsenin çekemediği taşa saplı kılıç.
- **Kurbağayı öp** (§2): kurbağanın başında beliren minik taç. Kod, var olan kurbağa görselinin (`iso_wild_frog`, 408) üstüne çizer; ayrı kurbağa üretilmez.
- **Balık tutma** (§1): şişe mektubu ve eski çizme. **Karga dostluğu** (§2): eşiğe bırakılan parlak ıvır zıvır.

Eşya kimlikleri henüz `items.json`'da yok; kod oturumu aşağıdaki adlarla ekler.

## Tarif
Dünya nesneleri **302 ölçeği ve yöntemi**, `se` yönü (aynalama kodla). Duvar eşyası **306 ile aynı yöntem** (`iso_wall_item_*_sw_ne`, duvara asılı, pivot duvar yüzünde). Ürün ikonları **420a ile aynı yöntem**.

Ortak ek (dünya nesneleri): `, weathered and old, half overgrown, earthy muted medieval palette, no blood, no person`.

| # | Kimlik (önerilen) | Dosya | Ayak izi | Tuval | Pivot (x, y alttan) | Açıklama metni |
|---|---|---|---|---|---|---|
| 1 | `item.rusty_sword_ground` | `iso_item_rusty_sword_ground_se.png` | 1x1 zemin | 64x64 | 32, 16 | `old rusty medieval arming sword stuck at an angle in the earth, half buried, grass tufts around the blade` |
| 2 | `item.helmet_nest` | `iso_item_helmet_nest_se.png` | 1x1 zemin | 64x48 | 32, 16 | `old rusty kettle helmet lying upside down in the grass with a small bird nest and two pale eggs inside` |
| 3 | `item.broken_shield_ground` | `iso_item_broken_shield_ground_se.png` | 1x1 zemin | 64x48 | 32, 16 | `broken wooden kite shield with faded paint, split in two, half sunk in the earth, moss on it` |
| 4 | `item.stone_tablet` | `iso_item_stone_tablet_se.png` | 1x1 | 64x80 | 32, 16 | `upright grey stone tablet with a few rows of worn carved lines, rough chiselled edges, standing on a cave floor` |
| 5 | `item.sword_in_stone` | `iso_item_sword_in_stone_se.png` | 1x1 | 64x80 | 32, 16 | `plain old sword driven point-down into a grey boulder, only the hilt and half the blade showing` |
| 6 | `item.old_sword_wall` | `iso_wall_item_old_sword_sw_ne.png` | duvar | 48x48 | 24, 24 | `old rusty sword hung horizontally on two iron wall hooks` |
| 7 | (kod katmanı) | `iso_fx_tiny_crown.png` | — | 16x16 | 8, 0 | `tiny simple golden crown with three points, side view` |
| 8 | `item.message_bottle` | `icon_item_message_bottle.png` | ikon | 32x32 | — | `corked greenish forest-glass bottle with a rolled parchment note inside` |
| 9 | `item.old_boot` | `icon_item_old_boot.png` | ikon | 32x32 | — | `single soggy worn-out leather boot with a hole in the toe, dripping water` |
| 10 | `item.sword_ploughshare` | `icon_item_sword_ploughshare.png` | ikon | 32x32 | — | `iron ploughshare blade with a sword hilt shape still faintly visible at one end` |
| 11 | `item.shiny_trinket` | `icon_item_shiny_trinket.png` | ikon | 32x32 | — | `small shiny brass button and a glass bead` |
| 12 | `item.old_sword` | `icon_item_old_sword.png` | ikon | 32x32 | — | `old rusty medieval arming sword with a chipped blade` |

## Doğrulama
- Dünya nesneleri: `python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family esya <dosya> ...`.
- İkonlar: 32x32, yarı saydam 0, ≤16 renk; `python tools/art/style.py check --family arayuz <dosya> ...` (420a'daki gibi).
- `iso_fx_tiny_crown.png`: 16x16, yarı saydam 0, ≤8 renk; raporda 408 kurbağasının (varsa `iso_wild_frog_cand_se.png`) üstüne oturtulmuş önizleme. 408 henüz teslim edilmediyse taç tek başına teslim edilir.
- Yan yana sayfası: 1-3 numara var olan bir zemin karosu ve bir çalı ile birlikte; yolda "bulunacak" kadar göze çarpmalı ama sahneyi bastırmamalı.
- Taş tablette okunur harf ya da kelime olmamalı (yazıyı oyun metin olarak gösterir).
- Dönem: kılıç, kazan miğfer (kettle helmet), uçurtma kalkan (kite shield) ortaçağ; barut, tüfek, plaka zırh yok. Kan, ceset, kemik yok (oyunda dövüş yok; ton hüzünlü ve sakin).

## Notlar
- Kod görseli adla bulur (`IsoArt.Get`); dosya adı değişmesin. 4 ve 5 maden haritasında durur; taş rengi maden zeminiyle (430) uyumlu olsun.
- Johan Wick'in bıraktığı altın sikke için yeni görsel yok: `item.relic_coin` kullanılır.
- Kılıcın gömüldüğü karoda açan çiçek var olan çiçek görsellerinden seçilir.
- NPC'ler (Johan Wick, atsız şövalye) 496'da.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
