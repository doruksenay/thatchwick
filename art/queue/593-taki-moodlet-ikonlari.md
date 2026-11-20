---
id: "593"
title: "Takı ruh hali ikonları (2)"
status: running
claimed_at: 2026-11-20T13:54Z
approved_by: user        # otomatik akış (CLAUDE.md, 2026-11-17); kullanıcı 20.11.2026 sohbet: "assetleri de sıraya ekle"
priority: 35
family: arayuz
style_gate: false
cost_est: 2
cost_max: 4
depends_on: ["212"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/ui_icon_moodlet_{jewelry_joy,jewelry_fine}.png
  - Assets/Art/Iso/_aday/ui_icon_moodlet_<ad>_32.png
created: 2026-11-20
---
# 593 Takı ruh hali ikonları (2)

## Amaç
Project belgesi 62 §2: takı takan kişide iki yeni ruh hali görünüyor (`eff_jewelry_joy` "Takının keyfi" +2, `eff_jewelry_fine` "Göz alıcı takılar" +4). Moodlet kartlarında ikonları yok.

## Tarif
**212 ile aynı yöntem** (`create_image_pixen` 32x32, `no_background=true`, `outline: single color outline`, `detail: low detail`; ikon başına tek çağrı). 32x32 ham hâli alfa ikili yapılıp `_32` ekiyle konur; 16x16 hâli en yakın komşuyla indirilir ve en çok 12 renge düşürülür. Stil, palet ve ortak ek 497 ile aynı (210 yönü, `parsomen`).

| Dosya | Moodlet | Açıklama (+ ortak ek) |
|---|---|---|
| ui_icon_moodlet_jewelry_joy | Takının keyfi | `a plain copper finger ring standing upright with one small white sparkle` |
| ui_icon_moodlet_jewelry_fine | Göz alıcı takılar | `a gold finger ring with one big round red gemstone and two white sparkles` |

## Doğrulama
- 16x16 (+ `_32`), yarı saydam 0, renk ≤ 12. `python tools/art/style.py check --family arayuz <dosya> ...`.
- Toplu önizleme x4, parşömen ve koyu zeminde: `art/results/593-ikonlar-x4.png`, `art/results/593-ikonlar-x4-koyu.png`.
- Göz kontrolü 3 kat ölçekte: ikisi metal renginden ve taştan ayırt ediliyor; 16x16'da tek ana şekil (212 dersi).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
