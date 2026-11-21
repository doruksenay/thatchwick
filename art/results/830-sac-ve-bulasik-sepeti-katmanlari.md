unity-import: pending

# 830 Saplı sac (_v3) ve bulaşık sepeti katmanları (rapor)

Yan yana (boş sepet, +kirli, +temiz, sac v2, sac v3): [830-yanyana.png](830-yanyana.png). Stil sayfası: [830-stilsayfasi.png](830-stilsayfasi.png). Masaüstü kopyası: yerel.

## Araç ve ayarlar
- `create_map_object`, `high top-down`, `low detail`, `basic shading`, `single color outline`. 3 çağrı x 1 gen = 3 generation (cost_max 8). `get_balance` sonrası: 9723 (diğer şeritlerin harcaması dahil).
- Sac: temel kip 48x40, kılavuzsuz; tarif metni aynen (+ "isometric 2:1 pixel art, light from top-left, no cast shadow"). Sap çıktı ama alet tava gibi okunuyor.
- Sepet katmanları: arka plan = boş sepet (16 renge indirilmiş; base64 boyutu için; saydamlık korundu, 0 piksel fark), maske = iç ağız elması (merkez 40,33; yarı genişlik 19; yarı yükseklik 9). Çıktı yalnızca yük; opak sınır kutusu (21,24)-(60,43) maske içinde; sepetle aynı 80x72 tuval, (0,0).
- İlk sepet çağrısı base64 kesilmesiyle reddedildi (ücret yok); küçültülmüş arka planla yeniden gönderildi (aktarım hatası, hız sınırı değil).

## Dosyalar (Assets/Art/Iso/_aday/), pivot x, pivot y ALTTAN
| Yol | Piksel | Pivot |
|---|---|---|
| iso_item_griddle_iron_se_v3.png | 48x40 | 24 / 12 |
| iso_item_dish_basket_load_dirty.png | 80x72 (katman) | sepetle aynı |
| iso_item_dish_basket_load_clean.png | 80x72 (katman) | sepetle aynı |

## Doğrulama (sepet için sepet+katman birleşik hâli)
| Dosya | Palete yakın | Uzaklık | Doygunluk | validate_iso | Sonuç |
|---|---|---|---|---|---|
| griddle_v3 | %100 | 5,4 | x0,61 | geçti | GEÇTİ |
| sepet+kirli | %99 | 7,3 | x1,12 | geçti | GEÇTİ |
| sepet+temiz | %100 | 7,4 | x1,14 | geçti | GEÇTİ |

Katmanda sepet pikseli yok (opak piksellerin yalnızca ufak kısmı rastlantıyla sepetle aynı renk).

## Sorunlar (kullanıcı yan yana sayfaya bakmalı)
- ⚠ Kirli katman: yalnızca küçük bir kil kap okunuyor; tabak/kase yığını yok.
- ⚠ Temiz katman: tabak yığını ve keten bez okunmuyor; taban düz kahverengi dalga. `rejected` adayı.
- ⚠ Sac: sap kısa, yuvarlak tava gibi; v2'den iyi ama "sac" yerine "tava" okunabilir.
- Öneri: katman yerine `inpaint_image_pro_flash` (5 gen/çağrı; cost_max 8'i aşar) ile ayrı iş, ya da maskeyi daha geniş tutmak.
