---
id: "488"
title: "Tavşan yastığı (yapay yuva tümseği)"
status: failed
claimed_at: 2026-11-20T10:09Z
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 (hayvan genetiği ve tür genişletme, tek PR), Project belgeleri 48 ve 48b
priority: 30
family: esya
style_gate: false
cost_est: 3
cost_max: 6
depends_on: []
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_warren_se.png
created: 2026-11-20
---
# 488 Tavşan yastığı

## Amaç
`item.warren` (belge 48 §8.3): ortaçağda tavşanlar için kurulan uzun, alçak toprak tümseği (pillow mound). Dış barınak, 6 tavşan.

## Tarif
Eşya ailesinin izometrik tarifi (`create_isometric_tile` yerine `create_image_pixflux` + izometrik görünüm, mevcut eşya işleri gibi), 112x80, `no_background=true`.
Açıklama metni: `long low grassy earth mound with several small burrow holes on its side, medieval rabbit warren (pillow mound), isometric view`.

## Doğrulama
`validate_iso.py`, `style.py check --family esya`. Pivot items.json'da (56, 32).

## Sorular
- Tek deneme: şekil tanınıyor (tümsek, yuva delikleri) ama esya paleti denetimi KALDI. Ham: doygunluk x1.95, palete yakın %18. `fit` (x0.59) sonrası: doygunluk x1.15, palete yakın %36, ort. uzaklık 20.8 (eşik 20). Teslim edilmedi. Karar: kullanıcı ham/fit hâlini art/results/488-red ve 488-yanyana.png üzerinden değerlendirsin; ya da yeni iş (kılavuzlu).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
