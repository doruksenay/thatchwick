unity-import: pending

# 483 Deniz balıkları ve kavanoz malzemeleri ikonları (7)

- Araç: `create_image_pixflux` 32x32, `no_background=true`, tarifteki açıklamalar; kavanozlar (5) `seed=483`. 7 çağrı x 1 gen = 7 gen. İlk turda 3 çağrı hız sınırı (10/10 iş) yüzünden reddedildi (ücret yok), iş bitince bir kez yeniden denendi.
- `get_balance` sonrası: 2896,9 (paylaşılan havuz, diğer şeritler de harcıyor).
- Yan yana: `art/results/483-yanyana.png`. Masaüstü kopyası: yerel.
- Doygunluk düzeltmesi (`style.py fit`): ringa (hayvan, x0.76), deniz suyu (zemin, x0.66). Ham hâller `art/results/483-red/`.

## Teslim (Assets/Art/Iso/_aday/), 32x32, pivot x ortada (16), pivot y alttan = alt boşluk
| Dosya | Pivot y (alttan) | validate | Stil denetimi |
|---|---|---|---|
| icon_fish_herring.png | 8 | geçti | UYARI (hayvan ailesi, fit sonrası, palet dışı; esya ailesinde KALDI) |
| icon_fish_mackerel.png | 9 | geçti | UYARI (hayvan; palete uzak, soluk) |
| icon_viv_forest_soil.png | 1 | geçti | UYARI (zemin; palet dışı; esya KALDI) |
| icon_viv_sea_sand.png | 3 | geçti | UYARI (zemin; palet dışı; esya KALDI) |
| icon_viv_marsh_mud.png | 3 | geçti | UYARI (zemin; palete uzak) |
| icon_viv_fresh_water.png | 3 | geçti | UYARI (zemin; doygun x1.41; esya KALDI) |
| icon_viv_sea_water.png | 3 | geçti | UYARI (zemin, fit sonrası; esya KALDI) |

## Sorunlar
- Esya ailesiyle 5 görsel KALDI (renkli içerik); tarifteki kural gereği hayvan/zemin ailesiyle yeniden denetlendi. Hayvan ve zemin kilitleri kurulmadı: palet uyumu yalnızca uyarı.
- Kavanozların aynı biçimde çıkıp çıkmadığına yan yana sayfasından kullanıcı karar versin (tohum aynı, açıklama farklı). Görsel kalite elle incelenmedi.
