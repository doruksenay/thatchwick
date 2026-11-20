---
id: "578"
title: "Servis kabı pilotu: çömlekte güveç, sinide pilav, beş doluluk (stil kapısı) (10)"
status: review
claimed_at: 2026-11-20T14:55Z
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 ("önerilerini uygula. geliştirmeye başla. assetleri sıraya at"), Project belgeleri 58-58a, 60-60c
priority: 30
family: esya
style_gate: true
cost_est: 10
cost_max: 20
depends_on: ["301"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_serve_pot_stew_{4,3,2,1,0}.png
  - Assets/Art/Iso/_aday/iso_serve_tray_pilaf_{4,3,2,1,0}.png
  - art/results/578-*.md
created: 2026-11-20
---
# 578 Servis kabı pilotu: çömlekte güveç, sinide pilav, beş doluluk (stil kapısı) (10)

## Amaç
Sofra ve servis (Project belgesi 58 §3.2, karar 58a): yemek masaya büyük bir kapla gelir, herkes kendi tabağına alır ve **ana kaptaki yemek porsiyon porsiyon azalır**. Her yemeğe ayrı kare çizilmez; "kap türü × yemek görünümü" birleşimi çizilir ve yemek bu sınıflardan birine bağlanır.

Beş doluluk: `_4` dolu, `_3` dörtte üç, `_2` yarım, `_1` çeyrek, `_0` boş (kapta yalnızca bulaşık izi).

## Tarif
- Masa üstü eşyası: masanın L yuvasına oturur. Boyut ve pivot, masa üstü dekorlarıyla aynı (meyve sepeti `iso_item_fruit_basket` ölçüsü; 27 §7.7 "K boy"). Tek yön yeter (kap yuvarlak ya da simetrik).
- Yöntem: eşya dalgası (301/302) `create_map_object` stil eşleme, aile `esya`. Önce `_4` (dolu) üretilir; `_3`…`_0` aynı kabın aynı açıdan, içi azalmış hâlidir. Beş karede **kap birebir aynı** kalmalı, yalnızca içindeki yemek alçalmalı: mümkünse dolu kareden düzenleme araçlarıyla türet, olmuyorsa ayrı üret ve kabı elle eşitle (rapora yaz).
- Ortak ek: `, medieval tableware, seen from the isometric game angle, no hands, no people, no text`.

| Dosya | Dolu hâlin açıklaması |
|---|---|
| `iso_serve_pot_stew_<0-4>.png` | `wide glazed clay pot filled to the brim with chunky brown meat and vegetable stew` |
| `iso_serve_tray_pilaf_<0-4>.png` | `round tinned copper tray heaped with golden bulgur pilaf in a dome` |

## Doğrulama
- `python tools/art/validate_iso.py`, `python tools/art/style.py check --family esya`.
- Beş kare üst üste bindirildiğinde kabın dış çizgisi 1 pikselden fazla oynamamalı.
- Yan yana şerit: her birleşim için 4→0 (`art/results/578-serit.png`), masa üstünde deneme görüntüsü.
- Işık kaynağı kuralı: buhar ya da parlama çizilmez (buhar kodla parçacıktır).

## Notlar
Kod: servis kabı örneği kalan porsiyona göre kareyi seçer (58 §3.2; sofra partisi). Görünüm sınıfı yemeğin verisinde tutulur.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
