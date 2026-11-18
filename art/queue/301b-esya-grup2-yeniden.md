---
id: "301b"
title: Ev eşyaları grup 2, yeniden (raf, pazar tezgâhı, kazan, bardak; yalnızca SE)
status: running
claimed_at: 2026-11-18T09:47Z
approved_by: user        # kullanıcı kararı 2026-11-18 (sohbet): 301'deki bu dört eşya kabul değil, baştan yapılsın
priority: 10
family: esya
style_gate: false
cost_est: 6
cost_max: 12
depends_on: ["301"]
max_attempts: 2          # parça başına en çok 2 deneme
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_shelf_wood_2x1_se_nw_v3.png
  - Assets/Art/Iso/_aday/iso_item_market_stall_2x1_se_nw_v2.png
  - Assets/Art/Iso/_aday/iso_item_cooking_pot_se_v2.png
  - Assets/Art/Iso/_aday/iso_item_clay_cup_se.png
created: 2026-11-18
---
# 301b Ev eşyaları grup 2, yeniden

## Amaç
301'de kullanıcının kabul etmediği dört eşyanın yeniden üretimi. 301'in yöntemi, ortak ayarları, ortak eki, kılavuz ve maske kuralları, doğrulama tablosu ve teslim kuralları **aynen geçerlidir** (`art/queue/301-esya-grup2.md` "Tarif" ve "Doğrulama"). Bu dosya yalnızca farkları yazar.

Kullanıcının red nedenleri (2026-11-18):
- **Raf** (`iso_item_shelf_wood_2x1_se_nw_v2.png`): soluk ve koyu (stil denetimi doygunluk x0.38, UYARI), raf değil tezgâh gibi duruyor.
- **Pazar tezgâhı** (`iso_item_market_stall_2x1_se_nw.png`): diğer eşyalardan açık ve turuncumsu, ayrı duruyor.
- **Kazan** ve **bardak**: 301'de iki denemede de teslim edilemedi (kazan tanınmadı, bardak tabana değmedi).

Reddedilen dosyalar `_aday`'da kalır (üzerine yazılmaz); yeni dosyalar aşağıdaki yeni adlarla teslim edilir.

Renk ve ölçek referansı (kabul edilmiş): masa `_aday/iso_item_table_wood_2x1_se_nw.png`, sandalye `_aday/iso_item_chair_wood_sw.png`, tabure `iso_item_stool_se.png`, 301'den kabul edilen sandık `_aday/iso_item_chest_wood_se.png` ve oymalı sandalye `_aday/iso_item_chair_carved_se.png`.

## Tarif
Yöntem, araç (`create_map_object`, stil eşleme, referanssız kılavuz + maske) ve ortak ek 301 ile aynı. PixelLab'de `seed` yok; ikinci deneme aynı kılavuzla yeniden gönderimdir.

| # | items.json id | Dosya (`_aday/`) | Ayak izi | Tuval | Kılavuz | Yükseklik | Açıklama metni (+ 301 ortak eki) |
|---|---|---|---|---|---|---|---|
| 1 | `item.shelf_wood` (2x1, 2 yüzey yuvası) | `iso_item_shelf_wood_2x1_se_nw_v3.png` | 2x1 | 112x96 | `--n 2 --axis x` | 34 | `low open wooden shelf unit seen from the front-left: two clearly visible horizontal shelf boards with dark open gaps between them, thin side panels, flat plank top surface, NOT a table, NOT a workbench, no legs showing under a tabletop, no drawers, warm medium brown wood exactly like the table, clearly readable as a shelf` |
| 2 | `item.market_stall` (2x1, köy) | `iso_item_market_stall_2x1_se_nw_v2.png` | 2x1 | 112x136 | `--n 2 --axis x` | 76 | `simple medieval wooden market stall: plank counter on four posts with a sloped awning of undyed natural linen (pale grey-beige, not orange, not red, not pink), counter empty, same dark brown wood as the table, muted and slightly dark like the other furniture` |
| 3 | `item.cooking_pot` | `iso_item_cooking_pot_se_v2.png` | 1x1 | 80x68 | `--n 1` | 24 | `round-bellied black iron cauldron standing directly on the floor on three short stubby iron legs, wide open round top seen from above showing a dark empty interior, thin rim, two small side handles, no table, no stand, no wooden base, no food, no liquid, no fire, no steam, about the size of a stool` |
| 4 | `item.clay_cup` (yüzey dekoru) | `iso_item_clay_cup_se.png` | yüzey dekoru | 32x32 | 301 dekor kılavuzu | 12 | `small plain brown clay drinking cup standing upright, its round base sitting flat on the floor patch, empty` |

Eşyaya özel kurallar:
- **Raf:** 2 denemede raf olarak okunmuyorsa (ön yüzde iki ayrı raf tahtası ve aralarında boşluk görünmüyorsa) teslim etme.
- **Pazar tezgâhı:** tente kumaşının ortalama tonu 20-50° (bej-gri) aralığında, kırmızı/turuncu piksel %10'dan az olmalı. Tutmazsa ikinci deneme.
- **Kazan:** ahşap sehpa, tepsi ya da tekne çıkarsa tanınmıyor sayılır (301'deki iki hata). İkinci denemede açıklamanın başına `cauldron, ` eklenir.
- **Bardak:** 301'de iki deneme de tabana değmedi. Üretimden sonra bardak tabana değmiyorsa **yalnızca dikey kaydırma** yapılabilir: opak pikseller, en alt opak satır taban elmasının alt yarısına düşecek kadar aşağı taşınır (piksel taşıma; yeniden boyama, ölçekleme yok). Raporda kaç piksel kaydırıldığı yazılır. Kaydırmadan sonra maske dışına taşma olmamalı.

### Teslim
- `Assets/Art/Iso/_aday/` altına `cp -n`. `.meta` yazma; var olan dosyanın üzerine yazma.
- Ham çıktılar `save_png.py` ile alınır; teslim edilmeyen denemeler `art/results/301b-red/`.
- Masaüstü kopyası: bulutta atlanır ("masaüstü kopyası: yerel").

## Doğrulama
301'deki tablonun aynısı (`validate_iso.py --kind object --max-colors 48 --footprint ...`, `style.py check --family esya`, palet ve ışık ölçümleri). Ek ölçütler:
- **Doygunluk:** `style.py check` doygunluk çarpanı raf ve tezgâhta **x0.6 ile x1.3** arasında olmalı. Dışındaysa ikinci deneme; ikinci de dışındaysa teslim edilir ama raporda ⚠ ve kullanıcı karar verir.
- **Masa ile ton kıyası:** raf ve tezgâh ahşabının ortalama tonu masanın ±8° içinde.
- **Raf:** iki raf tahtasının ve üst yüzeyin y aralıkları (ALTTAN) raporda; iki yüzey yuvası (u 0,3 / 0,7, v 0,5) üst yüzeye düşüyor mu.
- **Kazan:** içi boş ve koyu mu; üç ayak görünüyor mu; ahşap yok mu.
- **Bardak:** en alt opak satır taban elmasının alt yarısında mı (kaydırma yapıldıysa kaç piksel).
- Yan yana sayfası: `python tools/art/style.py sheet --family esya --out art/results/301b-yanyana.png <yeni dosyalar>`; ayrıca 301'in reddedilen raf ve tezgâhı ile yenisinin yan yana kıyası `art/results/301b-once-sonra.png` (4 kat, en yakın komşu).

## Notlar
- Pivotlar 301 ile aynı: raf ve tezgâh (56, 32), kazan (40, 24), bardak = küçük taban elmasının merkezi.
- Kullanıcı 301'den sandalye (oymalı), sandık ve yarma kütüğünü kabul etti; 301 `accepted` (bu dört eşya hariç).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
