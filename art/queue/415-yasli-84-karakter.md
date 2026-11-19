---
id: "415"
title: Yaşlı karakter 84 px gerçekçi oran, erkek ve kadın, 8 yön durma
status: accepted   # kullanıcı 19.11.2026: "bence okay böyle" (boy farkı kodda ölçekle)
claimed_at: 2026-11-18T19:35Z
approved_by: user        # kullanıcı 18.11.2026 (sohbet): "yaşlı geçsin"
priority: 50
family: karakter
style_gate: false
cost_est: 6
cost_max: 12
depends_on: ["012b"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/anim/iso_char_elder_m_r84_idle_<yön>.png
  - Assets/Art/Iso/_aday/anim/iso_char_elder_f_r84_idle_<yön>.png
  - art/results/415-yasli-84-karakter.md
created: 2026-11-18
---
# 415 Yaşlı karakter 84 px

## Amaç
Yaşlı evresi eski 56 px chibi setten 84 px gerçekçi tabana geçer (kullanıcı kararı 18.11.2026). Yetişkin 84 px adamla (012b `man_r84`) ve kadınla (`woman_r84`) aynı dünyadan görünmeli. Yürüme kareleri bu işte yok: yürüme yöntemi (018a2) çözülünce ayrı iş açılır.

## Tarif
- **Araç:** 012b ile aynı: `create_character`, `mode=v3`, `body_type=humanoid`, `view=high top-down`, `outline=single color black outline`, `detail=high detail`, `size=84`, **8 yön**.
- **Oran:** "realistic elderly proportions, about 6.5 heads tall, slightly stooped shoulders, not chibi, not cute, grounded medieval peasant".
- **Karakterler:**
  1. **elder_m:** "old village man, grey thinning hair and short white beard, weathered face, faded brown wool tunic with rope belt, worn linen sleeves, leg wraps, leather shoes, holds no tool".
  2. **elder_f:** "old village woman, grey hair in a linen head cloth, lined face, faded dark green wool dress with grey shawl and linen apron, leather shoes".
- **Ortak ek:** 012b'deki ortak ek ve 32 renk `reduce_colors` aynen.
- **Kimlik referansı:** araç referans alıyorsa `Assets/Art/Iso/_aday/iso_char_man_r84_cand_se.png` ve `iso_char_woman_r84_cand_se.png` (palet ve çizgi için).
- **Tuval ve pivot:** 84x84, ayaklar alttan 2 px, pivot x=42 y=2 (018 karelerinin kuralı). Dosya adı `iso_char_elder_{m,f}_r84_idle_<yön>.png`, yönler e, ne, n, nw, w, sw, s, se.

## Doğrulama
- `validate_iso`, `style.py check --family karakter`.
- Opak boy yetişkin 84 setinin %92-97'si (hafif kambur); tablo.
- Yan yana sayfası (`415-yanyana.png`): yetişkin erkek ve kadın 84 SE yanında iki yaşlı SE, 64x32 karonun üstünde, 4 kat.

## Notlar
- Kod hazır olunca `IsoArt.AdultR84` yaşlıyı da kapsayacak (kod oturumu); bu dosyalar gelene kadar yaşlı 56 setinde kalır.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
