---
id: "441"
title: "Sergi rafı, sergi dolabı, giysi sırığı, kurdele (4)"
status: accepted
claimed_at: 2026-11-19T13:33Z
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 19.11.2026 ("bunların hepsini yapalım", varyant çeşitliliği), Project belgesi 45
priority: 20
family: esya
style_gate: false
cost_est: 8
cost_max: 14
depends_on: ["301", "306"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_wall_item_display_shelf_2w_sw_ne.png
  - Assets/Art/Iso/_aday/iso_item_display_cabinet_se.png
  - Assets/Art/Iso/_aday/iso_wall_item_clothes_perch_2w_sw_ne.png
  - Assets/Art/Iso/_aday/iso_wall_item_home_ribbon_sw_ne.png
created: 2026-11-19
---
# 441 Sergi rafı, sergi dolabı, giysi sırığı, kurdele (4)

## Amaç
Belge 45: koleksiyon sergisi (her eşyadan bir tane; içerik oyunda dolgu olarak çizilir), giysi sırığı (kumaş asılır), Yaz Panayırı ev yarışmasının hatırası.

## Tarif
Raf ve sırık boş çizilir (içerik oyunda üstüne). 2 kenarlık eşya 380'deki kılavuz.

| # | items.json id | Dosya | Yer | Açıklama metni |
|---|---|---|---|---|
| 1 | `item.display_shelf` | `iso_wall_item_display_shelf_2w_sw_ne.png` | duvar, 2 kenar | `long wooden wall shelf with small compartments, empty, for displaying found curios` |
| 2 | `item.display_cabinet` | `iso_item_display_cabinet_se.png` | 1x1 uzun | `tall wooden cabinet with a wooden lattice front, shelves visible behind the lattice, iron corner fittings` |
| 3 | `item.clothes_perch` | `iso_wall_item_clothes_perch_2w_sw_ne.png` | duvar, 2 kenar | `horizontal wooden clothes pole on two wall brackets, empty` |
| 4 | `item.home_ribbon` | `iso_wall_item_home_ribbon_sw_ne.png` | duvar | `red and gold rosette ribbon prize pinned to a small wooden plaque` |


## Doğrulama
`python tools/art/validate_iso.py <dosya> ...`, `python tools/art/style.py check --family esya <dosya> ...`. Dönem kuralları (CLAUDE.md, Project belgesi 20): ortaçağ sonrası icat yok, Yeni Dünya malzemesi yok.

## Notlar
- Eşya → dosya eşlemesi `Assets/Scripts/Game/IsoArt.cs`'te hazır (bu adlarla); görsel gelince kendiliğinden kullanılır. Pivotları kullanıcı `Assets/Editor/IsoPivots.txt`'e ekler.
- Teslim `cp -n`, `.meta` yok.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
- 2026-11-20: kullanıcı kabul etti (sohbette toplu inceleme).
