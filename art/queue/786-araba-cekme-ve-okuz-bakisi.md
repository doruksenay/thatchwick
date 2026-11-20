---
id: "786"
title: "Arabaya koşulu at, eşek ve öküz (çekme klibi) ve öküz bakışı ölçek setinde"
status: pending
approved_by: user        # kullanıcı kararı 2026-11-21 sohbet: "At, inek ve eşek ... ekle" (araba çekme)
priority: 40
family: hayvan
style_gate: false
cost_est: 20
cost_max: 32
depends_on: ["784"]
max_attempts: 1
pixellab: true
supersedes:
  - Assets/Art/Iso/_aday/iso_animal_ox_cand_*.png          # öküz v1 (43 px, ölçek dışı); oyun ox_v2 kullanıyor
  - Assets/Art/Iso/_aday/iso_animal_ox_stare_*.png         # v1 gövdesinden
outputs:
  - Assets/Art/Iso/_aday/iso_animal_{horse_medium,donkey}_v2_pull_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/iso_animal_ox_v2_pull_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/iso_animal_ox_v2_stare_{se,sw}_<NN>.png
  - art/results/786-*.md
created: 2026-11-21
---
# 786 Araba çekme ve öküz bakışı

## Amaç
Avludaki araba köy yoluna giderken koşulu hayvan yolda çizilir (belge 57e, `GameRootYard.HitchedView`). Kod artık bu durumda `pull` klibini oynatır, yoksa `walk`. Hayvan koşum takımıyla çizilir; araba ayrı görsel olarak kalır (bu iş arabayı çizmez).

Öküz bakışı (`stare`, 513) eski, küçük öküz v1 gövdesinden üretilmişti; oyun artık `ox_v2` çiziyor, bakış da onun gövdesinden yeniden üretilir.

## Tarif
- **Koşumlu karakterler:** gövde başına tek kare (güney) üzerine boyama ile koşum (1 gen) + v3 döndürme (1 gen):
  - at: `padded medieval horse collar around the neck, leather harness straps and traces running back along the sides, plain bridle`
  - eşek: `small padded collar and simple rope and leather harness with traces along the sides`
  - öküz (`ox_v2`): `heavy wooden neck yoke resting on the neck in front of the shoulders, with a short chain hanging back`
- Sonra `animate_character` (`frame_count=8`, `keep_first_frame=true`), 4 çapraz yön: `leaning forward into the harness and pulling a heavy load with slow strong steps, head low, loop`. 3 gövde x 4 = 12 gen.
- **Öküz `stare`:** `ox_v2` karakteri (koşumsuz), 2 yön (se, sw), 513'ün metni: `the ox stands completely still staring blankly straight ahead, slow blink, jaw chewing slightly, one ear flick`. 2 gen.
- Toplam ≈ 6 + 12 + 2 = 20 gen.
- **Kırpma:** 780 kuralı; at ve eşek 784 tuvaliyle aynı boy, öküz 785'teki öküz kareleriyle aynı boy.
- Renk: `style.py fit --family hayvan`.

## Doğrulama
`scale_check.py` (00 kareleri), `validate_iso.py --kind object`, `style.py check --family hayvan`, klip başına GIF. Koşum her karede aynı yerde; at kimliği 784'le aynı.

## Notlar
- Dönem: boyunluk (horse collar) 10. yüzyıldan beri Avrupa'da; öküzde boyunduruk. Dönem kuralına uygun.
- İnek araba çekmez (oyunda öküz ve at koşulur).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
