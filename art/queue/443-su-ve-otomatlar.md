---
id: "443"
title: "Su ve dönem otomatları (5)"
status: pending
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 19.11.2026 ("bunların hepsini yapalım", varyant çeşitliliği), Project belgesi 45
priority: 20
family: esya
style_gate: false
cost_est: 12
cost_max: 20
depends_on: ["302"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_item_courtyard_pool_2x2_se.png
  - Assets/Art/Iso/_aday/iso_item_joke_fountain_se.png
  - Assets/Art/Iso/_aday/iso_item_jazari_peacock_se.png
  - Assets/Art/Iso/_aday/iso_item_water_clock_se.png
  - Assets/Art/Iso/_aday/iso_item_eagle_lectern_se.png
created: 2026-11-19
---
# 443 Su ve dönem otomatları (5)

## Amaç
Belge 45: avlu havuzu, şaka çeşmesi (Hesdin parkı, 1290'lar), Cezerî'nin el yıkama otomatı ve su saati (1206, Artuklu), dönen kartal rahle (Villard de Honnecourt, 1230'lar).

## Tarif
302 ölçeği. Otomatlar pirinç ve tunç; saat dişlisi dışarıdan görünmez (ortaçağ dışı mekanik saat izlenimi vermesin).

| # | items.json id | Dosya | Yer | Açıklama metni |
|---|---|---|---|---|
| 1 | `item.courtyard_pool` | `iso_item_courtyard_pool_2x2_se.png` | 2x2, dış | `square stone courtyard pool with a small central fountain jet, still water` |
| 2 | `item.joke_fountain` | `iso_item_joke_fountain_se.png` | 1x1, dış | `small stone garden fountain with a bronze spout, hidden jets` |
| 3 | `item.jazari_peacock` | `iso_item_jazari_peacock_se.png` | 1x1 | `13th century Anatolian hand-washing automaton: brass peacock on a basin column, small servant figure holding a ewer` |
| 4 | `item.water_clock` | `iso_item_water_clock_se.png` | 1x1 | `medieval brass and wood water clock: tall case with a float tank and a small dial` |
| 5 | `item.turning_eagle_lectern` | `iso_item_eagle_lectern_se.png` | 1x1 | `carved wooden lectern shaped as an eagle with spread wings holding a book` |


## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family esya <dosya> ...`. Dönem kuralları (CLAUDE.md, Project belgesi 20): ortaçağ sonrası icat yok, Yeni Dünya malzemesi yok.

## Notlar
- Eşya → dosya eşlemesi `Assets/Scripts/Game/IsoArt.cs`'te hazır (bu adlarla); görsel gelince kendiliğinden kullanılır. Pivotları kullanıcı `Assets/Editor/IsoPivots.txt`'e ekler.
- Teslim `cp -n`, `.meta` yok.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
