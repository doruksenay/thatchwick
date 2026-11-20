unity-import: pending

# 495 Easter egg dünya nesneleri ve ürün ikonları (rapor, durum: review)

Yan yana sayfası: [495-yanyana.png](495-yanyana.png). Teslim edilmeyenler: `art/results/495-red/`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object` basic mod (kılavuz/maske kullanılmadı, çağrı başı 1 gen), `low detail`, `basic shading`, `single color outline`; dünya nesneleri `high top-down`, duvar eşyası ve taç `side`, ikonlar `high top-down`. Toplam 12 gen (cost_est 12, cost_max 24). İlk turda 5 ikon çağrısı hız sınırına takıldı (ücret yok), bekleyip bir kez yeniden gönderildi. `get_balance` sonrası: 2356,9 (paralel şeritler dahil).
Sonradan işlem: opaklık 0/255; ikonlar 16, taç 8 renge median-cut ile indirildi (dither yok); taç 32x32 üretimden 16x16'ya NEAREST ile küçültüldü (araç min. 32).

## Dosyalar (`Assets/Art/Iso/_aday/`)
| Yol | Piksel | Pivot x | Pivot y (alttan) | Renk |
|---|---|---|---|---|
| iso_item_rusty_sword_ground_se.png | 64x64 | 32 | 16 | 15 |
| iso_item_stone_tablet_se.png | 64x80 | 32 | 16 | 24 |
| iso_item_sword_in_stone_se.png | 64x80 | 32 | 16 | 19 |
| iso_wall_item_old_sword_sw_ne.png | 48x48 | 24 | 24 | 17 |
| iso_fx_tiny_crown.png | 16x16 | 8 | 0 | 7 |
| icon_item_message_bottle.png | 32x32 | - | - | 15 |
| icon_item_old_boot.png | 32x32 | - | - | 13 |
| icon_item_sword_ploughshare.png | 32x32 | - | - | 9 |
| icon_item_shiny_trinket.png | 32x32 | - | - | 15 |
| icon_item_old_sword.png | 32x32 | - | - | 15 |

## TESLİM EDİLMEDİ (stil KALDI: palet dışı renkler)
- `iso_item_helmet_nest_se` (palete yakın %63): miğfer fıçıya benziyor; yuva ve yumurta var.
- `iso_item_broken_shield_ground_se` (%78): kırmızı-bej şeritli kalkan, tanınıyor ama palet dışı. `fit` yalnızca doygunluk sorunu içindir; uygulanmadı.
Ham hâller `495-red/` altında. Yeniden üretim 2 gen.

## Doğrulama
| Dosya | validate_iso | Stil (aile) | Sonuç |
|---|---|---|---|
| rusty_sword_ground | geçti, yarı saydam 0, alta boşluk 4 | esya: %100, uzaklık 10,5, x0,36 | UYARI (soluk) |
| stone_tablet | geçti, alta boşluk 8 | esya: %100, 12,3, x0,34 | UYARI (soluk) |
| sword_in_stone | geçti | esya: %95, 14,2, x0,31 | UYARI |
| wall_item_old_sword | geçti | esya: %100, 4,5, x0,63 | GEÇTİ |
| icon bottle / old_sword / trinket / ploughshare | 32x32, ≤16 renk, yarı saydam 0 | arayuz (kilit yok) | UYARI |
| icon old_boot | aynı | arayuz | GEÇTİ |
| tiny_crown | 16x16, 7 renk, yarı saydam 0 | - | geçti; kurbağa önizlemesi yok (408 teslim edilmemiş), taç tek başına |

Zemin döşeme testi gerekmedi (zemin yok).

## Sorunlar
1. `sword_ploughshare` ikonu saban demirinden çok kılıca benziyor; kullanıcı gözüyle bakmalı.
2. `wall_item_old_sword` dikey asılı (tarif yatay istiyordu), 14 piksel genişlikte; yeniden üretim önerilir.
3. `shiny_trinket` 10x10 ile çok küçük; düğme/boncuk ayrımı okunmuyor.
4. Taş tabletde oyuk çizgiler yok, düz levha (okunur yazı yok, şart sağlandı).
5. Dünya nesneleri soluk (x0,3); kılıç yarı gömülü değil, dik duruyor.
6. 4 ve 5 numaranın maden zeminiyle (430) renk uyumu denetlenmedi.
