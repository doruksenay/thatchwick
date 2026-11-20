unity-import: pending

# 542 Yeni ekinler 3/7: börülce, karabuğday, susam, aspir, hardal (rapor)

Yan yana sayfası: [542-yanyana.png](542-yanyana.png). Reddedilen ikinci denemeler (zemin karosu): `art/results/542-red/`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object`, stil eşleme, referanssız 1x1 kılavuz + maske (`footprint_guide.py`, tuval 80x72, pivot 40,24; maske yüksekliği 10/22/32, börülce 3 için 40), `high top-down`, `low detail`, `basic shading`, `single color outline`, 374'teki açıklama eki. Teslim edilenlere `style.py fit --family bitki` uygulandı (doygunluk x0,39 – x0,93 ile aileye indirildi: PixelLab çıktısı ailesinden çok doygundu).
Çağrılar: 15 ilk deneme + 11 ikinci deneme (ilk denemede zemin karosu/toprak tepsi çıkan 11 görsel için metne "floating alone, no soil, no dirt, no grass, no ground" eklendi) = **26 gen** (cost_max 30). Hız sınırı hatası 4 kez (ilk turda 2, ikinci turda 2): ilgili çağrılar bekleyip bir kez yeniden gönderildi, gen harcanmadı. `get_balance` sonrası: **2200,7** (paralel şeritler de harcıyor).

## Teslim edilenler (8/15), `Assets/Art/Iso/_aday/`
| Dosya | Boyut | Pivot x / y (alttan) | Opak sınır (yükseklik) | Renk | Doygunluk (fit sonrası) | Stil denetimi | validate_iso |
|---|---|---|---|---|---|---|---|
| iso_crop_cowpea_1_se.png | 80x72 | 40 / 24 | x17-56, y23-61 (38) | 19 | x1,15 | UYARI (palet dışı, kilit yok) | geçti |
| iso_crop_buckwheat_2_se.png (2. deneme) | 80x72 | 40 / 24 | x26-53, y9-48 (40) | 23 | x1,16 | UYARI | geçti |
| iso_crop_sesame_2_se.png | 80x72 | 40 / 24 | x24-59, y9-54 (46) | 27 | x1,15 | UYARI | geçti |
| iso_crop_sesame_3_se.png (2. deneme) | 80x72 | 40 / 24 | x24-55, y8-57 (50) | 47 | x1,15 | UYARI | geçti |
| iso_crop_safflower_1_se.png (2. deneme) | 80x72 | 40 / 24 | x32-45, y24-43 (20) | 17 | x1,15 | UYARI | geçti |
| iso_crop_safflower_2_se.png | 80x72 | 40 / 24 | x18-62, y10-54 (45) | 43 | x1,15 | UYARI | geçti |
| iso_crop_mustard_1_se.png (2. deneme) | 80x72 | 40 / 24 | x20-59, y22-45 (24) | 27 | x1,15 | UYARI | geçti |
| iso_crop_mustard_2_se.png | 80x72 | 40 / 24 | x29-51, y9-52 (44) | 47 | x1,11 | UYARI | geçti |

Yarı saydam piksel 0, zemin/tepsi yok, gölge pişirilmemiş. Bitki zemin parçası olmadığından döşeme testi yok.

## Teslim edilmeyenler (7/15): ikinci denemede de zemin karosu / toprak tepsi
Biçim sorunu (bitki bir zemin karosunun üstünde); `fit` ile çözülmez. `art/results/542-red/` içinde (ikinci deneme çıktıları):
`iso_crop_cowpea_2_se`, `iso_crop_cowpea_3_se`, `iso_crop_buckwheat_1_se`, `iso_crop_buckwheat_3_se`, `iso_crop_sesame_1_se`, `iso_crop_safflower_3_se`, `iso_crop_mustard_3_se`.
Bunlar için yeni iş gerekli (börülce 2-3, karabuğday 1 ve 3, susam 1, aspir 3, hardal 3). Öneri: 1x1 kılavuz yerine kılavuzsuz tek bitki üretimi ya da `create_image_pro` ile saydam arka planlı üretim; eksik evreler dolana kadar kod renkli şekil çizer.

## Sorunlar (kullanıcı yan yana sayfasına bakmalı)
- **Yükseklikler tarifin dışında** (374'teki gibi, model maske yüksekliğine uymadı): börülce 1 = 38 (hedef 10), karabuğday 2 = 40 (22), safflower 1 = 20 (10), mustard 1 = 24 (10); olgun evreler 44-50 (hedef 32). Ölçek gerekebilir.
- Börülce 1: sırık küçük, bitki yaprağı büyük; sırık okunuyor ama "filiz" gibi değil. Susam 2 ve aspir 2 / hardal 2 tanınıyor ama aspir 2 çiçekli (turuncu), "kapalı tomurcuk" değil. Susam 3: dikey kapsül kümesi (gövdesiz, yaprak az). Hardal 1: dokuz küçük fidenin ızgarası (yalın, zeminsiz). Aspir 1 çok küçük (81 piksel).
- Karabuğday 2 yeşil gövdeli; "kırmızımsı" ayrıntı belirgin değil.
- Stil kilidi bitki ailesinde kurulu değil: palet uyumu yalnızca uyarı.
