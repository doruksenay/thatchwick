unity-import: pending

# 543 Yeni ekinler 4/7: patlıcan, bamya, pazı, kereviz, maydanoz (rapor)

Yan yana sayfası: [543-yanyana.png](543-yanyana.png). Reddedilen çıktılar: `art/results/543-red/`. Masaüstü kopyası: yerel.

## Araç ve ayarlar
`create_map_object`, stil eşleme, referanssız 1x1 kılavuz + maske (`footprint_guide.py`, tuval 80x72, pivot 40,24; maske yüksekliği 10/22/32), `high top-down`, `low detail`, `basic shading`, `single color outline` (varsayılan), 374'teki açıklama eki; ilk denemeden itibaren ekine "floating alone, no soil, no dirt, no grass" eklendi (542'de 11/15 zemin karosu çıkmıştı). Teslim edilenlere `style.py fit --family bitki` uygulandı (çıktılar ailesinden çok doygundu; x0,36-0,85 ile aileye indirildi).
Çağrılar: 15 ilk deneme + 6 ikinci deneme (kereviz 1-3 ve maydanoz 1-3 zemin karosu çıkmıştı; metne "isolated plant cutout floating alone in empty transparent space, nothing underneath, no tile, no block" eklendi) = **21 gen** (cost_max 30). Hız sınırı: 1 çağrı (maydanoz 2, 2. deneme) reddedildi, gen harcanmadı, bekleyip bir kez yeniden gönderildi. `get_balance` sonrası: **2134,7** (paralel şeritler de harcıyor).

## Teslim edilenler (14/15), `Assets/Art/Iso/_aday/`
Hepsi 80x72, pivot x 40 / y 24 (alttan), yarı saydam piksel 0, zemin/tepsi yok, gölge pişirilmemiş. Stil denetimi hepsi UYARI (palet dışı, bitki ailesinde kilit kurulu değil); validate_iso hepsi geçti. Bitki zemin parçası olmadığından döşeme testi yok.

| Dosya | Opak yükseklik (hedef) | Renk | Palete yakın | Doygunluk |
|---|---|---|---|---|
| iso_crop_eggplant_1_se.png | 35 (10) | 37 | %65 | x1,16 |
| iso_crop_eggplant_2_se.png | 43 (22) | 60 | %45 | x1,15 |
| iso_crop_eggplant_3_se.png | 45 (32) | 45 | %46 | x1,14 |
| iso_crop_okra_1_se.png | 24 (10) | 35 | %55 | x1,16 |
| iso_crop_okra_2_se.png | 41 (22) | 37 | %34 | x1,15 |
| iso_crop_okra_3_se.png | 42 (32) | 29 | %33 | x1,15 |
| iso_crop_chard_1_se.png | 36 (10) | 50 | %59 | x1,15 |
| iso_crop_chard_2_se.png | 37 (22) | 45 | %55 | x1,15 |
| iso_crop_chard_3_se.png | 44 (32) | 34 | %41 | x1,15 |
| iso_crop_celery_1_se.png (2. deneme) | 28 (10) | 29 | %27 | x1,15 |
| iso_crop_celery_3_se.png (2. deneme) | 52 (32) | 39 | %51 | x1,15 |
| iso_crop_parsley_1_se.png (2. deneme) | 30 (10) | 59 | %54 | x1,15 |
| iso_crop_parsley_2_se.png (2. deneme) | 47 (22) | 19 | %39 | x1,15 |
| iso_crop_parsley_3_se.png (2. deneme) | 52 (32) | 42 | %31 | x1,15 |

## Teslim edilmeyen (1/15)
`iso_crop_celery_2_se`: iki denemede de zemin karosu (ikinci deneme yeşil kürelerle yapraklı tepsi); biçim sorunu, `fit` çözmez. `543-red/iso_crop_celery_2_se_deneme2.png`. Yeni iş gerekli (kereviz 2). Kod, evre gelene kadar renkli şekil çizer.
Ayrıca ilk deneme zemin karosu çıkan 5 görsel (kereviz 1, 3, maydanoz 1-3) ikinci denemede düzeldi; ilk denemeleri `543-red/*_deneme1.png`.

## Sorunlar (kullanıcı yan yana sayfasına bakmalı)
- **Yükseklikler tarifin dışında** (374 ve 542'deki gibi): filiz evreleri hedeften 2-3 kat uzun (24-36 px, hedef 10), olgun evreler 42-52 (hedef 32). Evreler arası büyüme tutarsız okunabilir (ör. pazı 1 = 36, pazı 2 = 37). Ölçek gerekebilir.
- Bamya 3: ince dikey yeşil koni, "ridged pod"lı gövde değil; selvi gibi okunuyor, zayıf. Kereviz 3: kök başı belirgin ama soğan/rezene gibi; yeşil saplar var.
- Patlıcan 1: fide yaprağı yanında küçük mor tomurcuk (filiz evresi için fazla); patlıcan 2: yapraklar arası mor meyveler (çiçek yok). Patlıcan 3 net.
- Maydanoz 1: küçük, saydam arka plan, yaprak kümesi; kereviz 1: tek filiz, düşük detay.
- Stil kilidi bitki ailesinde kurulu değil: palet uyumu yalnızca uyarı.
- Öneri (`tools/art` değişmedi): kılavuzsuz tek bitki üretimi ya da `create_image_pro` ile saydam arka planlı yeniden üretim, yükseklik sorunu için.
