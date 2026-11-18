---
id: "355"
title: Boru, kurşun ve yeni yiyecek ikonları; su yolu araç ikonları
status: running
claimed_at: 2026-11-18T13:05Z
approved_by: user        # otomatik akış (2026-11-17); su şebekesi PR (feat/su-sebekesi)
family: arayuz
style_gate: false
cost_est: 8
cost_max: 14
depends_on: ["316"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/icon_item_{lead,pipe_wood,pipe_clay,pipe_lead,kasar,kimiz,salgam}.png
  - Assets/Art/Iso/_aday/ui_icon_tool_conduit_{ditch,stone,flume,pipe_wood,pipe_clay,pipe_lead,erase}.png
created: 2026-11-17
---
# 355 Boru, kurşun ve yeni yiyecek ikonları

## Amaç
Yeni hammaddeler ve ürünler (envanter, tarif listesi, pazar): **kurşun** (külçe), **kütük boru**, **künk**, **kurşun boru**, **kaşar** (tekerlek peynir), **kımız** (deri tulum ya da testi), **şalgam suyu** (mor içecek, testi). Yapı modunda su yolu araç düğmeleri için 7 küçük ikon.

| Dosya | Boyut |
|---|---|
| `icon_item_*.png` | 32x32 (316 ile aynı) |
| `ui_icon_tool_conduit_*.png` | 16x16 (315 araç ikonlarıyla aynı) |

## Tarif
- 316 ile aynı stil ve araç; 32x32, saydam, tek renk koyu kontur, ışık sol-üstten, en çok 16 renk. Boru ikonları aynı boru şekli, üç malzeme rengi (ahşap kahve, kiremit, mat gri).
- Araç ikonları: ark (mavi şeritli toprak hendek), taş ark (taş kenarlı), oluk (sehpalı tahta oluk), üç boru (kesit), sil (boru üstünde çarpı). 16x16'da okunur olsun.

## Doğrulama
Boyut, ≤16 renk, koyu/açık zeminde 2x ölçekle okunuyor mu; üç boru ayırt ediliyor mu.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
