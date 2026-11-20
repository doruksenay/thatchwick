unity-import: pending

# 489 Böcek defteri ikonları (16)

- Araç: `create_image_pixflux` 32x32, `no_background=true`, `view=high top-down`, tarifteki açıklamalar. 16 çağrı x 1 gen = 16 gen. İlk dalgada 6 çağrı hız sınırı (10/10 iş) yüzünden reddedildi (ücret yok), slot açılınca bir kez yeniden gönderildi.
- `get_balance` sonrası: 2762,9 (paylaşılan havuz, diğer şeritler de harcıyor).
- Yan yana: `art/results/489-yanyana.png`. Masaüstü kopyası: yerel.
- Doygunluk düzeltmesi (`style.py fit --family hayvan`): karınca (x0.67), peygamber devesi (x0.63), kırlangıçkuyruk (x0.66). Ham hâller `art/results/489-red/`.

## Teslim (Assets/Art/Iso/_aday/), 32x32, pivot x ortada (16), pivot y alttan = alt boşluk
| Dosya | Pivot y (alttan) | validate | Stil denetimi |
|---|---|---|---|
| icon_bug_swallowtail.png | 3 | geçti | UYARI (hayvan, fit sonrası; esya KALDI) |
| icon_bug_peacock_moth.png | 7 | geçti | UYARI (palete uzak) |
| icon_bug_dragonfly.png | 3 | geçti | UYARI (hayvan; esya KALDI palet dışı) |
| icon_bug_mantis.png | 1 | geçti | UYARI (hayvan, fit sonrası; palet dışı) |
| icon_bug_cicada.png | 4 | geçti | UYARI (palete uzak) |
| icon_bug_stag_beetle.png | 1 | geçti | GEÇTİ |
| icon_bug_night_moth.png | 5 | geçti | UYARI (soluk) |
| icon_bug_dung_beetle.png | 5 | geçti | UYARI (palete uzak) |
| icon_bug_rhino_beetle.png | 6 | geçti | GEÇTİ (esya); hayvanda çok doygun x1.53 |
| icon_bug_ant.png | 5 | geçti | UYARI (hayvan, fit sonrası) |
| icon_bug_woodworm.png | 9 | geçti | GEÇTİ (esya) |
| icon_bug_snow_fly.png | 6 | geçti | UYARI (soluk, palet dışı; esya KALDI) |
| icon_bug_diving_beetle.png | 2 | geçti | UYARI (hayvan; esya KALDI palet dışı) |
| icon_bug_water_scorpion.png | 3 | geçti | GEÇTİ |
| icon_bug_pond_skater.png | 8 | geçti | UYARI (soluk, ince) |
| icon_bug_silver_water_beetle.png | 2 | geçti | GEÇTİ |

## Sorunlar
- Renkli böcekler esya ailesinde KALDI (kelebek, yusufçuk, dalgıç, kar sineği); tarif gereği hayvan ailesiyle yeniden denetlendi, kilit kurulmadığından uyarı.
- Yusufçuk tam genişlikte (0-32 px), kenara değiyor. Görsel kalite elle incelenmedi; yan yana sayfasına bakılmalı.
- Ikon başına yalnızca tek deneme; tutmayanlar için kullanıcı kararı.
