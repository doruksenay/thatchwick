---
id: "455a"
title: Beceri ikonları A (18)
status: running
claimed_at: 2026-11-20T07:02Z
approved_by: user
priority: 20
family: arayuz
style_gate: false
cost_est: 18
cost_max: 36
depends_on: []
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/ui_icon_skill_farming.png
  - Assets/Art/Iso/_aday/ui_icon_skill_woodcutting.png
  - Assets/Art/Iso/_aday/ui_icon_skill_cooking.png
  - Assets/Art/Iso/_aday/ui_icon_skill_carpentry.png
  - Assets/Art/Iso/_aday/ui_icon_skill_animalhusbandry.png
  - Assets/Art/Iso/_aday/ui_icon_skill_riding.png
  - Assets/Art/Iso/_aday/ui_icon_skill_fishing.png
  - Assets/Art/Iso/_aday/ui_icon_skill_leatherwork.png
  - Assets/Art/Iso/_aday/ui_icon_skill_weaving.png
  - Assets/Art/Iso/_aday/ui_icon_skill_trade.png
  - Assets/Art/Iso/_aday/ui_icon_skill_herbalism.png
  - Assets/Art/Iso/_aday/ui_icon_skill_engineering.png
  - Assets/Art/Iso/_aday/ui_icon_skill_salvage.png
  - Assets/Art/Iso/_aday/ui_icon_skill_masonry.png
  - Assets/Art/Iso/_aday/ui_icon_skill_mining.png
  - Assets/Art/Iso/_aday/ui_icon_skill_smithing.png
  - Assets/Art/Iso/_aday/ui_icon_skill_pottery.png
  - Assets/Art/Iso/_aday/ui_icon_skill_brewing.png
created: 2026-11-20
---
# 455a Beceri ikonları A (18)

## Amaç
Beceri sistemi (Project belgesi `45-beceri-seti-analizi.md`, bölüm 10): karakterin başının üstündeki beceri çubuğu, seviye atlama kartı ve beceri panelinin 1-10 şeridi her beceriyi ikonuyla gösterir. Kod hazır: `StatusHud.SkillIconKey` → `Resources/UI/Icons/ui_icon_skill_<beceri>.png`; ikon yoksa yalnızca yazı çıkar. Kullanıcı kararı (2026-11-19, sohbet): baş üstü çubuk ve seviye kartı ikisi de gelsin.

## Tarif
399a/407y yöntemi: `create_map_object` 32x32, saydam, `view: side`, `single color outline`, `basic shading`, `medium detail`; okunmazsa ikinci deneme `create_image_pixflux` 32x32 `no_background=true`. Teslim öncesi 16 renge indir (yarı saydam 0). Ortak ek: `, single game icon, one object centered, no frame, no background, no text, no face, no eyes, no creature`. Palet: `earthy medieval palette: wood browns, linen cream, burgundy, wheat gold, iron grey`. Nakış marka diliyle uyum (belge 17): kontur koyu kahve, parlak renk yok. Dönem kuralı: Yeni Dünya bitkisi ve ortaçağ sonrası nesne yok.

| Dosya | Açıklama (+ ortak ek) |
|---|---|
| ui_icon_skill_farming | `a wooden hoe crossed with a golden wheat sheaf` |
| ui_icon_skill_woodcutting | `an iron felling axe stuck in a tree stump` |
| ui_icon_skill_cooking | `a black iron cooking pot with steam on a small fire` |
| ui_icon_skill_carpentry | `a wooden plane tool on a plank with curled wood shavings` |
| ui_icon_skill_animalhusbandry | `a wooden milk pail with a tuft of sheep wool` |
| ui_icon_skill_riding | `a brown leather saddle with stirrups` |
| ui_icon_skill_fishing | `a wooden fishing rod with a hooked silver fish` |
| ui_icon_skill_leatherwork | `a curved leather awl and a folded piece of tan leather` |
| ui_icon_skill_weaving | `a wooden drop spindle wound with cream wool yarn` |
| ui_icon_skill_trade | `a small leather coin purse with a few copper coins` |
| ui_icon_skill_herbalism | `a bundle of dried green herbs tied with string and a small clay pot` |
| ui_icon_skill_engineering | `a wooden cogwheel gear with chunky teeth` |
| ui_icon_skill_salvage | `a wooden crate with a pry bar and loose nails` |
| ui_icon_skill_masonry | `a stone mason chisel and mallet on a cut grey stone block` |
| ui_icon_skill_mining | `an iron pickaxe on a dark rock with an orange ore vein` |
| ui_icon_skill_smithing | `an iron anvil with a hammer resting on it` |
| ui_icon_skill_pottery | `a terracotta clay jug on a potter wheel` |
| ui_icon_skill_brewing | `a clay cup of hot drink beside a small wooden barrel` |

## Doğrulama
- 32x32, yarı saydam 0, ≤16 renk; `python tools/art/style.py check --family arayuz <dosya> ...` (mum ve ateş içerenlerde `--light`).
- Göz kontrolü x3 koyu ve keten zeminde; 35 beceri ikonunun hepsi bir arada `art/results/455-yanyana.png` (455b bitince) — birbirine karışan çift olmasın (Toplayıcılık/Bahçıvanlık, Aktarlık/Hekimlik).

## Notlar
Kabul edilince aynı dosyalar `Assets/Resources/UI/Icons/` altına da kopyalanır (oyun oradan yükler; ad aynı kalır).

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
