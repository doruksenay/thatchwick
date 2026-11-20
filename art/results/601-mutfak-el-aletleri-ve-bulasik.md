unity-import: pending

# 601 Mutfak, el aletleri ve bulaşık (rapor)

Yan yana sayfası: [601-yanyana.png](601-yanyana.png). Teslim edilmeyenler: `art/results/601-red/`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object`, stil eşleme, referanssız kılavuz + maske (1 bit palet PNG; `footprint_guide.py` ve masa üstü için 12x6 elmaslı özel betik, `art/tmp601/`), `high top-down`, `low detail`, `basic shading`, `single color outline`; tarife "isometric 2:1 pixel art, light from top-left, no cast shadow" eki. Masa üstü: 40x40 tuval; sepet/leğen 80x72 (yükseklik 36); kürek 80x96 (64); yıkama taşı 80x48 (8). 11 çağrı x 1 gen = 11 generation (cost_max 22 içinde). Kürek ilk çağrısı base64 aktarım hatasıyla ücretsiz reddedildi, yeniden kodlanıp gönderildi. Seed yok; ikinci deneme yapılmadı.
`get_balance` iş sonu: 1801,7 (paralel şeritler de harcıyor; bu işin payı 11).

## Sonuç özeti
| # | Eşya | Durum | Not |
|---|---|---|---|
| 1 | kitchen_knife | teslim (UYARI) | soluk (x0,10); dikey duran küçük bıçak, düz yatmıyor |
| 2 | cutting_board | teslim (UYARI) | doygun x1,39; dikdörtgen tahta, delik seçilmiyor |
| 3 | wooden_spoon | teslim | 6x27 px; tanınıyor |
| 4 | rolling_board | teslim (UYARI) | doygun x1,50; oklava seçilmiyor, turuncu dikdörtgen; ⚠ kullanıcı bakmalı |
| 5 | colander | teslim | bakır kase, delik/kulp ayrıntısı yok; ⚠ |
| 6 | baker_peel | teslim (`fit` ile) | ham: palet dışı %97, x2,35 KALDI; `fit` sonrası x1,15 GEÇTİ. Kürek tanınıyor; duvara yaslı değil |
| 7 | dish_basket_empty | teslim | örgü sepet, iki kulp |
| 8 | dish_basket_dirty | TESLİM EDİLMEDİ | sepet değil ahşap sandık, yığın yok; boş sepetle aynı nesne değil |
| 9 | dish_basket_clean | TESLİM EDİLMEDİ | ahşap sandık, bez ve tabaklar; boş sepetten farklı nesne |
| 10 | dish_tub | teslim | iki bölmeli gri sulu leğen, saman kenarda; ⚠ tek bölmeli değil |
| 11 | washing_stone | TESLİM EDİLMEDİ | ayak izi mavi (su gibi), taş yok |

## Dosyalar (Assets/Art/Iso/_aday/)
| Yol | Piksel | Pivot x | Pivot y (alttan) |
|---|---|---|---|
| iso_item_kitchen_knife_se.png | 40x40 | 20 | 7 |
| iso_item_cutting_board_se.png | 40x40 | 20 | 7 |
| iso_item_wooden_spoon_se.png | 40x40 | 20 | 7 |
| iso_item_rolling_board_se.png | 40x40 | 20 | 7 |
| iso_item_colander_se.png | 40x40 | 20 | 7 |
| iso_item_baker_peel_se.png | 80x96 | 40 | 24 |
| iso_item_dish_basket_empty_se.png | 80x72 | 40 | 24 |
| iso_item_dish_tub_se.png | 80x72 | 40 | 24 |

## Doğrulama (validate_iso --kind object --max-colors 48; hepsi geçti)
| Dosya | Opak sınır kutusu | Renk | Yarı saydam | Alta değme |
|---|---|---|---|---|
| knife | x16-24, y9-31 | 11 | 0 | yok |
| cutting_board | x14-25, y9-35 | 9 | 0 | yok |
| spoon | x17-22, y7-33 | 12 | 0 | yok |
| rolling_board | x14-25, y10-29 | 11 | 0 | yok |
| colander | x14-25, y15-25 | 10 | 0 | yok |
| baker_peel | x26-53, y9-86 | 20 | 0 | yok |
| basket_empty | x8-71, y7-64 | 29 | 0 | yok |
| dish_tub | x10-68, y7-61 | 34 | 0 | yok |

## Stil denetimi (`style.py check --family esya`)
| Dosya | Palete yakın | Ort. uzaklık | Doygunluk | Sonuç |
|---|---|---|---|---|
| knife | %100 | 4,7 | x0,10 | UYARI |
| cutting_board | %100 | 11,0 | x1,39 | UYARI |
| spoon | %100 | 6,5 | x0,95 | GEÇTİ |
| rolling_board | %98 | 8,5 | x1,50 | UYARI |
| colander | %96 | 8,4 | x1,06 | GEÇTİ |
| baker_peel (fit) | %100 | 4,1 | x1,15 | GEÇTİ |
| basket_empty | %100 | 6,9 | x1,09 | GEÇTİ |
| dish_tub | %97 | 7,7 | x0,81 | GEÇTİ |
| dish_basket_dirty (red) | %81 | 10,3 | x0,93 | UYARI |
| dish_basket_clean (red) | %97 | 8,7 | x1,13 | GEÇTİ |
| washing_stone (red) | %99 | 12,1 | x0,71 | UYARI |

Zemin değil: döşeme testi yok. Gölge pişirilmemiş. Işık yönü ölçülmedi.

## Sorunlar
- Bulaşık sepetinin üç hâli tutarlı çıkmadı (üç ayrı çağrı). Kirli ve temiz hâl için boş sepeti kaynak alan bir düzenleme işi gerekir (kod oturumu karar verir); şu an yalnız boş sepet teslim.
- Yıkama taşı su gibi mavi çıktı; yeniden denemek için "grey stone slab" vurgulu yeni iş önerilir.
- Masa üstü eşyalar ince ve dikey (6-12 px genişlik); `copper_pot`/`ladle` ile ölçü uyumunu kullanıcı yan yana sayfasında değerlendirmeli.
