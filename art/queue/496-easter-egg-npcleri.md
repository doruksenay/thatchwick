---
id: "496"
title: "Easter egg NPC'leri: Johan Wick ve atsız şövalye (84 px, 8 yön durma)"
status: review
claimed_at: 2026-11-20T11:36Z
approved_by: user        # otomatik akış (art/budget.md); karar: kullanıcı 20.11.2026 sohbet ("hepsi olsun", "yaz bunları ve sen ekle sıraya"), Project belgesi 55 §1
priority: 50
family: karakter
style_gate: false
cost_est: 6
cost_max: 12
depends_on: ["012b"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/anim/iso_char_johan_wick_r84_idle_<yön>.png
  - Assets/Art/Iso/_aday/anim/iso_char_horseless_knight_r84_idle_<yön>.png
  - art/results/496-easter-egg-npcleri.md
created: 2026-11-20
---
# 496 Easter egg NPC'leri: Johan Wick ve atsız şövalye

## Amaç
Project belgesi `55-easter-eggler.md` §1. Çok nadiren görünen iki ziyaretçi:
- **Johan Wick:** gelir, oyuncunun köpeğini sever, tarlayı dağıtır, bir altın sikke bırakıp gider.
- **Atsız şövalye:** köy yolunda, atı olmadan, at sesi çıkararak "dörtnala" giden gezgin.

İkisi de köylülerden ve hane halkından siluetle ayırt edilmeli.

## Tarif
- **Araç:** 415 ile aynı (012b yöntemi): `create_character`, `mode=v3`, `body_type=humanoid`, `view=high top-down`, `outline=single color black outline`, `detail=high detail`, `size=84`, **8 yön**.
- **Oran:** "realistic adult proportions, about 7 heads tall, small head relative to body, long legs, not chibi, not cute, not big-headed, grounded medieval".
- **Karakterler:**
  1. **johan_wick:** "quiet weathered medieval traveller, short cropped grey hair, clean-shaven, calm tired face, long charcoal-grey wool travelling cloak with the hood down over a dark plain tunic, worn leather boots, a small leather pouch of dog treats on the belt, empty hands, no weapon".
  2. **horseless_knight:** "wandering medieval knight on foot, faded cream surcoat with a plain burgundy stripe over a mail shirt, dented kettle helmet, both fists held forward at chest height as if holding invisible reins, knees slightly bent, no horse, no weapon".
- **Ortak ek:** 012b'deki ortak ek ve 32 renk `reduce_colors` aynen.
- **Kimlik referansı:** araç referans alıyorsa `Assets/Art/Iso/_aday/iso_char_man_r84_cand_se.png` (palet ve çizgi için).
- **Tuval ve pivot:** 84x84, ayaklar alttan 2 px, pivot x=42 y=2. Dosya adı `iso_char_{johan_wick,horseless_knight}_r84_idle_<yön>.png`, yönler e, ne, n, nw, w, sw, s, se.

## Doğrulama
- 415 doğrulaması: boy ±3 px, taban hizası, renk ≤32, alfa uyumu; `python tools/art/style.py check --family karakter <dosya> ...`.
- Yan yana sayfası: `man_r84`, `woman_r84` ve iki yeni karakter aynı satırda; ikisi de köylülerle aynı ölçekte ve siluetten ayırt edilir olmalı.
- **Özgünlük (bağlayıcı):** Johan Wick yalnızca adıyla ve davranışıyla bir göndermedir. Görünüş bu tarifteki gibi özgün kalır: gerçek bir oyuncuya ya da bir filmin karakterine benzetilmez; siyah takım elbise, uzun koyu saç ve sakal, modern giysi ya da silah eklenmez. Böyle çıkarsa teslim edilmez.
- Dönem: pelerin, tunik, zincir zırh, kazan miğfer ortaçağ. Silah yok (oyunda dövüş yok).

## Notlar
- Yalnızca durma kareleri. Yürüme ve özel hareketler (köpeği sevme, tarlayı dağıtma, "dörtnala" sekme) bu işte yok: katmanlı manken hattı (460a/b, 465) oturunca ayrı iş yazılır. O zamana kadar kod durma karesini kaydırarak yürütür; atsız şövalyede sekme kodla (dikey zıplama) verilir.
- `Assets/Art/Iso/` köküne onaysız bir şey konmaz.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
