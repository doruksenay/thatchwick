unity-import: pending

# 318 Yapraklı ve tahıl ekinleri evreleri (rapor, 2026-11-20)

Sonuç: **15/15 teslim** (`Assets/Art/Iso/_aday/iso_crop_{radish,lettuce,broad_bean,barley,spinach}_{1,2,3}_se.png`). Yan yana: [318-yanyana.png](318-yanyana.png). Ham çıktılar `318-ham/`, reddedilen ilk denemeler `318-red/` (marul 3: cam disk, arpa 1: çim+toprak karosu, bakla 1: yaprak lekesi). Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object`, stil eşleme, referanssız kılavuz+maske (`footprint_guide.py --w 80 --h 72 --n 1`, yükseklik 10/22/32), `high top-down`, `low detail`, `basic shading`, `single color outline`, tarifteki açıklamalar + ortak ek ("muted green and earthy colours", "no soil"). Tuval 80x72, pivot x=40, y(alttan)=24. 15 ilk çağrı (5'i hız sınırına takıldı: bekleyip bir kez yeniden gönderildi) + 3 ikinci deneme (marul 3, arpa 1, bakla 1; açıklamaya "not a disc / no grass tile / not a blob" eklendi).
- **Maliyet:** `get_balance` 2891,90 → 2838,90 (53), ama aynı anda başka şeritler de harcıyordu; bu işin payı 18 çağrı (310'daki ~1,5/çağrı ile ~27). Kesin ölçülemedi; `cost_max` 24 aşılmış olabilir (2x sınırı 30'un altında).
- Doygunluk: ham çıktılar x2,0-3,2 (KALDI); `style.py fit --family bitki` ile x0,36-0,92 çarpanıyla aileye indirildi, hepsi UYARI (kilit yok, palet uyarı). Teslim edilenler fit sonrası hâlleri.

## Dosyalar (pivot x=40, y alttan=24)
| Dosya | Boyut | Opak kutu | Renk | Not |
|---|---|---|---|---|
| radish_1 | 80x72 | x21-58, y25-60 | 32 | |
| radish_2 | 80x72 | x14-70, y10-59 | 57 | renk >48 (yalnız rapor) |
| radish_3 | 80x72 | x9-69, y7-51 | 35 | solda beyaz kök çizgisi |
| lettuce_1 | 80x72 | x27-47, y21-48 | 19 | |
| lettuce_2 | 80x72 | x11-68, y11-60 | 47 | |
| lettuce_3 | 80x72 | x8-71, y0-64 | 37 | 2. deneme; 64 px geniş, üst satıra değiyor ⚠ |
| broad_bean_1 | 80x72 | x28-55, y22-50 | 21 | 2. deneme |
| broad_bean_2 | 80x72 | x28-54, y9-64 | 37 | tabanda küçük toprak izi ⚠ |
| broad_bean_3 | 80x72 | x19-60, y6-64 | 49 | renk >48 |
| barley_1 | 80x72 | x32-52, y21-43 | 7 | 2. deneme; çok küçük ve soluk ⚠ |
| barley_2 | 80x72 | x22-53, y10-61 | 41 | |
| barley_3 | 80x72 | x13-66, y6-61 | 57 | renk >48 |
| spinach_1 | 80x72 | x25-47, y26-47 | 21 | |
| spinach_2 | 80x72 | x11-68, y9-64 | 57 | renk >48 |
| spinach_3 | 80x72 | x11-69, y4-64 | 46 | |

## Doğrulama
`validate_iso.py --kind object --max-colors 48`: yarı saydam 0, alt satıra değme yok (alt boşluk 7-28). Renk >48: radish_2, barley_3, broad_bean_3, spinach_2 (tarif: yalnız raporla). `style.py check --family bitki`: 15 UYARI (kilit yok). Evre boyu sırası her bitkide 1<2<3 ama yükseklikler tarifin (10/22/32) üstünde (22-65 px): çoğu ekin 1 karoyu dikeyde aşıyor. 15 ekinin aynı karoda siluet testi ve ayrıntılı taban-hizası ölçümü yapılmadı.

## Sorunlar
- Boyutlar tarifin üstünde (araç yükseklik değerini maskeden değil açıklamadan okuyor gibi); karar kullanıcıda.
- barley_1 ve lettuce_1 küçük/okunaksız olabilir; bakla 2'de küçük toprak izi var.
- Araçta seed yok; ikinci denemeler açıklama değişikliğiyle yapıldı.
