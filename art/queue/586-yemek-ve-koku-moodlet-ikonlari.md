---
id: "586"
title: "Yemek, koku ve boş kalma moodlet ikonları (18)"
status: review
claimed_at: 2026-11-20T17:10Z
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 ("önerilerini uygula. geliştirmeye başla. assetleri sıraya at"), Project belgeleri 58-58a, 60-60c
priority: 50
family: arayuz
style_gate: false
cost_est: 18
cost_max: 26
depends_on: ["212"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/ui_icon_moodlet_{good_smell,mouth_watering,burnt_smell,hard_to_swallow,embarrassed,astonished,laughed_a_lot,cat_ate_it,me_time,bored,idle_too_long,taking_it_easy,family_warmth,mothers_cooking,ate_together,picnic,just_us_two,compliment_to_cook}.png
  - Assets/Art/Iso/_aday/ui_icon_moodlet_<ad>_32.png
created: 2026-11-20
---
# 586 Yemek, koku ve boş kalma moodlet ikonları (18)

## Amaç
Project belgeleri 60b ve 60c: koku tepkileri (Mis gibi koku, Ağzı sulandı, Yanık kokusu, Zor yuttu, Mahcup oldu), kedi hırsızlığı (Şaşakaldı, Çok güldü, Emeğini kedi yedi), boş kalma (Me time, Canı sıkılıyor, Aylaklıktan bunaldı, Keyfine bakıyor), sofra (Aile şefkati, Anamın yemeği, Birlikte yedik, Eline sağlık dediler), piknik (Piknik, Baş başa).

## Tarif
**212 ve 497 ile aynı yöntem** (`create_image_pixen` 32x32, `no_background=true`, `outline: single color outline`, `detail: low detail`; ikon başına tek çağrı; 16x16 ve `_32`). Stil 210 yönü, tek renk koyu kahve kontur, düz gölgeleme, ışık sol üstten, 3-5 renk. Her ikon tek ana şekil (212 dersi).

| Dosya (`ui_icon_moodlet_<ad>.png`) | Açıklama |
|---|---|
| `good_smell` | `wavy aroma lines rising from a small pot` |
| `mouth_watering` | `open smiling mouth with a single drop` |
| `burnt_smell` | `small pot with dark jagged smoke` |
| `hard_to_swallow` | `grimacing mouth with a spoon` |
| `embarrassed` | `blushing cheek with a sweat drop` |
| `astonished` | `wide round eyes with raised brows` |
| `laughed_a_lot` | `laughing mouth with a tear of joy` |
| `cat_ate_it` | `fish skeleton with a paw print` |
| `me_time` | `steaming cup beside a small cushion` |
| `bored` | `hourglass with a yawn mark` |
| `idle_too_long` | `drooping wilted flower` |
| `taking_it_easy` | `feet up on a stool` |
| `family_warmth` | `three bowls close together with a small heart` |
| `mothers_cooking` | `ladle with a heart` |
| `ate_together` | `two spoons crossed over a bowl` |
| `picnic` | `striped cloth with a basket` |
| `just_us_two` | `two cups touching with a small heart` |
| `compliment_to_cook` | `ladle with a small star` |

## Doğrulama
497 ile aynı: 16x16 (+ `_32`), yarı saydam 0, renk ≤ 12; `python tools/art/style.py check --family arayuz`; toplu önizleme x4 parşömen ve koyu zeminde (`art/results/586-ikonlar-x4.png`). `good_smell` ile `burnt_smell`, `bored` ile `idle_too_long` 3 kat ölçekte ayırt edilmeli.

## Notlar
Moodlet anahtarları `MoodCatalog`'a bu adlarla eklenir (kod oturumu).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
