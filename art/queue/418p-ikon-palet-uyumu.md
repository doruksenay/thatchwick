---
id: "418p"
title: "İkon palet uyumu: 6 ikonun donmuş arayüz paletiyle yeniden üretimi"
status: review
claimed_at: 2026-11-20T17:51Z
approved_by: user        # kullanıcı kararı 2026-11-20: palet karşılaştırmasında "yalnızca belirgin değişenleri yeniden üretelim" (Project belgesi 64c §0.1)
priority: 20
family: arayuz
style_gate: false
cost_est: 6
cost_max: 12
depends_on: []
max_attempts: 2
pixellab: true
outputs:
  - Assets/Art/Iso/_aday/icon_item_cloth_v2.png
  - Assets/Art/Iso/_aday/icon_item_game_meat_v2.png
  - Assets/Art/Iso/_aday/icon_item_jam_berry_v2.png
  - Assets/Art/Iso/_aday/icon_item_meat_v2.png
  - Assets/Art/Iso/_aday/icon_item_salted_meat_v2.png
  - Assets/Art/Iso/_aday/icon_item_serbet_v2.png
created: 2026-11-20
---
# 418p İkon palet uyumu

## Neden
Palet kilidinden sonra (PR #102, `art/results/palet-kilidi-tarama.md`) kabul edilmiş 31 ikon donmuş arayüz paletinden kaldı. Kullanıcı yan yana karşılaştırmaya baktı: 20'si palete oturtuldu (dosyalar yerinde değişti); 2 arayüz ikonu (`ui_icon_act_chop`, `ui_icon_act_warm`) olduğu gibi kaldı; lahana, şalgam ve balkabağı için 418t v2'leri bekliyor. Aşağıdaki 6 ikon oturtulunca fazla soldu (pembe ve morlar); yeniden üretilecek.

## Tarif
Her ikon kendi kaynak işinin tarifini (tuval 32x32, kenar, ışık yönü) kullanır; stil referansı olarak kabul edilmiş ve palete uyan ikonlar (ör. `icon_item_cherry`, `icon_item_pear`, `icon_item_smoked_fish`). Yüz, göz ya da ağız yok.

| İkon | Ne olmalı |
|---|---|
| `icon_item_cloth` | Katlanmış kumaş topu, doğal boya tonu (kökboya kırmızısı ya da gülkurusu); parlak pembe değil |
| `icon_item_game_meat` | Çiğ av eti dilimi: koyu kırmızı-kahve, ince yağ damarı |
| `icon_item_jam_berry` | Toprak ya da cam kavanozda koyu mor-bordo böğürtlen reçeli, bez kapak ve ip |
| `icon_item_meat` | Çiğ et parçası: kırmızı et, krem yağ kenarı, kemiksiz |
| `icon_item_salted_meat` | Tuzlanmış et: koyu kırmızı-kahve, yüzeyde beyaz tuz taneleri |
| `icon_item_serbet` | Ayaklı toprak kâsede meyve şerbeti: koyu vişne-kırmızı sıvı; dondurma topu gibi değil |

- Renkler `art/style/palette_arayuz.json` içinden. Çıktılar `_v2` ekiyle; var olan PNG'lerin üzerine yazılmaz.
- Kabul edilince kod eşlemesi (`IsoArt.IconOverrides`) `_v2` dosyalarına çevrilir.

## Doğrulama
`python tools/art/style.py check --family arayuz <dosya>...` GEÇMELİ (donmuş palet bağlayıcı). Yan yana sayfası `art/results/418p-yanyana.png`: eski | palete oturtulmuş (önizleme) | v2.

<!-- İşçi bundan sonrasına dokunmaz; kullanıcı kararı:
status: review → accepted ya da rejected yap, rejected ise nedenini buraya yaz. -->
