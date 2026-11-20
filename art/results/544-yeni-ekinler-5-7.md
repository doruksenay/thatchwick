unity-import: pending

# 544 Yeni ekinler 5/7: roka, karnabahar, kuşkonmaz, enginar, gül, aynısefa (rapor)

Yan yana sayfası: [544-yanyana.png](544-yanyana.png). Reddedilen: `art/results/544-red/`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object`, stil eşleme, referanssız 1x1 kılavuz + maske (`footprint_guide.py`, tuval 80x72, pivot 40,24; maske yüksekliği 10/22/32), `high top-down`, `low detail`, `basic shading`, `single color outline`, 374'teki açıklama eki. Teslim edilenlere `style.py fit --family bitki` uygulandı (doygunluk x0,37 – x0,97 ile aileye indirildi).
Çağrılar: 18 ilk deneme + 12 ikinci deneme (ilk denemede toprak tepsisi/zemin karosu çıkan 12 görsel için metne "floating alone, no soil, no dirt, no grass, no ground" eklendi) = **30 gen** (cost_max 36). Hız sınırı: rose 3 ilk çağrısında 1 kez ("rate limit exceeded"), 30 sn sonra yeniden gönderildi, gen harcanmadı. `get_balance` sonrası: **2120,7** (paralel şeritler de harcıyor).

## Teslim edilenler (17/18), `Assets/Art/Iso/_aday/`
| Dosya | Boyut | Pivot x / y (alttan) | Opak sınır | Stil denetimi | validate_iso |
|---|---|---|---|---|---|
| iso_crop_artichoke_1_se.png (1. deneme) | 80x72 | 40 / 24 | x19-60, y23-53 | UYARI (palet dışı, kilit yok) | geçti |
| iso_crop_artichoke_2_se.png (1. deneme) | 80x72 | 40 / 24 | x9-70, y14-54 | UYARI (palet dışı, kilit yok) | geçti |
| iso_crop_artichoke_3_se.png (2. deneme) | 80x72 | 40 / 24 | x23-56, y7-51 | UYARI (palet dışı, kilit yok) | geçti |
| iso_crop_asparagus_1_se.png (2. deneme) | 80x72 | 40 / 24 | x32-45, y23-45 | UYARI (palet dışı, kilit yok) | geçti |
| iso_crop_asparagus_2_se.png (2. deneme) | 80x72 | 40 / 24 | x18-61, y10-57 | UYARI (palet dışı, kilit yok) | geçti |
| iso_crop_asparagus_3_se.png (2. deneme) | 80x72 | 40 / 24 | x21-58, y7-51 | UYARI (palet dışı, kilit yok) | geçti |
| iso_crop_calendula_1_se.png (2. deneme) | 80x72 | 40 / 24 | x31-51, y23-52 | UYARI (palet dışı, kilit yok) | geçti |
| iso_crop_calendula_2_se.png (2. deneme) | 80x72 | 40 / 24 | x24-58, y10-49 | UYARI (palet dışı, kilit yok) | geçti |
| iso_crop_cauliflower_1_se.png (1. deneme) | 80x72 | 40 / 24 | x26-54, y24-51 | UYARI (palet dışı, kilit yok) | geçti |
| iso_crop_cauliflower_2_se.png (1. deneme) | 80x72 | 40 / 24 | x11-68, y9-51 | UYARI (palet dışı, kilit yok) | geçti |
| iso_crop_cauliflower_3_se.png (2. deneme) | 80x72 | 40 / 24 | x14-65, y8-54 | UYARI (palet dışı, kilit yok) | geçti |
| iso_crop_rocket_1_se.png (1. deneme) | 80x72 | 40 / 24 | x28-51, y23-47 | UYARI (palet dışı, kilit yok) | geçti |
| iso_crop_rocket_2_se.png (2. deneme) | 80x72 | 40 / 24 | x24-54, y10-53 | UYARI (palet dışı, kilit yok) | geçti |
| iso_crop_rocket_3_se.png (2. deneme) | 80x72 | 40 / 24 | x9-71, y6-55 | UYARI (palet dışı, kilit yok) | geçti |
| iso_crop_rose_1_se.png (2. deneme) | 80x72 | 40 / 24 | x29-49, y21-41 | UYARI (palet dışı, kilit yok) | geçti |
| iso_crop_rose_2_se.png (2. deneme) | 80x72 | 40 / 24 | x20-58, y11-53 | UYARI (palet dışı, kilit yok) | geçti |
| iso_crop_rose_3_se.png (1. deneme) | 80x72 | 40 / 24 | x17-63, y11-56 | UYARI (palet dışı, kilit yok) | geçti |

Yarı saydam piksel 0, gölge pişirilmemiş. Bitki zemin parçası olmadığından döşeme testi yok. 17 dosyanın tümü stil denetiminde UYARI (kilit kurulmamış aile), KALDI yok.

## Teslim edilmeyen (1/18)
`iso_crop_calendula_3_se.png`: ilk denemede toprak tepsisi, ikinci denemede çiçek temiz ama dibinde koyu yeşil elmas yaprak/zemin parçası kaldı. İkisi de `544-red/`de. Yeni iş gerekli (olgun aynısefa).

## Sorunlar (kullanıcı yan yana sayfasına bakmalı)
- Yükseklikler tarifin dışında (model maske yüksekliğine uymadı): filiz evreleri 8-25 px yerine bazıları 20-30 px; asparagus 1 (138 piksel) ve rose 1 (207 piksel) çok küçük; olgun evreler 40-50 px.
- Rocket 2 roka yaprağından çok yonca/üç yapraklı sap gibi (loblu rozet değil); rocket 3 geniş koyu yeşil yapraklı, roka olarak okunması zayıf.
- Asparagus 2 yeşil püsküllü otsu bir küme; asparagus 3 kalın sürgün kümesi okunuyor. Calendula 1 sarı tek çiçekli (fidan değil), calendula 2 yalnızca iki açık çiçekli (tomurcuk değil). Rose 2 koyu kırmızı çiçekli (kapalı tomurcuk + açık çiçek karışık), rose 3 pembe.
- Artichoke 1 beyazımsı çiçek benzeri rozet, artichoke 2 açık mint rozet (gümüşi ama aile paletine göre açık); artichoke 3 iyi okunuyor.
- Stil kilidi bitki ailesinde kurulu değil: palet uyumu yalnızca uyarı.
- Öneri: araçlar yetersiz olduğu için çözüm yok; zemin karosu sorunu için kılavuzsuz üretim ya da `create_image_pro` denenebilir.
