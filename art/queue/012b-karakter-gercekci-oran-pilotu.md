---
id: "012b"
title: Gerçekçi oranlı dünya karakteri pilotu (012'nin yerine)
status: pending
approved_by: user        # kullanıcı kararı 2026-11-18 (sohbet): 012 sprite'ı fazla chibi, daha uzun ve gerçekçi oran
family: karakter
style_gate: true
cost_est: 10
cost_max: 20
depends_on: []
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_char_man_r72_cand_se.png
  - Assets/Art/Iso/_aday/iso_char_woman_r72_cand_se.png
  - Assets/Art/Iso/_aday/iso_char_man_r84_cand_se.png
  - Assets/Art/Iso/_aday/iso_char_woman_r84_cand_se.png
created: 2026-11-18
---
# 012b Gerçekçi oranlı karakter pilotu

## Amaç
Kullanıcı kararı (2026-11-18): 012'deki 56 px dünya sprite'ı oyuna uymuyor. Kafa büyük, boy kısa, chibi duruyor. İstenen: **daha az chibi, daha uzun, gerçekçi oranlı** karakter. Portre pilotu 020'deki kişilerle aynı dünyadan görünmeli (gerçekçi yüz oranı, toprak tonlu ortaçağ kıyafeti). Bu iş, bütün karakter hattının (yürüme 014-015, katman 016-017, adlı köylüler 314) yeni stil pilotudur. Kullanıcı bir boy ve tasarım seçene kadar türev iş yazılmaz.

## Tarif
- **Araç:** `create_character`, `mode=v3`, `body_type=humanoid`, `view=high top-down`, `outline=single color black outline`, `detail=high detail`. Pilotta yalnızca **SE** yönü yeterli; aracın 8 yön zorunluysa 8 yön üret, raporda SE'yi kullan.
- **İki boy:** `size=72` ve `size=84` (010'da denenen boylar; 56'nın yerine). İki karakter × iki boy = 4 aday.
- **Oran:** her açıklamaya şunu ekle: "realistic adult proportions, about 7 heads tall, small head relative to body, long legs, not chibi, not cute, not big-headed, grounded medieval peasant".
- **Karakterler:**
  1. **man_r:** "adult village man, short brown hair and short beard, muted olive-brown wool tunic with leather belt, linen shirt sleeves, leg wraps, leather shoes" (020'deki sakallı adamın dünya hâli).
  2. **woman_r:** "adult village woman, long auburn hair loosely tied, faded brown-green wool dress with linen apron and laced bodice, leather shoes" (020'deki kızıl saçlı kadının dünya hâli).
- **Ortak ek:** ", earthy muted medieval village palette, desaturated ochre, brown, olive and grey cloth, clean pixel art, crisp black outline, readable face, no dithering, no noise, no cast shadow, transparent background".
- **Referans:** araç stil referansı alıyorsa `art/results/020-onizleme-x4.png` içindeki portreler kıyafet ve renk için referanstır (yüz detayı dünya sprite'ında sadeleşir).
- **Renk azaltma:** `reduce_colors`, karakter başına bir çağrı, `num_colors=32`, `dithering=none`.
- Kayıt `Assets/Art/Iso/_aday/` altına (köke değil), ham çıktı `_aday/_ham/`. Var olan dosyanın üzerine yazma.

## Doğrulama
- Her aday için tablo: gerçek boy (opak bbox), kafa yüksekliği ve **boy / kafa oranı** (hedef 6,5-7,5), genişlik, renk sayısı, yarı saydam piksel.
- `style.py check --family karakter`. Not: karakter ailesinin referansları 012'nin chibi karakterleri olabilir; oran farkı yüzünden UYARI çıkarsa teslim et, raporda yaz. Yalnızca palet nedeniyle KALDI olanı teslim etme.
- **Yan yana sayfası (`012b-yanyana.png`), 4 kat, #333333 zemin, 64x32 elmas karonun üstünde:** eski 56 px köylü (012), 4 aday, ve sağda 020 portrelerinden adam ile kadın. Kullanıcı boy ve tarzı buna bakarak seçecek.
- Ayrıca 008b duvarının (102x115) yanında bir aday: karakter kapı/pencere ölçeğine uyuyor mu.

## Sorular
- 2026-11-18 (bulut şerit 2): `get_balance` planı "Tier 2: Pixel Artisan", 4999/5000 generation, yenilenme 2026-12-18 gösteriyor. `art/budget.md` ise Tier 1, 2000 gen, yenilenme 2026-12-08 diyor. Plan Tier 1 değilse işçi durur; PixelLab çağrısı yapılmadı (0 gen).
- Karar kullanıcıda: `art/budget.md` Plan ve Dönem bölümleri güncellensin (işçi değiştirmez), sonra 012b `pending` yapılıp yeniden çalıştırılsın.

## Notlar
- Kabulden sonra: 013'te seçilen yürüme yöntemi (pixminimax) korunur, 014-015 ve 016-017, 314 yeni karaktere göre yeniden yazılır.
- Boy büyürse duvar yüksekliği ve kapı ölçeği yeniden değerlendirilebilir; bu kararı kullanıcı verir.
- 2026-11-18 (tamir): `art/budget.md` planı Tier 2 olarak güncellendi, ajan kuralı "Tier 1 değilse dur" yerine "budget.md planıyla uyuşmuyorsa dur" oldu; iş yeniden `pending`.
