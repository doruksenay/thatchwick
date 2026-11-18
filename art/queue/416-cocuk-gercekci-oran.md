---
id: "416"
title: Çocuk karakter gerçekçi oran, küçük boy (64 px tuval), erkek ve kız, 8 yön durma
status: pending
approved_by: user        # kullanıcı 18.11.2026 (sohbet): çocuk küçük kalsın; 84 setle aynı tarzda
priority: 50
family: karakter
style_gate: true
cost_est: 6
cost_max: 12
depends_on: ["012b"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/anim/iso_char_boy_r64_idle_<yön>.png
  - Assets/Art/Iso/_aday/anim/iso_char_girl_r64_idle_<yön>.png
  - art/results/416-cocuk-gercekci-oran.md
created: 2026-11-18
---
# 416 Çocuk karakter, gerçekçi oran, küçük boy

## Amaç
Çocuk 84 px'e büyütülmez; boyu küçük kalır ama tarzı 84 px yetişkinlerle aynı olur (eski 56 set chibi). Hedef: yetişkinin yanında 8-10 yaşında görünen, baş oranı yetişkinden büyük ama chibi olmayan çocuk. Bebek bu setin %60 ölçeğiyle çizilmeye devam eder (ayrı karar). Pilot (`style_gate`): kabulden sonra yürüme ve etkileşim işleri buna göre yazılır.

## Tarif
- **Araç:** 012b ile aynı ayarlar, `size=64`, **8 yön**.
- **Oran:** "realistic child proportions, about 5 heads tall, slender, roughly 8 to 10 years old, not chibi, not big-headed, not cute anime, grounded medieval peasant child".
- **Karakterler:**
  1. **boy:** "village boy, messy brown hair, plain undyed linen tunic with short sleeves and cord belt, bare legs, simple leather shoes".
  2. **girl:** "village girl, auburn hair in a single braid, faded olive wool dress with small linen apron, leather shoes".
- **Ortak ek ve renk:** 012b'deki gibi, 32 renk.
- **Kimlik referansı:** 84 px yetişkin adayları (palet ve çizgi).
- **Tuval ve pivot:** 64x64, ayaklar alttan 2 px, pivot x=32 y=2. Dosya adı `iso_char_{boy,girl}_r64_idle_<yön>.png`.

## Doğrulama
- `validate_iso`, `style.py check --family karakter`.
- **Boy ölçütü:** opak boy, 84 yetişkinin opak boyunun %62-72'si; boy/kafa 4,5-5,5. Tablo.
- Yan yana sayfası (`416-yanyana.png`): yetişkin erkek ve kadın 84 SE, eski 56 çocuk ve yeni iki çocuk SE, 64x32 karonun üstünde, 4 kat.

## Notlar
- Kod (`IsoArt`): çocuk seti `_r64_idle_<yön>`, pivot 32,2, ölçek 1; bebek aynı setin 0,6'sı. Kod oturumu bağlar.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
