---
id: "504"
title: P10 bebek mankeni pilotu
status: pending
approved_by: pending     # PixelLab bütçe kararı bekleniyor (kullanıcı, 20.11.2026: "daha sonra karar verelim"; 18.12'den önce sorulacak)
family: karakter
style_gate: true
cost_est: 45
cost_max: 60
depends_on: []
max_attempts: 1
pixellab: true
outputs:
  - Assets/Art/Iso/Characters/mk_baby_swaddle_s.png
  - Assets/Art/Iso/Characters/mk_baby_walk_s.png
created: 2026-11-20
---
# 504 P10 bebek mankeni pilotu

## Amaç
Belge 51 §2.2 ve §5 P10: bebek seti (kundak, bebek entarisi, takke, patik, battaniye, göğüslük) için anahtar renkli bebek mankeni.

## Tarif
İki biçim: kundak (kucakta, beşikte) ve yürüyen bebek; durma ve yürüme, s yönü pilot. Anahtar renkler 43c `KeyColors` (ten doğal rampa). Pilot kabulünden sonra kalan yönler ayrı işe bölünür (~200 gen toplam).

## Doğrulama
5 ten rengi varyantında rampa takası doğal; bebek takkesi baş çapasına oturuyor.

## Notlar
Kod tarafı hazır (belge 48c uygulama özeti): kesim maskeleri, çapalar, doku, kir, yama ve ikon kırpma `ClothingMasks` içinde; giysi verisi `clothing.json`.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
