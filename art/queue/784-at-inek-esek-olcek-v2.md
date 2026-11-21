---
id: "784"
title: "At, inek ve eşek: durma görseli ölçek setinde (8 yön)"
status: failed
claimed_at: 2026-11-21T00:42Z
approved_by: user        # kullanıcı kararı 2026-11-21 sohbet: "At, inek ve eşek ... ekle"
priority: 40
family: hayvan
style_gate: true         # 785-787 ve 486'nın büyük hayvan yolu buna bağlı
cost_est: 6
cost_max: 14
depends_on: []
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_animal_{horse_medium,cow,donkey}_v2_{se,sw,nw,ne,s,w,n,e}.png
  - art/results/784-*.md
created: 2026-11-21
---
# 784 At, inek ve eşek (durma)

## Amaç
Oyunda at, inek ve eşek var (içerik `animal.horse`, `animal.cow`, `animal.donkey`) ama görselleri yok: küp çiziliyor. 104 (inek, at) ve 486 (at tipleri, eşek) teslim edemedi (yandan duruş, zemin lekesi). Ölçek tablosu: sığır 56-68, at ve eşek 60-82 px. Kullanıcı kararı (2026-11-21): durma, yürüme, binilmiş poz ve araba çekme eklenir. Bu iş durma görselidir; öbürleri 785-787.

At, genetik gövde sisteminin orta gövdesi olarak üretilir (`horse_medium`, belge 48 §7): oyun at türü için bu gövdeyi çizer, renk genden boyanır.

## Tarif
Yol B (106): `create_image_pixflux` tek kare (güney) + v3 döndürme (`body_type="quadruped"`, şablon `horse`). Ortak kurallar, güney ve zemin kontrolü, 375 açıklama eki, `style.py fit --family hayvan`: 106 ve 375 ile aynı. Palet: `color_image_base64` = `Assets/Art/Iso/_aday/iso_animal_ox_v2_cand_se.png` (kabul edilen büyük hayvan).

| Gövde | Tuval | Hedef boy | Açıklama metni (+ ortak ekler) |
|---|---|---|---|
| `horse_medium` | 80 | 66-78 | `ordinary medieval riding horse seen straight from the front facing the viewer, medium build, neutral grey-brown coat, plain coat without markings, standing on all four legs, no saddle, no bridle` |
| `cow` | 64 | 56-64 | `medieval farm cow seen straight from the front facing the viewer, short curved horns, brown and white patched coat, udder, standing on all four legs` |
| `donkey` | 68 | 60-66 | `grey donkey seen straight from the front facing the viewer, long upright ears, short upright mane, dark dorsal stripe, small hooves, tufted tail, standing on all four legs` |

- Tuvaller ox_v2'nin doluluğundan (68 tuval → 62 px): boy ≈ tuval x 0,91 (tahmin). Eşek attan kısa, inekten biraz uzun kalmalı.
- **Güney kontrolü (486 dersi):** tek karede hayvan yandan duruyorsa (en kayıt: 486'nın 7 tek karesi de böyle elendi) açıklamaya `head and chest toward the camera, body foreshortened behind` eklenerek **bir kez** yeniden denenir (1 gen). Yine yandansa o gövde teslim edilmez, raporda yazılır; öbür gövdeler devam eder.
- **Kırpma:** 780 kırpma kuralı (tuval birleşik kutu, yatayda orta, ayak satırı alttan 2. satır). Dosya: `Assets/Art/Iso/_aday/iso_animal_<gövde>_v2_<yön>.png`.
- Karakter kimlikleri rapora yazılır (785 ve 786 kullanır).

## Doğrulama
- `scale_check.py`: inek sığır sınıfı 56-68, at ve eşek 60-82 (sınıf dışı yön teslim edilmez).
- `validate_iso.py --kind object`, `style.py check --family hayvan`.
- Yan yana sayfası: 84 px yetişkin, öküz v2, koyun 2, yeni üç gövde; karo konturu; aynı zemin çizgisi.

## Notlar
- Kod hazır: `IsoArt.Animals` at, inek, eşek, katır ve bardoyu eşler; `AnimalBody` önce `_v2` gövdesini arar.
- Hafif ve ağır at, midilli, katır, bardo 486'da (kalan gövdeler); 486'nın tuvalleri bu işin ölçeğine göre güncellendi ve 784'e bağlandı.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
