---
id: "480"
title: "İstek, süreli görev ve azık ikonları (16)"
status: pending
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 ("tüm kod işlerini hallet geriye sadece asset üretme ve test etme kalsın"), Project belgeleri 46 ve 47
priority: 30
family: arayuz
style_gate: false
cost_est: 16
cost_max: 32
depends_on: ["455a"]
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/ui_icon_request_favor.png
  - Assets/Art/Iso/_aday/ui_icon_request_order.png
  - Assets/Art/Iso/_aday/ui_icon_request_seasonal.png
  - Assets/Art/Iso/_aday/ui_icon_request_call.png
  - Assets/Art/Iso/_aday/ui_icon_request_story.png
  - Assets/Art/Iso/_aday/ui_icon_request_vow.png
  - Assets/Art/Iso/_aday/ui_icon_request_bequest.png
  - Assets/Art/Iso/_aday/ui_icon_request_marker.png
  - Assets/Art/Iso/_aday/ui_icon_duty_mine.png
  - Assets/Art/Iso/_aday/ui_icon_duty_fish.png
  - Assets/Art/Iso/_aday/ui_icon_duty_forest.png
  - Assets/Art/Iso/_aday/ui_icon_duty_forage.png
  - Assets/Art/Iso/_aday/ui_icon_duty_traps.png
  - Assets/Art/Iso/_aday/ui_icon_duty_clay.png
  - Assets/Art/Iso/_aday/ui_icon_duty_errand.png
  - Assets/Art/Iso/_aday/icon_item_miner_ration.png
created: 2026-11-20
---
# 480 İstek, süreli görev ve azık ikonları (16)

## Amaç
- **İstekler defteri** (I; Project belgesi 46 §12): her satırın başında tür ikonu. Kod hazır: `RequestPanel.IconKey` → `Resources/UI/Icons/ui_icon_request_<tür>.png`. İkon yoksa yalnız yazı çıkar.
- **Baş üstü işaret:** isteği olan köylünün başının üstünde küçük nakış işareti (`ui_icon_request_marker`; yoksa "!").
- **Süreli görev** (belge 47 §3): aile panelinde görevdeki kişinin satırında görev türü ikonu (`ui_icon_duty_<tür>`).
- **Azık heybesi** (`item.miner_ration`): göreve giden karakterin yanına aldığı paket yemek; envanter ve tarif listesinde ikon.

## Tarif
**455a ile aynı yöntem** (`create_map_object` 32x32, saydam, `view: side`, `single color outline`, `basic shading`, `medium detail`; okunmazsa `create_image_pixflux` 32x32 `no_background=true`; teslim öncesi 16 renge indir, yarı saydam 0). Ortak ek: `, single game icon, one object centered, no frame, no background, no text, no face, no eyes, no creature`. Palet: `earthy medieval palette: wood browns, linen cream, burgundy, wheat gold, iron grey`. Kontur koyu kahve, parlak renk yok (nakış marka dili, belge 17). `icon_item_miner_ration` için **420a ile aynı yöntem** (ürün ikonu).

| Dosya | Açıklama (+ ortak ek) |
|---|---|
| ui_icon_request_favor | `a small woven basket with a folded linen cloth on top` |
| ui_icon_request_order | `a rolled parchment order tied with a red string and a wax seal` |
| ui_icon_request_seasonal | `an open leather-bound book with a pressed wheat ear between the pages` |
| ui_icon_request_call | `a small bronze hand bell with a wooden handle` |
| ui_icon_request_story | `an open parchment scroll with a quill pen resting on it` |
| ui_icon_request_vow | `a small carved stone block with a burgundy ribbon tied around it` |
| ui_icon_request_bequest | `a sealed folded letter with a dark red wax seal and a sprig of rosemary` |
| ui_icon_request_marker | `a small embroidered cream cloth tag with a burgundy cross-stitch exclamation sign` (16x16 okunurluk: kalın, sade) |
| ui_icon_duty_mine | `an iron pickaxe leaning on a lump of grey stone` |
| ui_icon_duty_fish | `a wicker fish basket with a silver fish tail sticking out` |
| ui_icon_duty_forest | `a short iron hatchet on a split oak log` |
| ui_icon_duty_forage | `a small wicker basket with mushrooms and red berries` |
| ui_icon_duty_traps | `a simple rope snare loop tied to a wooden stake` |
| ui_icon_duty_clay | `a lump of wet reddish clay on a wooden board with a small trowel` |
| ui_icon_duty_errand | `a wooden walking staff with a small cloth bundle tied to it` |
| icon_item_miner_ration | `a small linen bundle tied with string, a loaf of bread and a wedge of cheese peeking out` |

## Doğrulama
- 32x32, yarı saydam 0, ≤16 renk; `python tools/art/style.py check --family arayuz <dosya> ...`.
- `python tools/art/validate_iso.py Assets/Art/Iso/_aday/icon_item_miner_ration.png`.
- Göz kontrolü x3 koyu ve keten zeminde; 16 ikon yan yana `art/results/480-yanyana.png`: tür ikonları birbirine karışmasın (sipariş/vasiyet: biri rulo, öteki katlanmış mektup).
- Dönem: çan, rulo, mühür, kazma, sepet ortaçağ nesnesi; Yeni Dünya bitkisi yok (mantar ve yemiş Eski Dünya).

## Notlar
- Kabul edilince `ui_icon_*` dosyaları `Assets/Resources/UI/Icons/` altına da kopyalanır (oyun oradan yükler; ad aynı kalır). `icon_item_miner_ration` eşlemesi ada göre kendiliğinden (`IsoArt.ItemIcon`).
- `ui_icon_request_marker` köylü başının üstünde 16x16 çizilir; 32x32 üretilip küçültmede okunur kalmalı.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
