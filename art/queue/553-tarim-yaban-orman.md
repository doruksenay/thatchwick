---
id: "553"
title: "Yaban bitkileri 1/3: orman (9)"
status: running
claimed_at: 2026-11-20T14:48Z
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 ("kodlamayı başlat, assetleri de sıraya ekle; hepsi tek seferde"), Project belgeleri 59 ve 59a
priority: 30
family: bitki
style_gate: false
cost_est: 9
cost_max: 18
depends_on: ["373"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_forage_bilberry_se.png
  - Assets/Art/Iso/_aday/iso_forage_wild_raspberry_se.png
  - Assets/Art/Iso/_aday/iso_forage_morel_se.png
  - Assets/Art/Iso/_aday/iso_forage_chanterelle_se.png
  - Assets/Art/Iso/_aday/iso_forage_porcini_se.png
  - Assets/Art/Iso/_aday/iso_forage_hawthorn_se.png
  - Assets/Art/Iso/_aday/iso_forage_sloe_se.png
  - Assets/Art/Iso/_aday/iso_forage_acorn_se.png
  - Assets/Art/Iso/_aday/iso_forage_wild_pear_se.png
created: 2026-11-20
---
# 553 Yaban bitkileri 1/3: orman (9)

## Amaç
Project belgesi 59 §6.2: **yalnız doğadan toplanan** bitkiler (ekilemez, pazarda satılmaz). Bölgeye ve mevsime göre belirir.

## Tarif
Yöntem **373 ile aynı**: tuval 80x72, 1x1 kılavuz + maske, pivot 40,24; 373'teki açıklama eki aynen. Üretimden sonra `style.py fit --family bitki`, ardından `check`. İkinci deneme yalnızca biçim için.

| Bitki | Dosya | Yükseklik | Açıklama metni (+ ek) |
|---|---|---|---|
| yaban mersini (çoban üzümü) | `iso_forage_bilberry_se.png` | 14 | `low bilberry shrub with small oval leaves and dark blue berries` |
| yabani ahududu | `iso_forage_wild_raspberry_se.png` | 24 | `tangle of wild raspberry canes with a few red berries` |
| kuzugöbeği | `iso_forage_morel_se.png` | 10 | `two morel mushrooms with honeycomb caps among leaf litter` |
| sarı mantar | `iso_forage_chanterelle_se.png` | 8 | `small group of golden-yellow funnel-shaped chanterelles in moss` |
| ayı mantarı | `iso_forage_porcini_se.png` | 12 | `one stout porcini mushroom with a brown cap and thick pale stem` |
| alıç | `iso_forage_hawthorn_se.png` | 30 | `thorny hawthorn shrub with lobed leaves and clusters of small red haws` |
| çakal eriği | `iso_forage_sloe_se.png` | 28 | `blackthorn shrub with dark thorny twigs and dusty blue-black sloes, few leaves` |
| meşe palamudu | `iso_forage_acorn_se.png` | 6 | `scatter of fallen acorns with caps and a few brown oak leaves` |
| ahlat | `iso_forage_wild_pear_se.png` | 34 | `small scrubby wild pear shrub with tiny hard green pears` |

- Maliyet 9 × 1 gen; ikinci deneme payıyla `cost_max`.

## Doğrulama
`validate_iso.py`, `style.py check --family bitki`. Dönem kuralları (CLAUDE.md, Project belgesi 20): Yeni Dünya bitkisi yok (bilinçli istisnalar: kahve, siyah çay, domates, avokado), ortaçağ sonrası icat yok.

## Notlar
- Kod düğümü adla bulur (`iso_forage_<ad>_se`).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
