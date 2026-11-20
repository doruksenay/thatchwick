---
id: "514"
title: "Easter egg: kedi gövdelerinin kutu pozu (3 boy)"
status: blocked
claimed_at: 2026-11-20T17:10Z
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 sohbet (easter egg sahneleri), Project belgeleri 55 ve 55d
priority: 50
family: hayvan
style_gate: false
cost_est: 6
cost_max: 12
depends_on: ["485k", "513"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_animal_cat_{medium,large,slim}_box_{se,sw}_<NN>.png
  - art/results/514-*.md
created: 2026-11-20
---
# 514 Kedi gövdelerinin kutu pozu

## Amaç
513'ün devamı: genetik kedi gövdeleri (485: `cat_medium`, `cat_large`, `cat_slim`) teslim edilince her boyun kendi `box` pozu. Kullanıcı kararı (20.11.2026): her tipe özel poz kareleri.

## Tarif
513'teki kedi `box` tarifi aynen (eylem metni, 2 yön, ek kırpma, tuval ve pivot gövdenin durma karesinden). Gövde kimlikleri 485 raporundan. 3 gövde x 2 yön = 6 gen. Uzun tüylü eşler (`_long`) için ayrı poz üretilmez; kod kısa tüylü gövdenin pozunu kullanır. 485'te teslim edilmeyen gövde atlanır, raporda yazılır.

## Doğrulama
513 ile aynı.

## Notlar
- Dosya adı: `iso_animal_cat_<medium|large|slim>_box_<se|sw>_<NN>.png`.

## Sorular
- Bağımlılık `485-kedi-govdeleri.md` hâlâ `status: pending`; `cat_medium`, `cat_large`, `cat_slim` gövdeleri üretilmedi ve `Assets/Art/Iso/_aday/` altında yok. Tarif "485'te teslim edilmeyen gövde atlanır" der, ama hiçbiri yok; PixelLab çağrısı yapılmadı (0 gen). 485 kedi gövdeleri `review` olunca 514 `pending`e alınıp yeniden çalıştırılmalı (iş numarası çakışması: iki 485 dosyası var, 485-kedi-govdeleri bağımlılık olarak kastediliyor).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
