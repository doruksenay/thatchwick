---
id: "513"
title: "Easter egg hayvan pozları: uluyan köpek, kutudaki kedi, bakan öküz (eldeki gövdeler)"
status: pending
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 sohbet (easter egg sahneleri), Project belgeleri 55 ve 55d
priority: 50
family: hayvan
style_gate: false
cost_est: 8
cost_max: 16
depends_on: ["375", "379"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_animal_dog_1_howl_{se,sw}_<NN>.png
  - Assets/Art/Iso/_aday/iso_animal_dog_lapdog_howl_{se,sw}_<NN>.png
  - Assets/Art/Iso/_aday/iso_animal_cat_1_box_{se,sw}_<NN>.png
  - Assets/Art/Iso/_aday/iso_animal_ox_stare_{se,sw}_<NN>.png
  - art/results/513-*.md
created: 2026-11-20
---
# 513 Easter egg hayvan pozları (eldeki gövdeler)

## Amaç
Kullanıcı (20.11.2026): hayvan davranışlarının animasyonu mutlaka olmalı ve **her tipe özel poz kareleri** çizilsin (kod efektiyle geçiştirilmesin). Üç poz (kod: `IsoArt.AnimalPose`, `SceneBook`):
- `howl`: dolunay gecesi köpek oturur, başını göğe kaldırıp ular.
- `box`: yeni konan sandığın içine giren kedi; sandığın üstüne çizilir, yalnızca baş ve omuz görünür.
- `stare`: öküz yeni yapılan şeyin karşısında durup boş boş bakar.

Bu iş bugün teslim edilmiş gövdeleri kapsar. Kalan köpek tipleri (484'ün teslim edilmeyen 6 gövdesi) ve kedi gövdeleri (485) gövdeleri gelince aynı tarifle eklenir (514 ve sonrası). Tipin pozu yoksa kod türün pozuna (`dog_1`, `cat_1`) düşer; o tipler şu an zaten bu görselle çiziliyor.

## Tarif
- Gövdeler `create_character` v3 (quadruped) ile döndürülmüş karakterlerdir. Kimlikler ilgili raporlarda ya da `list_characters` ile bulunur: `dog_1` (379), `dog_lapdog` (484), `cat_1` (375), `ox` (`art/results/407-okuz-ve-atlama-ates.md`). Kimliği bulunamayan gövde için çağrı yapılmaz; raporda yazılır.
- Yöntem: `animate_character`, `mode=pixminimax`, `frame_count=8`, `keep_first_frame=true`, yalnızca 2 yön (`south-east`, `south-west`). Gövde başına 2 çağrı; 4 gövde x 2 = 8 gen. Araç dört ayaklıda `pixminimax` kabul etmiyorsa işçi durur ve `## Sorular` yazar (başka moda kendiliğinden geçilmez).
- Eylem metinleri:
  - köpek `howl`: `the dog sits down on its haunches, raises its muzzle straight up to the sky and howls, ears back, then holds the pose`
  - kedi `box`: `the cat sits upright and compact with paws tucked under, ears twitching, head turning slowly left and right, looking around`
  - öküz `stare`: `the ox stands completely still staring blankly straight ahead, slow blink, jaw chewing slightly, one ear flick`
- Sonradan işlem (ücretsiz): tuval gövdenin durma karesiyle aynı (ölçekleme yok); pivot kodda kayıtlı olanla aynı kalır (x, y alttan: dog_1 19,4; lapdog 16,2; cat_1 15,2; öküz 28,12); ayak satırı sabit; palet gövdenin durma karelerinden; `style.py fit --family hayvan`.
- **Kedi `box` için ek kırpma:** her karede alt gövde silinir, yalnızca baş, boyun ve omuz kalır (yaklaşık üst %55); kalan kısım, en alt opak satırı pivot satırına gelecek şekilde aşağı kaydırılır. Oyunda sandığın üst kenarından bakıyormuş gibi çizilir.
- Dosya adı: `iso_animal_<gövde>_<poz>_<se|sw>_<NN>.png` (`Assets/Art/Iso/_aday/`, anim/ değil; kod hayvan görsellerini burada arar).

## Doğrulama
`python tools/art/validate_iso.py --kind object <dosya>`, `python tools/art/style.py check --family hayvan <dosya>` (KALDI teslim edilmez). Poz başına GIF ve yan yana sayfası (durma karesi + poz kareleri). Kimlik: tüy rengi ve boy durma karesiyle aynı; zemin lekesi, gölge diski yok.

## Notlar
- Tüy rengi oyunda genden boyanır; kareler gövdenin nötr tonunda kalır (desen eklenmez).
- Dönem ve gerçekçilik: uluyan köpek, kutuya giren kedi, bakan öküz türlerin gerçek davranışıdır.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
