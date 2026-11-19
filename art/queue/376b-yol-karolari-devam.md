---
id: "376b"
title: Yol karoları devam (çakıl a v2, sağlam kaldırım, bozuk kaldırım v2)
status: running
claimed_at: 2026-11-19T07:45Z
approved_by: user        # kullanıcı onayı sohbette 2026-11-18 (onay grubu 1: eksikler için iş)
family: zemin
style_gate: false
cost_est: 8
cost_max: 14
depends_on: ["376"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/iso_road_gravel_a_v2.png
  - Assets/Art/Iso/iso_road_paved_a.png
  - Assets/Art/Iso/iso_road_paved_broken_v2.png
created: 2026-11-18
---
# 376b Yol karoları devam

## Amaç
376'da beş karodan üçü teslim edilmedi:
- **çakıl a v2:** doku iyiydi, ama maske içinde 8-12 boş piksel vardı;
- **sağlam kaldırım:** boşluk vardı, kahverengimsi ve koyu kenarlıydı;
- **bozuk kaldırım v2:** boşluk ve koyu kenar.

## Tarif
1. **Çakıl a v2: PixelLab yok.** `art/results/376-red/tile_1.png` (ya da 5, 9, 13'ten en az boşluklu olanı) alınır. Elmas maskesi içindeki boş pikseller (en çok 15) en yakın opak komşunun rengiyle doldurulur. İşlem `uv run --with pillow` ile yerel bir betikle yapılır, betik `art/tmp/` altına yazılır. Bu bir boyama değil, delik onarımıdır; 15'ten fazla boş piksel varsa karo teslim edilmez.
2. **Sağlam kaldırım ve bozuk kaldırım v2:** yöntem **376 ile aynı** (`create_tiles_pro`, stil modu), şu farklarla:
   - `outline_mode=segmentation` (376 raporunun önerisi: kenarsız yüzey);
   - `style_images` = `iso_road_paved_worn` ve kabul edilen `iso_road_path_b_v2`;
   - açıklamalar 376 tablosundaki gibi, sonlarına `, light grey stone, no brown tint, no dark border`;
   - çağrıdan önce maliyeti yaz.
3. Her karoda boş piksel kontrolü. Maske içinde en çok 5 boş piksel varsa adım 1'deki onarım uygulanır; daha fazlası teslim edilmez.

- Maliyet: çakıl 0, iki kaldırım karosu için 376 ile orantılı (`cost_est` 8). Tek deneme, `cost_max` 14.

## Doğrulama
376 doğrulaması: `validate_iso.py --kind ground`, `style.py check --family zemin`, 8x8 döşeme ve dikiş oranı (hedef 0,95-1,05), yol ve çim kenarı testi. Yan yana: `path_b_v2`, `gravel_worn`, `gravel_a_v2`, `paved_a`, `paved_worn`, `paved_broken_v2`.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
