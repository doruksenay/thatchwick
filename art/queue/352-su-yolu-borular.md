---
id: "352"
title: Su yolu karoları: kütük boru, künk, kurşun boru (yapı modu görünümü)
status: pending
approved_by: user        # otomatik akış (2026-11-17); su şebekesi PR (feat/su-sebekesi)
priority: 20
family: zemin
style_gate: false
cost_est: 10
cost_max: 20
depends_on: ["351"]
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/iso_conduit_{pipe_wood,pipe_clay,pipe_lead}_center.png
  - Assets/Art/Iso/_aday/iso_conduit_{pipe_wood,pipe_clay,pipe_lead}_arm_ne.png
  - Assets/Art/Iso/_aday/iso_conduit_{pipe_wood,pipe_clay,pipe_lead}_arm_se.png
created: 2026-11-17
---
# 352 Su yolu karoları: borular

## Amaç
Gömülü borular toprağın ve döşemenin altından geçer, eve girer; yalnızca yapı modunda (arsa görünümü) yarı saydam bir şerit olarak görünür. Dönem malzemeleri: **kütük boru** (ortası oyulmuş ağaç gövdesi, demir halkalı ek yeri), **künk** (kiremit renkli pişmiş toprak boru, ek yerinde bilezik), **kurşun boru** (mat gri, dövme ek).

## Tarif
- 351 ile aynı parça düzeni (center, arm_ne, arm_se; NW/SW ayna) ve aynı araç; 64x32, gölgesiz, saydam zemin.
- Borular üstten görünen ince silindir: toprağın içinde gibi çizilsin (kenarları hafif toprak lekesiyle), %100 opak; motor yapı modunda saydamlığı kendisi verir.
- Kurşun boru `center` parçasına dikine çıkan kısa bir kolon ucu (üst kata çıkış) eklensin: kurşun boru kolonu üst kattaki tesisata su çıkarır.

## Doğrulama
`validate_iso.py`, `style.py check --family zemin`; 351'deki birleşim testi; üç boru yan yana ayırt ediliyor mu.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
