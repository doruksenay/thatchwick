---
id: "300"
title: Ev eşyaları grup 1 (masa, sandalye, sandık, raf, kazan, mum, saksı, bardak; yalnızca SE)
status: review
approved_by: user        # kullanıcı onayı 2026-11-08 (sohbet): cost_est 8, cost_max 15
cost_est: 8
cost_max: 15
depends_on: []
max_attempts: 2          # parça başına en çok 2 deneme (kullanıcı kararı); ikinci deneme yalnızca aşağıdaki kurala göre
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_table_wood_2x1_se_nw.png
  - Assets/Art/Iso/_aday/iso_item_chair_wood_se.png
  - Assets/Art/Iso/_aday/iso_item_chest_wood_se.png
  - Assets/Art/Iso/_aday/iso_item_shelf_wood_2x1_se_nw.png
  - Assets/Art/Iso/_aday/iso_item_cooking_pot_se.png
  - Assets/Art/Iso/_aday/iso_item_candle_se.png
  - Assets/Art/Iso/_aday/iso_item_flower_pot_se.png
  - Assets/Art/Iso/_aday/iso_item_clay_cup_se.png
created: 2026-11-08
---
# 300 Ev eşyaları grup 1

## Amaç
`Assets/Resources/Content/items.json`'da tanımlı, henüz görseli olmayan ev eşyalarının ilk 8'i. Her biri yalnızca SE yönünde; diğer yönler ve ayna kararı sonra. Eşyalar yatak (`iso_item_bed_straw_2x1_se_nw.png`) ve tabure (`iso_item_stool_se.png`) ile aynı dünyada durur. Köylü artık yaklaşık 56-58 px: `_aday/iso_char_farmer_a2_56_cand_se.png`.

Kilim `items.json`'da yok; bu işte yapılmaz.

## Tarif

### Yöntem (tur 3'te çalışan yöntem + stil referansı)
- Araç: `create_map_object`, stil eşleme modu (`background_image` + `inpainting` `{"type":"mask"}`). Maliyet: parça başı 1 generation (tur 3'te bakiyeyle doğrulandı).
- Ortak: `view="high top-down"`, `detail="low detail"`, `shading="basic shading"`, `outline="single color outline"`.
- **Kılavuz:** `tools/art/footprint_guide.py` ile kılavuz ve maske, tabloda verilen tuval, ayak izi ve yükseklikle. Uzun eksen `--axis x` (yataktaki `se_nw` ile aynı eksen).
- **Stil referansı (1. deneme):** `iso_item_stool_se.png` (80x64) tuvalin **soluna** yapıştırılır, alta hizalı. Eşyanın kılavuz tuvali **sağına** gelir. Tuval genişliği = 80 + eşya tuvali, en çok 192 (araç sınırı). Maske, tabure bölgesinde tamamen siyah, eşya bölgesinde kılavuz maskesi.
  - Çıktıdan yalnızca eşya bölgesi kendi tuval boyutunda kesilip alınır (yerleştirme, boyama değil).
  - Not: tur 4'te bu araç bağlamı silip yalnızca yeni nesneyi döndürdü; bu beklenir.
- **İkinci deneme kuralı:** 1. deneme ayak izini doldurmazsa, maske dışına taşarsa (eşya bölgesinde 20 pikselden fazla) ya da eşya tanınmazsa yapılır.
  - Ayak izi tutmuyorsa: "parça" ya da "yarım nesne" ise, ya da her iki yanda tabanın %80'inden küçükse.
  - İkinci deneme tabure referansı **olmadan**, yalnızca tur 3'teki gibi kılavuzla yapılır.
  - İkincisi de tutmazsa teslim etme, nedenini yaz.
- Bütün açıklama metinlerinin sonuna şu ek yazılır: `, medieval peasant village furniture, earthy muted palette matching the stool on the left, flat simple shading, single dark outline, light from top-left, no ground, no shadow, no extra objects, transparent background`
- Renk sınırı 48. Fazlaysa bu işte `reduce_colors` **yapılmaz**, yalnızca raporla (renk azaltma sonra toplu yapılacak).

### Eşyalar
| # | items.json id | Dosya | Ayak izi | Tuval (eşya) | Kılavuz | Yükseklik (px) | Açıklama metni (+ ortak ek) |
|---|---|---|---|---|---|---|---|
| 1 | `item.table_wood` (2x1, 3 yüzey yuvası) | `iso_item_table_wood_2x1_se_nw.png` | 2x1, `--axis x` | 112x88 | `--n 2 --axis x` | 26 | `simple rectangular wooden peasant table with four straight legs, plain flat plank tabletop that is completely empty and clearly visible from above` |
| 2 | `item.chair_wood` | `iso_item_chair_wood_se.png` | 1x1 | 80x84 | `--n 1` | 40 | `simple wooden peasant chair with a straight plank backrest and four legs, seat facing the viewer toward the lower right` |
| 3 | `item.chest_wood` (depo +40) | `iso_item_chest_wood_se.png` | 1x1 | 80x72 | `--n 1` | 26 | `closed wooden storage chest with dark iron bands and a simple latch, lid closed` |
| 4 | `item.shelf_wood` (2x1, 2 yüzey yuvası) | `iso_item_shelf_wood_2x1_se_nw.png` | 2x1, `--axis x` | 112x96 | `--n 2 --axis x` | 32 | `low wooden sideboard shelf with two open shelves on the front, flat empty plank top surface clearly visible from above` |
| 5 | `item.cooking_pot` | `iso_item_cooking_pot_se.png` | 1x1 | 80x68 | `--n 1` | 24 | `round dark iron cooking cauldron standing on three short legs, empty, no fire, no steam` |
| 6 | `item.candle` (yüzey) | `iso_item_candle_se.png` | yüzey dekoru | 32x32 | aşağıda | 14 | `single unlit beeswax candle in a small clay dish, no flame, no glow` |
| 7 | `item.flower_pot` (yüzey) | `iso_item_flower_pot_se.png` | yüzey dekoru | 32x32 | aşağıda | 18 | `small round clay flower pot with a few simple wild flowers` |
| 8 | `item.clay_cup` (yüzey) | `iso_item_clay_cup_se.png` | yüzey dekoru | 32x32 | aşağıda | 10 | `small plain clay drinking cup, empty` |

**Yüzey dekoru kılavuzu (6-8):**
- Tuval 32x32 (aracın en küçük boyu). Taban: tuvalin alt-orta noktasında 12x6 px'lik küçük elmas (tam 2:1, kılavuz rengiyle), alt kenardan 4 px yukarıda.
- Maske: bu elmasın tablodaki yükseklik kadar yukarı uzatılmışı, 1 px genişletilmiş.
- `footprint_guide.py` küçük taban desteklemiyorsa aynı mantıkla geçici bir betikte üret; repoya ekleme.
- Stil referansı olarak tabure yine solda; tuval 80+32 = 112x64.

### Teslim
- `Assets/Art/Iso/_aday/` altına `cp -n`. `.meta` yazma; var olan dosyanın üzerine yazma.
- Ad kuralı: 1x1 ve dekor `iso_item_<ad>_se.png`. 2x1'de uzun eksen adı: `iso_item_<ad>_2x1_se_nw.png` (README kuralı, yatakla aynı).

### Masaüstü kopyası (kullanıcının açık izni, yalnızca bu iş için)
Hiçbir şeyi silme, üzerine yazma. Klasör tarihi üretim günüdür.
- `C:\Users\User\OneDrive\Desktop\art\esya\<yyyy-mm-dd>_<ad>\`, ad örnekleri `table_wood`, `chair_wood`: eşyanın PNG'si; teslim edilmeyen denemeler de `_red\` alt klasöründe.
- Aynı klasörde `_onizleme_x4.png`: en yakın komşu ile 4 kat, `#333333` zemin.
  - Soldan sağa: köylü (`Assets/Art/Iso/_aday/iso_char_farmer_a2_56_cand_se.png`), 64x32 çimen karosu (`iso_ground_grass_set_a.png`), eşya.
  - Hepsi alta hizalı. Eşyanın ayak izi elması 1 px açık gri kontur ile işaretli.
  - Dekorlarda ayrıca masanın üst yüzeyine yerleştirilmiş hâli.
- Toplu: `C:\Users\User\OneDrive\Desktop\art\esya\<yyyy-mm-dd>_300_toplu_x4.png`, sekiz eşya yan yana, köylü ve çimen karosu başta.
- `C:\Users\User\OneDrive\Desktop\art\_indeks.md` sonuna her eşya için bir satır: `tarih | eşya | esya/<yyyy-mm-dd>_<ad> | create_map_object stil eşleme | <gen> | aday | <ayak izi, boy, renk>`.
- Python'da Pillow yok: `uv run --with pillow ...`. Geçici betikleri repoya ekleme.

## Doğrulama
- `uv run --with pillow python tools/art/validate_iso.py <dosya> --kind object --max-colors 48 --footprint X,Y,W,H` (her dosya; dekorlarda `--footprint` yok).
- Her dosya için tablo:
  - piksel boyutu, opak sınır kutusu, ölçülen boy (px), renk sayısı;
  - yarı saydam piksel sayısı (0 olmalı);
  - en alt satıra değiyor mu (değmemeli);
  - maske dışı opak piksel sayısı.
- Ayak izi ve pivot (ALTTAN, piksel):
  - 1x1 ve 2x1 için `footprint_guide` `_info.txt`'teki taban elması kutusu ve pivotu;
  - dekorlarda pivot = küçük taban elmasının merkezi (temas noktası).
- **Masa ve raf:** üst yüzeyin tuvaldeki y aralığı (ALTTAN) ve yüzeyin boş olup olmadığı. Yüzey yuvası u = 0,25 / 0,5 / 0,75 (masa) ve 0,3 / 0,7 (raf), v = 0,5. Yuvaların üst yüzeyde kaldığını bir önizlemede işaretle.
- **İstenmeyen ayrıntı:** kılavuz rengi (#786046) çıktıda kaldı mı, zemin lekesi, pişmiş gölge, alev, buhar, ekstra nesne var mı?
- Işık: üst en açık, sol yüz orta, sağ yüz en koyu (ortalama parlaklık).
- **Ayna uygunluğu:** her eşya için yatay ayna ile 4 yönden hangisi üretilebilir?
  - Sol ve sağ yarının ortalama parlaklık farkını yaz.
  - Şekil simetrik mi? Sandalye arkalığı ve sandık kilidi gibi asimetrik ayrıntılar var mı?
  - Not: aynada ışık sağ-üstten gelir; gorsel-yonerge ışığı sol-üstten ister.

## Notlar
- `items.json`'da olup bu işte olmayanlar (sonraki gruplar): `item.chair_carved` (1x1), `item.chopping_block` (1x1), `item.market_stall` (2x1), bitkiler `item.tree_oak`, `item.crop_turnip` (büyüme evreleri ayrı karar), kaynaklar `item.firewood`, `item.log`, `item.sapling_oak`, `item.turnip`, `item.seed_turnip` (yerleştirilmez; envanter ikonu ayrı karar).
- Pivot değerlerini kullanıcı `Assets/Editor/IsoPivots.txt`'e ekler; işçi bu dosyaya dokunmaz.
- Mum sönük (alev ve ışık efekti oyun tarafında ayrı); kazan ateşsiz.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
