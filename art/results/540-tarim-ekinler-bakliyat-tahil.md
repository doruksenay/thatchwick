unity-import: pending

# 540 Yeni ekinler 1/7: mercimek, nohut, darı, pirinç, haşhaş (rapor)

Yan yana sayfası: [540-yanyana.png](540-yanyana.png) (teslim edilen 9 görsel). Reddedilenler: `art/results/540-red/` (`_deneme1` / `_deneme2`). Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object`, stil eşleme, referanssız 1x1 kılavuz + maske (`footprint_guide.py`, tuval 80x72, pivot 40,24; maske yüksekliği 10/22/32), `high top-down`, `low detail`, `basic shading`, `single color outline`, 374 açıklama eki. Her çıktıya `style.py fit --family bitki`.
- Çağrılar: 15 ilk deneme + 11 ikinci deneme (yalnızca biçim: 11 görselde taban plakası vardı; ek: `floating alone, no soil, no dirt, no base plate, no tile`) = **26 gen** (cost_max 30). Hız sınırı: 4 çağrı reddedildi (ücretlendirilmedi), her biri bekledikten sonra bir kez yeniden gönderildi. `get_balance` sonrası: 2192,7 (paralel şeritler de harcıyor).

## Teslim edilenler (9/15), `Assets/Art/Iso/_aday/`
| Dosya | Boyut | Pivot x / y (alttan) | Opak yükseklik | Deneme | Stil denetimi | validate_iso |
|---|---|---|---|---|---|---|
| iso_crop_chickpea_1_se.png | 80x72 | 40 / 24 | 23 (hedef 10) | 2 | UYARI | geçti |
| iso_crop_chickpea_2_se.png | 80x72 | 40 / 24 | 34 (hedef 22) | 2 | UYARI | geçti |
| iso_crop_chickpea_3_se.png | 80x72 | 40 / 24 | 44 (hedef 32) | 1 | UYARI | geçti |
| iso_crop_lentil_1_se.png | 80x72 | 40 / 24 | 22 (hedef 10) | 1 | UYARI | geçti |
| iso_crop_millet_1_se.png | 80x72 | 40 / 24 | 36 (hedef 10) | 2 | UYARI | geçti |
| iso_crop_millet_2_se.png | 80x72 | 40 / 24 | 50 (hedef 22) | 1 | UYARI | geçti |
| iso_crop_poppy_1_se.png | 80x72 | 40 / 24 | 21 (hedef 10) | 1 | UYARI | geçti |
| iso_crop_poppy_3_se.png | 80x72 | 40 / 24 | 49 (hedef 32) | 2 | UYARI | geçti |
| iso_crop_rice_3_se.png | 80x72 | 40 / 24 | 48 (hedef 32) | 2 | UYARI | geçti |

Hepsi UYARI (palet dışı, bitki ailesinin kilidi kurulmamış); KALDI yok. Yarı saydam piksel 0. Gölge pişirilmemiş. Bitki zemin parçası olmadığından döşeme testi yok.

## Teslim edilmeyenler (6/15): taban plakası ya da yer karosu
lentil_2, lentil_3, millet_3, poppy_2, rice_2 (iki denemede de toprak/çim plakası), rice_1 (ikinci denemede küçük çim karosu gibi). Dosyalar `540-red/`de. Eksik evreler yeni iş gerektirir.

## Sorunlar (kullanıcı yan yana sayfasına bakmalı)
- **Yükseklikler tarif dışı:** model maske yüksekliğine uymadı (374'teki sorunla aynı); evre dizisi tutarsız büyüyebilir, kod tarafı ölçek gerektirebilir.
- millet_1: gri-yeşil dağınık öbek, darıya pek benzemiyor. poppy_3: tek çiçek, tohum kapsülü görünmüyor. Evre 1'ler (lentil_1, poppy_1) çok küçük.
- Pirinçte "sığ su" ifadesi karo üretiyor; 2. denemede çıkarılınca rice_3 temiz çıktı. Eksik evreler için öneri: aynı ek metinle `create_map_object` basic modu (referanssız, saydam arka plan).
