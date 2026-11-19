unity-import: pending

# 419c Ürün ikonları B parça 3/3 (durum: review)

Yan yana sayfası: [419c-yanyana.png](419c-yanyana.png) (arayuz ailesinde kilit yok, yalnızca yeni görseller). Masaüstü kopyası: yerel.

## Araç ve ayarlar
- 17 x `create_map_object` basic, 32x32, `high top-down`, `low detail`, `basic shading`, `single color outline` (401/417/418c yöntemi), açıklama sonuna "simple still-life object, no face, no eyes, no creature, no person". 17 çağrı x 1 gen = **17 gen** (cost_est 17, cost_max 34).
- Hız sınırı (paylaşılan hesap): birkaç çağrı "rate limit exceeded" aldı, harcama yapmadı; bekleyip bir kez yeniden gönderildi (tarifin tek istisnası).
- `get_balance` sonrası: **3870,5** (paralel şeritler dahil).
- Renk: 10 ham çıktı 16'dan fazla opak renk içeriyordu (carp 29, trout 21, flax 31, game_meat 17, poultry 19, rye 27, seed_flax 25, seed_rye 17, seed_wheat 27, wheat 31); Pillow median-cut ile 16'ya (en çok 15) indirildi, dither yok, alfa ikili, yeniden boyama yok. Ham hâller `art/tmp419c/raw` (commit edilmedi).

## Dosyalar (`Assets/Art/Iso/_aday/`, hepsi 32x32 RGBA, pivot x 16, pivot y 0 alttan; ikon)
icon_item_{sapling_willow,meat,poultry,fish_crayfish,fish_carp,fish_trout,fish_pike,fish_eel,game_meat,wheat,seed_wheat,rye,seed_rye,oats,seed_oats,flax,seed_flax}.png: 17 dosya.

## Doğrulama
| Ölçüt | Sonuç |
|---|---|
| Boyut | 32x32 (17/17) |
| Renk | ≤16 opak (17/17), yarı saydam piksel 0 |
| `style.py check --family arayuz` | KALDI yok; 4 GEÇTİ (trout, rye, seed_flax, seed_oats), 13 UYARI (kilit kurulmadı / palet dışı / palete uzak). En uzak: game_meat (%3 palete yakın), wheat (%13), oats (%35), meat (%36) |
| Yüz/göz/figür | Yok (17/17); seed_oats'ta kesenin ağzındaki tohumlar yakından bakınca yüz gibi okunabilir, gözle onayda bakılmalı |
| Teslim edilmeyen | Yok |

## Sorunlar
- Balıklar yandan değil, bazıları hafif eğik; carp ve crayfish iyi okunuyor, pike ve trout (küçük, soluk) birbirine benziyor.
- Aile kuralı: tohum keseleri arasında seed_wheat/seed_rye/seed_flax tohum şekli kesenin üstünde, seed_oats kesenin ağzında; biçim birliği orta.
- oats, rye ve wheat demetleri ayırt edilebiliyor ama oats parlak sarı, wheat ile yakın.
- Palet uyarıları kilit kurulmadığı için yalnızca bilgi.
- `validate_iso.py` uygulanmadı (UI ikonu).
