---
id: "455b"
title: Beceri ikonları B (17)
status: review
claimed_at: 2026-11-20T07:02Z
approved_by: user
priority: 20
family: arayuz
style_gate: false
cost_est: 17
cost_max: 34
depends_on: []
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/ui_icon_skill_baking.png
  - Assets/Art/Iso/_aday/ui_icon_skill_chandlery.png
  - Assets/Art/Iso/_aday/ui_icon_skill_horticulture.png
  - Assets/Art/Iso/_aday/ui_icon_skill_foraging.png
  - Assets/Art/Iso/_aday/ui_icon_skill_hunting.png
  - Assets/Art/Iso/_aday/ui_icon_skill_weatherlore.png
  - Assets/Art/Iso/_aday/ui_icon_skill_soilcare.png
  - Assets/Art/Iso/_aday/ui_icon_skill_parenting.png
  - Assets/Art/Iso/_aday/ui_icon_skill_teaching.png
  - Assets/Art/Iso/_aday/ui_icon_skill_literacy.png
  - Assets/Art/Iso/_aday/ui_icon_skill_charm.png
  - Assets/Art/Iso/_aday/ui_icon_skill_music.png
  - Assets/Art/Iso/_aday/ui_icon_skill_stewardship.png
  - Assets/Art/Iso/_aday/ui_icon_skill_carting.png
  - Assets/Art/Iso/_aday/ui_icon_skill_decorating.png
  - Assets/Art/Iso/_aday/ui_icon_skill_medicine.png
  - Assets/Art/Iso/_aday/ui_icon_skill_athletics.png
created: 2026-11-20
---
# 455b Beceri ikonları B (17)

## Amaç
Beceri sistemi (Project belgesi `45-beceri-seti-analizi.md`, bölüm 10): karakterin başının üstündeki beceri çubuğu, seviye atlama kartı ve beceri panelinin 1-10 şeridi her beceriyi ikonuyla gösterir. Kod hazır: `StatusHud.SkillIconKey` → `Resources/UI/Icons/ui_icon_skill_<beceri>.png`; ikon yoksa yalnızca yazı çıkar. Kullanıcı kararı (2026-11-19, sohbet): baş üstü çubuk ve seviye kartı ikisi de gelsin.

## Tarif
399a/407y yöntemi: `create_map_object` 32x32, saydam, `view: side`, `single color outline`, `basic shading`, `medium detail`; okunmazsa ikinci deneme `create_image_pixflux` 32x32 `no_background=true`. Teslim öncesi 16 renge indir (yarı saydam 0). Ortak ek: `, single game icon, one object centered, no frame, no background, no text, no face, no eyes, no creature`. Palet: `earthy medieval palette: wood browns, linen cream, burgundy, wheat gold, iron grey`. Nakış marka diliyle uyum (belge 17): kontur koyu kahve, parlak renk yok. Dönem kuralı: Yeni Dünya bitkisi ve ortaçağ sonrası nesne yok.

| Dosya | Açıklama (+ ortak ek) |
|---|---|
| ui_icon_skill_baking | `a round loaf of brown bread with a wheat ear` |
| ui_icon_skill_chandlery | `two beeswax candles with small flames and a bar of soap` |
| ui_icon_skill_horticulture | `a fruit tree branch with two red apples and leaves` |
| ui_icon_skill_foraging | `a woven basket with berries and a brown mushroom` |
| ui_icon_skill_hunting | `a wooden hunting bow with one arrow` |
| ui_icon_skill_weatherlore | `a wooden weather vane with a small cloud and sun` |
| ui_icon_skill_soilcare | `a mound of dark compost soil with a green sprout` |
| ui_icon_skill_parenting | `a small wooden cradle with a cream blanket` |
| ui_icon_skill_teaching | `a wooden slate board with a chalk letter` |
| ui_icon_skill_literacy | `an open parchment book with a quill` |
| ui_icon_skill_charm | `two clay cups clinking with a small red heart` |
| ui_icon_skill_music | `a wooden shepherd flute (kaval) with a musical note` |
| ui_icon_skill_stewardship | `a ring of iron keys` |
| ui_icon_skill_carting | `a wooden hand cart with two wheels and a sack` |
| ui_icon_skill_decorating | `a woven red rug corner with a small vase of flowers` |
| ui_icon_skill_medicine | `a rolled linen bandage and a small ointment jar` |
| ui_icon_skill_athletics | `a pair of leather running shoes with motion lines` |

## Doğrulama
- 32x32, yarı saydam 0, ≤16 renk; `python tools/art/style.py check --family arayuz <dosya> ...` (mum ve ateş içerenlerde `--light`).
- Göz kontrolü x3 koyu ve keten zeminde; 35 beceri ikonunun hepsi bir arada `art/results/455-yanyana.png` (455b bitince) — birbirine karışan çift olmasın (Toplayıcılık/Bahçıvanlık, Aktarlık/Hekimlik).

## Notlar
Kabul edilince aynı dosyalar `Assets/Resources/UI/Icons/` altına da kopyalanır (oyun oradan yükler; ad aynı kalır).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
