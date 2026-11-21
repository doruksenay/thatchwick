---
id: "482c"
title: "Deniz kıyısı: gelgit havuzu (çalısız) ve deniz kumu (dış çizgisiz) yeniden (2)"
status: running
claimed_at: 2026-11-21T00:15Z
approved_by: user        # kullanıcı kararı 2026-11-21 (sohbet): 482b havuzu çalılı, kumu dış çizgili; ikisi yeniden
priority: 20
family: zemin
style_gate: false
cost_est: 2
cost_max: 4
depends_on: []
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_forage_tidepool_se_v3.png
  - Assets/Art/Iso/_aday/iso_forage_sea_sand_se_v3.png
created: 2026-11-21
---
# 482c Gelgit havuzu ve deniz kumu yeniden

## Neden
482b incelemesi (kullanıcı, 2026-11-21):
- **Gelgit havuzu v2:** mor taş kutu gitti, kayalar doğal; ama kayaların üstünde tarifte olmayan **yeşil çalılar** var ve karo hâlâ
  yüksek bir **blok** gibi duruyor. v2 dosyası silinmez (kullanıcı: "belki kullanırız").
- **Deniz kumu v2:** kenarında öbür zemin karolarında olmayan **koyu dış çizgi** var; kumsalda yapışık çıkartma gibi duruyor.
Karşılaştırma: `art/results/482b-yanyana.png`.

## Tarif
482 ve 482b tarifinin aynısı (yöntem, ölçü, `se` yönü, taban ortası pivot, `zemin` ailesi tonları). Farklar:

- **Gelgit havuzu (tidepool_v3):** alçak ve yassı; kayalar yerden en çok birkaç piksel yükselir, karonun sınırını kutu gibi
  doldurmaz, aralardan kum görünür. Kayalar gri-kahve, ıslak; üstlerinde yalnızca ince yeşil-kahve yosun ve deniz yosunu.
  **Ağaç, çalı, ot öbeği yok.** Havuzda bir iki midye ve küçük bir deniz yıldızı. Mor ve parlak nane yeşili yok.
  Açıklama: `low flat rock pool on a sandy beach: a few wet grey-brown shore rocks barely above the sand, shallow clear sea water between them, thin dark seaweed and moss on the rocks, two mussels and a small orange starfish; no bushes, no trees, no grass, no stone box or frame`.
- **Deniz kumu (sea_sand_v3):** karonun üstünde alçak, yumuşak kenarlı bir kum yığını ve birkaç deniz kabuğu. **Dış çizgi
  (outline) yok**: kenar, iso_ground_sand ve iso_beach karolarında olduğu gibi zemine karışır. Kum tonu `iso_ground_sand_a` ile aynı aile.
  `outline` ayarı: yok / `no outline`. Açıklama: `small soft heap of pale sea sand with three small shells, edges blending into the beach, no outline`.
- Çıktılar `_v3` (bulut işçisi var olan dosyayı değiştiremez). Kabulden sonra `items.json` `alongX` eşlemesi `_v3`'e çevrilir.

## Doğrulama
- `python tools/art/validate_iso.py <dosya>`, `python tools/art/style.py check --family zemin <dosya>`.
- Kum: kenar pikselleri arasında koyu dış çizgi olmamalı (kenar parlaklığı iç parlaklığın %70'inden düşük piksel oranı < %5).
- Havuz: yeşil (G > R+20 ve G > B+20) piksel oranı < %8 ve hepsi kayaların üstünde ince şerit; öbek yok.
- Yan yana: `art/results/482c-yanyana.png` (482, 482b, 482c üç sıra, kumsal karosu üstünde).

## Notlar
- Sahil çizgisi (driftline_v2) kabul edildi, yeniden üretilmez.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
