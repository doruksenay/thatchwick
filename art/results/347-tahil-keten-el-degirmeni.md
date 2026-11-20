unity-import: pending

# 347 Buğday, çavdar, yulaf, keten ekinleri ve el değirmeni (rapor, 2026-11-20)

Sonuç: **13/13 teslim** (`Assets/Art/Iso/_aday/`). Yan yana: [347-yanyana.png](347-yanyana.png) (ekinler), [347-quern-yanyana.png](347-quern-yanyana.png) (el değirmeni). Ham çıktılar `347-ham/`, reddedilen ilk denemeler `347-red/` (buğday 1: toprak karosu, keten 2: ağaç gibi yuvarlak tepeler). Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object`, stil eşleme, referanssız kılavuz+maske (`footprint_guide.py --w 80 --h 72 --n 1`, yükseklik 10/22/32; değirmen 80x80, yükseklik 28), `high top-down`, `low detail`, `basic shading`, `single color outline`. Açıklamalar tariften + "muted green and earthy colours, no soil, no ground, no shadow". 13 ilk çağrı (keten 3 hız sınırına takıldı, 30 sn sonra bir kez yeniden gönderildi) + 2 ikinci deneme (buğday 1, keten 2; "not a tile / no trees" eklendi). Toplam 15 generation; `get_balance` 2838,9 → 2812,9 (paralel şeritler de harcıyor, kesin pay ölçülemedi). `cost_max` 20 aşılmadı.
- Doygunluk: ham ekinler x1,4-3,2 (KALDI); `style.py fit --family bitki` ile x0,36-0,83 çarpanıyla aileye indirildi. Teslim edilenler fit sonrası hâlleri. Değirmen ham teslim.

## Dosyalar (pivot x=40; pivot y alttan = 24)
| Dosya | Boyut | Opak kutu | Renk | Not |
|---|---|---|---|---|
| iso_crop_wheat_1_se | 80x72 | x29-50, y25-43 | 14 | 2. deneme; küçük tutam, tabanda küçük toprak izi |
| iso_crop_wheat_2_se | 80x72 | x8-71, y10-57 | 45 | |
| iso_crop_wheat_3_se | 80x72 | x12-67, y7-64 | 49 | renk >48; tabanda çim karosu ⚠ |
| iso_crop_rye_1_se | 80x72 | x20-59, y25-56 | 33 | |
| iso_crop_rye_2_se | 80x72 | x9-70, y10-60 | 37 | |
| iso_crop_rye_3_se | 80x72 | x16-63, y6-64 | 43 | tabanda elmas çizgi ⚠ |
| iso_crop_oats_1_se | 80x72 | x24-54, y25-55 | 57 | renk >48 |
| iso_crop_oats_2_se | 80x72 | x16-63, y11-57 | 47 | |
| iso_crop_oats_3_se | 80x72 | x18-63, y13-59 | 55 | renk >48 |
| iso_crop_flax_1_se | 80x72 | x24-55, y30-59 | 57 | renk >48; küçük |
| iso_crop_flax_2_se | 80x72 | x17-62, y12-59 | 33 | 2. deneme |
| iso_crop_flax_3_se | 80x72 | x19-58, y8-61 | 23 | mavi çiçek okunuyor |
| iso_item_quern_se | 80x80 | x12-67, y11-69 | 25 | pivot y=24 |

## Doğrulama
`validate_iso.py --kind object --max-colors 48`: yarı saydam 0, alt satıra değme yok. Renk >48: wheat_3 (49), oats_1 (57), oats_3 (55), flax_1 (57) (yalnızca raporlandı). Stil denetimi (`bitki`, kilit yok): 12 UYARI, 0 KALDI (fit sonrası; doygunluk x1,14-1,16, palete yakınlık %29-100). Değirmen (`esya`): UYARI, ailesinden soluk (x0,54), palete yakın %100. Evre boyu 1<2<3 her bitkide sağlanıyor; yükseklikler tarifin (10/22/32) üstünde (318'deki gibi araç yüksekliği maskeden okumuyor). Silüet: buğday dolgun altın başak, çavdar en uzun/ince, yulaf sarkık salkım, keten mavi çiçek; arpa (318) ile aynı karoda yan yana test yapılmadı.

## Sorunlar
- wheat_3 tabanında çim karosu, rye_3 tabanında elmas çizgi var (toprak çizilmez kuralı gevşek); karar kullanıcıda.
- wheat_1 ve flax_1 küçük/soluk.
- Araçta seed yok; ikinci denemeler açıklama değişikliğiyle yapıldı.
