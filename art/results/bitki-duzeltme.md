# Bitki ailesi stil kapısı tamiri ve 0 gen düzeltme adayları (2026-11-18)

## Ne oldu
310 (ekin evreleri, bitki pilotu), 311 ve 322 (toplama düğümleri), 319 (meyve ağaçları) ve 320 (ağaç türleri) işlerinde 48 adaydan yalnızca 2'si (erik olgun, kozalak) teslim edildi; geri kalanı `style.py check --family bitki` denetiminde KALDI. Yaklaşık 80 generation teslimsiz kaldı.

## Neden
- **Palet:** 64 renklik ana palette yalnızca 3 yeşil vardı. Palet tüm referansların piksellerinden çıkarılıyor; büyük zemin ve duvar karoları baskın, bitki referansı yok denecek kadar az (saksı ve çimen karosu). Bitkilerin yeşilleri bu yüzden "palet dışı" sayılıyordu.
- **Başlangıç sorunu:** bitki ailesinde kabul edilmiş bitki görseli yok; kilidi kuracak pilot (310) da kilitsiz bir paletle eleniyordu.
- **Doygunluk** denetimi ise haklıydı: üretilen ekin ve ağaçlar çimenden 1,5-2,6 kat doygun (oyunda neon gibi dururdu).

## Tamir (`tools/art/style.py`)
- **Kilidi kurulmamış aile** (`KILIT_ESIGI` = 3'ten az "kabul" referans): palet uyumu kalma sebebi değil UYARI. Doygunluk denetimi aynen geçerli. Kilit kurulunca (bitki pilotu kabul edilip referanslara eklenince, `style.py palette` ile palet yeniden üretilince) palet denetimi yine sert olur.
- **`style.py fit --family F <girdi> <çıktı>`:** yalnızca doygunluğu aile ortalamasının 1,15 katına indirir; ton, açıklık, saydamlık ve gölgeleme aynı kalır. Palete eşleme denendi ve reddedildi: yeşiller kahverengi-griye dönüyor (çam siyahlaşıyor, bezelye kuru dal oluyor).
- Ajan kuralı: kilitsiz ailede yalnızca "ailesinden çok doygun" yüzünden kalan ve şekli tanınan görsel `fit` ile düzeltilip yeniden denetlenir. Şekil sorunu `fit` ile çözülmez.
- Bilinen yan etki: `fit` bütün görsele uygulanır; meyve ve çiçek renkleri de soluklaşır (en belirgin: armut sarısı).

## Düzeltme adayları (0 generation)
Ham adaylar `art/results/NNN-red/`, düzeltilmiş hâlleri `art/results/NNN-duzeltme/` (aynı dosya adı), önce/sonra sayfası `art/results/NNN-duzeltme-once-sonra.png` (üst satır ham, alt satır düzeltilmiş, sağda çimen referansı). **Hiçbiri Assets'e konmadı ve iş durumları değişmedi:** kabul/red kararı kullanıcının. Raporlardaki şekil sorunları (toprak höyüğü, çim elması, yanlış nesne, ölçek, renk sayısı > 48) düzeltmeyle kalkmaz; tablodaki "Denetim" yalnızca stil kapısıdır.

### 310

| Aday | Önce (doygunluk / palete yakın) | Düzeltme | Sonra | Denetim |
|---|---|---|---|---|
| `iso_crop_cabbage_1_se.png` | x1.00 / %51 | x1.00 | x1.00 / %51 | UYARI |
| `iso_crop_cabbage_2_se.png` | x2.13 / %20 | x0.55 | x1.18 / %21 | UYARI |
| `iso_crop_cabbage_3_se.png` | x2.08 / %69 | x0.55 | x1.15 / %80 | UYARI |
| `iso_crop_leek_2_se.png` | x1.16 / %76 | x1.00 | x1.16 / %76 | UYARI |
| `iso_crop_leek_3_se.png` | x1.42 / %52 | x0.81 | x1.15 / %62 | UYARI |
| `iso_crop_pea_1_se.png` | x1.05 / %59 | x1.00 | x1.05 / %59 | UYARI |
| `iso_crop_pea_2_se.png` | x2.02 / %54 | x0.57 | x1.15 / %65 | UYARI |
| `iso_crop_pea_3_se.png` | x1.89 / %47 | x0.61 | x1.15 / %52 | UYARI |
| `iso_crop_pumpkin_1_se.png` | x2.12 / %26 | x0.52 | x1.11 / %80 | UYARI |
| `iso_crop_pumpkin_2_se.png` | x1.52 / %42 | x0.76 | x1.15 / %64 | UYARI |
| `iso_crop_pumpkin_3_se.png` | x1.73 / %24 | x0.68 | x1.19 / %64 | UYARI |
| `iso_crop_turnip_1_se.png` | x1.46 / %56 | x0.79 | x1.15 / %60 | UYARI |
| `iso_crop_turnip_2_se.png` | x1.69 / %9 | x0.68 | x1.15 / %47 | UYARI |
| `iso_crop_turnip_3_se.png` | x1.39 / %41 | x0.84 | x1.16 / %41 | UYARI |
| `r2_leek_1_ikinci.png` | x1.25 / %48 | x0.92 | x1.15 / %48 | UYARI |
| `raw_leek_1_ilk.png` | x2.06 / %22 | x0.56 | x1.15 / %27 | UYARI |

### 311

| Aday | Önce (doygunluk / palete yakın) | Düzeltme | Sonra | Denetim |
|---|---|---|---|---|
| `berry_raw.png` | x1.54 / %39 | x0.75 | x1.16 / %45 | UYARI |
| `branch_raw.png` | x2.42 / %84 | x0.48 | x1.17 / %87 | GEÇTİ |
| `hazel_raw.png` | x2.57 / %23 | x0.44 | x1.14 / %100 | GEÇTİ |
| `herbs_raw.png` | x2.55 / %3 | x0.45 | x1.15 / %14 | UYARI |
| `hive_raw.png` | x1.64 / %63 | x0.70 | x1.15 / %100 | GEÇTİ |
| `mushroom_raw.png` | x1.74 / %50 | x0.65 | x1.14 / %99 | UYARI |

### 319

| Aday | Önce (doygunluk / palete yakın) | Düzeltme | Sonra | Denetim |
|---|---|---|---|---|
| `iso_tree_apple_fruit_se.png` | x2.00 / %35 | x0.58 | x1.16 / %39 | UYARI |
| `iso_tree_apple_grown_se.png` | x2.40 / %21 | x0.48 | x1.15 / %21 | UYARI |
| `iso_tree_apple_young_se.png` | x1.96 / %36 | x0.59 | x1.15 / %39 | UYARI |
| `iso_tree_cherry_fruit_se.png` | x2.48 / %21 | x0.46 | x1.15 / %58 | UYARI |
| `iso_tree_cherry_grown_se.png` | x1.99 / %39 | x0.58 | x1.15 / %65 | UYARI |
| `iso_tree_cherry_young_se.png` | x1.54 / %55 | x0.75 | x1.15 / %65 | UYARI |
| `iso_tree_pear_fruit_se.png` | x2.57 / %22 | x0.45 | x1.15 / %48 | UYARI |
| `iso_tree_pear_grown_se.png` | x1.77 / %27 | x0.65 | x1.15 / %35 | UYARI |
| `iso_tree_pear_young_se.png` | x1.66 / %16 | x0.69 | x1.15 / %25 | UYARI |
| `iso_tree_plum_fruit_se.png` | x0.63 / %77 | x1.00 | x0.63 / %77 | UYARI |
| `iso_tree_plum_young_se.png` | x1.07 / %79 | x1.00 | x1.07 / %79 | UYARI |

### 320

| Aday | Önce (doygunluk / palete yakın) | Düzeltme | Sonra | Denetim |
|---|---|---|---|---|
| `iso_item_mushroom_bed_se.png` | x1.54 / %94 | x0.75 | x1.15 / %99 | GEÇTİ |
| `iso_tree_birch_grown_se.png` | x2.19 / %21 | x0.53 | x1.16 / %53 | UYARI |
| `iso_tree_birch_young_se.png` | x1.02 / %36 | x1.00 | x1.02 / %36 | UYARI |
| `iso_tree_pine_grown_se.png` | x1.13 / %52 | x1.00 | x1.13 / %52 | UYARI |
| `iso_tree_pine_young_se.png` | x2.58 / %15 | x0.44 | x1.12 / %35 | UYARI |
| `iso_tree_willow_grown_se.png` | x1.29 / %44 | x0.89 | x1.14 / %55 | UYARI |
| `iso_tree_willow_young_se.png` | x0.82 / %95 | x1.00 | x0.82 / %95 | GEÇTİ |
| `iso_tree_willow_young_se_deneme2.png` | x1.13 / %89 | x1.00 | x1.13 / %89 | UYARI |

### 322

| Aday | Önce (doygunluk / palete yakın) | Düzeltme | Sonra | Denetim |
|---|---|---|---|---|
| `chestnut_raw.png` | x2.22 / %13 | x0.52 | x1.15 / %89 | UYARI |
| `cress_raw.png` | x2.36 / %20 | x0.49 | x1.15 / %20 | UYARI |
| `elder_raw.png` | x1.25 / %74 | x0.93 | x1.16 / %79 | UYARI |
| `garlic_raw.png` | x2.55 / %11 | x0.45 | x1.15 / %31 | UYARI |
| `nettle_raw.png` | x1.96 / %82 | x0.58 | x1.13 / %96 | GEÇTİ |
| `rosehip_raw.png` | x2.14 / %16 | x0.54 | x1.14 / %73 | UYARI |
| `strawberry_raw.png` | x1.96 / %35 | x0.59 | x1.15 / %46 | UYARI |
