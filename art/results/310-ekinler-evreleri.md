unity-import: pending

# 310 Ekin büyüme evreleri (rapor, 2026-11-20 yeniden üretim)

Sonuç: **14/15 teslim** (`Assets/Art/Iso/_aday/iso_crop_*_se_v2.png`). Aynı adlı eski dosyalar (371 doygunluk kurtarma, `.meta`'lı) üzerine yazılmadığı için yeni dosyalar `_v2` ekiyle teslim edildi; pivot önek eşleşmesiyle değişmez. Teslim edilmeyen: **bezelye 3** (iki denemede de tanınmadı: ilkinde ağaç gibi, ikincide ağaç gövdesi + toprak karosu; `art/results/310-red2/`). Yan yana sayfası: [310-yanyana.png](310-yanyana.png) (teslim edilenler, fit sonrası). Ham çıktılar `art/results/310-ham/`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object`, stil eşleme, kılavuz + maske (`footprint_guide.py`, referanssız), `high top-down`, `low detail`, `basic shading`, `single color outline`, tarifteki ortak ek ("muted green and earthy colours", "no soil"). Tuval 80x72 (bezelye 3: 80x80, yükseklik 40); pivot x=40, y(alttan)=24. Gerekçe: 1x1 elması 64 px + kenar boşluğu.
- 15 ilk çağrı + 4 ikinci deneme (pırasa 1, pırasa 2, bezelye 2, bezelye 3; ikincide açıklamaya "no grass tile / no dirt" gibi kısıtlar eklendi). Bezelye 2 ilk gönderimde hız sınırına takıldı, 40 sn sonra bir kez yeniden gönderildi.
- **Maliyet:** `get_balance` 2920,90 → 2891,90 = **29 generation** (çağrı sayısı 19). `cost_est` 15, `cost_max` 24: **cost_max aşıldı** (2x sınırı olan 30'u aşmadı). Kullanıcı bilsin.
- Doygunluk: ham çıktılar çok doygundu (x1,4-2,7); `style.py fit` ile aileye indirildi (çarpan x0,42-0,83), hepsi UYARI ya da GEÇTİ'ye düştü. Bitki ailesinin kilidi kurulmadı (referans < 3), palet uyumu uyarıdır.

## Dosyalar (pivot x=40, y alttan=24)
| Dosya (_aday/) | Boyut | Opak sınır kutusu | Renk | Not |
|---|---|---|---|---|
| iso_crop_turnip_1_se_v2 | 80x72 | x30-50, y23-50 | 22 | küçük toprak höyüğü çıktı ("no soil" tutmadı) ⚠ |
| iso_crop_turnip_2_se_v2 | 80x72 | x14-65, y10-60 | 51 | renk >48 (yalnız rapor) |
| iso_crop_turnip_3_se_v2 | 80x72 | x22-61, y8-54 | 46 | |
| iso_crop_cabbage_1_se_v2 | 80x72 | x24-59, y24-39 | 42 | iki yaprak, küçük |
| iso_crop_cabbage_2_se_v2 | 80x72 | x8-71, y9-55 | 31 | |
| iso_crop_cabbage_3_se_v2 | 80x72 | x12-67, y11-61 | 17 | |
| iso_crop_leek_1_se_v2 | 80x72 | x28-52, y22-48 | 13 | ot tutamı (2. deneme; 1. deneme çim karosu) |
| iso_crop_leek_2_se_v2 | 80x72 | x17-62, y9-64 | 45 | 2. deneme |
| iso_crop_leek_3_se_v2 | 80x72 | x9-71, y8-55 | 44 | |
| iso_crop_pea_1_se_v2 | 80x72 | x30-52, y23-51 | 43 | çok küçük, kırmızı uç ⚠ |
| iso_crop_pea_2_se_v2 | 80x72 | x13-66, y10-62 | 39 | 2. deneme; sopalar iri, çapraz ⚠ |
| iso_crop_pumpkin_1_se_v2 | 80x72 | x18-62, y23-47 | 35 | |
| iso_crop_pumpkin_2_se_v2 | 80x72 | x10-65, y14-60 | 26 | |
| iso_crop_pumpkin_3_se_v2 | 80x72 | x8-71, y10-54 | 37 | şişe kabağı |

## Doğrulama
`validate_iso.py --kind object --max-colors 48`: 13 geçti, turnip_2 renk sayısı 51 (tarifte "fazlaysa yalnız raporla"). Yarı saydam 0, hiçbirinde alt satıra değme yok, alt boşluk 7-32 satır.
`style.py check --family bitki` (fit sonrası): turnip_1 GEÇTİ, diğer 13 UYARI (palet dışı, kilit yok; doygunluk x1,14-1,20).
Evre boyu sırası: turnip, cabbage, leek, pumpkin için 1<2<3 doğru; bezelye 3 yok. Ayrıntılı taban-hizası ölçümü, olgun evre ürün ikonu (316) kıyası ve döşeme testi (zemin değil) yapılmadı.

## Sorunlar
- Bezelye 3 eksik (tanınan çıktı yok). Öneri: ayrı iş, sopa+çit kılavuzuyla. Araçta seed yok.
- Çağrı başına gerçek maliyet ~1,5 (cost_est 1'in üstü).
- Pırasa 1 ve bezelye 1 küçük/okunaksız olabilir; karar kullanıcıda.
