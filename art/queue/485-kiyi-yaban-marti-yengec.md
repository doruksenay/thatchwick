---
id: "485"
title: "Yaban hayvanları: martı ve yengeç (2)"
status: pending
approved_by: user        # otomatik akış (art/budget.md); kullanıcı 20.11.2026 "asset lazımsa queueya ekle" (deniz kıyısı canlıları, belge 50)
priority: 20
family: hayvan
style_gate: false
cost_est: 5
cost_max: 8
depends_on: ["375"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_wild_gull_cand_{se,sw,nw,ne,s,w,n,e}.png
  - Assets/Art/Iso/_aday/iso_wild_crab_cand_{se,sw,nw,ne,s,w,n,e}.png
created: 2026-11-20
---
# 485 Yaban hayvanları: martı ve yengeç (2)

## Amaç
Belge 50: deniz kıyısı bölgesinde gezen yaban ziyaretçileri (`WildCatalog`: `wild.gull`, `wild.crab`; yalnızca kıyıda görülür). Doğa Defteri'ne işlenir.

## Tarif
Tarif **375 ve 409 ile birebir aynı** (106 yol B, 8 yön, v3 döndürme, `style.py fit --family hayvan`, palet referansı `iso_animal_sheep_1_cand_se.png`, açıklama ekine `earthy muted medieval palette, desaturated`). Her tür tek aday. Teslim `cp -n`, `.meta` yok.

| # | Tür id | Dosya (`_aday/`) | Tuval | Açıklama metni |
|---|---|---|---|---|
| 1 | `wild.gull` | `iso_wild_gull_cand_<yön>.png` | 32 | `herring gull standing on sand, white body, grey wings, yellow beak` |
| 2 | `wild.crab` | `iso_wild_crab_cand_<yön>.png` | 24 | `small green shore crab walking sideways on sand` |

## Doğrulama
375 doğrulaması: `validate_iso.py`, `style.py check --family hayvan`. Bir yön KALDI ise o türün tamamı teslim edilmez. Yan yana sayfası karga ve kurbağa ile aynı satırda; ölçek gerçeğe uygun (yengeç küçük).

## Notlar
Kod eşlemesi hazır: `IsoArt.Animals` tablosu bu adları arar, dosya yoksa yer tutucu küp çizer. Pivot 24,10.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
