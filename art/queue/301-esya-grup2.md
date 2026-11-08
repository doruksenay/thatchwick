---
id: "301"
title: Ev eşyaları grup 2 (raf, kazan, sandık, bardak, oymalı sandalye, yarma kütüğü, pazar tezgâhı; yalnızca SE)
status: pending
approved_by: pending     # kullanıcı kararı 2026-11-08: yarın çalışacak; kullanıcı "user" yapınca işçi alır
cost_est: 7
cost_max: 15
depends_on: []           # 300 sohbette değerlendirildi (masa, sandalye, mum, saksı accepted)
max_attempts: 2          # parça başına en çok 2 deneme
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_shelf_wood_2x1_se_nw_v2.png
  - Assets/Art/Iso/_aday/iso_item_cooking_pot_se_v2.png
  - Assets/Art/Iso/_aday/iso_item_chest_wood_se.png
  - Assets/Art/Iso/_aday/iso_item_clay_cup_se.png
  - Assets/Art/Iso/_aday/iso_item_chair_carved_se.png
  - Assets/Art/Iso/_aday/iso_item_chopping_block_se.png
  - Assets/Art/Iso/_aday/iso_item_market_stall_2x1_se_nw.png
created: 2026-11-08
---
# 301 Ev eşyaları grup 2

## Amaç
300'de reddedilen ya da teslim edilmeyen dört eşyanın yeniden denemesi (raf, kazan, sandık, bardak) ve `items.json`'daki üç yeni eşya: `item.chair_carved`, `item.chopping_block`, `item.market_stall`. Hepsi yalnızca SE yönünde. Diğer yönler ve ayna sonra (aynada ışık sağ-üstten gelir, Assets'e konmaz).

Kabul edilen 300 eşyaları renk ve ölçek referansıdır:
- masa: `_aday/iso_item_table_wood_2x1_se_nw.png`
- sandalye: `_aday/iso_item_chair_wood_sw.png`
- tabure: `iso_item_stool_se.png`

Köylü: `_aday/iso_char_farmer_a2_56_cand_se.png`.

## Tarif

### Yöntem: referanssız kılavuz (tur 3; 300'de 6 denemede 4 tuttu)
- Araç: `create_map_object`, stil eşleme modu. `background_image` = yalnızca kılavuz (saydam tuval + düz renkli taban elması). `inpainting` `{"type":"mask"}` = kılavuz maskesi. **Tabure ya da başka referans tuvale konmaz.**
- Maliyet: parça başı 1 generation.
- Ortak: `view="high top-down"`, `detail="low detail"`, `shading="basic shading"`, `outline="single color outline"`.
- Kılavuz ve maske: `tools/art/footprint_guide.py`, tablodaki tuval, ayak izi ve yükseklikle. 2x1'de `--axis x` (yataktaki `se_nw` ile aynı eksen).
- Bütün açıklama metinlerinin sonuna şu ek yazılır: `, medieval peasant village object, earthy muted palette, dark brown wood, flat simple shading, single dark outline, light from top-left, no ground, no shadow, no extra objects, transparent background`
- **İkinci deneme kuralı:**
  - Yalnızca şu durumlarda yapılır:
    - ayak izini doldurmuyor (her iki yanda tabanın %80'inden az, ya da yarım nesne);
    - eşya bölgesinde maske dışına 20 pikselden fazla taşıyor;
    - boş çıktı;
    - tabana değmiyor;
    - eşya tanınmıyor;
    - açıklamadaki yasak (dolu kazan, kırmızı/pembe ahşap) çiğnenmiş.
  - İkinci denemede aynı kılavuz ve maske kullanılır; `seed` değişir (1).
  - İkincisi de tutmazsa teslim etme, nedenini yaz, masaüstünde `_red\` altında sakla.
- Renk sınırı 48. Fazlaysa bu işte `reduce_colors` yapılmaz, yalnızca raporla.
- **Yön:** 1x1 asimetrik eşyalarda (oymalı sandalye) üretilen görselin gerçek yönünü ölç. Dosyayı gerçek yönüne göre adlandır (`_se` ya da `_sw`; 300'deki sandalye SW çıkmıştı) ve raporda yaz. Ayna dosyası üretme.

### Eşyalar
| # | items.json id | Dosya (`_aday/`) | Ayak izi | Tuval | Kılavuz | Yükseklik | Açıklama metni (+ ortak ek) |
|---|---|---|---|---|---|---|---|
| 1 | `item.shelf_wood` (2x1, 2 yüzey yuvası) | `iso_item_shelf_wood_2x1_se_nw_v2.png` | 2x1 | 112x96 | `--n 2 --axis x` | 32 | `low wooden sideboard shelf with two open shelves on the front, flat empty plank top surface clearly visible from above, same dark brown wood as the stool and table, no red, no pink` |
| 2 | `item.cooking_pot` | `iso_item_cooking_pot_se_v2.png` | 1x1 | 80x68 | `--n 1` | 24 | `empty black iron pot on three short legs, no food, no liquid, nothing inside, no fire, no steam, same size as a stool, 1x1` |
| 3 | `item.chest_wood` (depo +40) | `iso_item_chest_wood_se.png` | 1x1 | 80x72 | `--n 1` | 26 | `large closed wooden storage chest that fills the whole floor tile, dark brown planks with dark iron bands and a simple latch, lid closed` |
| 4 | `item.clay_cup` (yüzey) | `iso_item_clay_cup_se.png` | yüzey dekoru | 32x32 | dekor (aşağıda) | 12 | `small plain clay drinking cup standing on the floor patch, empty` |
| 5 | `item.chair_carved` (1x1, seviye 10) | `iso_item_chair_carved_se.png` (ya da `_sw`) | 1x1 | 80x88 | `--n 1` | 44 | `carved wooden chair with a tall backrest decorated with simple carved patterns, four turned legs, dark brown wood` |
| 6 | `item.chopping_block` | `iso_item_chopping_block_se.png` | 1x1 | 80x64 | `--n 1` | 20 | `thick round tree stump used as a chopping block, flat cut top with visible rings, bark on the sides, no axe` |
| 7 | `item.market_stall` (2x1, köy) | `iso_item_market_stall_2x1_se_nw.png` | 2x1 | 112x136 | `--n 2 --axis x` | 76 | `simple medieval wooden market stall: plank counter on posts with a sloped cloth awning on top, counter empty, muted faded cloth colour` |

- **`_v2` ekleri:** Raf ve kazanın reddedilen 300 sürümleri `_aday`'da duruyor. Üzerine yazılmaz; yeni dosyalar `_v2` ekiyle adlandırılır. Pivot tablosu en uzun önek eşleşmesini kullandığı için ek, pivotu değiştirmez.
- **Yüzey dekoru kılavuzu (bardak):** 300'deki gibi.
  - Tuval 32x32. Taban tuvalin alt-ortasında, alt kenardan 4 px yukarıda: 12x6 px tam 2:1 elmas, kılavuz rengiyle.
  - Maske: tabanın tablodaki yükseklik kadar yukarı uzatılmışı, 1 px genişletilmiş.
  - 300'de bardak tabana değmedi. Bu yüzden doğrulamada en alt opak satır, taban elmasının alt yarısında olmalı.

### Teslim
- `Assets/Art/Iso/_aday/` altına `cp -n`. `.meta` yazma; var olan dosyanın üzerine yazma.

### Masaüstü kopyası (kullanıcının açık izni, yalnızca bu iş için)
Hiçbir şeyi silme, üzerine yazma. Klasör tarihi üretim günüdür.
- `C:\Users\User\OneDrive\Desktop\art\esya\<yyyy-mm-dd>_<ad>\`, ad örnekleri `shelf_wood`, `chair_carved`, `market_stall`: teslim edilen PNG ve teslim edilmeyen denemeler (`_red\` alt klasöründe).
- `_onizleme_x4.png`: en yakın komşu ile 4 kat, `#333333` zemin.
  - Soldan sağa: köylü (`_aday/iso_char_farmer_a2_56_cand_se.png`), 64x32 çimen karosu (`iso_ground_grass_set_a.png`), kabul edilmiş masa (`_aday/iso_item_table_wood_2x1_se_nw.png`, ölçek ve renk kıyası için), eşya.
  - Hepsi alta hizalı; eşyanın ayak izi elması 1 px açık gri kontur.
  - Bardak ayrıca masanın üst yüzeyine yerleştirilmiş hâliyle.
- Toplu: `C:\Users\User\OneDrive\Desktop\art\esya\<yyyy-mm-dd>_301_toplu_x4.png`.
- `C:\Users\User\OneDrive\Desktop\art\_indeks.md` sonuna her eşya için bir satır: `tarih | eşya | esya/<yyyy-mm-dd>_<ad> | create_map_object kılavuz (referanssız) | <gen> | aday | <ayak izi, boy, renk>`.
- Python'da Pillow yok: `uv run --with pillow ...`. Geçici betikleri repoya ekleme.
- Görselleri araca gönderirken bozulmaması için kayıpsız palet (P kipi) PNG kullan (300'de base64 bozulması iki çağrıyı reddettirdi).

## Doğrulama
- `uv run --with pillow python tools/art/validate_iso.py <dosya> --kind object --max-colors 48 --footprint X,Y,W,H` (her dosya; bardakta `--footprint` yok).
- Her dosya için tablo:
  - piksel boyutu, opak sınır kutusu, ölçülen boy (px), renk sayısı;
  - yarı saydam (0 olmalı);
  - en alt satıra değme (olmamalı);
  - maske dışı opak piksel;
  - kılavuz rengi (#786046) kalıntısı;
  - ayak izi kaplaması (sol/sağ, yüzde).
- Ayak izi ve pivot (ALTTAN, piksel): `footprint_guide` `_info.txt`'teki taban elması kutusu ve pivot. Bardakta pivot = küçük taban elmasının merkezi.
- **Raf:** üst yüzeyin y aralığı (ALTTAN) ve iki yuvanın (u 0,3 / 0,7, v 0,5) üst yüzeye düştüğü, önizlemede işaretli.
- **Palet:** raf ve kazan için kabul edilen masayla renk kıyası.
  - Baskın ahşap renklerinin ortalama ton (hue) ve doygunluğu, masayla yan yana.
  - Kırmızı ya da pembe kayma varsa ⚠.
  - Kazanın içi boş mu?
- **Işık:** üst en açık, sol yüz orta, sağ yüz en koyu (ortalama parlaklık).
- **İstenmeyen ayrıntı:** zemin lekesi, pişmiş gölge, alev, buhar, ekstra nesne (yarma kütüğünde balta), dolu kap.
- **Ayna uygunluğu:** her eşya için sol/sağ yarı parlaklık farkı ve şekil simetrisi. 4 yönden hangisi tek sprite ya da ayna ile çıkar? Aynada ışığın sağ-üstten geldiği not edilir; ayna dosyası Assets'e konmaz.

## Notlar
- 300'den kabul edilenler (kullanıcı, 2026-11-08):
  - masa `iso_item_table_wood_2x1_se_nw.png`
  - sandalye `iso_item_chair_wood_sw.png` (300'de `_se` adıyla teslim edilmişti, gerçek yönü SW; yeniden adlandırıldı, pivot (40,24))
  - mum `iso_item_candle_se.png`
  - saksı `iso_item_flower_pot_se.png`
- Reddedilenler: raf ve kazan (palet ve içi dolu). Teslim edilmeyenler: sandık ve bardak.
- Pivot değerlerini kullanıcı `Assets/Editor/IsoPivots.txt`'e ekler; işçi o dosyaya dokunmaz.
- `items.json`'da olup hâlâ görseli olmayanlar (sonraki kararlar):
  - bitkiler: `item.tree_oak`, `item.crop_turnip` (büyüme evreleri);
  - kaynaklar: `item.firewood`, `item.log`, `item.sapling_oak`, `item.turnip`, `item.seed_turnip` (envanter ikonu).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
