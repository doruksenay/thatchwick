---
id: "489"
title: "Böcek defteri ikonları (16)"
status: review
claimed_at: 2026-11-20T10:14Z
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 (hayvan genetiği ve tür genişletme, tek PR), Project belgeleri 48 ve 48b
priority: 30
family: esya
style_gate: false
cost_est: 16
cost_max: 24
depends_on: []
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/icon_bug_swallowtail.png
  - Assets/Art/Iso/_aday/icon_bug_peacock_moth.png
  - Assets/Art/Iso/_aday/icon_bug_dragonfly.png
  - Assets/Art/Iso/_aday/icon_bug_mantis.png
  - Assets/Art/Iso/_aday/icon_bug_cicada.png
  - Assets/Art/Iso/_aday/icon_bug_stag_beetle.png
  - Assets/Art/Iso/_aday/icon_bug_night_moth.png
  - Assets/Art/Iso/_aday/icon_bug_dung_beetle.png
  - Assets/Art/Iso/_aday/icon_bug_rhino_beetle.png
  - Assets/Art/Iso/_aday/icon_bug_ant.png
  - Assets/Art/Iso/_aday/icon_bug_woodworm.png
  - Assets/Art/Iso/_aday/icon_bug_snow_fly.png
  - Assets/Art/Iso/_aday/icon_bug_diving_beetle.png
  - Assets/Art/Iso/_aday/icon_bug_water_scorpion.png
  - Assets/Art/Iso/_aday/icon_bug_pond_skater.png
  - Assets/Art/Iso/_aday/icon_bug_silver_water_beetle.png
created: 2026-11-20
---
# 489 Böcek ikonları

## Amaç
Belge 48 §9: böcek defteri (mevsim setleri ve setin efsanevisi). Cam kavanozla toplama yerlerinden yakalanır; envanter, defter ve pazar ikonu (`IsoArt.ItemIcon`).

## Tarif
483 ile aynı: `create_image_pixflux` 32x32, `no_background=true`, ikon görünümü (hafif yukarıdan). Efsaneviler biraz daha iri ve parlak, ama aynı ışıkta.

| # | items.json id | Ad | Açıklama metni |
|---|---|---|---|
| 1 | `item.bug_swallowtail` | Kırlangıçkuyruk kelebeği | `yellow and black swallowtail butterfly with tail streamers, wings open, item icon` |
| 2 | `item.bug_peacock_moth` | Büyük tavus güvesi (efsanevi) | `giant peacock moth with large eye-spots on brown-grey wings, item icon` |
| 3 | `item.bug_dragonfly` | İmparator yusufçuk | `emperor dragonfly, blue-green body, clear wings spread, item icon` |
| 4 | `item.bug_mantis` | Peygamber devesi | `green praying mantis with folded front legs, item icon` |
| 5 | `item.bug_cicada` | Ağustos böceği | `brown cicada with clear veined wings, item icon` |
| 6 | `item.bug_stag_beetle` | Geyik böceği (efsanevi) | `large stag beetle with big branched reddish-brown jaws, item icon` |
| 7 | `item.bug_night_moth` | Gece kelebeği | `small dusty grey-brown night moth, item icon` |
| 8 | `item.bug_dung_beetle` | Bok böceği | `black dung beetle pushing a small dung ball, item icon` |
| 9 | `item.bug_rhino_beetle` | Gergedan böceği (efsanevi) | `glossy dark brown rhinoceros beetle with a curved horn, item icon` |
| 10 | `item.bug_ant` | Karınca | `single red-brown ant, item icon` |
| 11 | `item.bug_woodworm` | Tahta kurdu | `small pale woodworm larva on a bit of wood, item icon` |
| 12 | `item.bug_snow_fly` | Kar sineği (efsanevi) | `tiny wingless dark snow fly on a white snow patch, item icon` |
| 13 | `item.bug_diving_beetle` | Dalgıç böcek | `dark olive diving beetle with yellow edge, item icon` |
| 14 | `item.bug_water_scorpion` | Su akrebi | `flat brown water scorpion with thin tail tube, item icon` |
| 15 | `item.bug_pond_skater` | Su kayağı | `thin long-legged pond skater insect, item icon` |
| 16 | `item.bug_silver_water_beetle` | Büyük gümüş su böceği (efsanevi) | `large glossy black water beetle with silvery underside shimmer, item icon` |

## Doğrulama
`python tools/art/style.py check --family esya <dosya> ...`; renkli böcek palet dışı çıkarsa `hayvan` ailesiyle yeniden denetle (474 dersi).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
