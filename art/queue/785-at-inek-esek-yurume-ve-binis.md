---
id: "785"
title: "At, inek, eşek ve öküz yürüme; eyerli at binilmiş yürüme (4 çapraz yön)"
status: pending
approved_by: user        # kullanıcı kararı 2026-11-21 sohbet: "At, inek ve eşek ... ekle" (yürüme, binilmiş poz)
priority: 40
family: hayvan
style_gate: false
cost_est: 22
cost_max: 34
depends_on: ["784"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_animal_{horse_medium,cow,donkey}_v2_walk_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/iso_animal_ox_v2_walk_{se,sw,nw,ne}_<NN>.png
  - Assets/Art/Iso/_aday/iso_animal_horse_medium_v2_ride_{se,sw,nw,ne}_<NN>.png
  - art/results/785-*.md
created: 2026-11-21
---
# 785 Büyük hayvanların yürümesi ve binilmiş at

## Amaç
Kod artık yürüyen hayvanda `walk`, binilen atta (binici hareket ederken) `ride` klibini oynatır (`AnimalViews`, `IsoArt.AnimalMotion`). Öküz de (`ox_v2`, 104t) yürüme kazanır.

## Tarif
- Karakterler: 784'ün `horse_medium_v2`, `cow_v2`, `donkey_v2` karakterleri; öküz `ox_v2` (104t; kimlik `art/results/104t-inek-keci-okuz.md` ya da `list_characters`).
- `animate_character`, `frame_count=8`, `keep_first_frame=true`, yön başına 1 çağrı, 4 çapraz yön.
- `walk` eylem metinleri:
  - at: `walking forward at a calm steady walk, head nodding gently, tail swaying, loop`
  - inek: `walking forward slowly and heavily, head low, loop`
  - eşek: `walking forward with short brisk steps, long ears bobbing, loop`
  - öküz: `walking forward slowly and powerfully, head low, loop`
- **Eyerli at (`ride`):** önce eyerli karakter: 784'ün at tek karesi (güney) üzerine `create_map_object` ya da boyama (inpainting) ile maske = sırt ve baş bölgesi, metin `simple medieval leather saddle with stirrups and a plain leather bridle and reins, brown leather` (1 gen); ardından v3 döndürme (1 gen; 784'le aynı tuval). Bu karakterle `walk` metniyle 4 yön (4 gen). Binici çizilmez (oyunda binici ayrı çizilir, 787).
- Toplam: yürüme 16 + eyer 2 + binilmiş 4 = 22 gen.
- **Kırpma:** her gövdenin kareleri 784'teki durma tuvaliyle aynı boyda, aynı pivot (780 kuralı). Öküzün durma görseli 104t'deki gibi kalır (68x68); öküz kareleri yine 780 kuralıyla kırpılır (yatayda orta, ayak satırı alttan 2. satır) ve `iso_animal_ox_v2_walk_<yön>_<NN>` adını alır: kod `ox_v2` kökünü ölçek seti sayar, pivotu tuvalden alır.
- Renk: `style.py fit --family hayvan` (ücretsiz).

## Doğrulama
- Her klibin 00 karesinde `scale_check.py` (sığır, at ve eşek sınıfları).
- `validate_iso.py --kind object`, `style.py check --family hayvan`; ayak satırı kayması ≤1 px.
- Klip başına GIF. Eyerli at: eyer sırtta, üzengi görünür, at kimliği (boy, renk) durma karesiyle aynı.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
