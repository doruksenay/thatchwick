---
id: "358"
title: Zemin süsleri (çiçek, yonca, çakıl, ot öbeği, eğrelti, yaprak, köstebek tepeciği)
status: pending
approved_by: user        # otomatik akış; kullanıcı onayı sohbette 2026-11-18 ("yap bunu", görsel analiz partisi)
family: bitki
style_gate: false
cost_est: 8
cost_max: 16
depends_on: ["001"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_decal_daisies.png
  - Assets/Art/Iso/_aday/iso_decal_buttercups.png
  - Assets/Art/Iso/_aday/iso_decal_clover.png
  - Assets/Art/Iso/_aday/iso_decal_pebbles.png
  - Assets/Art/Iso/_aday/iso_decal_grass_tuft.png
  - Assets/Art/Iso/_aday/iso_decal_fern.png
  - Assets/Art/Iso/_aday/iso_decal_leaves_autumn.png
  - Assets/Art/Iso/_aday/iso_decal_molehill.png
created: 2026-11-18
---
# 358 Zemin süsleri

## Amaç
Görsel analiz (Project `26-gorsel-analiz.md`, P1 madde 8): çimenin üç varyantı geniş alanda tekrar ediyor. Bu küçük süsler, çimen karolarının üstüne kodla seyrek ve sabit tohumla serpilir. Eşya değildir: dokunulmaz, yürümeyi engellemez, yapı moduyla çakışmaz. Mevsime göre süzülür (çiçek ilkbahar-yaz, yaprak sonbahar, kışın yalnızca çakıl ve köstebek tepeciği).

## Tarif
- Araç: `create_map_object`, stil eşleme modu, referans `iso_ground_grass_a.png` (çimenle aynı palet). Kılavuz yok: saydam tuval, maske tuvalin alt yarısı (süs zemine yatık durur).
- Tuval 32x24 (yarım karo kadar). Yükseklik en çok 10 px; ot öbeği ve eğrelti 14 px.
- Ortak ek: `, tiny ground detail seen from high top-down isometric view, lying flat on grass, earthy muted palette matching the grass tile, single dark outline only where needed, no ground tile, no shadow, transparent background`
- Açıklamalar:
  1. daisies: `three small white daisies with yellow centres`
  2. buttercups: `small cluster of yellow buttercup flowers`
  3. clover: `small patch of green clover leaves with two pale pink clover heads`
  4. pebbles: `four small grey pebbles of different sizes`
  5. grass_tuft (14 px): `a tuft of taller wild grass blades, slightly darker than the grass tile`
  6. fern (14 px): `a young bracken fern sprout with three fronds`
  7. leaves_autumn: `a few fallen oak leaves, orange and brown`
  8. molehill: `small mound of fresh brown earth, a molehill`
- Maliyet 8 × 1 gen; ikinci deneme payı 8.

## Doğrulama
- `validate_iso.py` (zemin karosu değil, nesne türü), `style.py check --family bitki`.
- Sayfa: 8x8 çimen döşemesi üstünde her süsten 3 kopya rastgele dağılmış, 4 kat büyütme. Süsler çimenle aynı ailede mi, karo dikişini gizlemeye yardım ediyor mu, göz yormuyor mu.
- Hiçbiri 32x24'ü aşmıyor; yarı saydam piksel yok.

## Notlar
- Dönem: hepsi Eski Dünya türü (papatya, düğün çiçeği, yonca, eğrelti otu).
- Kod tarafı (serpme yoğunluğu, mevsim süzgeci) bu iş review'a geçince yazılır.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
