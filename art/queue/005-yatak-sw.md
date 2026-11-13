---
id: "005"
title: Yatak 2x1, SW-NE ekseni
status: blocked
approved_by: auto
family: esya
style_gate: false
cost_est: 1
cost_max: 3
depends_on: []
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/iso_item_bed_straw_2x1_sw_ne.png
created: 2026-11-08
---
# 005 Yatak 2x1, SW-NE ekseni

## Amaç
Yapı modunda eşya döndürülünce yatağın ikinci ekseni. SE-NW ekseni zaten var (`iso_item_bed_straw_2x1_se_nw.png`). Aynaya çevirme ışık yönünü bozar, o yüzden ayrı üretim. Ad kuralı: uzun eksenin yönü (`_se_nw`, `_sw_ne`).

## Tarif
- Araç: `create_map_object`, stil eşleme modu, `background_image` + `inpainting` `type: mask` (tur 3'te çalışan yöntem; maliyeti 1 generation, bakiyeyle doğrulandı).
- Kılavuz ve maske: `python tools/art/footprint_guide.py --w 112 --h 84 --n 2 --axis y --height 22 --out <geçici>/bed_sw`. Üretilen `_guide.png` arka plan, `_mask.png` maske (beyaz = üretim alanı). Footprint: x 8–103, y 28–75; pivot (56, 32 alttan).
- Açıklama: tur 3'teki yatak açıklaması (krem yastık, koyu kırmızı-kahve battaniye, ahşap çerçeve, düz az tonlu). Yastık uzun eksenin **kuzey-doğu** ucunda olsun. Referans stil: `Assets/Art/Iso/iso_item_bed_straw_2x1_se_nw.png`. "No shadow, no glow" yaz.

## Doğrulama
- `validate_iso.py <dosya> --footprint 8,28,96,48`, maske dışı opak piksel 0 olmalı (maskeyle kontrol et).
- Ayak izi 2x1 ve uzun eksen SW-NE mi, kenar basamakları 2:1 düzenli mi, ışık üst > sol > sağ (ortalama parlaklık sayısıyla).
- Yastık/battaniye okunuyor mu, kısa ayaklı puf gibi mi (görsel not).

## Notlar
Tek deneme; ayak izi tutmazsa tekrar deneme, `failed` yaz ve göster.

## Sorular
BLOKE (2026-11-08, Unity/geliştirme sohbeti): 005 işi zemin oturumunda 2 kez denendi (stil eşleme modu düzenleme yapmıyor, başarısız); inpaint_image_pro_flash ile 1 deneme daha onaylı. Çift harcama olmasın diye bloke. Zemin oturumu bitince kullanıcı durumu accepted/rejected yapar ya da pending'e geri alır.
