---
id: "318t"
title: "Bitki tamamlama: eksik parçalar ve tepeden çizilen 3 ekin (9 sprite)"
status: pending
approved_by: user        # kullanıcı kararı 2026-11-20 (sohbette toplu inceleme: "önerini uygula")
priority: 20
family: bitki
style_gate: true
cost_est: 12
cost_max: 24
depends_on: []
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_crop_carrot_2_se.png
  - Assets/Art/Iso/_aday/iso_crop_carrot_3_se.png
  - Assets/Art/Iso/_aday/iso_crop_cabbage_2_se_v2.png
  - Assets/Art/Iso/_aday/iso_crop_lettuce_3_se_v2.png
  - Assets/Art/Iso/_aday/iso_crop_spinach_2_se_v2.png
  - Assets/Art/Iso/_aday/iso_forage_hazel_se.png
  - Assets/Art/Iso/_aday/iso_forage_berry_bush_v2_se.png
  - Assets/Art/Iso/_aday/iso_edge_trees_c.png
  - Assets/Art/Iso/_aday/iso_edge_undergrowth_2x1_a.png
created: 2026-11-20
---
# 318t Bitki tamamlama

## Neden
Bitki incelemesinde (2026-11-20) 310, 317, 318, 370 ve 378 kabul edildi. Ancak bazı parçalar stil kapısından geçemediği için hiç teslim edilmemişti; üç ekin evresi de yandan-üstten değil tepeden ve yıldız biçiminde çizilmişti. Bu iş yalnızca o parçaları tamamlar.

## Tarif
Her parça kendi kaynak işinin tarifini (ölçü, tuval, kılavuz, açıklama metni, doğrulama) aynen kullanır:

| Parça | Kaynak iş | Not |
|---|---|---|
| `crop_carrot_2`, `crop_carrot_3` | 317 | 317'deki `carrot_1` ve kabul edilen yaban havucu (`parsnip_1-3`) ile aynı kamera ve palet. Dönem kuralı: mor havuç (17. yy öncesi). 3. evrede mor havuç omzu yoğun yeşil tüylü yaprakların arasından görünür. Yaban havucundan ayırt edilmeli. Önceki reddedilen adaylar: `art/results/317-red/`. |
| `crop_cabbage_2_v2` | 310 | Gevşek, açık yeşil geniş yapraklı bir rozet, **yandan-üstten (iso SE) bakış**. Tepeden görülen yıldız ya da yıldız deseni olmamalı. `cabbage_1` ile `cabbage_3` arasında boyu ve doluluğu orta. |
| `crop_lettuce_3_v2` | 318 | Olgun marul başı, yandan-üstten bakışta kabarık, kıvrık kenarlı yapraklar. Tepeden düz bir çiçek ya da diyagram gibi olmamalı. |
| `crop_spinach_2_v2` | 318 | Toprağa yakın, birkaç sap üstünde kaşık biçimli koyu yeşil yapraklar, yandan-üstten bakış. Yıldız biçimi olmamalı. |
| `forage_hazel` | 378 | Ağaçtan küçük, çok gövdeli fındık çalısı. Yaprakları yuvarlak, dişli kenarlı, aralarında yeşil-kahve fındık kümeleri. Önceki adaylar: `art/results/378-red/`, `373-red/`, `311-red/`. |
| `forage_berry_bush_v2` | 378 | Böğürtlen: dikenli, kemerli dallar, üç parçalı yapraklar, siyah-mor ve kırmızı meyveler. Eğrelti değil. |
| `edge_trees_c` | 370 | 370'teki a, b ve d ile aynı aileden harita kenarı öbeği. a (geniş yapraklı) ile b (iğne yapraklı) karışık bir öbek. |
| `edge_undergrowth_2x1_a` | 370 | 2x1 karoluk alçak çalılık şeridi. Öbeklerin önüne dizilir; eğrelti, böğürtlen ve genç fidan karışık, yüksekliği öbeklerin üçte biri. |

- Yeni ekin sprite'ları, kendi bitkisinin kabul edilen öteki evreleriyle aynı taban elmasına ve orta çizgiye hizalı olmalı (animasyonsuz geçiş).
- Yan yana sayfası: `art/results/318t-yanyana.png`. Her ekin parçası kendi 3 evresiyle birlikte gösterilmeli.

## Notlar
